using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using PigeonGame.Data;
using PigeonGame.Gameplay;

namespace PigeonGame.UI
{
    public class TrapShopUI : MonoBehaviour
    {
        [Header("Main Panel")]
        [SerializeField] private GameObject shopPanel;
        [SerializeField] private Transform trapContainer;
        [SerializeField] private GameObject trapSlot;
        [SerializeField] private Button closeButton;
        [SerializeField] private TextMeshProUGUI goldText;

        private List<GameObject> trapItems = new List<GameObject>();

        private void Start()
        {
            ShopUIHelper.InitializeShopPanel(shopPanel, closeButton, goldText, OnCloseButtonClicked);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnTrapUnlocked += OnTrapUnlocked;
                GameManager.Instance.OnMoneyChanged += OnMoneyChanged;
            }

            UpdateShopDisplay();
        }

        public void OpenShopPanel()
        {
            ShopUIHelper.OpenShopPanel(shopPanel, goldText, UpdateShopDisplay);
        }

        private void OnTrapUnlocked(TrapType trapType)
        {
            UpdateShopDisplay();
        }

        private void OnMoneyChanged(int money)
        {
            ShopUIHelper.HandleMoneyChanged(goldText, UpdateShopDisplay);
        }

        private void UpdateShopDisplay()
        {
            if (trapContainer == null || trapSlot == null)
                return;

            UIHelper.ClearSlotList(trapItems);

            var registry = GameDataRegistry.Instance;
            if (registry == null || registry.Traps == null)
                return;

            var allTraps = registry.Traps.traps;

            foreach (var trapData in allTraps)
            {
                GameObject slotObj = Instantiate(trapSlot, trapContainer, false);
                trapItems.Add(slotObj);

                SetupTrapSlot(slotObj, trapData);
            }
        }

        private void SetupTrapSlot(GameObject slotObj, TrapDefinition trapData)
        {
            TrapShopSlotUI slotUI = slotObj.GetComponent<TrapShopSlotUI>();
            if (slotUI == null)
                return;

            bool isUnlocked = GameManager.Instance != null && GameManager.Instance.IsTrapUnlocked(trapData.trapType);
            bool canAfford = GameManager.Instance != null && GameManager.Instance.CurrentMoney >= trapData.unlockCost;

            UIHelper.SetTrapIcon(slotUI.IconImage, trapData.icon);

            if (slotUI.NameText != null)
            {
                slotUI.NameText.text = trapData.name;
            }

            if (slotUI.PreferenceText != null)
            {
                slotUI.PreferenceText.text = UIHelper.GetFavoriteSpeciesText(species => species.favoriteTrapType == trapData.trapType, "선호 비둘기");
            }

            if (slotUI.BuyButton != null)
            {
                slotUI.BuyButton.onClick.RemoveAllListeners();
                slotUI.BuyButton.onClick.AddListener(() => OnBuyClicked(trapData.trapType));
                ShopUIHelper.SetupUnlockButton(slotUI.BuyButton, slotUI.ButtonText, isUnlocked, canAfford, trapData.unlockCost);
            }
        }

        private void OnBuyClicked(TrapType trapType)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.UnlockTrap(trapType);
                UpdateShopDisplay();
            }
        }

        private void OnCloseButtonClicked()
        {
            ShopUIHelper.CloseShopPanel(shopPanel);
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnTrapUnlocked -= OnTrapUnlocked;
                GameManager.Instance.OnMoneyChanged -= OnMoneyChanged;
            }
            UIHelper.SafeRemoveListener(closeButton);
        }
    }
}