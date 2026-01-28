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

            UIHelper.SafeAddListener(closeButton, OnCloseButtonClicked);

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
                // 스크롤을 맨 위로 초기화
                ScrollRectHelper.ScrollToTop(shopPanel);
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
            UIHelper.UpdateGoldText(goldText);
        }

        private void UpdateInventoryDisplay()
        {
            if (GameManager.Instance == null)
                return;

            ClearShopItems();

            if (itemContainer == null || shopSlot == null)
                return;

            var inventory = GameManager.Instance.Inventory;
            int maxSlots = GameManager.Instance.MaxInventorySlots;
            int slotCount = Mathf.Min(inventory.Count, maxSlots);

            // 인벤토리 아이템으로 슬롯 채우기
            for (int i = 0; i < slotCount; i++)
            {
                int index = i; // 클로저를 위한 로컬 변수
                var pigeon = inventory[index];
                
                GameObject slotObj = Instantiate(shopSlot, itemContainer, false);
                itemInstances.Add(slotObj);
                SetupShopSlot(slotObj, pigeon, index);
            }

            // 빈 슬롯 채우기
            for (int i = slotCount; i < maxSlots; i++)
            {
                GameObject slotObj = Instantiate(shopSlot, itemContainer, false);
                itemInstances.Add(slotObj);
                SetupEmptySlot(slotObj);
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

        private void SetupEmptySlot(GameObject slotObj)
        {
            PigeonShopSlotUI slotUI = slotObj.GetComponent<PigeonShopSlotUI>();
            if (slotUI == null) return;

            if (slotUI.IconImage != null) slotUI.IconImage.enabled = false;
            if (slotUI.FaceIconImage != null) slotUI.FaceIconImage.enabled = false;
            if (slotUI.NameText != null) slotUI.NameText.text = "";
            if (slotUI.DetailButton != null) slotUI.DetailButton.gameObject.SetActive(false);
            if (slotUI.SellButton != null) slotUI.SellButton.gameObject.SetActive(false);
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
            UIHelper.ClearSlotList(itemInstances);
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPigeonAddedToInventory -= OnPigeonAdded;
                GameManager.Instance.OnMoneyChanged -= OnMoneyChanged;
            }
            UIHelper.SafeRemoveListener(closeButton);
        }
    }
}
