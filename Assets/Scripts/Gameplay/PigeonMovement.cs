using UnityEngine;
using System.Collections.Generic;

namespace PigeonGame.Gameplay
{
    /// <summary>
    /// 비둘기의 이동 상태
    /// </summary>
    public enum MovementState
    {
        Idle,      // 멈춤
        Walking,   // 걷기
        Flying     // 날기 (Flee 상태일 때)
    }

    [RequireComponent(typeof(Rigidbody2D))]
    public class PigeonMovement : MonoBehaviour
    {
        [SerializeField] private float wanderSpeed = 2f;
        [SerializeField] private float backoffSpeed = 2f;
        [SerializeField] private float fleeSpeed = 4f;
        [SerializeField] private float wanderRadius = 2f;
        [SerializeField] private float wanderInterval = 2f;
        [SerializeField] private float eatingRadius = 0.1f;
        [SerializeField] private float detectionRadius = 2f; // 모든 비둘기 공통 감지 반경
        [SerializeField] private float alertWeight = 2.0f;
        [SerializeField] private float warnThreshold = 45f;
        [SerializeField] private float backoffThreshold = 70f;
        [SerializeField] private float fleeThreshold = 100f;
        
        public float DetectionRadius => detectionRadius;
        public float WarnThreshold => warnThreshold;
        public float BackoffThreshold => backoffThreshold;
        public float FleeThreshold => fleeThreshold;
        public float AlertWeight => alertWeight;
        
        /// <summary>
        /// 현재 이동 상태를 반환합니다.
        /// </summary>
        public MovementState CurrentMovementState
        {
            get
            {
                if (rb == null)
                    return MovementState.Idle;
                
                // Flee 상태면 Flying으로 처리
                if (ai != null && ai.CurrentState == PigeonState.Flee)
                {
                    return MovementState.Flying;
                }
                
                // 현재 목적지까지의 거리 체크
                Vector2? currentTarget = GetCurrentTarget();
                if (currentTarget == null)
                {
                    return MovementState.Idle; // 목적지가 없으면 Idle
                }
                
                float sqrDistance = ((Vector2)transform.position - currentTarget.Value).sqrMagnitude;
                const float arrivalThreshold = 0.01f; // 도착 판정 거리 (MoveTowardsTarget과 동일)
                
                if (sqrDistance < arrivalThreshold)
                {
                    return MovementState.Idle; // 목적지에 도착했으면 Idle
                }
                else
                {
                    return MovementState.Walking; // 목적지로 가는 중이면 Walking
                }
            }
        }
        
        /// <summary>
        /// 현재 활성 목적지 반환 (없으면 null)
        /// </summary>
        private Vector2? GetCurrentTarget()
        {
            // BackOff 중이면 backoffTarget
            if (backoffTargetSet)
            {
                return backoffTarget;
            }
            
            // Normal 상태면 targetFoodTrap 또는 wanderTarget
            if (targetFoodTrap != null && !targetFoodTrap.HasCapturedPigeon)
            {
                return targetFoodTrap.transform.position;
            }
            
            return wanderTarget;
        }
        
        private Rigidbody2D rb;
        private PigeonAI ai;
        private PigeonController controller;
        private Vector2 wanderTarget;
        private float wanderTimer;
        private FoodTrap targetFoodTrap;
        private Vector2 backoffTarget;
        private bool backoffTargetSet = false;
        private Vector2 backoffStartPosition; // BackOff 시작 위치
        private Camera mainCamera;
        private bool backoffCausedByPlayer = false;
        private float backoffEndTime = 0f; // BackOff 종료 시간
        private const float BACKOFF_COOLDOWN = 2f; // BackOff 종료 후 먹이 탐색 금지 시간 (초)
        private Vector2 lastMovementDirection = Vector2.right; // 마지막 이동 방향 (기본값: 오른쪽)
        
        /// <summary>
        /// 현재 이동 방향 (정규화된 벡터)
        /// </summary>
        public Vector2 MovementDirection => lastMovementDirection;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            if (rb == null)
                return;
            rb.gravityScale = 0;
            rb.linearDamping = 5f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            ai = GetComponent<PigeonAI>();
            controller = GetComponent<PigeonController>();
            mainCamera = Camera.main;
        }

