using UnityEngine;

namespace PigeonGame.Gameplay
{
    /// <summary>
    /// 월드맵에 배치되는 상점 오브젝트
    /// 플레이어가 근처에 있을 때 상호작용 가능
    /// </summary>
    public class WorldShop : MonoBehaviour, IInteractable
    {
        public enum ShopType
        {
            PigeonShop,    // 비둘기 판매 상점
            TrapShop       // 덫 구매 상점
        }

        [SerializeField] private ShopType shopType;
        [SerializeField] private float interactionRadius = 2f;
        [SerializeField] private bool showDebugGizmos = true;

        private Collider2D interactionTrigger; // 상호작용 트리거 영역
        private bool isPlayerInRange = false; // 플레이어가 범위 안에 있는지

        public ShopType Type => shopType;
        public float InteractionRadius => interactionRadius;
        public bool IsPlayerInRange => isPlayerInRange;

        private void Start()
        {
            SetupInteractionTrigger();
        }

        /// <summary>
        /// 상호작용 트리거 설정
        /// </summary>
        private void SetupInteractionTrigger()
        {
            // 기존 콜라이더 찾기
            interactionTrigger = GetComponent<Collider2D>();
            
            // 콜라이더가 없으면 생성
            if (interactionTrigger == null)
            {
                CircleCollider2D circleCollider = gameObject.AddComponent<CircleCollider2D>();
                circleCollider.radius = interactionRadius;
                circleCollider.isTrigger = true;
                interactionTrigger = circleCollider;
            }
            else
            {
                // 기존 콜라이더를 트리거로 설정
                interactionTrigger.isTrigger = true;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // 플레이어인지 확인 (컴포넌트로만 체크)
            if (other.GetComponent<PlayerController>() != null)
            {
                isPlayerInRange = true;
                // InteractionSystem에 알림
                if (InteractionSystem.Instance != null)
                {
                    InteractionSystem.Instance.RegisterInteractable(this);
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            // 플레이어인지 확인 (컴포넌트로만 체크)
            if (other.GetComponent<PlayerController>() != null)
            {
                isPlayerInRange = false;
                // InteractionSystem에서 제거
                if (InteractionSystem.Instance != null)
                {
                    InteractionSystem.Instance.UnregisterInteractable(this);
                }
            }
        }

        // IInteractable 구현
        public bool CanInteract()
        {
            return isPlayerInRange;
        }

        public void OnInteract()
        {
            if (!CanInteract())
                return;

            // 상점 타입에 따라 다른 패널 열기
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
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!showDebugGizmos)
                return;

            Gizmos.color = shopType == ShopType.PigeonShop ? Color.green : Color.blue;
            Gizmos.DrawWireSphere(transform.position, interactionRadius);
        }
    }
}

