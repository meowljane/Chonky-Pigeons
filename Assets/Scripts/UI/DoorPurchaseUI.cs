using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PigeonGame.Gameplay;
using PigeonGame.Data;

namespace PigeonGame.UI
{
    /// <summary>
    /// 문 구매 UI
    /// </summary>
    public class DoorPurchaseUI : MonoBehaviour
    {
        [Header("Main Panel")]
        [SerializeField] private GameObject purchasePanel;
        [SerializeField] private TextMeshProUGUI doorNameText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private Button purchaseButton;
        [SerializeField] private Button closeButton;

        private DoorType currentDoorType;
        private int currentCost;
        private Door currentDoor;
        private MapType currentUnlocksMap;

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
                GameManager.Instance.OnDoorUnlocked += OnDoorUnlocked;
            }

            UpdateGoldText();
        }

        /// <summary>
        /// 구매 패널 열기
        /// </summary>
        public void OpenPurchasePanel(Door door, DoorType doorType, int cost, MapType unlocksMap)
        {
            if (purchasePanel == null || door == null)
                return;

            currentDoor = door;
            currentDoorType = doorType;
            currentCost = cost;
            currentUnlocksMap = unlocksMap;

            purchasePanel.SetActive(true);
            UpdateDisplay();
        }

        private void OnMoneyChanged(int money)
        {
            UpdateGoldText();
            UpdatePurchaseButton();
        }

        private void OnDoorUnlocked(DoorType doorType)
        {
            if (doorType == currentDoorType)
            {
                // 문 해제 (이벤트에서도 호출하여 확실히 처리)
                if (currentDoor != null && currentDoor.gameObject != null)
                {
                    currentDoor.UnlockDoor();
                }
                // 구매 완료 후 패널 닫기
                ClosePanel();
            }
        }

        private void UpdateDisplay()
        {
            // 맵 이름 표시
            if (doorNameText != null)
            {
                string mapName = currentUnlocksMap.ToString();
                var registry = GameDataRegistry.Instance;
                if (registry?.MapTypes != null)
                {
                    var mapDef = registry.MapTypes.GetMapById(currentUnlocksMap);
                    if (mapDef != null)
                    {
                        mapName = mapDef.displayName;
                    }
                }
                doorNameText.text = mapName;
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

            bool isUnlocked = GameManager.Instance != null && GameManager.Instance.IsDoorUnlocked(currentDoorType);
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

            if (currentDoor == null)
                return;

            // 구매 처리 (성공하면 OnDoorUnlocked 이벤트가 발생하여 문이 삭제됨)
            GameManager.Instance.UnlockDoor(currentDoorType, currentCost);
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
            currentDoor = null;
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnMoneyChanged -= OnMoneyChanged;
                GameManager.Instance.OnDoorUnlocked -= OnDoorUnlocked;
            }
            UIHelper.SafeRemoveListener(purchaseButton);
            UIHelper.SafeRemoveListener(closeButton);
        }
    }
}
