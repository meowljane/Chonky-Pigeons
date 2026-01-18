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
        [SerializeField] private GameObject inventorySlot; // 슬롯 프리팹
        [SerializeField] private Button closeButton;
        [SerializeField] private TextMeshProUGUI inventoryCountText;
        [SerializeField] private TextMeshProUGUI exhibitionCountText;

        [Header("Detail Panel")]
        [SerializeField] private PigeonDetailPanelUI detailPanelUI; // 상세 정보 패널 UI

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

            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(OnCloseButtonClicked);
            }

            // GameManager 이벤트 구독
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPigeonAddedToInventory += OnPigeonAdded;
                GameManager.Instance.OnPigeonAddedToExhibition += OnPigeonAddedToExhibition;
                GameManager.Instance.OnPigeonRemovedFromExhibition += OnPigeonRemovedFromExhibition;
            }

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
                // 스크롤을 맨 위로 초기화
                ScrollRectHelper.ScrollToTop(exhibitionPanel);
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
            if (GameManager.Instance == null || inventoryGridContainer == null || inventorySlot == null)
                return;

            ClearSlots(inventorySlotInstances);

            var inventory = GameManager.Instance.Inventory;
            int maxSlots = GameManager.Instance != null ? GameManager.Instance.MaxInventorySlots : 20;
            int slotCount = Mathf.Min(inventory.Count, maxSlots);

            // 인벤토리 아이템으로 슬롯 채우기
            for (int i = 0; i < slotCount; i++)
            {
                var pigeon = inventory[i];
                GameObject slotObj = Instantiate(inventorySlot, inventoryGridContainer, false);
                inventorySlotInstances.Add(slotObj);
                SetupSlotUI(slotObj, pigeon, true, i);
            }

            // 빈 슬롯 채우기
            for (int i = slotCount; i < maxSlots; i++)
            {
                GameObject slotObj = Instantiate(inventorySlot, inventoryGridContainer, false);
                inventorySlotInstances.Add(slotObj);
                SetupEmptySlot(slotObj);
            }

            if (inventoryCountText != null)
            {
                inventoryCountText.text = $"인벤토리: {inventory.Count}";
            }
        }

        private void UpdateExhibitionDisplay()
        {
            if (GameManager.Instance == null || exhibitionGridContainer == null || inventorySlot == null)
                return;

            ClearSlots(exhibitionSlotInstances);

            var exhibition = GameManager.Instance.Exhibition;
            int slotCount = Mathf.Min(exhibition.Count, MAX_EXHIBITION_SLOTS);

            // 전시관 아이템으로 슬롯 채우기
            for (int i = 0; i < slotCount; i++)
            {
                var pigeon = exhibition[i];
                GameObject slotObj = Instantiate(inventorySlot, exhibitionGridContainer, false);
                exhibitionSlotInstances.Add(slotObj);
                SetupSlotUI(slotObj, pigeon, false, i);
            }

            // 빈 슬롯 채우기
            for (int i = slotCount; i < MAX_EXHIBITION_SLOTS; i++)
            {
                GameObject slotObj = Instantiate(inventorySlot, exhibitionGridContainer, false);
                exhibitionSlotInstances.Add(slotObj);
                SetupEmptySlot(slotObj);
            }

            if (exhibitionCountText != null)
            {
                exhibitionCountText.text = $"전시관: {exhibition.Count}/{MAX_EXHIBITION_SLOTS}";
            }
        }

        private void SetupSlotUI(GameObject slotObj, PigeonInstanceStats stats, bool isInventory, int index)
        {
            InventorySlotUI slotUI = slotObj.GetComponent<InventorySlotUI>();
            if (slotUI == null)
                return;

            var registry = GameDataRegistry.Instance;
            var species = (registry != null && registry.SpeciesSet != null) 
                ? registry.SpeciesSet.GetSpeciesById(stats.speciesId) 
                : null;

            // 아이콘 설정
            if (slotUI.IconImage != null)
            {
                if (species != null && species.icon != null)
                {
                    slotUI.IconImage.sprite = species.icon;
                    slotUI.IconImage.enabled = true;
                }
                else
                {
                    slotUI.IconImage.enabled = false;
                }
            }

            // 이름 설정
            if (slotUI.NameText != null)
            {
                slotUI.NameText.text = species != null ? species.name : stats.speciesId.ToString();
            }

            // 버튼 클릭 이벤트
            if (slotUI.Button != null)
            {
                slotUI.Button.onClick.RemoveAllListeners();
                slotUI.Button.onClick.AddListener(() => OnSlotClicked(stats, isInventory, index));
            }
        }

        /// <summary>
        /// 빈 슬롯 설정
        /// </summary>
        private void SetupEmptySlot(GameObject slotObj)
        {
            InventorySlotUI slotUI = slotObj.GetComponent<InventorySlotUI>();
            if (slotUI == null)
                return;

            // 아이콘 비활성화
            if (slotUI.IconImage != null)
            {
                slotUI.IconImage.enabled = false;
            }

            // 이름 비우기
            if (slotUI.NameText != null)
            {
                slotUI.NameText.text = "";
            }

            // 버튼 비활성화
            if (slotUI.Button != null)
            {
                slotUI.Button.interactable = false;
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
            if (stats == null || detailPanelUI == null)
                return;

            // 이동 버튼 텍스트 설정
            string buttonText = isInventory ? "전시관으로" : "인벤토리로";

            // 디테일 패널 UI에 위임
            detailPanelUI.ShowDetail(stats, 
                onClosedCallback: null, 
                onMoveCallback: (stats) => OnMoveButtonClicked(), 
                showMoveButton: true, 
                moveButtonText: buttonText);
        }

        private void OnMoveButtonClicked()
        {
            if (currentDetailPigeonStats == null || currentDetailIndex < 0)
                return;

            if (GameManager.Instance == null)
                return;

            if (isDetailFromInventory)
            {
                // 전시관으로 이동
                if (GameManager.Instance.ExhibitionCount >= MAX_EXHIBITION_SLOTS)
                {
                    Debug.LogWarning("전시관이 가득 찼습니다!");
                    return;
                }

                var inventory = GameManager.Instance.Inventory;
                if (currentDetailIndex >= 0 && currentDetailIndex < inventory.Count)
                {
                    GameManager.Instance.AddPigeonToExhibition(currentDetailIndex);
                    if (detailPanelUI != null)
                    {
                        detailPanelUI.ClosePanel();
                    }
                }
            }
            else
            {
                // 인벤토리로 이동
                if (GameManager.Instance != null && GameManager.Instance.InventoryCount >= GameManager.Instance.MaxInventorySlots)
                {
                    Debug.LogWarning("인벤토리가 가득 찼습니다!");
                    return;
                }

                var exhibition = GameManager.Instance.Exhibition;
                if (currentDetailIndex >= 0 && currentDetailIndex < exhibition.Count)
                {
                    GameManager.Instance.RemovePigeonFromExhibition(currentDetailIndex);
                    if (detailPanelUI != null)
                    {
                        detailPanelUI.ClosePanel();
                    }
                }
            }
        }

        private void OnCloseButtonClicked()
        {
            if (exhibitionPanel != null)
            {
                exhibitionPanel.SetActive(false);
            }
            
            if (detailPanelUI != null)
            {
                detailPanelUI.ClosePanel();
            }
        }

        private void ClearSlots(List<GameObject> list)
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

        }
    }
}
