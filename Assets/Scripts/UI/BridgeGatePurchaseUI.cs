using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PigeonGame.Gameplay;

namespace PigeonGame.UI
{
    /// <summary>
    /// 다리 게이트 구매 UI
    /// </summary>
    public class BridgeGatePurchaseUI : MonoBehaviour
    {
        [Header("Main Panel")]
        [SerializeField] private GameObject purchasePanel;
        [SerializeField] private TextMeshProUGUI areaNameText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private Button purchaseButton;
        [SerializeField] private Button closeButton;

        private int currentAreaNumber;
        private int currentCost;
        private BridgeGate currentGate;
        private string currentAreaName;

        private void Start()
        {
            if (purchasePanel != null)
            {
                purchasePanel.SetActive(false);
            }

            UIHelper.SafeAddListener(purchaseButton, OnPurchaseButtonClicked);
            UIHelper.SafeAddListener(closeButton, OnCloseButtonClicked);

            // GameManager 이벤트 구독
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnMoneyChanged += OnMoneyChanged;
                GameManager.Instance.OnAreaUnlocked += OnAreaUnlocked;
            }

            UpdateGoldText();
        }

        /// <summary>
        /// 구매 패널 열기
        /// </summary>
        public void OpenPurchasePanel(BridgeGate gate, int areaNumber, int cost, string areaName = null)
        {
            if (purchasePanel == null || gate == null)
                return;

            currentGate = gate;
            currentAreaNumber = areaNumber;
            currentCost = cost;
            currentAreaName = areaName;

            purchasePanel.SetActive(true);
            UpdateDisplay();
        }

        private void OnMoneyChanged(int money)
        {
            UpdateGoldText();
            UpdatePurchaseButton();
        }

        private void OnAreaUnlocked(int areaNumber)
        {
            if (areaNumber == currentAreaNumber)
            {
                // 게이트 해제 (이벤트에서도 호출하여 확실히 처리)
                if (currentGate != null && currentGate.gameObject != null && currentGate.gameObject.activeSelf)
                {
                    currentGate.UnlockGate();
                }
                // 구매 완료 후 패널 닫기
                ClosePanel();
            }
        }

        private void UpdateDisplay()
        {
            // 지역 이름 표시
            if (areaNameText != null)
            {
                if (!string.IsNullOrEmpty(currentAreaName))
                {
                    areaNameText.text = currentAreaName;
                }
                else
                {
                    areaNameText.text = $"지역 {currentAreaNumber}";
                }
            }

            // 비용 표시
            if (costText != null)
            {
                costText.text = $"해금 비용: {currentCost}G";
            }

            UpdateGoldText();
            UpdatePurchaseButton();
        }

        private void UpdateGoldText()
        {
            UIHelper.UpdateGoldText(goldText);
        }

        private void UpdatePurchaseButton()
        {
            if (purchaseButton == null)
                return;

            bool isUnlocked = GameManager.Instance != null && GameManager.Instance.IsAreaUnlocked(currentAreaNumber);
            bool canAfford = GameManager.Instance != null && GameManager.Instance.CurrentMoney >= currentCost;

            purchaseButton.interactable = !isUnlocked && canAfford;

            // 버튼 텍스트 업데이트
            TextMeshProUGUI buttonText = purchaseButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                if (isUnlocked)
                {
                    buttonText.text = "이미 해금됨";
                }
                else if (canAfford)
                {
                    buttonText.text = "해금";
                }
                else
                {
                    buttonText.text = "돈부족";
                }
            }
        }

        private void OnPurchaseButtonClicked()
        {
            if (GameManager.Instance == null)
                return;

            if (currentGate == null)
                return;

            // 구매 처리 (성공하면 OnAreaUnlocked 이벤트가 발생하여 게이트가 해제됨)
            GameManager.Instance.UnlockArea(currentAreaNumber, currentCost);
        }

        private void OnCloseButtonClicked()
        {
            ClosePanel();
        }

        private void ClosePanel()
        {
            if (purchasePanel != null)
            {
                purchasePanel.SetActive(false);
            }
            currentGate = null;
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnMoneyChanged -= OnMoneyChanged;
                GameManager.Instance.OnAreaUnlocked -= OnAreaUnlocked;
            }
            UIHelper.SafeRemoveListener(purchaseButton);
            UIHelper.SafeRemoveListener(closeButton);
        }
    }
}
