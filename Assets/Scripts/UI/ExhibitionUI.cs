using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using PigeonGame.Data;
using PigeonGame.Gameplay;

namespace PigeonGame.UI
{
    /// <summary>
    /// 전시관 UI
    /// 인벤토리에서 비둘기를 전시관에 넣거나, 전시관에서 인벤토리로 꺼낼 수 있음
    /// </summary>
    public class ExhibitionUI : MonoBehaviour
    {
        [Header("Main Panel")]
        [SerializeField] private GameObject exhibitionPanel;
        [SerializeField] private Transform inventoryGridContainer; // 인벤토리 그리드
        [SerializeField] private Transform exhibitionGridContainer; // 전시관 그리드
        [SerializeField] private GameObject slotPrefab;
        [SerializeField] private Button closeButton;
        [SerializeField] private TextMeshProUGUI inventoryCountText;
        [SerializeField] private TextMeshProUGUI exhibitionCountText;

        [Header("Detail Panel")]
        [SerializeField] private GameObject detailPanel;
        [SerializeField] private Image detailIconImage;
        [SerializeField] private TextMeshProUGUI detailNameText;
        [SerializeField] private TextMeshProUGUI detailObesityText;
        [SerializeField] private TextMeshProUGUI detailPriceText;
        [SerializeField] private TextMeshProUGUI detailRarityText;
        [SerializeField] private Button detailCloseButton;
        [SerializeField] private Button moveToExhibitionButton; // 전시관으로 이동 버튼
        [SerializeField] private Button moveToInventoryButton; // 인벤토리로 이동 버튼

        private List<GameObject> inventorySlotInstances = new List<GameObject>();
        private List<GameObject> exhibitionSlotInstances = new List<GameObject>();
        private const int MAX_EXHIBITION_SLOTS = 50; // 전시관 최대 슬롯 수

        private PigeonInstanceStats currentDetailPigeonStats;
        private bool isDetailFromInventory = true; // 상세 정보가 인벤토리에서 온 것인지
        private int currentDetailIndex = -1; // 현재 상세 정보의 인덱스

        private void Start()
        {
            if (exhibitionPanel != null)
            {
                exhibitionPanel.SetActive(false);
            }

            if (detailPanel != null)
            {
                detailPanel.SetActive(false);
            }

            // 닫기 버튼 찾기 및 연결
            if (closeButton == null && exhibitionPanel != null)
            {
                closeButton = exhibitionPanel.transform.Find("CloseButton")?.GetComponent<Button>();
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(OnCloseButtonClicked);
            }

            // 상세 정보 닫기 버튼
            if (detailCloseButton == null && detailPanel != null)
            {
                detailCloseButton = detailPanel.transform.Find("CloseButton")?.GetComponent<Button>();
            }

            if (detailCloseButton != null)
            {
                detailCloseButton.onClick.RemoveAllListeners();
                detailCloseButton.onClick.AddListener(CloseDetailPanel);
            }

            // 이동 버튼
            if (moveToExhibitionButton != null)
            {
                moveToExhibitionButton.onClick.RemoveAllListeners();
                moveToExhibitionButton.onClick.AddListener(OnMoveToExhibitionClicked);
            }

            if (moveToInventoryButton != null)
            {
                moveToInventoryButton.onClick.RemoveAllListeners();
                moveToInventoryButton.onClick.AddListener(OnMoveToInventoryClicked);
            }

            // GameManager 이벤트 구독
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPigeonAddedToInventory += OnPigeonAdded;
                GameManager.Instance.OnPigeonAddedToExhibition += OnPigeonAddedToExhibition;
                GameManager.Instance.OnPigeonRemovedFromExhibition += OnPigeonRemovedFromExhibition;
            }

            SetupGridLayouts();
        }

        private void SetupGridLayouts()
        {
            SetupGridLayout(inventoryGridContainer);
            SetupGridLayout(exhibitionGridContainer);
        }

        private void SetupGridLayout(Transform container)
        {
            if (container == null)
                return;

            GridLayoutGroup gridLayout = container.GetComponent<GridLayoutGroup>();
            if (gridLayout == null)
            {
                gridLayout = container.gameObject.AddComponent<GridLayoutGroup>();
            }

            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 5;
            gridLayout.cellSize = new Vector2(100, 100);
            gridLayout.spacing = new Vector2(10, 10);
        }

        /// <summary>
        /// 전시관 패널 열기
        /// </summary>
        public void OpenExhibitionPanel()
        {
            if (exhibitionPanel != null)
            {
                exhibitionPanel.SetActive(true);
                UpdateDisplay();
            }
        }

        private void OnPigeonAdded(PigeonInstanceStats stats)
        {
            UpdateDisplay();
        }

        private void OnPigeonAddedToExhibition(PigeonInstanceStats stats)
        {
            UpdateDisplay();
        }

        private void OnPigeonRemovedFromExhibition(PigeonInstanceStats stats)
        {
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            UpdateInventoryDisplay();
            UpdateExhibitionDisplay();
        }

        private void UpdateInventoryDisplay()
        {
            if (GameManager.Instance == null || inventoryGridContainer == null || slotPrefab == null)
                return;

            ClearItemList(inventorySlotInstances);

            var inventory = GameManager.Instance.Inventory;
            for (int i = 0; i < inventory.Count; i++)
            {
                var pigeon = inventory[i];
                GameObject slotObj = Instantiate(slotPrefab, inventoryGridContainer, false);
                inventorySlotInstances.Add(slotObj);
                SetupSlotUI(slotObj, pigeon, true, i);
            }

            if (inventoryCountText != null)
            {
                inventoryCountText.text = $"인벤토리: {inventory.Count}";
            }
        }

