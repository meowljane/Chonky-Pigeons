using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using PigeonGame.Data;
using PigeonGame.Gameplay;

namespace PigeonGame.UI
{
    /// <summary>
    /// 비둘기 상점 UI (판매용)
    /// </summary>
    public class PigeonShopUI : MonoBehaviour
    {
        [SerializeField] private GameObject shopPanel;
        [SerializeField] private Transform itemContainer;
        [SerializeField] private GameObject itemPrefab;
        [SerializeField] private TextMeshProUGUI inventoryCountText;
        [SerializeField] private Button closeButton;

        private List<GameObject> itemInstances = new List<GameObject>();
        private InventoryUI inventoryUI; // 같은 오브젝트에서 가져옴

        private void Start()
        {
            // 같은 오브젝트에서 InventoryUI 가져오기
            inventoryUI = GetComponent<InventoryUI>();
            if (inventoryUI == null)
            {
                // 같은 오브젝트에 없으면 씬에서 찾기
                inventoryUI = FindObjectOfType<InventoryUI>();
                if (inventoryUI == null)
                {
                    Debug.LogError("InventoryUI 컴포넌트를 찾을 수 없습니다! 상세 정보 기능이 작동하지 않습니다.");
                }
                else
                {
                    Debug.LogWarning("같은 오브젝트에 InventoryUI가 없어서 씬에서 찾았습니다.");
                }
            }

            if (shopPanel != null)
            {
                shopPanel.SetActive(false);
            }

            // 닫기 버튼 연결
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
        /// 상점 패널 열기 (상호작용 시스템에서 호출)
        /// </summary>
        public void OpenShopPanel()
        {
            if (shopPanel != null)
            {
                shopPanel.SetActive(true);
                UpdateInventoryDisplay();
            }
        }

        private void OnPigeonAdded(PigeonInstanceStats stats)
        {
            UpdateInventoryDisplay();
        }

        private void UpdateInventoryDisplay()
        {
            if (GameManager.Instance == null)
                return;

            // 기존 아이템 제거
            foreach (var item in itemInstances)
            {
                if (item != null)
                    Destroy(item);
            }
            itemInstances.Clear();

            // 인벤토리 아이템 표시
            var inventory = GameManager.Instance.Inventory;
            if (itemContainer != null && itemPrefab != null)
            {
                for (int i = 0; i < inventory.Count; i++)
                {
                    int index = i; // 클로저를 위한 로컬 변수
                    var pigeon = inventory[index];
                    
                    // worldPositionStays를 false로 설정하여 로컬 좌표계로 배치
                    GameObject itemObj = Instantiate(itemPrefab, itemContainer, false);
                    itemInstances.Add(itemObj);

                    // RectTransform 강제 업데이트
                    RectTransform rectTransform = itemObj.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        rectTransform.localScale = Vector3.one;
                        rectTransform.localPosition = Vector3.zero;
                    }

                    // 아이템 정보 설정
                    SetupItemUI(itemObj, pigeon, index);
                }
            }

            // Canvas 강제 업데이트 (레이아웃 재계산)
            Canvas.ForceUpdateCanvases();
            
            // Layout Group이 있다면 강제로 레이아웃 재구성
            if (itemContainer is RectTransform rectContainer)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rectContainer);
            }

            // 인벤토리 개수 업데이트
            if (inventoryCountText != null)
            {
                inventoryCountText.text = $"인벤토리: {inventory.Count}";
            }
        }

        private void SetupItemUI(GameObject itemObj, PigeonInstanceStats stats, int index)
        {
            // ShopItemUI 컴포넌트 사용 (직접 참조 방식)
            ShopItemUI shopItemUI = itemObj.GetComponent<ShopItemUI>();
            if (shopItemUI != null)
            {
                shopItemUI.Setup(stats, index, ShowPigeonDetail, SellPigeon);
            }
            else
            {
                // ShopItemUI가 없으면 기존 방식으로 폴백 (하위 호환성)
                Debug.LogWarning($"ShopItemUI 컴포넌트가 없습니다. {itemObj.name}에 ShopItemUI를 추가하세요.");
            }
        }

        /// <summary>
        /// 비둘기 상세 정보 표시
        /// </summary>
        private void ShowPigeonDetail(PigeonInstanceStats stats)
        {
            if (stats == null || inventoryUI == null)
                return;

            inventoryUI.ShowPigeonDetail(stats);
        }

        /// <summary>
        /// 비둘기 판매 (상점에서 호출)
        /// </summary>
        public void SellPigeon(int index)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SellPigeon(index);
                UpdateInventoryDisplay();
            }
        }

        /// <summary>
        /// 모든 비둘기 판매 (상점에서 호출)
        /// </summary>
        public void SellAllPigeons()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SellAllPigeons();
                UpdateInventoryDisplay();
            }
        }

        private void OnCloseButtonClicked()
        {
            if (shopPanel != null)
            {
                shopPanel.SetActive(false);
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
        }
    }
}
