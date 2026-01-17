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
        [Header("Main Panel")]
        [SerializeField] private GameObject shopPanel;
        [SerializeField] private Transform itemContainer;
        [SerializeField] private GameObject shopSlot;
        [SerializeField] private TextMeshProUGUI inventoryCountText;
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private Button closeButton;

        private List<GameObject> itemInstances = new List<GameObject>();

        private void Start()
        {
            if (shopPanel != null)
            {
                shopPanel.SetActive(false);
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
                GameManager.Instance.OnMoneyChanged += OnMoneyChanged;
            }

            UpdateGoldText();
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
                UpdateGoldText();
                UpdateInventoryDisplay();
            }
        }

        private void OnPigeonAdded(PigeonInstanceStats stats)
        {
            UpdateInventoryDisplay();
        }

        private void OnMoneyChanged(int money)
        {
            UpdateGoldText();
        }

        private void UpdateGoldText()
        {
            if (goldText != null && GameManager.Instance != null)
            {
                goldText.text = $"현재 골드: {GameManager.Instance.CurrentMoney}G";
            }
        }

        private void UpdateInventoryDisplay()
        {
            if (GameManager.Instance == null)
                return;

            ClearShopItems();

            // 인벤토리 아이템 표시
            var inventory = GameManager.Instance.Inventory;
            if (itemContainer != null && shopSlot != null)
            {
                for (int i = 0; i < inventory.Count; i++)
                {
                    int index = i; // 클로저를 위한 로컬 변수
                    var pigeon = inventory[index];
                    
                    GameObject slotObj = Instantiate(shopSlot, itemContainer, false);
                    itemInstances.Add(slotObj);
                    SetupShopSlot(slotObj, pigeon, index);
                }
            }

            // 인벤토리 개수 업데이트
            if (inventoryCountText != null)
            {
                inventoryCountText.text = $"인벤토리: {inventory.Count}";
            }
        }

        private void SetupShopSlot(GameObject slotObj, PigeonInstanceStats stats, int index)
        {
            PigeonShopSlotUI slotUI = slotObj.GetComponent<PigeonShopSlotUI>();
            if (slotUI != null)
            {
                slotUI.Setup(stats, index, ShowPigeonDetail, SellPigeon);
            }
        }

        [Header("Detail Panel")]
        [SerializeField] private PigeonDetailPanelUI detailPanelUI; // 상세 정보 패널 UI

        /// <summary>
        /// 비둘기 상세 정보 표시
        /// </summary>
        private void ShowPigeonDetail(PigeonInstanceStats stats)
        {
            if (stats == null || detailPanelUI == null)
                return;

            detailPanelUI.ShowDetail(stats);
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

        private void OnCloseButtonClicked()
        {
            if (shopPanel != null)
            {
                shopPanel.SetActive(false);
            }
        }

        private void ClearShopItems()
        {
            foreach (var item in itemInstances)
            {
                if (item != null)
                    Destroy(item);
            }
            itemInstances.Clear();
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPigeonAddedToInventory -= OnPigeonAdded;
                GameManager.Instance.OnMoneyChanged -= OnMoneyChanged;
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
            }
        }
    }
}