        private void UpdateExhibitionDisplay()
        {
            if (GameManager.Instance == null || exhibitionGridContainer == null || slotPrefab == null)
                return;

            ClearItemList(exhibitionSlotInstances);

            var exhibition = GameManager.Instance.Exhibition;
            for (int i = 0; i < exhibition.Count; i++)
            {
                var pigeon = exhibition[i];
                GameObject slotObj = Instantiate(slotPrefab, exhibitionGridContainer, false);
                exhibitionSlotInstances.Add(slotObj);
                SetupSlotUI(slotObj, pigeon, false, i);
            }

            if (exhibitionCountText != null)
            {
                exhibitionCountText.text = $"전시관: {exhibition.Count}/{MAX_EXHIBITION_SLOTS}";
            }
        }

        private void SetupSlotUI(GameObject slotObj, PigeonInstanceStats stats, bool isInventory, int index)
        {
            // 아이콘 설정
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
            }

            // 이름 설정
            TextMeshProUGUI nameText = slotObj.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            if (nameText == null)
            {
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
            }

            // 버튼 클릭 이벤트
            Button slotButton = slotObj.GetComponent<Button>();
            if (slotButton != null)
            {
                slotButton.onClick.RemoveAllListeners();
                slotButton.onClick.AddListener(() => OnSlotClicked(stats, isInventory, index));
            }
        }

        private void OnSlotClicked(PigeonInstanceStats stats, bool isInventory, int index)
        {
            currentDetailPigeonStats = stats;
            isDetailFromInventory = isInventory;
            currentDetailIndex = index;
            ShowPigeonDetail(stats, isInventory);
        }

        private void ShowPigeonDetail(PigeonInstanceStats stats, bool isInventory)
        {
            if (stats == null || detailPanel == null)
                return;

            var registry = GameDataRegistry.Instance;
            if (registry == null || registry.SpeciesSet == null)
                return;

            var species = registry.SpeciesSet.GetSpeciesById(stats.speciesId);
            if (species == null)
                return;

            var face = registry.Faces != null ? registry.Faces.GetFaceById(stats.faceId) : null;

            detailPanel.SetActive(true);

            // 아이콘
            if (detailIconImage != null && species.icon != null)
            {
                detailIconImage.sprite = species.icon;
                detailIconImage.enabled = true;
            }

            // 종 이름
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

            // 희귀도
            if (detailRarityText != null)
            {
                detailRarityText.text = $"희귀도: {species.rarityTier}";
            }

            // 이동 버튼 표시/숨김
            if (moveToExhibitionButton != null)
            {
                moveToExhibitionButton.gameObject.SetActive(isInventory);
            }

            if (moveToInventoryButton != null)
            {
                moveToInventoryButton.gameObject.SetActive(!isInventory);
            }
        }

        private void OnMoveToExhibitionClicked()
        {
            if (currentDetailPigeonStats == null || !isDetailFromInventory || currentDetailIndex < 0)
                return;

            if (GameManager.Instance == null)
                return;

            // 전시관 슬롯 체크
            if (GameManager.Instance.ExhibitionCount >= MAX_EXHIBITION_SLOTS)
            {
                Debug.LogWarning("전시관이 가득 찼습니다!");
                return;
            }

            // 인덱스 유효성 검사
            var inventory = GameManager.Instance.Inventory;
            if (currentDetailIndex >= 0 && currentDetailIndex < inventory.Count)
            {
                GameManager.Instance.AddPigeonToExhibition(currentDetailIndex);
                CloseDetailPanel();
            }
        }

        private void OnMoveToInventoryClicked()
        {
            if (currentDetailPigeonStats == null || isDetailFromInventory || currentDetailIndex < 0)
                return;

            if (GameManager.Instance == null)
                return;

            // 인벤토리 슬롯 체크
            if (GameManager.Instance.InventoryCount >= 20)
            {
                Debug.LogWarning("인벤토리가 가득 찼습니다!");
                return;
            }

            // 인덱스 유효성 검사
            var exhibition = GameManager.Instance.Exhibition;
            if (currentDetailIndex >= 0 && currentDetailIndex < exhibition.Count)
            {
                GameManager.Instance.RemovePigeonFromExhibition(currentDetailIndex);
                CloseDetailPanel();
            }
        }

        private void CloseDetailPanel()
        {
            if (detailPanel != null)
            {
                detailPanel.SetActive(false);
            }
            currentDetailPigeonStats = null;
            currentDetailIndex = -1;
        }

        private void OnCloseButtonClicked()
        {
            if (exhibitionPanel != null)
            {
                exhibitionPanel.SetActive(false);
            }
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

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPigeonAddedToInventory -= OnPigeonAdded;
                GameManager.Instance.OnPigeonAddedToExhibition -= OnPigeonAddedToExhibition;
                GameManager.Instance.OnPigeonRemovedFromExhibition -= OnPigeonRemovedFromExhibition;
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
            }

            if (detailCloseButton != null)
            {
                detailCloseButton.onClick.RemoveAllListeners();
            }

            if (moveToExhibitionButton != null)
            {
                moveToExhibitionButton.onClick.RemoveAllListeners();
            }

            if (moveToInventoryButton != null)
            {
                moveToInventoryButton.onClick.RemoveAllListeners();
            }
        }
    }
}
