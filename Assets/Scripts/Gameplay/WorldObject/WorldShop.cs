using UnityEngine;

namespace PigeonGame.Gameplay
{
    /// <summary>
    /// 월드맵에 배치되는 상점/건물 오브젝트
    /// 플레이어가 근처에 있을 때 상호작용 가능
    /// </summary>
    public class WorldShop : InteractableBase
    {
        public enum ShopType
        {
            PigeonShop,        // 비둘기 판매 상점
            TrapShop,          // 덫 구매 상점
            PigeonResearch,    // 비둘기 연구소
            UpgradeShop,       // 업그레이드 상점
            Exhibition         // 전시관
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

