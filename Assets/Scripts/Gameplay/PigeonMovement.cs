using UnityEngine;

namespace PigeonGame.Gameplay
{
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
        [SerializeField] private bool showDebugGizmos = true;
        
        public float WarnThreshold => warnThreshold;
        public float BackoffThreshold => backoffThreshold;
        public float FleeThreshold => fleeThreshold;
        public float AlertWeight => alertWeight;
        
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

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                Debug.LogError("PigeonMovement: Rigidbody2D가 없습니다!");
                return;
            }
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
                float distanceToTarget = Vector2.Distance(transform.position, backoffTarget);
                if (distanceToTarget >= 0.2f)
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
                float distance = Vector2.Distance(transform.position, PlayerController.Instance.Position);
                if (distance <= detectionRadius)
                {
                    float distanceFactor = Mathf.Clamp01(1f - (distance / detectionRadius));
                    ai.AddPlayerAlert(Time.deltaTime * distanceFactor);
                }
            }
        }

        private bool IsPlayerNearby()
        {
            if (PlayerController.Instance == null)
                return false;

            float distance = Vector2.Distance(transform.position, PlayerController.Instance.Position);
            return distance <= detectionRadius;
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

            Vector2 targetPos = targetFoodTrap != null && !targetFoodTrap.IsDepleted
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
                backoffTargetSet = true;
            }

            // 목표에 도달했는지 확인 (더 큰 거리로 판단)
            float distanceToTarget = Vector2.Distance(transform.position, backoffTarget);
            if (distanceToTarget < 0.2f)
            {
                // 목표에 도달했으면 현재 위치에서 더 멀리 떨어진 새로운 목표 설정
                Vector2 backoffDirection = CalculateBackoffDirection();
                backoffTarget = (Vector2)transform.position + backoffDirection * backoffDistance;
            }

            MoveTowardsTarget(backoffTarget, backoffSpeed);
        }

        private Vector2 CalculateBackoffDirection()
        {
            if (backoffCausedByPlayer && PlayerController.Instance != null)
            {
                Vector2 toPlayer = PlayerController.Instance.Position - (Vector2)transform.position;
                if (toPlayer.magnitude > 0.1f)
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

            Vector2 fleeDirection = CalculateFleeDirection();
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
            Vector2 direction = (target - (Vector2)transform.position).normalized;
            float distance = Vector2.Distance(transform.position, target);
            
            if (distance < 0.1f)
            {
                rb.linearVelocity = Vector2.zero;
            }
            else
            {
                rb.linearVelocity = direction * speed;
            }
        }

        private void FindNearestFoodTrap()
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius);
            FoodTrap nearestTrap = null;
            float nearestDistance = float.MaxValue;

            foreach (var col in colliders)
            {
                if (col == null)
                    continue;

                FoodTrap trap = col.GetComponent<FoodTrap>();
                if (trap != null && !trap.IsDepleted)
                {
                    float distance = Vector2.Distance(transform.position, col.transform.position);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
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

        private void OnDrawGizmosSelected()
        {
            if (!showDebugGizmos)
                return;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, eatingRadius);

            if (ai != null)
            {
                float alert = ai.Alert;
                PigeonState state = ai.CurrentState;
                
                Color stateColor = state switch
                {
                    PigeonState.Normal => Color.green,
                    PigeonState.Cautious => Color.yellow,
                    PigeonState.BackOff => Color.magenta,
                    PigeonState.Flee => Color.red,
                    _ => Color.white
                };

                Gizmos.color = stateColor;
                Gizmos.DrawWireSphere(transform.position, 0.3f);

                #if UNITY_EDITOR
                UnityEditor.Handles.Label(
                    transform.position + Vector3.up * 0.5f,
                    $"Alert: {alert:F1}\nState: {state}"
                );
                #endif
            }
        }
    }
}
