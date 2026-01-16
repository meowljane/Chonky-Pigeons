using UnityEngine;

namespace PigeonGame.Gameplay
{
    /// <summary>
    /// 비둘기 연구소 건물
    /// 플레이어가 근처에 있을 때 상호작용 가능
    /// </summary>
    public class PigeonResearchBuilding : MonoBehaviour, IInteractable
    {
        [SerializeField] private float interactionRadius = 2f;

        private Collider2D interactionTrigger;
        private bool isPlayerInRange = false;

        public float InteractionRadius => interactionRadius;
        public bool IsPlayerInRange => isPlayerInRange;

        private void Start()
        {
            SetupInteractionTrigger();
        }

        private void SetupInteractionTrigger()
        {
            interactionTrigger = GetComponent<Collider2D>();
            
            if (interactionTrigger == null)
            {
                CircleCollider2D circleCollider = gameObject.AddComponent<CircleCollider2D>();
                circleCollider.radius = interactionRadius;
                circleCollider.isTrigger = true;
                interactionTrigger = circleCollider;
            }
            else
            {
                interactionTrigger.isTrigger = true;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
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

        private void OnTriggerExit2D(Collider2D other)
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

        // IInteractable 구현
        public bool CanInteract()
        {
            return isPlayerInRange;
        }

        public void OnInteract()
        {
            if (!CanInteract())
                return;

            var interactionSystem = InteractionSystem.Instance;
            if (interactionSystem != null)
            {
                interactionSystem.OpenPigeonResearch();
            }
        }
    }
}
