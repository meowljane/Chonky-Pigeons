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
        private PigeonShop shop;

        private void Start()
        {
            shop = FindObjectOfType<PigeonShop>();
            if (shop == null)
            {
                shop = gameObject.AddComponent<PigeonShop>();
            }

            if (shopPanel != null)
            {
                shopPanel.SetActive(false);
            }

            // 닫기 버튼 찾기 및 연결
            if (closeButton == null && shopPanel != null)
            {
                Transform closeButtonTransform = shopPanel.transform.Find("CloseButton");
                if (closeButtonTransform != null)
                {
                    closeButton = closeButtonTransform.GetComponent<Button>();
                }
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
            // 종 이름 표시
            TextMeshProUGUI nameText = itemObj.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
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

            // 가격 표시
            TextMeshProUGUI priceText = itemObj.transform.Find("PriceText")?.GetComponent<TextMeshProUGUI>();
            if (priceText != null)
            {
                priceText.text = $"가격: {stats.price}";
            }

            // 비만도 표시
            TextMeshProUGUI obesityText = itemObj.transform.Find("ObesityText")?.GetComponent<TextMeshProUGUI>();
            if (obesityText != null)
            {
                obesityText.text = $"비만도: {stats.obesity}";
            }

            // 판매 버튼 (상점 패널에서만 표시)
            Button sellButton = itemObj.transform.Find("SellButton")?.GetComponent<Button>();
            if (sellButton != null)
            {
                sellButton.onClick.RemoveAllListeners();
                sellButton.onClick.AddListener(() => SellPigeon(index));
            }
        }

        /// <summary>
        /// 비둘기 판매 (상점에서 호출)
        /// </summary>
        public void SellPigeon(int index)
        {
            if (shop != null)
            {
                shop.SellPigeon(index);
                UpdateInventoryDisplay();
            }
        }

        /// <summary>
        /// 모든 비둘기 판매 (상점에서 호출)
        /// </summary>
        public void SellAllPigeons()
        {
            if (shop != null)
            {
                shop.SellAllPigeons();
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
