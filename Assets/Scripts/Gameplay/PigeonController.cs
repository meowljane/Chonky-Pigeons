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
        [SerializeField, Tooltip("좌우 이동 시 스프라이트를 반전할지 여부")]
        private bool flipSpriteOnMovement = true;
        [SerializeField, Tooltip("왼쪽으로 갈 때 반전 (true: 왼쪽=반전, false: 오른쪽=반전)")]
        private bool flipOnLeft = true;
        private PigeonMovement movement;
        private bool isExhibitionPigeon = false; // 전시관 비둘기 여부
        private Collider2D exhibitionArea = null; // 전시관 영역
        private MovementState lastMovementState; // 이전 이동 상태 추적

        public PigeonInstanceStats Stats => stats;
        public bool IsExhibitionPigeon => isExhibitionPigeon;
        public Collider2D ExhibitionArea => exhibitionArea;

        private void Awake()
        {
            // 컴포넌트 자동 찾기
            if (animator == null)
                animator = GetComponent<Animator>();
            if (animator == null)
                animator = GetComponentInChildren<Animator>();
            
            movement = GetComponent<PigeonMovement>();
        }

        public void Initialize(PigeonInstanceStats stats)
        {
            this.stats = stats;
            if (ai == null)
                ai = GetComponent<PigeonAI>();
            if (ai != null)
                ai.Initialize(stats);

            // SpriteRenderer 찾기 (없으면 자동으로 찾음)
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
            
            if (spriteRenderer == null)
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            // SpriteRenderer 활성화 및 스프라이트 설정
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
                
                // 스프라이트 설정 (애니메이션이 없을 때만 사용)
                var species = GameDataRegistry.Instance?.SpeciesSet?.GetSpeciesById(stats.speciesId);
                if (species?.icon != null && animator == null)
                {
                    spriteRenderer.sprite = species.icon;
                }
            }
            
            // Animator Controller 설정 (종별로 다른 애니메이션 사용)
            if (animator != null)
            {
                var species = GameDataRegistry.Instance?.SpeciesSet?.GetSpeciesById(stats.speciesId);
                if (species?.animatorController != null)
                {
                    animator.runtimeAnimatorController = species.animatorController;
                }
            }
            
            // 초기 상태 설정
            lastMovementState = MovementState.Idle;
        }
        
        private void Update()
        {
            // 애니메이션 상태 업데이트
            UpdateAnimationState();
            
            // 스프라이트 반전 업데이트
            UpdateSpriteFlip();
        }
        
        /// <summary>
        /// 이동 상태에 따라 애니메이션 파라미터 업데이트
        /// </summary>
        private void UpdateAnimationState()
        {
            if (animator == null || movement == null)
                return;
            
            MovementState currentState = movement.CurrentMovementState;
            
            // 상태가 변경되었을 때만 Animator 파라미터 업데이트
            if (currentState != lastMovementState)
            {
                // Int 파라미터 사용 (0: Idle, 1: Walking, 2: Flying)
                animator.SetInteger("MovementState", (int)currentState);
                
                // 또는 Bool 파라미터 사용 (선택사항)
                animator.SetBool("IsIdle", currentState == MovementState.Idle);
                animator.SetBool("IsWalking", currentState == MovementState.Walking);
                animator.SetBool("IsFlying", currentState == MovementState.Flying);
                
                lastMovementState = currentState;
            }
        }
        
        /// <summary>
        /// 이동 방향에 따라 스프라이트 반전 업데이트
        /// </summary>
        private void UpdateSpriteFlip()
        {
            if (!flipSpriteOnMovement || spriteRenderer == null || movement == null)
                return;
            
            Vector2 movementDir = movement.MovementDirection;
            
            // X 방향만 체크 (좌우 반전)
            if (Mathf.Abs(movementDir.x) > 0.01f)
            {
                // flipOnLeft가 true면 왼쪽(x < 0)일 때 반전, false면 오른쪽(x > 0)일 때 반전
                bool shouldFlip = flipOnLeft ? movementDir.x < 0 : movementDir.x > 0;
                spriteRenderer.flipX = shouldFlip;
            }
        }

        /// <summary>
        /// 전시관 비둘기로 설정
        /// </summary>
        public void SetAsExhibitionPigeon(Collider2D area)
        {
            isExhibitionPigeon = true;
            exhibitionArea = area;
        }
    }
}

