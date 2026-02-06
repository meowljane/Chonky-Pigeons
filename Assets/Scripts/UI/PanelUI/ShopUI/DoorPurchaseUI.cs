using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using TMPro;
using System.Collections.Generic;
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
        private WorldShop currentDoorShop;
        private MapType currentUnlocksMap;

        private void Start()
        {
            ShopUIHelper.InitializeShopPanel(purchasePanel, closeButton, goldText, OnCloseButtonClicked);
            UIHelper.SafeAddListener(purchaseButton, OnPurchaseButtonClicked);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnMoneyChanged += OnMoneyChanged;
                GameManager.Instance.OnDoorUnlocked += OnDoorUnlocked;
            }
        }

        public void OpenShopPanel(WorldShop doorShop)
        {
            if (purchasePanel == null || doorShop == null)
                return;

            currentDoorShop = doorShop;
            currentDoorType = doorShop.DoorType;

            var registry = GameDataRegistry.Instance;
            var doorDefinition = registry?.DoorSet?.GetDoorById(currentDoorType);
            currentCost = doorDefinition?.unlockCost ?? 100;
            currentUnlocksMap = doorDefinition?.unlocksMap ?? MapType.MAP1;

            ShopUIHelper.OpenShopPanel(purchasePanel, goldText, UpdateDisplay);
        }

        private void OnMoneyChanged(int money)
        {
            ShopUIHelper.HandleMoneyChanged(goldText, UpdateDisplay);
        }

        private void OnDoorUnlocked(DoorType doorType)
        {
            UnlockDoor(doorType);

            if (doorType == currentDoorType)
            {
                ShopUIHelper.CloseShopPanel(purchasePanel);
                currentDoorShop = null;
            }
        }

        private void UpdateDisplay()
        {
            if (doorNameText != null)
            {
                doorNameText.text = UIHelper.GetMapName(currentUnlocksMap);
            }

            if (costText != null)
            {
                costText.text = $"해금 비용: {currentCost}G";
            }

            UIHelper.UpdateGoldText(goldText);
            UpdatePurchaseButton();
        }

        private void UpdatePurchaseButton()
        {
            if (purchaseButton == null) return;

            bool isUnlocked = GameManager.Instance?.IsDoorUnlocked(currentDoorType) ?? false;
            bool canAfford = (GameManager.Instance?.CurrentMoney ?? 0) >= currentCost;
            ShopUIHelper.SetupUnlockButton(purchaseButton, purchaseButtonText, isUnlocked, canAfford, currentCost, "이미 해금됨", "해금", "돈부족");
        }

        private void OnPurchaseButtonClicked()
        {
            if (GameManager.Instance == null || currentDoorShop == null)
                return;

            GameManager.Instance.UnlockDoor(currentDoorType, currentCost);
        }

        public static void UnlockDoor(DoorType doorType)
        {
            Tilemap doorTilemap = TilemapRangeManager.Instance?.GetDoorTilemapByType(doorType);
            if (doorTilemap == null) return;

            BoundsInt bounds = doorTilemap.cellBounds;
            List<Vector3Int> positionsToClear = new List<Vector3Int>();

            foreach (var pos in bounds.allPositionsWithin)
            {
                if (doorTilemap.HasTile(pos))
                    positionsToClear.Add(pos);
            }

            foreach (var pos in positionsToClear)
                doorTilemap.SetTile(pos, null);

            if (TilemapRangeManager.Instance != null)
            {
                foreach (var pos in positionsToClear)
                {
                    Vector3 worldPos = doorTilemap.CellToWorld(pos) + doorTilemap.cellSize * 0.5f;
                    TilemapRangeManager.Instance.InvalidatePlayerMovementCache(worldPos);
                }
            }
        }

        private void OnCloseButtonClicked()
        {
            ShopUIHelper.CloseShopPanel(purchasePanel);
            currentDoorShop = null;
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
