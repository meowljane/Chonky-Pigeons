using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PigeonGame.Gameplay;
using PigeonGame.Data;
using System.Collections.Generic;

namespace PigeonGame.UI
{
    /// <summary>
    /// 우측 하단 덫 설치 UI
    /// 덫 설치 버튼과 덫 선택 그리드를 관리
    /// </summary>
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
        [SerializeField] private Button feedDecreaseButton; // 모이 수량 감소 버튼
        [SerializeField] private Button feedIncreaseButton; // 모이 수량 증가 버튼
        [SerializeField] private TextMeshProUGUI totalPriceText;
        [SerializeField] private Button installButton;


        private TrapPlacer trapPlacer;
        private WorldPigeonManager pigeonManager;
        private List<GameObject> trapItemObjects = new List<GameObject>();
        private TrapType selectedTrapId;
        private GameObject selectedTrapItem; // 현재 선택된 덫 아이템

        private void Start()
        {
            // TrapPlacer 찾기
            trapPlacer = FindFirstObjectByType<TrapPlacer>();
            if (trapPlacer == null)
            {
                enabled = false;
                return;
            }

            // WorldPigeonManager 찾기
            pigeonManager = FindFirstObjectByType<WorldPigeonManager>();

            UIHelper.SafeAddListener(trapPlacementButton, OnTrapPlacementButtonClicked);
            if (trapSelectionPanel != null) trapSelectionPanel.SetActive(false);
            UIHelper.SafeAddListener(closeButton, OnCloseButtonClicked);
            if (feedAmountInput != null) feedAmountInput.onValueChanged.AddListener(OnFeedAmountChanged);
            UIHelper.SafeAddListener(feedDecreaseButton, OnFeedDecreaseClicked);
            UIHelper.SafeAddListener(feedIncreaseButton, OnFeedIncreaseClicked);
            UIHelper.SafeAddListener(installButton, OnInstallButtonClicked);

            // 덫 그리드 생성
            CreateTrapGrid();
            
            // 첫 번째 덫 자동 선택
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
            
            // GameManager 이벤트 구독
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

            // 모든 덫 표시
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
            if (slotUI == null)
                return;

            bool isUnlocked = GameManager.Instance != null && 
                             GameManager.Instance.IsTrapUnlocked(trapData.trapType);

            // 해금 상태에 따른 색상 및 활성화 설정
            slotUI.SetUnlocked(isUnlocked);

            // 아이콘 표시
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

            // 덫 이름 표시
            if (slotUI.NameText != null)
            {
                slotUI.NameText.text = isUnlocked ? trapData.name : $"{trapData.name}\n(해금 필요)";
            }

            // 버튼 클릭 이벤트
            if (slotUI.Button != null)
            {
                slotUI.Button.onClick.RemoveAllListeners();
                slotUI.Button.onClick.AddListener(() => OnTrapSelected(trapData.trapType));
            }

            // 체크마크 초기화
            if (slotUI.Checkmark != null)
            {
                slotUI.Checkmark.SetActive(false);
            }

            // 선택 상태 업데이트
            UpdateTrapSlotSelection(slotObj, trapData.trapType);
        }

        private void UpdateTrapSlotSelection(GameObject slotObj, TrapType trapType)
        {
            TrapPlacementSlotUI slotUI = slotObj.GetComponent<TrapPlacementSlotUI>();
            if (slotUI == null) return;

            bool isSelected = selectedTrapId == trapType;
            if (slotUI.Checkmark != null) slotUI.Checkmark.SetActive(isSelected);
            if (isSelected) selectedTrapItem = slotObj;
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
                    
                    TrapPlacementSlotUI slotUI = itemObj.GetComponent<TrapPlacementSlotUI>();
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

            // 현재 Terrain 정보
            TerrainType currentTerrain = TerrainType.SAND;
            if (PlayerController.Instance != null && MapManager.Instance != null)
            {
                currentTerrain = MapManager.Instance.GetTerrainTypeAtPosition(PlayerController.Instance.Position);
            }

            // Terrain 표시
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

            // 선택한 덫 이름 표시
            if (selectedTrapNameText != null)
            {
                selectedTrapNameText.text = $"선택한 덫: {trapData.name}";
            }

            // Terrain을 좋아하는 비둘기 표시
            if (terrainPigeonsText != null && registry.SpeciesSet != null)
            {
                List<string> terrainPigeonNames = new List<string>();
                foreach (var species in registry.SpeciesSet.species)
                {
                    if (species.favoriteTerrain == currentTerrain)
                    {
                        terrainPigeonNames.Add(species.name);
                    }
                }
                terrainPigeonsText.text = terrainPigeonNames.Count > 0 
                    ? $"선호 비둘기: {string.Join(", ", terrainPigeonNames)}"
                    : "선호 비둘기: 없음";
            }

            // 덫을 좋아하는 비둘기 표시
            if (trapPigeonsText != null && registry.SpeciesSet != null)
            {
                List<string> trapPigeonNames = new List<string>();
                foreach (var species in registry.SpeciesSet.species)
                {
                    if (species.favoriteTrapType == trapType)
                    {
                        trapPigeonNames.Add(species.name);
                    }
                }
                trapPigeonsText.text = trapPigeonNames.Count > 0
                    ? $"선호 비둘기: {string.Join(", ", trapPigeonNames)}"
                    : "선호 비둘기: 없음";
            }

            // 모이 수량 초기화
            if (feedAmountInput != null)
            {
                feedAmountInput.text = trapData.feedAmount.ToString();
            }
        }

        private void OnFeedAmountChanged(string value)
        {
            // 최대값 1000 제한
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

            currentAmount = Mathf.Max(1, currentAmount - 1); // 최소 1
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

            currentAmount = Mathf.Min(1000, currentAmount + 1); // 최대 1000
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

                // 패널이 열릴 때 덫 상태 업데이트
                if (!isActive)
                {
                    UpdateTrapItems();
                    // 선택된 덫이 있으면 정보 업데이트
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
                
                TrapPlacementSlotUI slotUI = itemObj.GetComponent<TrapPlacementSlotUI>();
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
            // 가격 표시 업데이트
            UpdatePriceDisplay();
        }

        private void ClearTrapItems()
        {
            UIHelper.ClearSlotList(trapItemObjects);
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

