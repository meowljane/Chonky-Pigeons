using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PigeonGame.Gameplay;
using PigeonGame.Data;
using System.Collections.Generic;

namespace PigeonGame.UI
{
    public class TrapPlacementUI : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button trapPlacementButton;

        [Header("Trap Selection Panel")]
        [SerializeField] private GameObject trapSelectionPanel;
        [SerializeField] private Transform trapGridContainer;
        [SerializeField] private GameObject trapSlot;
        [SerializeField] private Button closeButton;

        [Header("Info Display")]
        [SerializeField] private TextMeshProUGUI currentTerrainText;
        [SerializeField] private TextMeshProUGUI selectedTrapNameText;
        [SerializeField] private TextMeshProUGUI terrainPigeonsText;
        [SerializeField] private TextMeshProUGUI trapPigeonsText;

        [Header("Bottom Controls")]
        [SerializeField] private TMPro.TMP_InputField feedAmountInput;
        [SerializeField] private Button feedDecreaseButton; 
        [SerializeField] private Button feedIncreaseButton; 
        [SerializeField] private TextMeshProUGUI totalPriceText;
        [SerializeField] private Button installButton;

        [Header("References")]
        [SerializeField] private TrapPlacer trapPlacer;
        [SerializeField] private WorldPigeonManager pigeonManager;
        private List<GameObject> trapItemObjects = new List<GameObject>();
        private TrapType selectedTrapId;

        private void Start()
        {
            if (trapPlacer == null)
            {
                Debug.LogError("TrapPlacer가 할당되지 않았습니다!", this);
                enabled = false;
                return;
            }

            if (pigeonManager == null)
                Debug.LogError("WorldPigeonManager가 할당되지 않았습니다!", this);

            UIHelper.SafeAddListener(trapPlacementButton, OnTrapPlacementButtonClicked);
            PanelUIHelper.InitializePanel(trapSelectionPanel, closeButton, OnCloseButtonClicked);
            if (feedAmountInput != null) feedAmountInput.onValueChanged.AddListener(OnFeedAmountChanged);
            UIHelper.SafeAddListener(feedDecreaseButton, OnFeedDecreaseClicked);
            UIHelper.SafeAddListener(feedIncreaseButton, OnFeedIncreaseClicked);
            UIHelper.SafeAddListener(installButton, OnInstallButtonClicked);

            CreateTrapGrid();

            var registry = GameDataRegistry.Instance;
            if (registry != null && registry.Traps != null && registry.Traps.traps.Length > 0)
            {
                var firstTrap = registry.Traps.traps[0];
                if (firstTrap != null && GameManager.Instance != null && 
                    GameManager.Instance.IsTrapUnlocked(firstTrap.trapType))
                {
                    OnTrapSelected(firstTrap.trapType);
                }
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnMoneyChanged += OnMoneyChanged;
                GameManager.Instance.OnTrapUnlocked += OnTrapUnlocked;
            }
        }

        private void CreateTrapGrid()
        {
            if (trapGridContainer == null || trapSlot == null)
                return;

            var registry = GameDataRegistry.Instance;
            if (registry == null || registry.Traps == null)
                return;

            UIHelper.ClearSlotList(trapItemObjects);

            var allTraps = registry.Traps.traps;
            foreach (var trapData in allTraps)
            {
                GameObject slotObj = Instantiate(trapSlot, trapGridContainer, false);
                SetupTrapSlot(slotObj, trapData);
                trapItemObjects.Add(slotObj);
            }
        }

        private void SetupTrapSlot(GameObject slotObj, TrapDefinition trapData)
        {
            TrapPlacementSlotUI slotUI = slotObj.GetComponent<TrapPlacementSlotUI>();
            if (slotUI == null) return;

            bool isUnlocked = GameManager.Instance?.IsTrapUnlocked(trapData.trapType) ?? false;
            slotUI.SetUnlocked(isUnlocked);

            UIHelper.SetTrapIcon(slotUI.IconImage, trapData.icon);

            if (slotUI.NameText != null)
            {
                slotUI.NameText.text = isUnlocked ? trapData.name : $"{trapData.name}\n(해금 필요)";
            }

            if (slotUI.Button != null)
            {
                slotUI.Button.onClick.RemoveAllListeners();
                slotUI.Button.onClick.AddListener(() => OnTrapSelected(trapData.trapType));
            }

            UpdateTrapSlotSelection(slotObj, trapData.trapType);
        }

        private void UpdateTrapSlotSelection(GameObject slotObj, TrapType trapType)
        {
            TrapPlacementSlotUI slotUI = slotObj.GetComponent<TrapPlacementSlotUI>();
            if (slotUI == null) return;

            bool isSelected = selectedTrapId == trapType;
            slotUI.SetSelected(isSelected);
        }

        private void OnTrapSelected(TrapType trapType)
        {
            selectedTrapId = trapType;

            var registry = GameDataRegistry.Instance;
            if (registry?.Traps != null)
            {
                var allTraps = registry.Traps.traps;
                for (int i = 0; i < trapItemObjects.Count && i < allTraps.Length; i++)
                {
                    if (trapItemObjects[i] == null) continue;
                    UpdateTrapSlotSelection(trapItemObjects[i], allTraps[i].trapType);
                }
            }

            UpdateInfoDisplay(trapType);
            UpdatePriceDisplay();
        }

