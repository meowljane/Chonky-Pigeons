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
    /// 덫 설치 버튼과 상호작용 버튼, 덫 선택 그리드를 관리
    /// </summary>
    public class TrapPlacementUI : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button trapPlacementButton;
        [SerializeField] private Button interactionButton;

        [Header("Trap Selection Panel")]
        [SerializeField] private GameObject trapSelectionPanel;
        [SerializeField] private Transform trapGridContainer;
        [SerializeField] private GameObject trapItemPrefab;
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

        [Header("Trap Item UI")]
        [SerializeField] private Color unlockedColor = Color.white;
        [SerializeField] private Color lockedColor = Color.gray;
        [SerializeField] private Sprite checkmarkSprite; // 체크마크 이미지 (Inspector에서 할당)

        private TrapPlacer trapPlacer;
        private WorldPigeonManager pigeonManager;
        private List<GameObject> trapItemObjects = new List<GameObject>();
        private string selectedTrapId;
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

            // 버튼 이벤트 연결
            if (trapPlacementButton != null)
            {
                trapPlacementButton.onClick.AddListener(OnTrapPlacementButtonClicked);
            }

            if (interactionButton != null)
            {
                interactionButton.onClick.AddListener(OnInteractionButtonClicked);
            }

            // 덫 선택 패널 초기화
            if (trapSelectionPanel != null)
            {
                trapSelectionPanel.SetActive(false);
            }

            // 닫기 버튼 찾기 및 연결
            if (closeButton == null && trapSelectionPanel != null)
            {
                closeButton = trapSelectionPanel.GetComponentInChildren<Button>();
                if (closeButton == null)
                {
                    closeButton = trapSelectionPanel.transform.Find("CloseButton")?.GetComponent<Button>();
                }
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(OnCloseButtonClicked);
            }

            // 모이 수량 입력 필드 이벤트
            if (feedAmountInput != null)
            {
                feedAmountInput.onValueChanged.AddListener(OnFeedAmountChanged);
            }

            // 모이 수량 조절 버튼 이벤트
            if (feedDecreaseButton != null)
            {
                feedDecreaseButton.onClick.RemoveAllListeners();
                feedDecreaseButton.onClick.AddListener(OnFeedDecreaseClicked);
            }

            if (feedIncreaseButton != null)
            {
                feedIncreaseButton.onClick.RemoveAllListeners();
                feedIncreaseButton.onClick.AddListener(OnFeedIncreaseClicked);
            }

            // 설치 버튼 이벤트
            if (installButton != null)
            {
                installButton.onClick.RemoveAllListeners();
                installButton.onClick.AddListener(OnInstallButtonClicked);
            }

            // 덫 그리드 생성
            CreateTrapGrid();
            
            // 첫 번째 덫 자동 선택
            var registry = GameDataRegistry.Instance;
            if (registry != null && registry.Traps != null && registry.Traps.traps.Length > 0)
            {
                var firstTrap = registry.Traps.traps[0];
                if (firstTrap != null && GameManager.Instance != null && 
                    GameManager.Instance.IsTrapUnlocked(firstTrap.id))
                {
                    OnTrapSelected(firstTrap.id);
                }
            }
            
            // GameManager 돈 변경 이벤트 구독
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnMoneyChanged += OnMoneyChanged;
            }
        }

        private void CreateTrapGrid()
        {
            if (trapGridContainer == null || trapItemPrefab == null)
                return;

            var registry = GameDataRegistry.Instance;
            if (registry == null || registry.Traps == null)
                return;

            ClearItemList(trapItemObjects);

            // 모든 덫 표시
            var allTraps = registry.Traps.traps;
            foreach (var trapData in allTraps)
            {
                GameObject itemObj = Instantiate(trapItemPrefab, trapGridContainer, false);
                SetupTrapItem(itemObj, trapData);
                trapItemObjects.Add(itemObj);
            }
        }

        private void SetupTrapItem(GameObject itemObj, TrapDefinition trapData)
        {
            bool isUnlocked = GameManager.Instance != null && 
                             GameManager.Instance.IsTrapUnlocked(trapData.id);

            // 배경색 설정
            Image bg = itemObj.GetComponent<Image>();
            if (bg != null)
            {
                bg.color = isUnlocked ? unlockedColor : lockedColor;
            }

            // 선택 체크마크 생성 (없으면)
            GameObject checkmarkObj = itemObj.transform.Find("Checkmark")?.gameObject;
            if (checkmarkObj == null)
            {
                checkmarkObj = new GameObject("Checkmark");
                checkmarkObj.transform.SetParent(itemObj.transform, false);
                
                RectTransform checkmarkRect = checkmarkObj.AddComponent<RectTransform>();
                // 버튼 중앙에 크게 표시
                checkmarkRect.anchorMin = new Vector2(0.2f, 0.2f);
                checkmarkRect.anchorMax = new Vector2(0.8f, 0.8f);
                checkmarkRect.sizeDelta = Vector2.zero;
                checkmarkRect.anchoredPosition = Vector2.zero;

                Image checkmarkImage = checkmarkObj.AddComponent<Image>();
                if (checkmarkSprite != null)
                {
                    checkmarkImage.sprite = checkmarkSprite;
                }
                else
                {
                    // 스프라이트가 없으면 기본 색상으로 체크 표시
                    checkmarkImage.color = new Color(0f, 1f, 0f, 0f); // 초기에는 투명
                }
                checkmarkImage.preserveAspect = true;
                checkmarkImage.raycastTarget = false; // 클릭 이벤트 방해하지 않도록
            }

            // 버튼 클릭 이벤트
            Button button = itemObj.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => OnTrapSelected(trapData.id));
            }

            // 덫 이름 텍스트만 표시
            TextMeshProUGUI nameText = itemObj.transform.Find("Text")?.GetComponent<TextMeshProUGUI>();
            if (nameText == null)
                nameText = itemObj.GetComponentInChildren<TextMeshProUGUI>();
            
            if (nameText != null)
            {
                nameText.text = isUnlocked ? trapData.name : $"{trapData.name}\n(해금 필요)";
                nameText.color = isUnlocked ? Color.white : Color.gray;
                nameText.fontSize = 14f;
                nameText.alignment = TextAlignmentOptions.Center;
            }

            // 버튼 활성화/비활성화
            if (button != null)
            {
                button.interactable = isUnlocked;
            }

            // 선택 상태 업데이트
            UpdateTrapItemSelection(itemObj, trapData.id);
        }

        private void UpdateTrapItemSelection(GameObject itemObj, string trapId)
        {
            // 선택 체크마크 표시/숨김
            GameObject checkmarkObj = itemObj.transform.Find("Checkmark")?.gameObject;
            if (checkmarkObj != null)
            {
                Image checkmarkImage = checkmarkObj.GetComponent<Image>();
                if (checkmarkImage != null)
                {
                    if (selectedTrapId == trapId)
                    {
                        if (checkmarkSprite != null)
                        {
                            checkmarkImage.sprite = checkmarkSprite;
                            checkmarkImage.color = Color.white;
                        }
                        else
                        {
                            // 스프라이트가 없으면 초록색으로 표시
                            checkmarkImage.color = new Color(0f, 1f, 0f, 1f);
                        }
                        checkmarkObj.SetActive(true);
                        selectedTrapItem = itemObj;
                    }
                    else
                    {
                        checkmarkObj.SetActive(false);
                    }
                }
            }
        }

        private void OnTrapSelected(string trapId)
        {
            // 이전 선택 해제
            if (selectedTrapItem != null)
            {
                UpdateTrapItemSelection(selectedTrapItem, selectedTrapId);
            }

            selectedTrapId = trapId;
            
            // 새로 선택한 덫 찾기
            var registry = GameDataRegistry.Instance;
            if (registry != null && registry.Traps != null)
            {
                var trapData = registry.Traps.GetTrapById(trapId);
                if (trapData != null)
                {
                    foreach (var itemObj in trapItemObjects)
                    {
                        if (itemObj != null)
                        {
                            TextMeshProUGUI nameText = itemObj.transform.Find("Text")?.GetComponent<TextMeshProUGUI>();
                            if (nameText == null)
                                nameText = itemObj.GetComponentInChildren<TextMeshProUGUI>();
                            
                            if (nameText != null)
                            {
                                string itemName = nameText.text.Replace("\n(해금 필요)", "").Trim();
                                if (itemName == trapData.name)
                                {
                                    selectedTrapItem = itemObj;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            // 선택 상태 업데이트
            foreach (var itemObj in trapItemObjects)
            {
                if (itemObj != null)
                {
                    // trapId 찾기
                    var registry2 = GameDataRegistry.Instance;
                    if (registry2 != null && registry2.Traps != null)
                    {
                        TextMeshProUGUI nameText = itemObj.transform.Find("Text")?.GetComponent<TextMeshProUGUI>();
                        if (nameText == null)
                            nameText = itemObj.GetComponentInChildren<TextMeshProUGUI>();
                        
                        if (nameText != null)
                        {
                            string itemName = nameText.text.Replace("\n(해금 필요)", "").Trim();
                            foreach (var trap in registry2.Traps.traps)
                            {
                                if (trap.name == itemName)
                                {
                                    UpdateTrapItemSelection(itemObj, trap.id);
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            UpdateInfoDisplay(trapId);
            UpdatePriceDisplay();
        }

        private void UpdateInfoDisplay(string trapId)
        {
            var registry = GameDataRegistry.Instance;
            if (registry == null || registry.Traps == null)
                return;

            var trapData = registry.Traps.GetTrapById(trapId);
            if (trapData == null)
                return;

            // 현재 Terrain 정보
            string currentTerrain = "sand";
            if (PlayerController.Instance != null && pigeonManager != null)
            {
                currentTerrain = pigeonManager.GetTerrainTypeAtPosition(PlayerController.Instance.Position);
            }

            // Terrain 표시
            if (currentTerrainText != null)
            {
                currentTerrainText.text = $"현재 지형: {currentTerrain}";
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
                    if (!string.IsNullOrEmpty(species.favoriteTerrain) && 
                        species.favoriteTerrain == currentTerrain)
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
                    if (!string.IsNullOrEmpty(species.favoriteTrap) && 
                        species.favoriteTrap == trapId)
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
            if (string.IsNullOrEmpty(selectedTrapId) || GameManager.Instance == null || totalPriceText == null)
                return;

            var registry = GameDataRegistry.Instance;
            if (registry == null || registry.Traps == null)
                return;

            var trapData = registry.Traps.GetTrapById(selectedTrapId);
            if (trapData == null)
                return;

            int feedAmount = trapData.feedAmount;
            if (feedAmountInput != null && int.TryParse(feedAmountInput.text, out int parsedAmount))
            {
                feedAmount = Mathf.Clamp(parsedAmount, 1, 1000); // 최소 1, 최대 1000
            }

            int installCost = GameManager.Instance.CalculateTrapInstallCost(selectedTrapId, feedAmount);
            int currentMoney = GameManager.Instance.CurrentMoney;

            totalPriceText.text = $"총 비용: {installCost} 골드 / 보유 골드: {currentMoney} 골드";
            totalPriceText.color = currentMoney >= installCost ? Color.white : Color.red;

            // 설치 버튼 활성화/비활성화
            if (installButton != null)
            {
                installButton.interactable = currentMoney >= installCost && feedAmount > 0;
            }
        }

        private void OnInstallButtonClicked()
        {
            if (string.IsNullOrEmpty(selectedTrapId))
                return;

            int feedAmount = 0;
            var registry = GameDataRegistry.Instance;
            if (registry != null && registry.Traps != null)
            {
                var trapData = registry.Traps.GetTrapById(selectedTrapId);
                if (trapData != null)
                {
                    feedAmount = trapData.feedAmount;
                }
            }

            if (feedAmountInput != null && int.TryParse(feedAmountInput.text, out int parsedAmount))
            {
                feedAmount = Mathf.Clamp(parsedAmount, 1, 1000); // 최소 1, 최대 1000
            }

            if (trapPlacer != null && feedAmount > 0)
            {
                bool success = trapPlacer.PlaceTrapAtPlayerPosition(selectedTrapId, feedAmount);
                if (success)
                {
                    // 패널 닫기
                    if (trapSelectionPanel != null)
                    {
                        trapSelectionPanel.SetActive(false);
                    }
                    // 가격 업데이트
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
                    if (!string.IsNullOrEmpty(selectedTrapId))
                    {
                        UpdateInfoDisplay(selectedTrapId);
                        UpdatePriceDisplay();
                    }
                }
            }
        }

        private void OnInteractionButtonClicked()
        {
            // 통합 상호작용 시스템 사용
            InteractionSystem interactionSystem = InteractionSystem.Instance;
            
            // 인스턴스가 없으면 자동 생성
            if (interactionSystem == null)
            {
                GameObject interactionObj = new GameObject("InteractionSystem");
                interactionSystem = interactionObj.AddComponent<InteractionSystem>();
            }
            
            if (interactionSystem != null)
            {
                interactionSystem.OnInteract();
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
            // 모든 덫 아이템 상태 업데이트
            var registry = GameDataRegistry.Instance;
            if (registry == null || registry.Traps == null)
                return;

            var allTraps = registry.Traps.traps;
            for (int i = 0; i < allTraps.Length && i < trapItemObjects.Count; i++)
            {
                if (trapItemObjects[i] != null)
                {
                    SetupTrapItem(trapItemObjects[i], allTraps[i]);
                }
            }

            // 선택 상태 다시 적용
            if (!string.IsNullOrEmpty(selectedTrapId))
            {
                foreach (var itemObj in trapItemObjects)
                {
                    if (itemObj != null)
                    {
                        TextMeshProUGUI nameText = itemObj.transform.Find("Text")?.GetComponent<TextMeshProUGUI>();
                        if (nameText == null)
                            nameText = itemObj.GetComponentInChildren<TextMeshProUGUI>();
                        
                        if (nameText != null)
                        {
                            string itemName = nameText.text.Replace("\n(해금 필요)", "").Trim();
                            foreach (var trap in allTraps)
                            {
                                if (trap.name == itemName)
                                {
                                    UpdateTrapItemSelection(itemObj, trap.id);
                                    if (trap.id == selectedTrapId)
                                    {
                                        selectedTrapItem = itemObj;
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            // GameManager 돈 변경 이벤트 구독 (가격 업데이트용)
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnMoneyChanged -= OnMoneyChanged;
                GameManager.Instance.OnMoneyChanged += OnMoneyChanged;
            }
        }

        private void OnMoneyChanged(int money)
        {
            // 가격 표시 업데이트
            UpdatePriceDisplay();
        }

        private void ClearItemList(List<GameObject> list)
        {
            foreach (var item in list)
            {
                if (item != null)
                    Destroy(item);
            }
            list.Clear();
        }

        private void OnDestroy()
        {
            if (trapPlacementButton != null)
            {
                trapPlacementButton.onClick.RemoveAllListeners();
            }

            if (interactionButton != null)
            {
                interactionButton.onClick.RemoveAllListeners();
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
            }

            if (installButton != null)
            {
                installButton.onClick.RemoveAllListeners();
            }

            if (feedAmountInput != null)
            {
                feedAmountInput.onValueChanged.RemoveAllListeners();
            }

            if (feedDecreaseButton != null)
            {
                feedDecreaseButton.onClick.RemoveAllListeners();
            }

            if (feedIncreaseButton != null)
            {
                feedIncreaseButton.onClick.RemoveAllListeners();
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnMoneyChanged -= OnMoneyChanged;
            }
        }
    }
}

