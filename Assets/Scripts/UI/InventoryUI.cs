using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using PigeonGame.Data;
using PigeonGame.Gameplay;

namespace PigeonGame.UI
{
    /// <summary>
    /// 인벤토리 UI (5x4 그리드, 아이콘과 이름만 표시)
    /// </summary>
    public class InventoryUI : MonoBehaviour
    {
        [Header("Main Panel")]
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private Transform gridContainer; // Grid Layout Group이 있는 컨테이너
        [SerializeField] private GameObject slotPrefab; // 슬롯 프리팹 (아이콘 + 이름)
        [SerializeField] private TextMeshProUGUI inventoryCountText; // 인벤토리 개수 텍스트
        [SerializeField] private Button closeButton;
        [SerializeField] private Button inventoryButton; // 인벤토리 토글 버튼

        [Header("Detail Panel")]
        [SerializeField] private GameObject detailPanel; // 상세 정보 패널
        [SerializeField] private Image detailIconImage; // 상세 정보 아이콘
        [SerializeField] private TextMeshProUGUI detailNameText; // 상세 정보 종 이름
        [SerializeField] private TextMeshProUGUI detailObesityText; // 상세 정보 비만도
        [SerializeField] private TextMeshProUGUI detailPriceText; // 상세 정보 가격
        [SerializeField] private TextMeshProUGUI detailRarityText; // 상세 정보 희귀도
        [SerializeField] private Button detailCloseButton; // 상세 정보 닫기 버튼

        private List<GameObject> slotInstances = new List<GameObject>();
        private const int GRID_WIDTH = 5;
        private const int GRID_HEIGHT = 4;
        private const int MAX_SLOTS = GRID_WIDTH * GRID_HEIGHT; // 20칸

        // 상세정보 패널 닫기 콜백 (덫 상호작용 등에서 사용)
        private System.Action<PigeonInstanceStats> onDetailPanelClosed;
        private PigeonInstanceStats currentDetailPigeonStats; // 현재 표시 중인 비둘기 정보

        private void Start()
        {
            if (inventoryPanel != null)
            {
                inventoryPanel.SetActive(false);
            }

            // 상세 정보 패널 초기화
            if (detailPanel != null)
            {
                detailPanel.SetActive(false);
            }

            // 인벤토리 버튼 찾기 및 연결
            if (inventoryButton == null)
            {
                GameObject buttonObj = GameObject.Find("InventoryButton");
                if (buttonObj != null)
                {
                    inventoryButton = buttonObj.GetComponent<Button>();
                }
            }

            if (inventoryButton != null)
            {
                inventoryButton.onClick.RemoveAllListeners();
                inventoryButton.onClick.AddListener(ToggleInventory);
            }

            // 닫기 버튼 찾기 및 연결
            if (closeButton == null && inventoryPanel != null)
            {
                closeButton = inventoryPanel.transform.Find("CloseButton")?.GetComponent<Button>();
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(OnCloseButtonClicked);
            }

            // 상세 정보 닫기 버튼 찾기 및 연결
            if (detailCloseButton == null && detailPanel != null)
            {
                detailCloseButton = detailPanel.transform.Find("CloseButton")?.GetComponent<Button>();
            }

            if (detailCloseButton != null)
            {
                detailCloseButton.onClick.RemoveAllListeners();
                detailCloseButton.onClick.AddListener(CloseDetailPanel);
            }

            // GameManager 이벤트 구독
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPigeonAddedToInventory += OnPigeonAdded;
            }

            // Grid Layout Group 설정 확인
            SetupGridLayout();

            UpdateInventoryDisplay();
        }

        /// <summary>
        /// Grid Layout Group 설정
        /// </summary>
        private void SetupGridLayout()
        {
            if (gridContainer == null)
                return;

            GridLayoutGroup gridLayout = gridContainer.GetComponent<GridLayoutGroup>();
            if (gridLayout == null)
            {
                gridLayout = gridContainer.gameObject.AddComponent<GridLayoutGroup>();
            }

            // 5x4 그리드 설정
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = GRID_WIDTH;
            gridLayout.cellSize = new Vector2(100, 100); // 적절한 크기로 조정 가능
            gridLayout.spacing = new Vector2(10, 10);
            gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
            gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
            gridLayout.childAlignment = TextAnchor.UpperLeft;
        }

