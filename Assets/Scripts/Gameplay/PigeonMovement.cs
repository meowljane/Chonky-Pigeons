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
        [SerializeField] private float foodDetectionRadius = 5f; // 덫을 탐색하는 반경
        [SerializeField] private float eatingRadius = 2f; // 실제로 먹을 수 있는 범위 반경
        [SerializeField] private float playerDetectionRadius = 10f;
        [SerializeField] private float crowdDetectionRadius = 2f;
        [SerializeField] private float randomFlyAwayChance = 0.01f; // 초당 화면 밖으로 나갈 확률
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

            // 플레이어가 가까이 있으면 무조건 뒷걸음질 (alert와 관계없이)
            if (PlayerController.Instance != null)
            {
                float distanceToPlayer = Vector2.Distance(transform.position, PlayerController.Instance.Position);
                if (distanceToPlayer <= playerDetectionRadius)
                {
                    HandleBackOff();
                    return; // 플레이어가 가까이 있으면 다른 행동 무시
                }
            }

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
            // 랜덤하게 화면 밖으로 날아가기
            if (Random.value < randomFlyAwayChance * Time.deltaTime)
            {
                FlyAwayFromScreen();
                return;
            }

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
            
            MoveTowardsTarget(targetPos, wanderSpeed);
        }

        /// <summary>
        /// 화면 밖으로 날아가기
        /// </summary>
        private void FlyAwayFromScreen()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;

            if (mainCamera == null)
                return;

            // 화면 밖 방향 계산
            Vector2 screenPos = mainCamera.WorldToViewportPoint(transform.position);
            Vector2 screenCenter = new Vector2(0.5f, 0.5f);
            Vector2 awayFromCenter = ((Vector2)transform.position - (Vector2)mainCamera.transform.position).normalized;

            // 랜덤한 화면 밖 방향
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            Vector2 randomDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            // 화면 밖으로 이동
            rb.linearVelocity = randomDirection * fleeSpeed;
        }

        private void HandleBackOff()
        {
            if (controller == null || controller.Stats == null)
                return;

            backoffTimer += Time.deltaTime;

            // BackOff 목표 설정 (덫에서 멀어지는 방향)
            if (!backoffTargetSet || backoffTimer >= controller.Stats.backoffDuration)
            {
                Vector2 backoffDirection = CalculateBackoffDirection();
                backoffTarget = (Vector2)transform.position + backoffDirection * controller.Stats.backoffDistance;
                backoffTimer = 0f;
                backoffTargetSet = true;
            }

            // BackOff 목표로 이동
            MoveTowardsTarget(backoffTarget, backoffSpeed);
        }

        private Vector2 CalculateBackoffDirection()
        {
            if (targetFoodTrap != null)
            {
                Vector2 toTrap = (Vector2)targetFoodTrap.transform.position - (Vector2)transform.position;
                return toTrap.magnitude > 0.1f ? -toTrap.normalized : Random.insideUnitCircle.normalized;
            }

            if (PlayerController.Instance != null)
            {
                Vector2 toPlayer = PlayerController.Instance.Position - (Vector2)transform.position;
                return toPlayer.magnitude > 0.1f ? -toPlayer.normalized : Random.insideUnitCircle.normalized;
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

        private void FindNearestFoodTrap()
        {
            // 모든 덫을 확인하여 foodDetectionRadius 내에 있는 덫 찾기
            FoodTrap[] allTraps = FindObjectsOfType<FoodTrap>();
            FoodTrap nearestTrap = null;
            float nearestDistance = float.MaxValue;

            foreach (var trap in allTraps)
            {
                if (trap == null || trap.IsDepleted)
                    continue;

                float distance = Vector2.Distance(transform.position, trap.transform.position);
                // 비둘기의 foodDetectionRadius 내에 있는지 확인
                if (distance <= foodDetectionRadius && distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestTrap = trap;
                }
            }

            targetFoodTrap = nearestTrap;
        }

        /// <summary>
        /// 비둘기의 먹기 반경 가져오기 (FoodTrap에서 사용)
        /// </summary>
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

            // 먹이 탐색 반경
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, foodDetectionRadius);

            // 먹기 반경
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, eatingRadius);

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

