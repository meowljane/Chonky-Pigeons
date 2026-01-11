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
        [SerializeField] private Button closeButton;

        private List<GameObject> trapItems = new List<GameObject>();

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
                // CloseButton이라는 이름의 버튼 찾기
                if (closeButton == null)
                {
                    Transform closeButtonTransform = shopPanel.transform.Find("CloseButton");
                    if (closeButtonTransform != null)
                    {
                        closeButton = closeButtonTransform.GetComponent<Button>();
                    }
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
                GameManager.Instance.OnTrapUnlocked += OnTrapUnlocked;
                GameManager.Instance.OnMoneyChanged += OnMoneyChanged;
            }

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
                UpdateShopDisplay();
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
            if (trapContainer == null || trapItemPrefab == null)
                return;

            // 기존 아이템 제거
            foreach (var item in trapItems)
            {
                if (item != null)
                    Destroy(item);
            }
            trapItems.Clear();

            // 모든 덫 표시
            var registry = GameDataRegistry.Instance;
            if (registry == null || registry.Traps == null)
                return;

            var allTraps = registry.Traps.traps;

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
                bool isUnlocked = GameManager.Instance != null && GameManager.Instance.IsTrapUnlocked(trapData.id);
                statusText.text = isUnlocked ? "해금됨" : "미해금";
                statusText.color = isUnlocked ? Color.green : Color.red;
            }

            // 구매 버튼
            Button buyButton = itemObj.transform.Find("BuyButton")?.GetComponent<Button>();
            if (buyButton != null)
            {
                bool isUnlocked = GameManager.Instance != null && GameManager.Instance.IsTrapUnlocked(trapData.id);
                bool canAfford = GameManager.Instance != null && GameManager.Instance.CurrentMoney >= trapData.cost;

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
            if (GameManager.Instance != null)
            {
                GameManager.Instance.UnlockTrap(trapId);
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