        private void Start()
        {
            SetNewWanderTarget();
        }

        private void Update()
        {
            if (ai == null || controller == null || controller.Stats == null)
                return;

            // 전시관 비둘기는 wander만 수행
            if (controller.IsExhibitionPigeon)
            {
                HandleExhibitionWander();
                return;
            }

            // Flee 상태는 최우선 처리
            if (ai.CurrentState == PigeonState.Flee)
            {
                HandleFlee();
                return;
            }

            // Alert 시스템 업데이트 (플레이어 감지 및 alert 증가) - 항상 호출
            UpdateAlertSystem();

            // 상태에 따른 행동 처리
            PigeonState state = ai.CurrentState;

            // BackOff 목표가 설정되어 있으면 목표에 도달할 때까지 BackOff 유지
            if (backoffTargetSet)
            {
                float sqrDistanceToTarget = ((Vector2)transform.position - backoffTarget).sqrMagnitude;
                if (sqrDistanceToTarget >= 0.04f) // 0.2f * 0.2f
                {
                    // 목표에 아직 도달하지 않았으면 BackOff 계속
                    HandleBackOff();
                    return;
                }
                else
                {
                    // 목표에 도달했으면 BackOff 목표 초기화
                    backoffTargetSet = false;
                    backoffCausedByPlayer = false; // BackOff 종료 시 초기화
                    targetFoodTrap = null; // BackOff 종료 시 먹이 타겟 초기화
                    backoffEndTime = Time.time; // BackOff 종료 시간 기록
                }
            }

            // 플레이어가 가까이 있으면 무조건 BackOff
            if (IsPlayerNearby())
            {
                backoffCausedByPlayer = true;
                HandleBackOff();
                return;
            }

            // BackOff 상태 처리
            if (state == PigeonState.BackOff)
            {
                HandleBackOff();
            }
            else
            {
                HandleNormalMovement();
            }
        }

        private void UpdateAlertSystem()
        {
            if (controller == null || controller.Stats == null || ai == null)
                return;

            // Flee 상태일 때는 alert 업데이트 안 함
            if (ai.CurrentState == PigeonState.Flee)
                return;

            // 플레이어 감지 및 Alert 증가
            if (PlayerController.Instance != null)
            {
                Vector2 toPlayer = PlayerController.Instance.Position - (Vector2)transform.position;
                float sqrDistance = toPlayer.sqrMagnitude;
                float sqrRadius = detectionRadius * detectionRadius;
                
                if (sqrDistance <= sqrRadius)
                {
                    float distance = Mathf.Sqrt(sqrDistance);
                    float distanceFactor = Mathf.Clamp01(1f - (distance / detectionRadius));
                    ai.AddPlayerAlert(Time.deltaTime * distanceFactor);
                }
            }
        }

        private bool IsPlayerNearby()
        {
            if (PlayerController.Instance == null)
                return false;

            float sqrDistance = ((Vector2)transform.position - PlayerController.Instance.Position).sqrMagnitude;
            float sqrRadius = detectionRadius * detectionRadius;
            return sqrDistance <= sqrRadius;
        }

        private void HandleNormalMovement()
        {
            // BackOff 종료 후 일정 시간 동안은 먹이 탐색 금지
            if (Time.time - backoffEndTime >= BACKOFF_COOLDOWN)
            {
                FindNearestFoodTrap();
            }
            else
            {
                targetFoodTrap = null; // 쿨다운 중에는 먹이 타겟 초기화
            }

            wanderTimer += Time.deltaTime;
            if (wanderTimer >= wanderInterval)
            {
                SetNewWanderTarget();
                wanderTimer = 0f;
            }

            if (rb == null) return;

            Vector2 targetPos = targetFoodTrap != null && !targetFoodTrap.HasCapturedPigeon
                ? (Vector2)targetFoodTrap.transform.position
                : wanderTarget;
            
            MoveTowardsTarget(targetPos, wanderSpeed);
        }

