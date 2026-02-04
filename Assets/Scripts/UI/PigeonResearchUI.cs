using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using PigeonGame.Data;
using PigeonGame.Gameplay;

namespace PigeonGame.UI
{
    public class PigeonResearchUI : MonoBehaviour
    {
        [Header("Main Panel")]
        [SerializeField] private GameObject shopPanel;
        [SerializeField] private Transform speciesContainer;
        [SerializeField] private GameObject speciesSlot;
        [SerializeField] private Button closeButton;
        [SerializeField] private TextMeshProUGUI goldText;

        private List<GameObject> speciesItems = new List<GameObject>();

        private void Start()
        {
            if (shopPanel != null)
            {
                shopPanel.SetActive(false);
            }

            UIHelper.SafeAddListener(closeButton, OnCloseButtonClicked);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnSpeciesUnlocked += OnSpeciesUnlocked;
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

        private void OnSpeciesUnlocked(PigeonSpecies speciesType)
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
            if (speciesContainer == null || speciesSlot == null)
                return;

            ClearSpeciesItems();

            var registry = GameDataRegistry.Instance;
            if (registry == null || registry.SpeciesSet == null)
                return;

            var allSpecies = registry.SpeciesSet.species;

            System.Array.Sort(allSpecies, (a, b) => 
            {
                int tierCompare = a.rarityTier.CompareTo(b.rarityTier);
                if (tierCompare != 0)
                    return tierCompare;
                return a.name.CompareTo(b.name);
            });

            foreach (var speciesData in allSpecies)
            {
                GameObject slotObj = Instantiate(speciesSlot, speciesContainer, false);
                speciesItems.Add(slotObj);

                SetupSpeciesSlot(slotObj, speciesData);
            }
        }

        private void SetupSpeciesSlot(GameObject slotObj, SpeciesDefinition speciesData)
        {
            PigeonResearchSlotUI slotUI = slotObj.GetComponent<PigeonResearchSlotUI>();
            if (slotUI == null)
                return;

            bool isUnlocked = GameManager.Instance != null && GameManager.Instance.IsSpeciesUnlocked(speciesData.speciesType);
            bool canAfford = GameManager.Instance != null && GameManager.Instance.CurrentMoney >= speciesData.unlockCost;

            if (slotUI.NameText != null)
            {
                slotUI.NameText.text = speciesData.name;
            }

            var registry = GameDataRegistry.Instance;
            var defaultSpecies = (registry != null && registry.SpeciesSet != null)
                ? registry.SpeciesSet.GetSpeciesById(PigeonSpecies.SP01)
                : null;
            var defaultFace = (registry != null && registry.Faces != null)
                ? registry.Faces.GetFaceById(FaceType.F00)
                : null;

            if (slotUI.IconImage != null)
            {
                var iconToUse = speciesData?.icon ?? defaultSpecies?.icon;
                if (iconToUse != null)
                {
                    slotUI.IconImage.sprite = iconToUse;
                    slotUI.IconImage.enabled = true;
                }
            }

            if (slotUI.FaceIconImage != null && defaultFace?.icon != null)
            {
                slotUI.FaceIconImage.sprite = defaultFace.icon;
                slotUI.FaceIconImage.enabled = true;
            }

            if (slotUI.BuyButton != null)
            {
                slotUI.BuyButton.interactable = !isUnlocked && canAfford;
                slotUI.BuyButton.onClick.RemoveAllListeners();
                slotUI.BuyButton.onClick.AddListener(() => OnBuyClicked(speciesData.speciesType));

                if (slotUI.ButtonText != null)
                {
                    if (isUnlocked)
                    {
                        slotUI.ButtonText.text = "해금됨";
                    }
                    else if (canAfford)
                    {
                        slotUI.ButtonText.text = $"해금\n{speciesData.unlockCost}G";
                    }
                    else
                    {
                        slotUI.ButtonText.text = $"돈부족\n{speciesData.unlockCost}G";
                    }
                }
            }
        }

        private void ClearSpeciesItems()
        {
            UIHelper.ClearSlotList(speciesItems);
        }

        private void OnBuyClicked(PigeonSpecies speciesType)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.UnlockSpecies(speciesType);
                UpdateShopDisplay();
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
                GameManager.Instance.OnSpeciesUnlocked -= OnSpeciesUnlocked;
                GameManager.Instance.OnMoneyChanged -= OnMoneyChanged;
            }
            UIHelper.SafeRemoveListener(closeButton);
        }
    }
}
