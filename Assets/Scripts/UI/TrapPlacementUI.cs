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
        private Dictionary<GameObject, TrapPlacementSlotUI> slotUICache = new Dictionary<GameObject, TrapPlacementSlotUI>(); 
        private TrapType selectedTrapId;
        private GameObject selectedTrapItem; 

        private List<string> reusablePigeonNamesList = new List<string>();

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
            if (trapSelectionPanel != null) trapSelectionPanel.SetActive(false);
            UIHelper.SafeAddListener(closeButton, OnCloseButtonClicked);
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

            ClearTrapItems();

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
            if (!slotUICache.TryGetValue(slotObj, out TrapPlacementSlotUI slotUI))
            {
                slotUI = slotObj.GetComponent<TrapPlacementSlotUI>();
                if (slotUI == null)
                    return;
                slotUICache[slotObj] = slotUI;
            }

            bool isUnlocked = GameManager.Instance != null && 
                             GameManager.Instance.IsTrapUnlocked(trapData.trapType);

            slotUI.SetUnlocked(isUnlocked);

            if (slotUI.IconImage != null)
            {
                if (trapData.icon != null)
                {
                    slotUI.IconImage.sprite = trapData.icon;
                    slotUI.IconImage.enabled = true;
                }
                else
                {
                    slotUI.IconImage.enabled = false;
                }
            }

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
            if (!slotUICache.TryGetValue(slotObj, out TrapPlacementSlotUI slotUI))
            {
                slotUI = slotObj.GetComponent<TrapPlacementSlotUI>();
                if (slotUI == null) return;
                slotUICache[slotObj] = slotUI;
            }

            bool isSelected = selectedTrapId == trapType;
            if (isSelected) selectedTrapItem = slotObj;

            slotUI.SetSelected(isSelected);
        }

        private void OnTrapSelected(TrapType trapType)
        {
            selectedTrapId = trapType;

            var registry = GameDataRegistry.Instance;
            if (registry?.Traps != null)
            {
                foreach (var itemObj in trapItemObjects)
                {
                    if (itemObj == null) continue;

                    if (!slotUICache.TryGetValue(itemObj, out TrapPlacementSlotUI slotUI))
                    {
                        slotUI = itemObj.GetComponent<TrapPlacementSlotUI>();
                        if (slotUI == null) continue;
                        slotUICache[itemObj] = slotUI;
                    }

                    if (slotUI?.NameText == null) continue;

                    string itemName = slotUI.NameText.text.Replace("\n(해금 필요)", "").Trim();
                    foreach (var trap in registry.Traps.traps)
                    {
                        if (trap.name == itemName)
                        {
                            UpdateTrapSlotSelection(itemObj, trap.trapType);
                            break;
                        }
                    }
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
            if (PlayerController.Instance != null && MapManager.Instance != null)
            {
                currentTerrain = MapManager.Instance.GetTerrainTypeAtPosition(PlayerController.Instance.Position);
            }

            if (currentTerrainText != null)
            {
                string terrainName = currentTerrain.ToString();
                if (registry.TerrainTypes != null)
                {
                    var terrainDef = registry.TerrainTypes.GetTerrainById(currentTerrain);
                    if (terrainDef != null)
                    {
                        terrainName = terrainDef.koreanName;
                    }
                }
                currentTerrainText.text = $"현재 지형: {terrainName}";
            }

            if (selectedTrapNameText != null)
            {
                selectedTrapNameText.text = $"선택한 덫: {trapData.name}";
            }

            if (terrainPigeonsText != null && registry.SpeciesSet != null)
            {
                reusablePigeonNamesList.Clear();
                foreach (var species in registry.SpeciesSet.species)
                {
                    if (species.favoriteTerrain == currentTerrain)
                    {
                        reusablePigeonNamesList.Add(species.name);
                    }
                }
                terrainPigeonsText.text = reusablePigeonNamesList.Count > 0 
                    ? $"선호 비둘기: {string.Join(", ", reusablePigeonNamesList)}"
                    : "선호 비둘기: 없음";
            }

            if (trapPigeonsText != null && registry.SpeciesSet != null)
            {
                reusablePigeonNamesList.Clear();
                foreach (var species in registry.SpeciesSet.species)
                {
                    if (species.favoriteTrapType == trapType)
                    {
                        reusablePigeonNamesList.Add(species.name);
                    }
                }
                trapPigeonsText.text = reusablePigeonNamesList.Count > 0
                    ? $"선호 비둘기: {string.Join(", ", reusablePigeonNamesList)}"
                    : "선호 비둘기: 없음";
            }

            if (feedAmountInput != null)
            {
                feedAmountInput.text = trapData.feedAmount.ToString();
            }
        }

        private void OnFeedAmountChanged(string value)
        {
            if (!string.IsNullOrEmpty(value) && int.TryParse(value, out int amount))
            {
                if (amount > 1000)
                {
                    if (feedAmountInput != null)
                    {
                        feedAmountInput.text = "1000";
                    }
                }
            }
            UpdatePriceDisplay();
        }

        private void OnFeedDecreaseClicked()
        {
            if (feedAmountInput == null)
                return;

            int currentAmount = 1;
            if (int.TryParse(feedAmountInput.text, out int parsedAmount))
            {
                currentAmount = parsedAmount;
            }

            currentAmount = Mathf.Max(1, currentAmount - 1); 
            feedAmountInput.text = currentAmount.ToString();
            UpdatePriceDisplay();
        }

        private void OnFeedIncreaseClicked()
        {
            if (feedAmountInput == null)
                return;

            int currentAmount = 1;
            if (int.TryParse(feedAmountInput.text, out int parsedAmount))
            {
                currentAmount = parsedAmount;
            }

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
            if (installButton != null) installButton.interactable = currentMoney >= installCost && feedAmount > 0;
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

            if (trapPlacer != null && feedAmount > 0)
            {
                if (trapPlacer.PlaceTrapAtPlayerPosition(selectedTrapId, feedAmount))
                {
                    if (trapSelectionPanel != null) trapSelectionPanel.SetActive(false);
                    UpdatePriceDisplay();
                }
            }
        }

        private void OnTrapPlacementButtonClicked()
        {
            if (trapSelectionPanel != null)
            {
                bool isActive = trapSelectionPanel.activeSelf;
                trapSelectionPanel.SetActive(!isActive);

                if (!isActive)
                {
                    UpdateTrapItems();
                    UpdateInfoDisplay(selectedTrapId);
                    UpdatePriceDisplay();
                }
            }
        }

        private void OnCloseButtonClicked()
        {
            if (trapSelectionPanel != null)
            {
                trapSelectionPanel.SetActive(false);
            }
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

            foreach (var itemObj in trapItemObjects)
            {
                if (itemObj == null) continue;

                if (!slotUICache.TryGetValue(itemObj, out TrapPlacementSlotUI slotUI))
                {
                    slotUI = itemObj.GetComponent<TrapPlacementSlotUI>();
                    if (slotUI == null) continue;
                    slotUICache[itemObj] = slotUI;
                }

                if (slotUI?.NameText == null) continue;

                string itemName = slotUI.NameText.text.Replace("\n(해금 필요)", "").Trim();
                foreach (var trap in allTraps)
                {
                    if (trap.name == itemName)
                    {
                        UpdateTrapSlotSelection(itemObj, trap.trapType);
                        break;
                    }
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

        private void ClearTrapItems()
        {
            UIHelper.ClearSlotList(trapItemObjects);
            slotUICache.Clear();
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

