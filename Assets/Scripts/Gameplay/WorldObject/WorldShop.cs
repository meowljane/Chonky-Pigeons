using UnityEngine;

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
            Exhibition         
        }

        [SerializeField] private ShopType shopType;

        public ShopType Type => shopType;

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
            }
        }
    }
}

