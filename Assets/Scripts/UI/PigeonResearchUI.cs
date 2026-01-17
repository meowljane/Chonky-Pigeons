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
        [SerializeField] private GameObject shopPanel;
        [SerializeField] private Transform speciesContainer;
        [SerializeField] private GameObject speciesItemPrefab;
        [SerializeField] private Button closeButton;

        private List<GameObject> speciesItems = new List<GameObject>();

        private void Start()
        {
            if (shopPanel != null)
            {
                shopPanel.SetActive(false);
            }

            // 닫기 버튼 찾기 및 연결
            if (closeButton == null && shopPanel != null)
            {
                closeButton = shopPanel.GetComponentInChildren<Button>();
                if (closeButton == null)
                {
                    closeButton = shopPanel.transform.Find("CloseButton")?.GetComponent<Button>();
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
                GameManager.Instance.OnSpeciesUnlocked += OnSpeciesUnlocked;
                GameManager.Instance.OnMoneyChanged += OnMoneyChanged;
            }

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
                UpdateShopDisplay();
            }
        }

        private void OnSpeciesUnlocked(PigeonSpecies speciesType)
        {
            UpdateShopDisplay();
        }

        private void OnMoneyChanged(int money)
        {
            UpdateShopDisplay();
        }

        private void UpdateShopDisplay()
        {
            if (speciesContainer == null || speciesItemPrefab == null)
                return;

            // 기존 아이템 제거
            foreach (var item in speciesItems)
            {
                if (item != null)
                    Destroy(item);
            }
            speciesItems.Clear();

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
                GameObject itemObj = Instantiate(speciesItemPrefab, speciesContainer);
                speciesItems.Add(itemObj);

                SetupSpeciesItemUI(itemObj, speciesData);
            }
        }

        private void SetupSpeciesItemUI(GameObject itemObj, SpeciesDefinition speciesData)
        {
            // 종 이름 표시
            TextMeshProUGUI nameText = itemObj.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            if (nameText != null)
            {
                nameText.text = speciesData.name;
            }

            // 티어 표시
            TextMeshProUGUI tierText = itemObj.transform.Find("TierText")?.GetComponent<TextMeshProUGUI>();
            if (tierText != null)
            {
                tierText.text = $"티어: {speciesData.rarityTier}";
            }

            // 아이콘 표시
            Image iconImage = itemObj.transform.Find("IconImage")?.GetComponent<Image>();
            if (iconImage != null && speciesData.icon != null)
            {
                iconImage.sprite = speciesData.icon;
                iconImage.enabled = true;
            }
            else if (iconImage != null)
            {
                iconImage.enabled = false;
            }

            // 가격 표시
            TextMeshProUGUI priceText = itemObj.transform.Find("PriceText")?.GetComponent<TextMeshProUGUI>();
            if (priceText != null)
            {
                priceText.text = $"해금 가격: {speciesData.unlockCost}";
            }

            // 해금 상태 표시
            TextMeshProUGUI statusText = itemObj.transform.Find("StatusText")?.GetComponent<TextMeshProUGUI>();
            if (statusText != null)
            {
                bool isUnlocked = GameManager.Instance != null && GameManager.Instance.IsSpeciesUnlocked(speciesData.speciesType);
                statusText.text = isUnlocked ? "해금됨" : "미해금";
                statusText.color = isUnlocked ? Color.green : Color.red;
            }

            // 구매 버튼
            Button buyButton = itemObj.transform.Find("BuyButton")?.GetComponent<Button>();
            if (buyButton != null)
            {
                bool isUnlocked = GameManager.Instance != null && GameManager.Instance.IsSpeciesUnlocked(speciesData.speciesType);
                bool canAfford = GameManager.Instance != null && GameManager.Instance.CurrentMoney >= speciesData.unlockCost;

                buyButton.interactable = !isUnlocked && canAfford;
                buyButton.onClick.RemoveAllListeners();
                buyButton.onClick.AddListener(() => OnBuyClicked(speciesData.speciesType));

                // 버튼 텍스트
                TextMeshProUGUI buttonText = buyButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = isUnlocked ? "해금됨" : (canAfford ? "연구" : "돈 부족");
                }
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

            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
            }
        }
    }
}
