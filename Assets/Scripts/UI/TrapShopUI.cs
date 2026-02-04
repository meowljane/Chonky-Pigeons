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
            if (shopPanel != null)
            {
                shopPanel.SetActive(false);
            }

            UIHelper.SafeAddListener(closeButton, OnCloseButtonClicked);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnTrapUnlocked += OnTrapUnlocked;
                GameManager.Instance.OnMoneyChanged += OnMoneyChanged;
            }

            UpdateGoldText();
            UpdateShopDisplay();
        }

        public void OpenShopPanel()
        {
            if (shopPanel != null)
            {
                shopPanel.SetActive(true);
                UpdateGoldText();
                UpdateShopDisplay();
                ScrollRectHelper.ScrollToTop(shopPanel);
            }
        }

        private void OnTrapUnlocked(TrapType trapType)
        {
            UpdateShopDisplay();
        }

        private void OnMoneyChanged(int money)
        {
            UpdateGoldText();
            UpdateShopDisplay();
        }

        private void UpdateGoldText()
        {
            UIHelper.UpdateGoldText(goldText);
        }

        private void UpdateShopDisplay()
        {
            if (trapContainer == null || trapSlot == null)
                return;

            ClearTrapItems();

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

            if (slotUI.IconImage != null)
            {
                if (trapData.icon != null)
                {
                    slotUI.IconImage.sprite = trapData.icon;
                    slotUI.IconImage.enabled = true;
                }
                else
                {
                    slotUI.IconImage.enabled = false;
                }
            }

            if (slotUI.NameText != null)
            {
                slotUI.NameText.text = trapData.name;
            }

            if (slotUI.PreferenceText != null)
            {
                var registry = GameDataRegistry.Instance;
                List<string> favoriteSpeciesNames = new List<string>();

                if (registry != null && registry.SpeciesSet != null)
                {
                    foreach (var species in registry.SpeciesSet.species)
                    {
                        if (species.favoriteTrapType == trapData.trapType)
                        {
                            favoriteSpeciesNames.Add(species.name);
                        }
                    }
                }

                if (favoriteSpeciesNames.Count > 0)
                {
                    slotUI.PreferenceText.text = $"선호 비둘기 : {string.Join(", ", favoriteSpeciesNames)}";
                }
                else
                {
                    slotUI.PreferenceText.text = "선호 비둘기 : 없음";
                }
            }

            if (slotUI.BuyButton != null)
            {
                slotUI.BuyButton.interactable = !isUnlocked && canAfford;
                slotUI.BuyButton.onClick.RemoveAllListeners();
                slotUI.BuyButton.onClick.AddListener(() => OnBuyClicked(trapData.trapType));

                if (slotUI.ButtonText != null)
                {
                    if (isUnlocked)
                    {
                        slotUI.ButtonText.text = "해금됨";
                    }
                    else if (canAfford)
                    {
                        slotUI.ButtonText.text = $"해금\n{trapData.unlockCost}G";
                    }
                    else
                    {
                        slotUI.ButtonText.text = $"돈부족\n{trapData.unlockCost}G";
                    }
                }
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

        private void ClearTrapItems()
        {
            UIHelper.ClearSlotList(trapItems);
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
                GameManager.Instance.OnTrapUnlocked -= OnTrapUnlocked;
                GameManager.Instance.OnMoneyChanged -= OnMoneyChanged;
            }
            UIHelper.SafeRemoveListener(closeButton);
        }
    }
}