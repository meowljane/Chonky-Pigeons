using UnityEngine;

namespace PigeonGame.Gameplay
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PigeonMovement : MonoBehaviour
    {
        [SerializeField] private float wanderSpeed = 2f;
        [SerializeField] private float backoffSpeed = 3f;
        [SerializeField] private float fleeSpeed = 4f;
        [SerializeField] private float wanderRadius = 1f;
        [SerializeField] private float wanderInterval = 2f;
        [SerializeField] private float foodDetectionRadius = 5f;
        [SerializeField] private float playerDetectionRadius = 10f;
        [SerializeField] private float crowdDetectionRadius = 2f;
        [SerializeField] private bool showDebugGizmos = true;
        
        private Rigidbody2D rb;
        private PigeonAI ai;
        private PigeonController controller;
        private Vector2 wanderTarget;
        private float wanderTimer;
        private FoodTrap targetFoodTrap;
        private float backoffTimer = 0f;
        private Vector2 backoffTarget;
        private bool backoffTargetSet = false;
        private Camera mainCamera;

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
            // 회전 고정 (2D 게임이므로 스프라이트가 회전하지 않도록)
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

            // Alert 시스템 업데이트
            UpdateAlertSystem();

            // 상태에 따른 행동 처리
            PigeonState currentState = ai.CurrentState;
            
            if (currentState == PigeonState.Flee)
            {
                HandleFlee();
            }
            else if (currentState == PigeonState.BackOff)
            {
                HandleBackOff();
            }
            else
            {
                HandleNormalMovement();
                // Normal 상태로 돌아오면 BackOff 관련 변수 리셋
                backoffTargetSet = false;
                backoffTimer = 0f;
            }
        }

        private void UpdateAlertSystem()
        {
            float deltaTime = Time.deltaTime;

            // 플레이어 접근 감지 및 Alert 증가
            if (PlayerController.Instance != null)
            {
                float distanceToPlayer = Vector2.Distance(transform.position, PlayerController.Instance.Position);
                if (distanceToPlayer <= playerDetectionRadius)
                {
                    // 거리에 반비례하여 Alert 증가 (가까울수록 더 많이 증가)
                    float distanceFactor = 1f - (distanceToPlayer / playerDetectionRadius);
                    ai.AddPlayerAlert(deltaTime * distanceFactor);
                }
            }

            // 군집 밀도 계산 및 Alert 증가
            int neighborCount = CountNearbyPigeons();
            if (neighborCount > 0)
            {
                ai.AddCrowdAlert(neighborCount, deltaTime);
            }
        }

        private int CountNearbyPigeons()
        {
            if (controller == null || controller.Stats == null)
                return 0;

            float detectionRadius = controller.Stats.personalSpaceRadius;
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius);
            int count = 0;

            foreach (var col in colliders)
            {
                PigeonAI otherPigeon = col.GetComponent<PigeonAI>();
                if (otherPigeon != null && otherPigeon != ai)
                {
                    count++;
                }
            }

            return count;
        }

        private void HandleNormalMovement()
        {
            // 덫 찾기
            FindNearestFoodTrap();

            wanderTimer += Time.deltaTime;
            
            if (wanderTimer >= wanderInterval)
            {
                SetNewWanderTarget();
                wanderTimer = 0f;
            }

            // 목표 지점으로 이동
            if (rb == null) return;

            Vector2 targetPos = targetFoodTrap != null && !targetFoodTrap.IsDepleted
                ? (Vector2)targetFoodTrap.transform.position
                : wanderTarget;
            
            Vector2 direction = (targetPos - (Vector2)transform.position).normalized;
            
            // 거리가 매우 가까우면 멈춤
            float distance = Vector2.Distance(transform.position, targetPos);
            if (distance < 0.1f)
            {
                rb.linearVelocity = Vector2.zero;
            }
            else
            {
                rb.linearVelocity = direction * wanderSpeed;
            }
        }

        private void HandleBackOff()
        {
            if (controller == null || controller.Stats == null)
                return;

            backoffTimer += Time.deltaTime;

            // BackOff 목표 설정 (덫에서 멀어지는 방향)
            if (!backoffTargetSet || backoffTimer >= controller.Stats.backoffDuration)
            {
                Vector2 backoffDirection = Vector2.zero;
                
                if (targetFoodTrap != null)
                {
                    // 덫에서 멀어지는 방향
                    Vector2 toTrap = (Vector2)targetFoodTrap.transform.position - (Vector2)transform.position;
                    if (toTrap.magnitude > 0.1f)
                    {
                        backoffDirection = -toTrap.normalized;
                    }
                    else
                    {
                        backoffDirection = Random.insideUnitCircle.normalized;
                    }
                }
                else
                {
                    // 플레이어에서 멀어지는 방향
                    if (PlayerController.Instance != null)
                    {
                        Vector2 toPlayer = PlayerController.Instance.Position - (Vector2)transform.position;
                        if (toPlayer.magnitude > 0.1f)
                        {
                            backoffDirection = -toPlayer.normalized;
                        }
                        else
                        {
                            backoffDirection = Random.insideUnitCircle.normalized;
                        }
                    }
                    else
                    {
                        // 랜덤 방향
                        backoffDirection = Random.insideUnitCircle.normalized;
                    }
                }

                backoffTarget = (Vector2)transform.position + backoffDirection * controller.Stats.backoffDistance;
                backoffTimer = 0f;
                backoffTargetSet = true;
            }

            // BackOff 목표로 이동
            Vector2 direction = (backoffTarget - (Vector2)transform.position).normalized;
            float distance = Vector2.Distance(transform.position, backoffTarget);
            
            if (distance < 0.1f)
            {
                rb.linearVelocity = Vector2.zero;
            }
            else
            {
                rb.linearVelocity = direction * backoffSpeed;
            }
        }

        private void HandleFlee()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;

            Vector2 fleeDirection = Vector2.zero;

            // 플레이어에서 멀어지는 방향
            if (PlayerController.Instance != null)
            {
                Vector2 toPlayer = PlayerController.Instance.Position - (Vector2)transform.position;
                fleeDirection = -toPlayer.normalized;
            }
            else
            {
                // 화면 밖으로 도망 (카메라 중심에서 멀어지는 방향)
                if (mainCamera != null)
                {
                    Vector2 screenPos = mainCamera.WorldToScreenPoint(transform.position);
                    Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
                    Vector2 toCenter = screenCenter - screenPos;
                    fleeDirection = toCenter.normalized;
                }
                else
                {
                    fleeDirection = Random.insideUnitCircle.normalized;
                }
            }

            rb.linearVelocity = fleeDirection * fleeSpeed;

            // 화면 밖으로 나가면 제거 (선택사항)
            if (mainCamera != null)
            {
                Vector2 viewportPos = mainCamera.WorldToViewportPoint(transform.position);
                if (viewportPos.x < -0.1f || viewportPos.x > 1.1f || viewportPos.y < -0.1f || viewportPos.y > 1.1f)
                {
                    // 화면 밖으로 나감 - 제거하거나 비활성화
                    // Destroy(gameObject);
                }
            }
        }

        private void FindNearestFoodTrap()
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, foodDetectionRadius);
            FoodTrap nearestTrap = null;
            float nearestDistance = float.MaxValue;

            foreach (var col in colliders)
            {
                FoodTrap trap = col.GetComponent<FoodTrap>();
                if (trap != null && !trap.IsDepleted)
                {
                    float distance = Vector2.Distance(transform.position, trap.transform.position);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestTrap = trap;
                    }
                }
            }

            targetFoodTrap = nearestTrap;
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

            // 먹이 감지 반경
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, foodDetectionRadius);

            // 플레이어 감지 반경
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, playerDetectionRadius);

            // 군집 감지 반경
            if (controller != null && controller.Stats != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(transform.position, controller.Stats.personalSpaceRadius);
            }

            // Alert 상태 시각화
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

                // Alert 수치를 텍스트로 표시 (디버그용)
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

