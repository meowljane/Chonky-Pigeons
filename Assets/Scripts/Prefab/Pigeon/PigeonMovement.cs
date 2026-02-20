using UnityEngine;
using System.Collections.Generic;

namespace PigeonGame.Gameplay
{
    public enum MovementState
    {
        Idle,      
        Walking,   
        Flying     
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
        [SerializeField] private float detectionRadius = 2f; 
        [SerializeField] private float alertWeight = 2.0f;
        [SerializeField] private float warnThreshold = 45f;
        [SerializeField] private float backoffThreshold = 70f;
        [SerializeField] private float fleeThreshold = 100f;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip backoffAudioClip;
        [SerializeField] private AudioClip fleeAudioClip;

        public float DetectionRadius => detectionRadius;
        public float WarnThreshold => warnThreshold;
        public float BackoffThreshold => backoffThreshold;
        public float FleeThreshold => fleeThreshold;
        public float AlertWeight => alertWeight;

        public MovementState CurrentMovementState
        {
            get
            {
                if (ai.CurrentState == PigeonState.Flee)
                    return MovementState.Flying;

                Vector2? currentTarget = GetCurrentTarget();
                if (currentTarget == null)
                    return MovementState.Idle; 

                float sqrDistance = ((Vector2)transform.position - currentTarget.Value).sqrMagnitude;
                const float arrivalThreshold = 0.01f; 

                return sqrDistance < arrivalThreshold ? MovementState.Idle : MovementState.Walking;
            }
        }

        private Vector2? GetCurrentTarget()
        {
            if (backoffTargetSet)
            {
                return backoffTarget;
            }

            if (targetFoodTrap != null && !targetFoodTrap.HasCapturedPigeon)
            {
                return targetFoodTrap.transform.position;
            }

            return wanderTarget;
        }

        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private PigeonAI ai;
        [SerializeField] private PigeonController controller;
        private Vector2 wanderTarget;
        private float wanderTimer;
        private FoodTrap targetFoodTrap;
        private Vector2 backoffTarget;
        private bool backoffTargetSet = false;
        private Camera mainCamera;
        private bool backoffCausedByPlayer = false;
        private float backoffEndTime = 0f; 
        private const float BACKOFF_COOLDOWN = 2f; 
        private Vector2 lastMovementDirection = Vector2.right;
        private float sqrDetectionRadius;
        private bool hasPlayedBackoffAudio = false;
        private bool hasPlayedFleeAudio = false;
        private const float AUDIO_TRIGGER_DISTANCE = 2f;
        private float sqrAudioTriggerDistance;

        public Vector2 MovementDirection => lastMovementDirection;

        private void Awake()
        {
            rb.gravityScale = 0;
            rb.linearDamping = 5f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            mainCamera = Camera.main;
            sqrDetectionRadius = detectionRadius * detectionRadius;
            sqrAudioTriggerDistance = AUDIO_TRIGGER_DISTANCE * AUDIO_TRIGGER_DISTANCE;
            
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
            
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
        }

        private void Start()
        {
            SetNewWanderTarget();
        }

        private void Update()
        {
            if (controller.IsExhibitionPigeon)
            {
                HandleExhibitionWander();
                return;
            }

            if (ai.CurrentState == PigeonState.Flee)
            {
                HandleFlee();
                // 플레이어가 멀어지면 오디오 플래그 리셋
                if (!IsPlayerWithinAudioRange())
                {
                    hasPlayedFleeAudio = false;
                }
                return;
            }
            
            // Flee 상태가 아니면 flee 오디오 플래그 리셋
            hasPlayedFleeAudio = false;

            UpdateAlertSystem();

            PigeonState state = ai.CurrentState;

            // 플레이어가 가까이 있으면 항상 우선 체크 (backoff 중이어도)
            if (IsPlayerNearby())
            {
                backoffCausedByPlayer = true;
                HandleBackOff();
                return;
            }
            
            // 플레이어가 멀어지면 오디오 플래그 리셋
            if (!IsPlayerWithinAudioRange())
            {
                hasPlayedBackoffAudio = false;
            }

            if (backoffTargetSet)
            {
                float sqrDistanceToTarget = ((Vector2)transform.position - backoffTarget).sqrMagnitude;
                if (sqrDistanceToTarget >= 0.04f) 
                {
                    HandleBackOff();
                    return;
                }
                backoffTargetSet = false;
                backoffCausedByPlayer = false; 
                targetFoodTrap = null; 
                backoffEndTime = Time.time;
                hasPlayedBackoffAudio = false; // backoff 종료 시 플래그 리셋
            }

            if (state == PigeonState.BackOff)
                HandleBackOff();
            else
            {
                // BackOff 상태가 아니면 오디오 플래그 리셋
                hasPlayedBackoffAudio = false;
                HandleNormalMovement();
            }
        }

