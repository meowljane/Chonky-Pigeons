using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using PigeonGame.Data;
using PigeonGame.Gameplay;

namespace PigeonGame.UI
{
    /// <summary>
    /// 비둘기 연구소 UI (종 해금)
    /// </summary>
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

            // GameManager 이벤트 구독
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnSpeciesUnlocked += OnSpeciesUnlocked;
                GameManager.Instance.OnMoneyChanged += OnMoneyChanged;
            }

            UpdateGoldText();
            UpdateShopDisplay();
        }

        /// <summary>
        /// 연구소 패널 열기 (상호작용 시스템에서 호출)
        /// </summary>
        public void OpenShopPanel()
        {
            if (shopPanel != null)
            {
                shopPanel.SetActive(true);
                UpdateGoldText();
                UpdateShopDisplay();
                // 스크롤을 맨 위로 초기화
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

            // 기존 아이템 제거
            ClearSpeciesItems();

            // 모든 비둘기 종 표시
            var registry = GameDataRegistry.Instance;
            if (registry == null || registry.SpeciesSet == null)
                return;

            var allSpecies = registry.SpeciesSet.species;

            // 티어별로 정렬
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

            // 종 이름 표시
            if (slotUI.NameText != null)
            {
                slotUI.NameText.text = speciesData.name;
            }

            // 기본값 설정
            var registry = GameDataRegistry.Instance;
            var defaultSpecies = (registry != null && registry.SpeciesSet != null)
                ? registry.SpeciesSet.GetSpeciesById(PigeonSpecies.SP01)
                : null;
            var defaultFace = (registry != null && registry.Faces != null)
                ? registry.Faces.GetFaceById(FaceType.F00)
                : null;

            // IconImage: Species icon 표시 (없으면 기본값 SP01 사용)
            if (slotUI.IconImage != null)
            {
                var iconToUse = speciesData?.icon ?? defaultSpecies?.icon;
                if (iconToUse != null)
                {
                    slotUI.IconImage.sprite = iconToUse;
                    slotUI.IconImage.enabled = true;
                }
            }

            // FaceIconImage: 기본 표정(F00) icon 표시 (무조건 표시)
            if (slotUI.FaceIconImage != null && defaultFace?.icon != null)
            {
                slotUI.FaceIconImage.sprite = defaultFace.icon;
                slotUI.FaceIconImage.enabled = true;
            }

            // 구매 버튼
            if (slotUI.BuyButton != null)
            {
                slotUI.BuyButton.interactable = !isUnlocked && canAfford;
                slotUI.BuyButton.onClick.RemoveAllListeners();
                slotUI.BuyButton.onClick.AddListener(() => OnBuyClicked(speciesData.speciesType));

                // 버튼 텍스트
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