        /// <summary>
        /// 인벤토리 패널 토글
        /// </summary>
        public void ToggleInventory()
        {
            if (inventoryPanel == null)
            {
                Debug.LogWarning("InventoryUI: inventoryPanel이 설정되지 않았습니다!");
                return;
            }

            bool isActive = inventoryPanel.activeSelf;
            inventoryPanel.SetActive(!isActive);
            
            if (!isActive)
            {
                UpdateInventoryDisplay();
            }
        }

        private void OnPigeonAdded(PigeonInstanceStats stats)
        {
            UpdateInventoryDisplay();
        }

        /// <summary>
        /// 인벤토리 표시 업데이트
        /// </summary>
        private void UpdateInventoryDisplay()
        {
            if (GameManager.Instance == null)
                return;

            ClearItemList(slotInstances);

            if (gridContainer == null || slotPrefab == null)
                return;

            var inventory = GameManager.Instance.Inventory;
            int slotCount = Mathf.Min(inventory.Count, MAX_SLOTS);

            // 인벤토리 아이템으로 슬롯 채우기
            for (int i = 0; i < slotCount; i++)
            {
                var pigeon = inventory[i];
                GameObject slotObj = Instantiate(slotPrefab, gridContainer, false);
                slotInstances.Add(slotObj);

                SetupSlotUI(slotObj, pigeon, i);
            }

            // 빈 슬롯 채우기
            for (int i = slotCount; i < MAX_SLOTS; i++)
            {
                GameObject slotObj = Instantiate(slotPrefab, gridContainer, false);
                slotInstances.Add(slotObj);
                SetupEmptySlot(slotObj);
            }

            UpdateCanvasLayout(gridContainer);

            // 인벤토리 개수 업데이트 (현재/최대 형식)
            UpdateInventoryCountText(inventory.Count);
        }

        /// <summary>
        /// 인벤토리 개수 텍스트 업데이트
        /// </summary>
        private void UpdateInventoryCountText(int currentCount)
        {
            if (inventoryCountText != null)
            {
                inventoryCountText.text = $"({currentCount}/{MAX_SLOTS})";
            }
        }

        /// <summary>
        /// 슬롯 UI 설정 (아이콘 + 이름)
        /// </summary>
        private void SetupSlotUI(GameObject slotObj, PigeonInstanceStats stats, int index)
        {
            // 아이콘 설정
            Image iconImage = slotObj.transform.Find("Icon")?.GetComponent<Image>();
            if (iconImage == null)
            {
                // Icon이 없으면 첫 번째 Image 컴포넌트 사용
                iconImage = slotObj.GetComponent<Image>();
                if (iconImage == null)
                {
                    iconImage = slotObj.GetComponentInChildren<Image>();
                }
            }

            if (iconImage != null)
            {
                var registry = GameDataRegistry.Instance;
                if (registry != null && registry.SpeciesSet != null)
                {
                    var species = registry.SpeciesSet.GetSpeciesById(stats.speciesId);
                    if (species != null && species.icon != null)
                    {
                        iconImage.sprite = species.icon;
                        iconImage.enabled = true;
                    }
                    else
                    {
                        iconImage.enabled = false;
                    }
                }
                else
                {
                    iconImage.enabled = false;
                }
            }

            // 이름 설정
            TextMeshProUGUI nameText = slotObj.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            if (nameText == null)
            {
                // NameText가 없으면 첫 번째 TextMeshProUGUI 사용
                nameText = slotObj.GetComponentInChildren<TextMeshProUGUI>();
            }

            if (nameText != null)
            {
                var registry = GameDataRegistry.Instance;
                if (registry != null && registry.SpeciesSet != null)
                {
                    var species = registry.SpeciesSet.GetSpeciesById(stats.speciesId);
                    nameText.text = species != null ? species.name : stats.speciesId;
                }
                else
                {
                    nameText.text = stats.speciesId;
                }
            }

            // 버튼 클릭 이벤트 (선택 사항 - 상세 정보 표시 등)
            Button slotButton = slotObj.GetComponent<Button>();
            if (slotButton != null)
            {
                int capturedIndex = index; // 클로저를 위한 로컬 변수
                slotButton.onClick.RemoveAllListeners();
                slotButton.onClick.AddListener(() => OnSlotClicked(capturedIndex));
            }
        }

