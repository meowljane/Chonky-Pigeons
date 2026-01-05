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
        [SerializeField] private GameObject shopPanel;
        [SerializeField] private Transform trapContainer;
        [SerializeField] private GameObject trapItemPrefab;
        [SerializeField] private KeyCode toggleKey = KeyCode.T;

        private List<GameObject> trapItems = new List<GameObject>();
        private TrapShop shop;

        private void Start()
        {
            shop = FindObjectOfType<TrapShop>();
            if (shop == null)
            {
                shop = gameObject.AddComponent<TrapShop>();
            }

            if (shopPanel != null)
            {
                shopPanel.SetActive(false);
            }

            // GameManager 이벤트 구독
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnTrapUnlocked += OnTrapUnlocked;
                GameManager.Instance.OnMoneyChanged += OnMoneyChanged;
            }

            UpdateShopDisplay();
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                ToggleShop();
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnTrapUnlocked -= OnTrapUnlocked;
                GameManager.Instance.OnMoneyChanged -= OnMoneyChanged;
            }
        }

        public void ToggleShop()
        {
            if (shopPanel != null)
            {
                bool isActive = shopPanel.activeSelf;
                shopPanel.SetActive(!isActive);
                
                if (!isActive)
                {
                    UpdateShopDisplay();
                }
            }
        }

        private void OnTrapUnlocked(string trapId)
        {
            UpdateShopDisplay();
        }

        private void OnMoneyChanged(int money)
        {
            UpdateShopDisplay();
        }

        private void UpdateShopDisplay()
        {
            if (shop == null || trapContainer == null || trapItemPrefab == null)
                return;

            // 기존 아이템 제거
            foreach (var item in trapItems)
            {
                if (item != null)
                    Destroy(item);
            }
            trapItems.Clear();

            // 모든 덫 표시
            var allTraps = shop.GetAllTraps();
            var registry = GameDataRegistry.Instance;

            foreach (var trapData in allTraps)
            {
                GameObject itemObj = Instantiate(trapItemPrefab, trapContainer);
                trapItems.Add(itemObj);

                SetupTrapItemUI(itemObj, trapData);
            }
        }

        private void SetupTrapItemUI(GameObject itemObj, TrapDefinition trapData)
        {
            // 덫 이름 표시
            TextMeshProUGUI nameText = itemObj.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            if (nameText != null)
            {
                nameText.text = trapData.name;
            }

            // 가격 표시
            TextMeshProUGUI priceText = itemObj.transform.Find("PriceText")?.GetComponent<TextMeshProUGUI>();
            if (priceText != null)
            {
                priceText.text = $"가격: {trapData.cost}";
            }

            // 해금 상태 표시
            TextMeshProUGUI statusText = itemObj.transform.Find("StatusText")?.GetComponent<TextMeshProUGUI>();
            if (statusText != null)
            {
                bool isUnlocked = shop.IsTrapUnlocked(trapData.id);
                statusText.text = isUnlocked ? "해금됨" : "미해금";
                statusText.color = isUnlocked ? Color.green : Color.red;
            }

            // 구매 버튼
            Button buyButton = itemObj.transform.Find("BuyButton")?.GetComponent<Button>();
            if (buyButton != null)
            {
                bool isUnlocked = shop.IsTrapUnlocked(trapData.id);
                bool canAfford = shop.CanAffordTrap(trapData.id);

                buyButton.interactable = !isUnlocked && canAfford;
                buyButton.onClick.RemoveAllListeners();
                buyButton.onClick.AddListener(() => OnBuyClicked(trapData.id));

                // 버튼 텍스트
                TextMeshProUGUI buttonText = buyButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = isUnlocked ? "해금됨" : (canAfford ? "구매" : "돈 부족");
                }
            }
        }

        private void OnBuyClicked(string trapId)
        {
            if (shop != null)
            {
                shop.TryPurchaseTrap(trapId);
                UpdateShopDisplay();
            }
        }
    }
}

