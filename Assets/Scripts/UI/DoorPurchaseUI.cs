using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PigeonGame.Gameplay;
using PigeonGame.Data;

namespace PigeonGame.UI
{
    public class DoorPurchaseUI : MonoBehaviour
    {
        [Header("Main Panel")]
        [SerializeField] private GameObject purchasePanel;
        [SerializeField] private TextMeshProUGUI doorNameText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private Button purchaseButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private TextMeshProUGUI purchaseButtonText; 

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

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnMoneyChanged += OnMoneyChanged;
                GameManager.Instance.OnDoorUnlocked += OnDoorUnlocked;
            }

            UpdateGoldText();
        }

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
                if (currentDoor != null && currentDoor.gameObject != null)
                {
                    currentDoor.UnlockDoor();
                }
                ClosePanel();
            }
        }

        private void UpdateDisplay()
        {
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

            if (purchaseButtonText != null)
            {
                if (isUnlocked)
                {
                    purchaseButtonText.text = "이미 해금됨";
                }
                else if (canAfford)
                {
                    purchaseButtonText.text = "해금";
                }
                else
                {
                    purchaseButtonText.text = "돈부족";
                }
            }
            else
            {
                Debug.LogWarning("PurchaseButtonText가 할당되지 않았습니다. 버튼 텍스트가 업데이트되지 않습니다.", this);
            }
        }

        private void OnPurchaseButtonClicked()
        {
            if (GameManager.Instance == null)
                return;

            if (currentDoor == null)
                return;

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
