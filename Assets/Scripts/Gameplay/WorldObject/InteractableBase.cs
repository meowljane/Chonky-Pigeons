using UnityEngine;

namespace PigeonGame.Gameplay
{
    /// <summary>
    /// 상호작용 가능한 오브젝트 인터페이스
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// 상호작용 가능한지 확인
        /// </summary>
        bool CanInteract();

        /// <summary>
        /// 상호작용 실행
        /// </summary>
        void OnInteract();
    }

    /// <summary>
    /// 상호작용 가능한 오브젝트의 베이스 클래스
    /// 공통 상호작용 로직을 제공
    /// </summary>
    public abstract class InteractableBase : MonoBehaviour, IInteractable
    {
        [Header("Interaction")]
        [SerializeField] protected float interactionRadius = 2f;

        protected Collider2D interactionTrigger;
        protected bool isPlayerInRange = false;

        public float InteractionRadius => interactionRadius;
        public bool IsPlayerInRange => isPlayerInRange;

        protected virtual void Start()
        {
            SetupInteractionTrigger();
        }

        /// <summary>
        /// 상호작용 트리거 설정
        /// </summary>
        protected void SetupInteractionTrigger()
        {
            interactionTrigger = GetComponent<Collider2D>();

            if (interactionTrigger == null)
            {
                CircleCollider2D interactionCol = gameObject.AddComponent<CircleCollider2D>();
                interactionCol.radius = interactionRadius;
                interactionCol.isTrigger = true;
                interactionTrigger = interactionCol;
            }
            else
            {
                interactionTrigger.isTrigger = true;
                if (interactionTrigger is CircleCollider2D circleCol)
                {
                    circleCol.radius = interactionRadius;
                }
            }

            interactionTrigger.enabled = true;
        }

        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<PlayerController>() != null)
            {
                isPlayerInRange = true;
                if (InteractionSystem.Instance != null)
                {
                    InteractionSystem.Instance.RegisterInteractable(this);
                }
            }
        }

        protected virtual void OnTriggerExit2D(Collider2D other)
        {
            if (other.GetComponent<PlayerController>() != null)
            {
                isPlayerInRange = false;
                if (InteractionSystem.Instance != null)
                {
                    InteractionSystem.Instance.UnregisterInteractable(this);
                }
            }
        }

        public virtual bool CanInteract()
        {
            return isPlayerInRange;
        }

        public abstract void OnInteract();
    }
}
