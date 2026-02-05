using UnityEngine;
using PigeonGame.Data;

namespace PigeonGame.Gameplay
{
    public class PigeonController : MonoBehaviour
    {
        private PigeonInstanceStats stats;
        [SerializeField] private PigeonAI ai;
        [SerializeField] private SpriteRenderer spriteRenderer; 
        [SerializeField] private Animator animator; 
        [SerializeField] private SpriteRenderer faceSpriteRenderer; 
        [SerializeField] private Animator faceAnimator; 
        [SerializeField] private PigeonMovement movement;
        private static int nextSortingOrder = 0;
        private bool isExhibitionPigeon = false; 
        private MovementState lastMovementState; 

        public PigeonInstanceStats Stats => stats;
        public bool IsExhibitionPigeon => isExhibitionPigeon;

        public void Initialize(PigeonInstanceStats stats)
        {
            this.stats = stats;
            ai.Initialize(stats);

            int bodySortingOrder = nextSortingOrder;
            nextSortingOrder += 10;

            spriteRenderer.enabled = true;
            spriteRenderer.sortingOrder = bodySortingOrder; 

            var species = GameDataRegistry.Instance?.SpeciesSet?.GetSpeciesById(stats.speciesId);
            if (species != null)
            {
                if (species.icon != null)
                    spriteRenderer.sprite = species.icon;
                if (species.animatorController != null)
                    animator.runtimeAnimatorController = species.animatorController;
            }

            SetupFace(bodySortingOrder);

            lastMovementState = MovementState.Idle;
        }

        private void SetupFace(int bodySortingOrder)
        {
            var registry = GameDataRegistry.Instance;
            if (registry?.Faces == null)
                return;

            var face = registry.Faces.GetFaceById(stats.faceId);
            if (face == null)
                return;

            faceSpriteRenderer.enabled = true;
            faceSpriteRenderer.sortingLayerID = spriteRenderer.sortingLayerID;
            faceSpriteRenderer.sortingOrder = bodySortingOrder + 1; 
            faceSpriteRenderer.color = spriteRenderer.color;

            if (face.animatorController != null)
                faceAnimator.runtimeAnimatorController = face.animatorController;
        }

        private void Update()
        {
            MovementState currentState = movement.CurrentMovementState;
            if (currentState != lastMovementState)
            {
                animator.SetInteger("MovementState", (int)currentState);
                faceAnimator.SetInteger("MovementState", (int)currentState);
                lastMovementState = currentState;
            }

            Vector2 movementDir = movement.MovementDirection;
            if (Mathf.Abs(movementDir.x) > 0.01f)
            {
                bool shouldFlip = movementDir.x > 0;
                spriteRenderer.flipX = shouldFlip;
                faceSpriteRenderer.flipX = shouldFlip;
            }
        }

        public void SetAsExhibitionPigeon()
        {
            isExhibitionPigeon = true;
        }
    }
}

