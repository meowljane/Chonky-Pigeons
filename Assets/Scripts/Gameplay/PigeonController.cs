using UnityEngine;
using PigeonGame.Data;

namespace PigeonGame.Gameplay
{
    public class PigeonController : MonoBehaviour
    {
        private PigeonInstanceStats stats;
        [SerializeField] private PigeonAI ai;
        [SerializeField] private SpriteRenderer spriteRenderer; // Body SpriteRenderer
        [SerializeField] private Animator animator; // Body Animator
        [SerializeField] private SpriteRenderer faceSpriteRenderer; // Face SpriteRenderer (Body 위에 덮어씌워짐)
        [SerializeField] private Animator faceAnimator; // Face Animator (Body와 동기화)
        [SerializeField, Tooltip("좌우 이동 시 스프라이트를 반전할지 여부")]
        private bool flipSpriteOnMovement = true;
        [SerializeField, Tooltip("왼쪽으로 갈 때 반전 (true: 왼쪽=반전, false: 오른쪽=반전)")]
        private bool flipOnLeft = true;
        [SerializeField, Tooltip("비둘기별 고유한 Sorting Order (0이면 자동 생성, 겹칠 때 각 비둘기의 Body와 Face가 같은 그룹으로 묶임)")]
        private int baseSortingOrder = 0; // Inspector에서 설정 가능, 0이면 자동 생성
        private static int nextSortingOrder = 0; // 자동 생성용 정적 카운터
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
            
            // Face SpriteRenderer 찾기 (없으면 자동 생성)
            if (faceSpriteRenderer == null)
            {
                // "Face"라는 이름의 자식 오브젝트에서 찾기
                Transform faceTransform = transform.Find("Face");
                if (faceTransform != null)
                {
                    faceSpriteRenderer = faceTransform.GetComponent<SpriteRenderer>();
                }
                
                // 없으면 자동 생성
                if (faceSpriteRenderer == null)
                {
                    GameObject faceObj = new GameObject("Face");
                    faceObj.transform.SetParent(transform);
                    faceObj.transform.localPosition = Vector3.zero;
                    faceObj.transform.localRotation = Quaternion.identity;
                    faceObj.transform.localScale = Vector3.one;
                    faceSpriteRenderer = faceObj.AddComponent<SpriteRenderer>();
                    // sortingOrder는 Initialize에서 설정됨
                }
            }
            
            // Face Animator 찾기 (Face 오브젝트에 있어야 함)
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

            // SpriteRenderer 찾기 (없으면 자동으로 찾음)
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
            
            if (spriteRenderer == null)
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            // Sorting Order 설정 (각 비둘기마다 고유한 값 부여)
            int bodySortingOrder = baseSortingOrder;
            if (bodySortingOrder == 0)
            {
                // 자동 생성: 각 비둘기마다 고유한 값 (10 간격으로 생성하여 여유 공간 확보)
                bodySortingOrder = nextSortingOrder;
                nextSortingOrder += 10; // 다음 비둘기를 위해 10씩 증가
            }
            
            // Body SpriteRenderer 활성화 및 스프라이트 설정
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
                spriteRenderer.sortingOrder = bodySortingOrder; // 고유한 sortingOrder 설정
                
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
            
            // Face 설정 (Body의 sortingOrder를 기반으로 설정)
            SetupFace(stats, bodySortingOrder);
            
            // 초기 상태 설정
            lastMovementState = MovementState.Idle;
        }
        
        /// <summary>
        /// Face SpriteRenderer와 Animator 설정
        /// </summary>
        private void SetupFace(PigeonInstanceStats stats, int bodySortingOrder)
        {
            var registry = GameDataRegistry.Instance;
            if (registry == null || registry.Faces == null)
                return;
            
            var face = registry.Faces.GetFaceById(stats.faceId);
            if (face == null)
                return;
            
            // Face SpriteRenderer 설정
            if (faceSpriteRenderer != null)
            {
                faceSpriteRenderer.enabled = true;
                
                // Body SpriteRenderer와 동일한 설정
                if (spriteRenderer != null)
                {
                    faceSpriteRenderer.sortingLayerID = spriteRenderer.sortingLayerID;
                    faceSpriteRenderer.sortingOrder = bodySortingOrder + 1; // Body 위에 렌더링 (같은 비둘기 그룹 내에서)
                    faceSpriteRenderer.color = spriteRenderer.color;
                }
            }
            
            // Face Animator Controller 설정
            if (faceAnimator != null && face.animatorController != null)
            {
                faceAnimator.runtimeAnimatorController = face.animatorController;
                // Body Animator와 동일한 파라미터 구조를 가져야 함
            }
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
                
                // Face Animator도 동일하게 동기화
                if (faceAnimator != null)
                {
                    faceAnimator.SetInteger("MovementState", (int)currentState);
                }
                
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
                
                // Face도 동일하게 반전
                if (faceSpriteRenderer != null)
                {
                    faceSpriteRenderer.flipX = shouldFlip;
                }
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