        private void UpdateAlertSystem()
        {
            if (ai.CurrentState == PigeonState.Flee || PlayerController.Instance == null)
                return;

            Vector2 toPlayer = PlayerController.Instance.Position - (Vector2)transform.position;
            float sqrDistance = toPlayer.sqrMagnitude;

            if (sqrDistance <= sqrDetectionRadius)
            {
                float distanceFactor = Mathf.Clamp01(1f - (sqrDistance / sqrDetectionRadius));
                ai.AddPlayerAlert(Time.deltaTime * distanceFactor);
            }
        }

        private bool IsPlayerNearby()
        {
            if (PlayerController.Instance == null)
                return false;

            float sqrDistance = ((Vector2)transform.position - PlayerController.Instance.Position).sqrMagnitude;
            return sqrDistance <= sqrDetectionRadius;
        }
        
        private bool IsPlayerWithinAudioRange()
        {
            if (PlayerController.Instance == null)
                return false;

            float sqrDistance = ((Vector2)transform.position - PlayerController.Instance.Position).sqrMagnitude;
            return sqrDistance <= sqrAudioTriggerDistance;
        }

        private void HandleNormalMovement()
        {
            if (Time.time - backoffEndTime >= BACKOFF_COOLDOWN)
                FindNearestFoodTrap();
            else
                targetFoodTrap = null;

            wanderTimer += Time.deltaTime;
            if (wanderTimer >= wanderInterval)
            {
                SetNewWanderTarget();
                wanderTimer = 0f;
            }

            Vector2 targetPos = targetFoodTrap != null && !targetFoodTrap.HasCapturedPigeon
                ? (Vector2)targetFoodTrap.transform.position
                : wanderTarget;

            MoveTowardsTarget(targetPos, wanderSpeed);
        }

        private void HandleBackOff()
        {
            // 플레이어가 2f 이내에 있고 아직 오디오를 재생하지 않았다면 재생
            if (IsPlayerWithinAudioRange() && !hasPlayedBackoffAudio && backoffAudioClip != null && audioSource != null)
            {
                audioSource.PlayOneShot(backoffAudioClip);
                hasPlayedBackoffAudio = true;
            }
            
            float backoffDistance = backoffCausedByPlayer ? detectionRadius : detectionRadius * 2f;
            float sqrDistanceToTarget = ((Vector2)transform.position - backoffTarget).sqrMagnitude;

            if (!backoffTargetSet || sqrDistanceToTarget < 0.04f)
            {
                Vector2 backoffDirection = CalculateBackoffDirection();
                backoffTarget = (Vector2)transform.position + backoffDirection * backoffDistance;
                backoffTarget = ClampToMapBounds(backoffTarget);
                backoffTargetSet = true;
            }

            MoveTowardsTarget(backoffTarget, backoffSpeed);
        }

        private Vector2 CalculateBackoffDirection()
        {
            if (backoffCausedByPlayer && PlayerController.Instance != null)
            {
                Vector2 toPlayer = PlayerController.Instance.Position - (Vector2)transform.position;
                if (toPlayer.sqrMagnitude > 0.01f)
                    return -toPlayer.normalized;
            }
            return Random.insideUnitCircle.normalized;
        }

