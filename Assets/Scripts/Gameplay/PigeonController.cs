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
        [SerializeField, Tooltip("좌우 이동 시 스프라이트를 반전할지 여부")]
        private bool flipSpriteOnMovement = true;
        [SerializeField, Tooltip("왼쪽으로 갈 때 반전 (true: 왼쪽=반전, false: 오른쪽=반전)")]
        private bool flipOnLeft = true;
        [SerializeField, Tooltip("비둘기별 고유한 Sorting Order (0이면 자동 생성, 겹칠 때 각 비둘기의 Body와 Face가 같은 그룹으로 묶임)")]
        private int baseSortingOrder = 0; 
        private static int nextSortingOrder = 0; 
        private PigeonMovement movement;
        private bool isExhibitionPigeon = false; 
        private MovementState lastMovementState; 

        public PigeonInstanceStats Stats => stats;
        public bool IsExhibitionPigeon => isExhibitionPigeon;

        private void Awake()
        {
            if (animator == null)
                animator = GetComponent<Animator>();
            if (animator == null)
                animator = GetComponentInChildren<Animator>();

            if (faceSpriteRenderer == null)
            {
                Transform faceTransform = transform.Find("Face");
                if (faceTransform != null)
                {
                    faceSpriteRenderer = faceTransform.GetComponent<SpriteRenderer>();
                }

                if (faceSpriteRenderer == null)
                {
                    GameObject faceObj = new GameObject("Face");
                    faceObj.transform.SetParent(transform);
                    faceObj.transform.localPosition = Vector3.zero;
                    faceObj.transform.localRotation = Quaternion.identity;
                    faceObj.transform.localScale = Vector3.one;
                    faceSpriteRenderer = faceObj.AddComponent<SpriteRenderer>();
                }
            }

            if (faceAnimator == null && faceSpriteRenderer != null)
            {
                faceAnimator = faceSpriteRenderer.GetComponent<Animator>();
                if (faceAnimator == null)
                {
                    faceAnimator = faceSpriteRenderer.gameObject.AddComponent<Animator>();
                }
            }

            movement = GetComponent<PigeonMovement>();
        }

        public void Initialize(PigeonInstanceStats stats)
        {
            this.stats = stats;
            if (ai == null)
                ai = GetComponent<PigeonAI>();
            if (ai != null)
                ai.Initialize(stats);

            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            if (spriteRenderer == null)
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            int bodySortingOrder = baseSortingOrder;
            if (bodySortingOrder == 0)
            {
                bodySortingOrder = nextSortingOrder;
                nextSortingOrder += 10; 
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
                spriteRenderer.sortingOrder = bodySortingOrder; 

                var species = GameDataRegistry.Instance?.SpeciesSet?.GetSpeciesById(stats.speciesId);
                if (species?.icon != null && animator == null)
                {
                    spriteRenderer.sprite = species.icon;
                }
            }

            if (animator != null)
            {
                var species = GameDataRegistry.Instance?.SpeciesSet?.GetSpeciesById(stats.speciesId);
                if (species?.animatorController != null)
                {
                    animator.runtimeAnimatorController = species.animatorController;
                }
            }

            SetupFace(stats, bodySortingOrder);

            lastMovementState = MovementState.Idle;
        }

        private void SetupFace(PigeonInstanceStats stats, int bodySortingOrder)
        {
            var registry = GameDataRegistry.Instance;
            if (registry == null || registry.Faces == null)
                return;

            var face = registry.Faces.GetFaceById(stats.faceId);
            if (face == null)
                return;

            if (faceSpriteRenderer != null)
            {
                faceSpriteRenderer.enabled = true;

                if (spriteRenderer != null)
                {
                    faceSpriteRenderer.sortingLayerID = spriteRenderer.sortingLayerID;
                    faceSpriteRenderer.sortingOrder = bodySortingOrder + 1; 
                    faceSpriteRenderer.color = spriteRenderer.color;
                }
            }

            if (faceAnimator != null && face.animatorController != null)
            {
                faceAnimator.runtimeAnimatorController = face.animatorController;
            }
        }

        private void Update()
        {
            UpdateAnimationState();

            UpdateSpriteFlip();
        }

        private void UpdateAnimationState()
        {
            if (animator == null || movement == null)
                return;

            MovementState currentState = movement.CurrentMovementState;

            if (currentState != lastMovementState)
            {
                animator.SetInteger("MovementState", (int)currentState);

                if (faceAnimator != null)
                {
                    faceAnimator.SetInteger("MovementState", (int)currentState);
                }

                lastMovementState = currentState;
            }
        }

        private void UpdateSpriteFlip()
        {
            if (!flipSpriteOnMovement || spriteRenderer == null || movement == null)
                return;

            Vector2 movementDir = movement.MovementDirection;

            if (Mathf.Abs(movementDir.x) > 0.01f)
            {
                bool shouldFlip = flipOnLeft ? movementDir.x < 0 : movementDir.x > 0;
                spriteRenderer.flipX = shouldFlip;

                if (faceSpriteRenderer != null)
                {
                    faceSpriteRenderer.flipX = shouldFlip;
                }
            }
        }

        public void SetAsExhibitionPigeon()
        {
            isExhibitionPigeon = true;
        }
    }
}

