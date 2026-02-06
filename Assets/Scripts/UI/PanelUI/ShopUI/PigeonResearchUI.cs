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
            ShopUIHelper.InitializeShopPanel(shopPanel, closeButton, goldText, OnCloseButtonClicked);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnSpeciesUnlocked += OnSpeciesUnlocked;
                GameManager.Instance.OnMoneyChanged += OnMoneyChanged;
            }

            UpdateShopDisplay();
        }

        public void OpenShopPanel()
        {
            ShopUIHelper.OpenShopPanel(shopPanel, goldText, UpdateShopDisplay);
        }

        private void OnSpeciesUnlocked(PigeonSpecies speciesType)
        {
            UpdateShopDisplay();
        }

        private void OnMoneyChanged(int money)
        {
            ShopUIHelper.HandleMoneyChanged(goldText, UpdateShopDisplay);
        }

        private void UpdateShopDisplay()
        {
            if (speciesContainer == null || speciesSlot == null)
                return;

            UIHelper.ClearSlotList(speciesItems);

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

            UIHelper.SetSpeciesIcon(slotUI.IconImage, speciesData);
            UIHelper.SetFaceIcon(slotUI.FaceIconImage, null);

            if (slotUI.BuyButton != null)
            {
                slotUI.BuyButton.onClick.RemoveAllListeners();
                slotUI.BuyButton.onClick.AddListener(() => OnBuyClicked(speciesData.speciesType));
                ShopUIHelper.SetupUnlockButton(slotUI.BuyButton, slotUI.ButtonText, isUnlocked, canAfford, speciesData.unlockCost);
            }
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
            ShopUIHelper.CloseShopPanel(shopPanel);
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
