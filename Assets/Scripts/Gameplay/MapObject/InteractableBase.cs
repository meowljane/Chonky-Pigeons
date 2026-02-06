using UnityEngine;

namespace PigeonGame.Gameplay
{
    public interface IInteractable
    {
        bool CanInteract();

        void OnInteract();
    }

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
            if (other.CompareTag("Player"))
            {
                isPlayerInRange = true;
                InteractionSystem.Instance?.RegisterInteractable(this);
            }
        }

        protected virtual void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                isPlayerInRange = false;
                InteractionSystem.Instance?.UnregisterInteractable(this);
            }
        }

        public virtual bool CanInteract()
        {
            return isPlayerInRange;
        }

        public abstract void OnInteract();
    }
}