        /// <summary>
        /// 빈 슬롯 설정
        /// </summary>
        private void SetupEmptySlot(GameObject slotObj)
        {
            // 아이콘 비활성화
            Image iconImage = slotObj.transform.Find("Icon")?.GetComponent<Image>();
            if (iconImage == null)
            {
                iconImage = slotObj.GetComponent<Image>();
                if (iconImage == null)
                {
                    iconImage = slotObj.GetComponentInChildren<Image>();
                }
            }

            if (iconImage != null)
            {
                iconImage.enabled = false;
            }

            // 이름 비우기
            TextMeshProUGUI nameText = slotObj.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            if (nameText == null)
            {
                nameText = slotObj.GetComponentInChildren<TextMeshProUGUI>();
            }

            if (nameText != null)
            {
                nameText.text = "";
            }

            // 버튼 비활성화
            Button slotButton = slotObj.GetComponent<Button>();
            if (slotButton != null)
            {
                slotButton.interactable = false;
            }
        }

        /// <summary>
        /// 슬롯 클릭 이벤트 - 상세 정보 표시
        /// </summary>
        private void OnSlotClicked(int index)
        {
            if (GameManager.Instance == null)
                return;

            var inventory = GameManager.Instance.Inventory;
            if (index >= 0 && index < inventory.Count)
            {
                var pigeon = inventory[index];
                ShowPigeonDetail(pigeon);
            }
        }

        /// <summary>
        /// 비둘기 상세 정보 표시 (public으로 외부에서도 사용 가능)
        /// </summary>
        /// <param name="stats">비둘기 정보</param>
        /// <param name="onClosed">패널이 닫힐 때 호출될 콜백 (선택사항)</param>
        public void ShowPigeonDetail(PigeonInstanceStats stats, System.Action<PigeonInstanceStats> onClosed = null)
        {
            if (stats == null || detailPanel == null)
                return;

            var registry = GameDataRegistry.Instance;
            if (registry == null || registry.SpeciesSet == null)
            {
                Debug.LogWarning("GameDataRegistry를 찾을 수 없습니다!");
                return;
            }

            var species = registry.SpeciesSet.GetSpeciesById(stats.speciesId);
            if (species == null)
            {
                Debug.LogWarning($"Species를 찾을 수 없습니다: {stats.speciesId}");
                return;
            }

            var face = registry.Faces != null ? registry.Faces.GetFaceById(stats.faceId) : null;

            // 콜백 및 현재 비둘기 정보 저장
            onDetailPanelClosed = onClosed;
            currentDetailPigeonStats = stats;

            // 상세 정보 패널 표시
            detailPanel.SetActive(true);

            // 아이콘
            if (detailIconImage != null && species.icon != null)
            {
                detailIconImage.sprite = species.icon;
                detailIconImage.enabled = true;
            }
            else if (detailIconImage != null)
            {
                detailIconImage.enabled = false;
            }

            // 종 이름 (얼굴 포함)
            if (detailNameText != null)
            {
                string faceName = face != null ? face.name : stats.faceId;
                detailNameText.text = $"{species.name}({faceName})";
            }

            // 비만도
            if (detailObesityText != null)
            {
                detailObesityText.text = $"비만도: {stats.obesity}";
            }

            // 가격
            if (detailPriceText != null)
            {
                detailPriceText.text = $"가격: {stats.price}";
            }

            // 희귀도 (숫자만 표시)
            if (detailRarityText != null)
            {
                detailRarityText.text = $"희귀도: {species.rarityTier}";
            }
        }

        /// <summary>
        /// 상세 정보 패널 닫기
        /// </summary>
        private void CloseDetailPanel()
        {
            if (detailPanel != null)
            {
                detailPanel.SetActive(false);
            }

            // 콜백 호출 (있는 경우)
            if (onDetailPanelClosed != null && currentDetailPigeonStats != null)
            {
                var stats = currentDetailPigeonStats;
                onDetailPanelClosed.Invoke(stats);
                onDetailPanelClosed = null;
            }

            currentDetailPigeonStats = null;
        }

        private void OnCloseButtonClicked()
        {
            if (inventoryPanel != null)
            {
                inventoryPanel.SetActive(false);
            }
            
            // 인벤토리 닫을 때 상세 정보도 함께 닫기
            CloseDetailPanel();
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

        private void UpdateCanvasLayout(Transform container)
        {
            Canvas.ForceUpdateCanvases();
            if (container is RectTransform rectContainer)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rectContainer);
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPigeonAddedToInventory -= OnPigeonAdded;
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
            }

            if (inventoryButton != null)
            {
                inventoryButton.onClick.RemoveAllListeners();
            }

            if (detailCloseButton != null)
            {
                detailCloseButton.onClick.RemoveAllListeners();
            }
        }
    }
}