        private void HandleBackOff()
        {
            if (controller == null || controller.Stats == null)
                return;

            // 먹이 경쟁으로 인한 BackOff는 2배 멀어짐
            float backoffDistance = backoffCausedByPlayer ? detectionRadius : detectionRadius * 2f;

            // BackOff 시작 위치 기록 (처음 BackOff 상태가 되었을 때)
            if (!backoffTargetSet)
            {
                backoffStartPosition = transform.position;
                Vector2 backoffDirection = CalculateBackoffDirection();
                backoffTarget = backoffStartPosition + backoffDirection * backoffDistance;
                // 타일맵 기반 맵 경계 내로 제한
                backoffTarget = ClampToMapBounds(backoffTarget);
                backoffTargetSet = true;
            }

            // 목표에 도달했는지 확인 (더 큰 거리로 판단)
            float sqrDistanceToTarget = ((Vector2)transform.position - backoffTarget).sqrMagnitude;
            if (sqrDistanceToTarget < 0.04f) // 0.2f * 0.2f
            {
                // 목표에 도달했으면 현재 위치에서 더 멀리 떨어진 새로운 목표 설정
                Vector2 backoffDirection = CalculateBackoffDirection();
                backoffTarget = (Vector2)transform.position + backoffDirection * backoffDistance;
                // 타일맵 기반 맵 경계 내로 제한
                backoffTarget = ClampToMapBounds(backoffTarget);
            }

            MoveTowardsTarget(backoffTarget, backoffSpeed);
        }

        private Vector2 CalculateBackoffDirection()
        {
            if (backoffCausedByPlayer && PlayerController.Instance != null)
            {
                Vector2 toPlayer = PlayerController.Instance.Position - (Vector2)transform.position;
                if (toPlayer.sqrMagnitude > 0.01f) // sqrMagnitude 사용으로 성능 개선
                {
                    return -toPlayer.normalized;
                }
            }

            // 먹이 경쟁으로 인한 BackOff는 랜덤 방향으로 멀어짐
            return Random.insideUnitCircle.normalized;
        }

        private void HandleFlee()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;

            // Flee 상태일 때는 맵 경계를 무시하고 자유롭게 이동
            Vector2 fleeDirection = CalculateFleeDirection();
            
            // 이동 방향 추적 (스프라이트 반전용)
            if (fleeDirection.sqrMagnitude > 0.01f)
            {
                lastMovementDirection = fleeDirection;
            }
            