        private void HandleFlee()
        {
            // 플레이어가 2f 이내에 있고 아직 오디오를 재생하지 않았다면 재생
            if (IsPlayerWithinAudioRange() && !hasPlayedFleeAudio && fleeAudioClip != null && audioSource != null)
            {
                audioSource.PlayOneShot(fleeAudioClip);
                hasPlayedFleeAudio = true;
            }
            
            if (mainCamera == null)
                mainCamera = Camera.main;

            Vector2 fleeDirection = CalculateFleeDirection();
            if (fleeDirection.sqrMagnitude > 0.01f)
                lastMovementDirection = fleeDirection;

            rb.linearVelocity = fleeDirection * fleeSpeed;
        }

        private Vector2 CalculateFleeDirection()
        {
            if (PlayerController.Instance != null)
            {
                Vector2 toPlayer = PlayerController.Instance.Position - (Vector2)transform.position;
                return toPlayer.sqrMagnitude > 0.01f ? -toPlayer.normalized : Random.insideUnitCircle.normalized;
            }

            if (mainCamera != null)
            {
                Vector2 screenPos = mainCamera.WorldToScreenPoint(transform.position);
                Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
                Vector2 toCenter = screenCenter - screenPos;
                return toCenter.sqrMagnitude > 0.01f ? toCenter.normalized : Random.insideUnitCircle.normalized;
            }

            return Random.insideUnitCircle.normalized;
        }

        private void MoveTowardsTarget(Vector2 target, float speed, System.Func<Vector2, Vector2> clampFunc = null)
        {
            Vector2 toTarget = target - (Vector2)transform.position;
            float sqrDistance = toTarget.sqrMagnitude;

            if (sqrDistance < 0.01f)
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }

            Vector2 direction = toTarget.normalized;
            if (direction.sqrMagnitude > 0.01f)
                lastMovementDirection = direction;

            Vector2 newVelocity = direction * speed;
            Vector2 newPosition = (Vector2)transform.position + newVelocity * Time.fixedDeltaTime;
            newPosition = (clampFunc ?? ClampToMapBounds)(newPosition);
            rb.MovePosition(newPosition);
        }

        private Vector2 ClampToMapBounds(Vector2 position)
        {
            if (TilemapRangeManager.Instance?.IsInMapRange(position) == true)
                return position;
            return transform.position;
        }

        private Dictionary<Collider2D, FoodTrap> trapComponentCache = new Dictionary<Collider2D, FoodTrap>();

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

                if (!trapComponentCache.TryGetValue(col, out FoodTrap trap))
                {
                    trap = col.GetComponent<FoodTrap>();
                    if (trap != null)
                        trapComponentCache[col] = trap;
                }

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

        public float GetEatingRadius() => eatingRadius;

        private void SetNewWanderTarget()
        {
            Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * wanderRadius;
            wanderTarget = (Vector2)transform.position + randomOffset;
        }

        private void HandleExhibitionWander()
        {
            wanderTimer += Time.deltaTime;
            if (wanderTimer >= wanderInterval)
            {
                SetNewExhibitionWanderTarget();
                wanderTimer = 0f;
            }

            MoveTowardsExhibitionTarget(wanderTarget, wanderSpeed);
        }

        private void MoveTowardsExhibitionTarget(Vector2 target, float speed)
        {
            MoveTowardsTarget(target, speed, ClampToExhibitionBounds);
        }

        private Vector2 ClampToExhibitionBounds(Vector2 position)
        {
            if (TilemapRangeManager.Instance?.IsInExhibitionArea(position) == false)
                return transform.position;
            return position;
        }

        private void SetNewExhibitionWanderTarget()
        {
            if (!controller.IsExhibitionPigeon)
            {
                SetNewWanderTarget();
                return;
            }

            Vector3 randomPos = TilemapRangeManager.Instance?.GetRandomPositionInExhibitionArea() ?? Vector3.zero;
            wanderTarget = randomPos != Vector3.zero ? randomPos : transform.position;
        }

    }
}
