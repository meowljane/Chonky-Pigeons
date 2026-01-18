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
        [SerializeField] private GameObject inventorySlot; // 슬롯 프리팹 (아이콘 + 이름)
        [SerializeField] private TextMeshProUGUI inventoryCountText; // 인벤토리 개수 텍스트
        [SerializeField] private Button closeButton;
        [SerializeField] private Button inventoryButton; // 인벤토리 토글 버튼

        [Header("Detail Panel")]
        [SerializeField] private PigeonDetailPanelUI detailPanelUI; // 상세 정보 패널 UI

        private List<GameObject> slotInstances = new List<GameObject>();
        private const int MAX_SLOTS = 20; // 5x4 그리드

        // 상세정보 패널 닫기 콜백 (덫 상호작용 등에서 사용)
        private System.Action<PigeonInstanceStats> onDetailPanelClosed;
        private PigeonInstanceStats currentDetailPigeonStats; // 현재 표시 중인 비둘기 정보

        private void Start()
        {
            if (inventoryPanel != null)
            {
                inventoryPanel.SetActive(false);
            }


            // 버튼 이벤트 연결
            if (inventoryButton != null)
            {
                inventoryButton.onClick.RemoveAllListeners();
                inventoryButton.onClick.AddListener(ToggleInventory);
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
            }

            UpdateInventoryDisplay();
        }

        /// <summary>
        /// 인벤토리 패널 토글
        /// </summary>
        public void ToggleInventory()
        {
            if (inventoryPanel == null)
                return;

            bool isActive = inventoryPanel.activeSelf;
            inventoryPanel.SetActive(!isActive);
            
            if (!isActive)
            {
                UpdateInventoryDisplay();
                // 스크롤을 맨 위로 초기화
                ScrollRectHelper.ScrollToTop(inventoryPanel);
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

            if (gridContainer == null || inventorySlot == null)
                return;

            var inventory = GameManager.Instance.Inventory;
            int maxSlots = GameManager.Instance != null ? GameManager.Instance.MaxInventorySlots : 20;
            int slotCount = Mathf.Min(inventory.Count, maxSlots);

            // 인벤토리 아이템으로 슬롯 채우기
            for (int i = 0; i < slotCount; i++)
            {
                var pigeon = inventory[i];
                GameObject slotObj = Instantiate(inventorySlot, gridContainer, false);
                slotInstances.Add(slotObj);

                SetupSlotUI(slotObj, pigeon, i);
            }

            // 빈 슬롯 채우기
            for (int i = slotCount; i < maxSlots; i++)
            {
                GameObject slotObj = Instantiate(inventorySlot, gridContainer, false);
                slotInstances.Add(slotObj);
                SetupEmptySlot(slotObj);
            }

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
                int maxSlots = GameManager.Instance != null ? GameManager.Instance.MaxInventorySlots : 20;
                inventoryCountText.text = $"({currentCount}/{maxSlots})";
            }
        }

        /// <summary>
        /// 슬롯 UI 설정 (아이콘 + 이름)
        /// </summary>
        private void SetupSlotUI(GameObject slotObj, PigeonInstanceStats stats, int index)
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
                int capturedIndex = index; // 클로저를 위한 로컬 변수
                slotUI.Button.onClick.RemoveAllListeners();
                slotUI.Button.onClick.AddListener(() => OnSlotClicked(capturedIndex));
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
            if (detailPanelUI == null)
                return;

            // 콜백 및 현재 비둘기 정보 저장
            onDetailPanelClosed = onClosed;
            currentDetailPigeonStats = stats;

            // 디테일 패널 UI에 위임
            detailPanelUI.ShowDetail(stats, (closedStats) => {
                // 디테일 패널이 닫힐 때 콜백 호출
                if (onDetailPanelClosed != null && currentDetailPigeonStats != null)
            {
                    var savedStats = currentDetailPigeonStats;
                    onDetailPanelClosed.Invoke(savedStats);
                    onDetailPanelClosed = null;
            }
                currentDetailPigeonStats = null;
            });
        }

        /// <summary>
        /// 상세 정보 패널 닫기
        /// </summary>
        public void CloseDetailPanel()
        {
            if (detailPanelUI != null)
            {
                detailPanelUI.ClosePanel();
            }
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

        }
    }
}