            rb.linearVelocity = fleeDirection * fleeSpeed;
        }

        private Vector2 CalculateFleeDirection()
        {
            if (PlayerController.Instance != null)
            {
                Vector2 toPlayer = PlayerController.Instance.Position - (Vector2)transform.position;
                return -toPlayer.normalized;
            }

            if (mainCamera != null)
            {
                Vector2 screenPos = mainCamera.WorldToScreenPoint(transform.position);
                Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
                Vector2 toCenter = screenCenter - screenPos;
                return toCenter.normalized;
            }

            return Random.insideUnitCircle.normalized;
        }

        private void MoveTowardsTarget(Vector2 target, float speed)
        {
            Vector2 toTarget = target - (Vector2)transform.position;
            float sqrDistance = toTarget.sqrMagnitude;
            
            if (sqrDistance < 0.01f) // sqrMagnitude 사용으로 성능 개선
            {
                rb.linearVelocity = Vector2.zero;
            }
            else
            {
                Vector2 direction = toTarget.normalized;
                
                // 이동 방향 추적 (스프라이트 반전용)
                if (direction.sqrMagnitude > 0.01f)
                {
                    lastMovementDirection = direction;
                }
                
                Vector2 newVelocity = direction * speed;
                Vector2 newPosition = (Vector2)transform.position + newVelocity * Time.fixedDeltaTime;
                
                // 타일맵 기반 맵 경계 체크
                newPosition = ClampToMapBounds(newPosition);
                
                // 위치 직접 설정 (경계를 벗어나지 않도록)
                rb.MovePosition(newPosition);
            }
        }
        
        /// <summary>
        /// 맵 경계 내로 위치 제한 (타일맵 기반)
        /// </summary>
        private Vector2 ClampToMapBounds(Vector2 position)
        {
            // 타일맵 기반 체크
            if (TilemapRangeManager.Instance != null)
            {
                // 맵 범위 내에 있으면 그대로 반환
                if (TilemapRangeManager.Instance.IsInMapRange(position))
                {
                    return position;
                }
                
                // 맵 범위를 벗어났으면 현재 위치 유지
                return transform.position;
            }
            
            // TilemapRangeManager가 없으면 현재 위치 유지
            return transform.position;
        }

        private void FindNearestFoodTrap()
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius);
            FoodTrap nearestTrap = null;
            float nearestSqrDistance = float.MaxValue;
            Vector2 myPosition = transform.position;

            foreach (var col in colliders)
            {
                if (col == null)
                    continue;

                FoodTrap trap = col.GetComponent<FoodTrap>();
                if (trap != null && !trap.HasCapturedPigeon)
                {
                    float sqrDistance = ((Vector2)col.transform.position - myPosition).sqrMagnitude;
                    if (sqrDistance < nearestSqrDistance)
                    {
                        nearestSqrDistance = sqrDistance;
                        nearestTrap = trap;
                    }
                }
            }

            targetFoodTrap = nearestTrap;
        }

        public float GetEatingRadius()
        {
            return eatingRadius;
        }

        private void SetNewWanderTarget()
        {
            Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * wanderRadius;
            wanderTarget = (Vector2)transform.position + randomOffset;
        }

        /// <summary>
        /// 전시관 비둘기 전용 wander (플레이어 감지, 먹이 탐색 없음)
        /// </summary>
        private void HandleExhibitionWander()
        {
            if (rb == null || controller == null)
                return;

            wanderTimer += Time.deltaTime;
            if (wanderTimer >= wanderInterval)
            {
                SetNewExhibitionWanderTarget();
                wanderTimer = 0f;
            }

            MoveTowardsExhibitionTarget(wanderTarget, wanderSpeed);
        }

        /// <summary>
        /// 전시 영역 내에서만 이동
        /// </summary>
        private void MoveTowardsExhibitionTarget(Vector2 target, float speed)
        {
            if (controller == null || controller.ExhibitionArea == null || rb == null)
                return;

            Vector2 toTarget = target - (Vector2)transform.position;
            float sqrDistance = toTarget.sqrMagnitude;
            
            if (sqrDistance < 0.01f)
            {
                rb.linearVelocity = Vector2.zero;
            }
            else
            {
                Vector2 direction = toTarget.normalized;
                
                // 이동 방향 추적 (스프라이트 반전용)
                if (direction.sqrMagnitude > 0.01f)
                {
                    lastMovementDirection = direction;
                }
                
                Vector2 newVelocity = direction * speed;
                Vector2 newPosition = (Vector2)transform.position + newVelocity * Time.fixedDeltaTime;
                
                // 전시 영역 경계 체크
                newPosition = ClampToExhibitionBounds(newPosition);
                
                rb.MovePosition(newPosition);
            }
        }

        /// <summary>
        /// 전시 영역 경계로 제한
        /// </summary>
        private Vector2 ClampToExhibitionBounds(Vector2 position)
        {
            if (controller == null || controller.ExhibitionArea == null)
                return position;
            
            Bounds bounds = controller.ExhibitionArea.bounds;
            position.x = Mathf.Clamp(position.x, bounds.min.x, bounds.max.x);
            position.y = Mathf.Clamp(position.y, bounds.min.y, bounds.max.y);
            return position;
        }

        /// <summary>
        /// 전시관 영역 내에서만 wander 타겟 설정
        /// </summary>
        private void SetNewExhibitionWanderTarget()
        {
            if (controller == null || controller.ExhibitionArea == null)
            {
                // 전시 영역이 없으면 일반 wander
                SetNewWanderTarget();
                return;
            }

            // 전시 영역 내 랜덤 위치 생성
            Bounds bounds = controller.ExhibitionArea.bounds;
            int attempts = 0;
            Vector2 target;
            do
            {
                target = new Vector2(
                    Random.Range(bounds.min.x, bounds.max.x),
                    Random.Range(bounds.min.y, bounds.max.y)
                );
                attempts++;
            } while (attempts < 10 && !IsPointInCollider(target, controller.ExhibitionArea));

            wanderTarget = target;
        }

        /// <summary>
        /// 점이 콜라이더 안에 있는지 확인
        /// </summary>
        private bool IsPointInCollider(Vector2 point, Collider2D collider)
        {
            if (collider == null)
                return false;

            return collider.OverlapPoint(point);
        }

    }
}
