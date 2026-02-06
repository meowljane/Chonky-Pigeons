using UnityEngine;
using PigeonGame.Data;
using PigeonGame.UI;

namespace PigeonGame.Gameplay
{
    public class WorldShop : InteractableBase
    {
        public enum ShopType
        {
            PigeonShop,        
            TrapShop,          
            PigeonResearch,    
            UpgradeShop,       
            Exhibition,
            Door
        }

        [SerializeField] private ShopType shopType;
        [SerializeField] private DoorType doorType = DoorType.DOOR1;

        public ShopType Type => shopType;
        public DoorType DoorType => doorType;

        protected override void Start()
        {
            base.Start();

            if (shopType == ShopType.Door)
            {
                if (GameManager.Instance?.IsDoorUnlocked(doorType) == true)
                {
                    DoorPurchaseUI.UnlockDoor(doorType);
                    Destroy(gameObject);
                    return;
                }

                if (GameManager.Instance != null)
                    GameManager.Instance.OnDoorUnlocked += OnDoorUnlocked;
            }
        }

        private void OnDoorUnlocked(DoorType unlockedDoorType)
        {
            if (unlockedDoorType == doorType)
            {
                DoorPurchaseUI.UnlockDoor(doorType);
                if (GameManager.Instance != null)
                    GameManager.Instance.OnDoorUnlocked -= OnDoorUnlocked;
                Destroy(gameObject);
            }
        }

        public override void OnInteract()
        {
            if (!CanInteract())
                return;

            var interactionSystem = InteractionSystem.Instance;
            if (interactionSystem == null)
                return;

            switch (Type)
            {
                case ShopType.PigeonShop:
                    interactionSystem.OpenPigeonShop();
                    break;
                case ShopType.TrapShop:
                    interactionSystem.OpenTrapShop();
                    break;
                case ShopType.PigeonResearch:
                    interactionSystem.OpenPigeonResearch();
                    break;
                case ShopType.UpgradeShop:
                    interactionSystem.OpenUpgradeShop();
                    break;
                case ShopType.Exhibition:
                    interactionSystem.OpenExhibition();
                    break;
                case ShopType.Door:
                    interactionSystem.OpenDoor(this);
                    break;
            }
        }

        private void OnDestroy()
        {
            if (shopType == ShopType.Door && GameManager.Instance != null)
                GameManager.Instance.OnDoorUnlocked -= OnDoorUnlocked;
        }
    }
}

