using UnityEngine;

namespace PigeonGame.Gameplay
{
    /// <summary>
    /// 월드맵에 배치되는 상점 오브젝트
    /// 플레이어가 근처에 있을 때 상호작용 가능
    /// </summary>
    public class WorldShop : MonoBehaviour
    {
        public enum ShopType
        {
            PigeonShop,    // 비둘기 판매 상점
            TrapShop       // 덫 구매 상점
        }

        [SerializeField] private ShopType shopType;
        [SerializeField] private float interactionRadius = 2f;
        [SerializeField] private bool showDebugGizmos = true;

        public ShopType Type => shopType;
        public float InteractionRadius => interactionRadius;

        private void OnDrawGizmosSelected()
        {
            if (!showDebugGizmos)
                return;

            Gizmos.color = shopType == ShopType.PigeonShop ? Color.green : Color.blue;
            Gizmos.DrawWireSphere(transform.position, interactionRadius);
        }

        /// <summary>
        /// 플레이어가 상점 근처에 있는지 확인
        /// </summary>
        public bool IsPlayerNearby()
        {
            if (PlayerController.Instance == null)
                return false;

            float distance = Vector2.Distance(transform.position, PlayerController.Instance.Position);
            return distance <= interactionRadius;
        }

        /// <summary>
        /// 플레이어와의 거리 반환
        /// </summary>
        public float GetDistanceToPlayer()
        {
            if (PlayerController.Instance == null)
                return float.MaxValue;

            return Vector2.Distance(transform.position, PlayerController.Instance.Position);
        }
    }
}