        private void UpdateInfoDisplay(TrapType trapType)
        {
            var registry = GameDataRegistry.Instance;
            if (registry == null || registry.Traps == null)
                return;

            var trapData = registry.Traps.GetTrapById(trapType);
            if (trapData == null)
                return;

            TerrainType currentTerrain = TerrainType.SAND;
            if (PlayerController.Instance != null)
            {
                currentTerrain = TilemapRangeManager.Instance?.GetTerrainTypeAtPosition(PlayerController.Instance.Position) ?? TerrainType.SAND;
            }

            if (currentTerrainText != null)
            {
                currentTerrainText.text = $"현재 지형: {UIHelper.GetTerrainName(currentTerrain)}";
            }

            if (selectedTrapNameText != null)
                selectedTrapNameText.text = $"선택한 덫: {trapData.name}";

            UpdateFavoriteSpeciesList(terrainPigeonsText, species => species.favoriteTerrain == currentTerrain);
            UpdateFavoriteSpeciesList(trapPigeonsText, species => species.favoriteTrapType == trapType);

            if (feedAmountInput != null)
                feedAmountInput.SetTextWithoutNotify(trapData.feedAmount.ToString());
        }

        private void UpdateFavoriteSpeciesList(TextMeshProUGUI text, System.Func<SpeciesDefinition, bool> predicate)
        {
            if (text == null) return;
            text.text = UIHelper.GetFavoriteSpeciesText(predicate, "선호 비둘기");
        }

        private void OnFeedAmountChanged(string value)
        {
            if (!string.IsNullOrEmpty(value) && int.TryParse(value, out int amount) && amount > 1000)
                feedAmountInput?.SetTextWithoutNotify("1000");
            UpdatePriceDisplay();
        }

        private void OnFeedDecreaseClicked()
        {
            if (feedAmountInput == null) return;

            int.TryParse(feedAmountInput.text, out int currentAmount);
            currentAmount = Mathf.Max(1, currentAmount - 1);
            feedAmountInput.text = currentAmount.ToString();
            UpdatePriceDisplay();
        }

        private void OnFeedIncreaseClicked()
        {
            if (feedAmountInput == null) return;

            int.TryParse(feedAmountInput.text, out int currentAmount);
            currentAmount = Mathf.Min(1000, currentAmount + 1);
            feedAmountInput.text = currentAmount.ToString();
            UpdatePriceDisplay();
        }

        private void UpdatePriceDisplay()
        {
            if (GameManager.Instance == null || totalPriceText == null) return;

            var registry = GameDataRegistry.Instance;
            var trapData = registry?.Traps?.GetTrapById(selectedTrapId);
            if (trapData == null) return;

            int feedAmount = trapData.feedAmount;
            if (feedAmountInput != null && int.TryParse(feedAmountInput.text, out int parsedAmount))
            {
                feedAmount = Mathf.Clamp(parsedAmount, 1, 1000);
            }

            int installCost = GameManager.Instance.CalculateTrapInstallCost(selectedTrapId, feedAmount);
            int currentMoney = GameManager.Instance.CurrentMoney;

            totalPriceText.text = $"총 비용: {installCost}G / 현재 골드: {currentMoney}G";
            totalPriceText.color = currentMoney >= installCost ? Color.white : Color.red;
            installButton.interactable = currentMoney >= installCost && feedAmount > 0;
        }

        private void OnInstallButtonClicked()
        {
            var registry = GameDataRegistry.Instance;
            var trapData = registry?.Traps?.GetTrapById(selectedTrapId);
            int feedAmount = trapData?.feedAmount ?? 0;

            if (feedAmountInput != null && int.TryParse(feedAmountInput.text, out int parsedAmount))
            {
                feedAmount = Mathf.Clamp(parsedAmount, 1, 1000);
            }

            if (feedAmount > 0 && trapPlacer?.PlaceTrapAtPlayerPosition(selectedTrapId, feedAmount) == true)
            {
                trapSelectionPanel?.SetActive(false);
                UpdatePriceDisplay();
            }
        }

        private void OnTrapPlacementButtonClicked()
        {
            if (trapSelectionPanel == null) return;

            bool isActive = trapSelectionPanel.activeSelf;
            if (!isActive)
            {
                PanelUIHelper.OpenPanel(trapSelectionPanel, () => {
                    UpdateTrapItems();
                    UpdateInfoDisplay(selectedTrapId);
                    UpdatePriceDisplay();
                });
            }
            else
            {
                PanelUIHelper.ClosePanel(trapSelectionPanel);
            }
        }

        private void OnCloseButtonClicked()
        {
            PanelUIHelper.ClosePanel(trapSelectionPanel);
        }

        private void UpdateTrapItems()
        {
            var registry = GameDataRegistry.Instance;
            if (registry?.Traps == null) return;

            var allTraps = registry.Traps.traps;
            for (int i = 0; i < allTraps.Length && i < trapItemObjects.Count; i++)
            {
                if (trapItemObjects[i] != null)
                {
                    SetupTrapSlot(trapItemObjects[i], allTraps[i]);
                }
            }
        }

        private void OnTrapUnlocked(TrapType trapType)
        {
            UpdateTrapItems();
        }

        private void OnMoneyChanged(int money)
        {
            UpdatePriceDisplay();
        }

        private void OnDestroy()
        {
            UIHelper.SafeRemoveListener(trapPlacementButton);
            UIHelper.SafeRemoveListener(closeButton);
            UIHelper.SafeRemoveListener(installButton);
            UIHelper.SafeRemoveListener(feedDecreaseButton);
            UIHelper.SafeRemoveListener(feedIncreaseButton);
            if (feedAmountInput != null) feedAmountInput.onValueChanged.RemoveAllListeners();
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnMoneyChanged -= OnMoneyChanged;
                GameManager.Instance.OnTrapUnlocked -= OnTrapUnlocked;
            }
        }
    }
}

