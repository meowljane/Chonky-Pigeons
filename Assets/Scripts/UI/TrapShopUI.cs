using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using PigeonGame.Data;
using PigeonGame.Gameplay;

namespace PigeonGame.UI
{
    /// <summary>
    /// 덫 구매 UI
    /// </summary>
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

            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(OnCloseButtonClicked);
            }

            // GameManager 이벤트 구독
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnTrapUnlocked += OnTrapUnlocked;
                GameManager.Instance.OnMoneyChanged += OnMoneyChanged;
            }

            UpdateGoldText();
            UpdateShopDisplay();
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
                UpdateShopDisplay();
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
            if (goldText != null && GameManager.Instance != null)
            {
                goldText.text = $"현재 골드: {GameManager.Instance.CurrentMoney}G";
            }
        }

        private void UpdateShopDisplay()
        {
            if (trapContainer == null || trapSlot == null)
                return;

            // 기존 아이템 제거
            ClearTrapItems();

            // 모든 덫 표시
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

            // 아이콘 표시
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

            // 덫 이름 표시
            if (slotUI.NameText != null)
            {
                slotUI.NameText.text = trapData.name;
            }

            // 선호 비둘기 목록 표시
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

            // 구매 버튼
            if (slotUI.BuyButton != null)
            {
                slotUI.BuyButton.interactable = !isUnlocked && canAfford;
                slotUI.BuyButton.onClick.RemoveAllListeners();
                slotUI.BuyButton.onClick.AddListener(() => OnBuyClicked(trapData.trapType));

                // 버튼 텍스트
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
            foreach (var item in trapItems)
            {
                if (item != null)
                    Destroy(item);
            }
            trapItems.Clear();
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

            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
            }
        }
    }
}