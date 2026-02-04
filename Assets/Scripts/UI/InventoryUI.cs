using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using PigeonGame.Data;
using PigeonGame.Gameplay;

namespace PigeonGame.UI
{
    public class InventoryUI : MonoBehaviour
    {
        [Header("Main Panel")]
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private Transform gridContainer; 
        [SerializeField] private GameObject inventorySlot; 
        [SerializeField] private TextMeshProUGUI inventoryCountText; 
        [SerializeField] private Button closeButton;
        [SerializeField] private Button inventoryButton; 

        [Header("Detail Panel")]
        [SerializeField] private PigeonDetailPanelUI detailPanelUI; 

        private List<GameObject> slotInstances = new List<GameObject>();
        private const int MAX_SLOTS = 20; 

        private System.Action<PigeonInstanceStats> onDetailPanelClosed;
        private PigeonInstanceStats currentDetailPigeonStats; 

        private void Start()
        {
            if (inventoryPanel != null)
            {
                inventoryPanel.SetActive(false);
            }

            UIHelper.SafeAddListener(inventoryButton, ToggleInventory);
            UIHelper.SafeAddListener(closeButton, OnCloseButtonClicked);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPigeonAddedToInventory += OnPigeonAdded;
            }

            UpdateInventoryDisplay();
        }

        public void ToggleInventory()
        {
            if (inventoryPanel == null)
                return;

            bool isActive = inventoryPanel.activeSelf;
            inventoryPanel.SetActive(!isActive);

            if (!isActive)
            {
                UpdateInventoryDisplay();
                ScrollRectHelper.ScrollToTop(inventoryPanel);
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

            ClearItemList(slotInstances);

            if (gridContainer == null || inventorySlot == null)
                return;

            var inventory = GameManager.Instance.Inventory;
            int maxSlots = GameManager.Instance.MaxInventorySlots;
            int slotCount = Mathf.Min(inventory.Count, maxSlots);

            for (int i = 0; i < slotCount; i++)
            {
                var pigeon = inventory[i];
                GameObject slotObj = Instantiate(inventorySlot, gridContainer, false);
                slotInstances.Add(slotObj);

                SetupSlotUI(slotObj, pigeon, i);
            }

            for (int i = slotCount; i < maxSlots; i++)
            {
                GameObject slotObj = Instantiate(inventorySlot, gridContainer, false);
                slotInstances.Add(slotObj);
                SetupEmptySlot(slotObj);
            }

            UpdateInventoryCountText(inventory.Count);
        }

        private void UpdateInventoryCountText(int currentCount)
        {
            if (inventoryCountText != null)
            {
                inventoryCountText.text = $"({currentCount}/{GameManager.Instance.MaxInventorySlots})";
            }
        }

        private void SetupSlotUI(GameObject slotObj, PigeonInstanceStats stats, int index)
        {
            UIHelper.SetupPigeonSlot(slotObj, stats, index, OnSlotClicked);
        }

        private void SetupEmptySlot(GameObject slotObj)
        {
            UIHelper.SetupEmptySlot(slotObj);
        }

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

        public void ShowPigeonDetail(PigeonInstanceStats stats, System.Action<PigeonInstanceStats> onClosed = null)
        {
            if (detailPanelUI == null)
                return;

            onDetailPanelClosed = onClosed;
            currentDetailPigeonStats = stats;

            detailPanelUI.ShowDetail(stats, (closedStats) => {
                if (onDetailPanelClosed != null && currentDetailPigeonStats != null)
            {
                    var savedStats = currentDetailPigeonStats;
                    onDetailPanelClosed.Invoke(savedStats);
                    onDetailPanelClosed = null;
            }
                currentDetailPigeonStats = null;
            });
        }

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

            CloseDetailPanel();
        }

        private void ClearItemList(List<GameObject> list)
        {
            UIHelper.ClearSlotList(list);
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPigeonAddedToInventory -= OnPigeonAdded;
            }
            UIHelper.SafeRemoveListener(closeButton);
            UIHelper.SafeRemoveListener(inventoryButton);
        }
    }
}
