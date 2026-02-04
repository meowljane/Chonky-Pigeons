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

        public float DetectionRadius => detectionRadius;
        public float WarnThreshold => warnThreshold;
        public float BackoffThreshold => backoffThreshold;
        public float FleeThreshold => fleeThreshold;
        public float AlertWeight => alertWeight;

        public MovementState CurrentMovementState
        {
            get
            {
                if (rb == null)
                    return MovementState.Idle;

                if (ai != null && ai.CurrentState == PigeonState.Flee)
                {
                    return MovementState.Flying;
                }

                Vector2? currentTarget = GetCurrentTarget();
                if (currentTarget == null)
                {
                    return MovementState.Idle; 
                }

                float sqrDistance = ((Vector2)transform.position - currentTarget.Value).sqrMagnitude;
                const float arrivalThreshold = 0.01f; 

                if (sqrDistance < arrivalThreshold)
                {
                    return MovementState.Idle; 
                }
                else
                {
                    return MovementState.Walking; 
                }
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

        private Rigidbody2D rb;
        private PigeonAI ai;
        private PigeonController controller;
        private Vector2 wanderTarget;
        private float wanderTimer;
        private FoodTrap targetFoodTrap;
        private Vector2 backoffTarget;
        private bool backoffTargetSet = false;
        private Vector2 backoffStartPosition; 
        private Camera mainCamera;
        private bool backoffCausedByPlayer = false;
        private float backoffEndTime = 0f; 
        private const float BACKOFF_COOLDOWN = 2f; 
        private Vector2 lastMovementDirection = Vector2.right; 

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

            if (controller.IsExhibitionPigeon)
            {
                HandleExhibitionWander();
                return;
            }

            if (ai.CurrentState == PigeonState.Flee)
            {
                HandleFlee();
                return;
            }

            UpdateAlertSystem();

            PigeonState state = ai.CurrentState;

            if (backoffTargetSet)
            {
                float sqrDistanceToTarget = ((Vector2)transform.position - backoffTarget).sqrMagnitude;
                if (sqrDistanceToTarget >= 0.04f) 
                {
                    HandleBackOff();
                    return;
                }
                else
                {
                    backoffTargetSet = false;
                    backoffCausedByPlayer = false; 
                    targetFoodTrap = null; 
                    backoffEndTime = Time.time; 
                }
            }

            if (IsPlayerNearby())
            {
                backoffCausedByPlayer = true;
                HandleBackOff();
                return;
            }

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

            if (ai.CurrentState == PigeonState.Flee)
                return;

            if (PlayerController.Instance != null)
            {
                Vector2 toPlayer = PlayerController.Instance.Position - (Vector2)transform.position;
                float sqrDistance = toPlayer.sqrMagnitude;
                float sqrRadius = detectionRadius * detectionRadius;

                if (sqrDistance <= sqrRadius)
                {
                    float distanceFactor = Mathf.Clamp01(1f - (sqrDistance / sqrRadius));
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
            if (Time.time - backoffEndTime >= BACKOFF_COOLDOWN)
            {
                FindNearestFoodTrap();
            }
            else
            {
                targetFoodTrap = null; 
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

            float backoffDistance = backoffCausedByPlayer ? detectionRadius : detectionRadius * 2f;

            if (!backoffTargetSet)
            {
                backoffStartPosition = transform.position;
                Vector2 backoffDirection = CalculateBackoffDirection();
                backoffTarget = backoffStartPosition + backoffDirection * backoffDistance;
                backoffTarget = ClampToMapBounds(backoffTarget);
                backoffTargetSet = true;
            }

            float sqrDistanceToTarget = ((Vector2)transform.position - backoffTarget).sqrMagnitude;
            if (sqrDistanceToTarget < 0.04f) 
            {
                Vector2 backoffDirection = CalculateBackoffDirection();
                backoffTarget = (Vector2)transform.position + backoffDirection * backoffDistance;
                backoffTarget = ClampToMapBounds(backoffTarget);
            }

            MoveTowardsTarget(backoffTarget, backoffSpeed);
        }

        private Vector2 CalculateBackoffDirection()
        {
            if (backoffCausedByPlayer && PlayerController.Instance != null)
            {
                Vector2 toPlayer = PlayerController.Instance.Position - (Vector2)transform.position;
                if (toPlayer.sqrMagnitude > 0.01f) 
                {
                    return -toPlayer.normalized;
                }
            }

            return Random.insideUnitCircle.normalized;
        }

        private void HandleFlee()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;

            Vector2 fleeDirection = CalculateFleeDirection();

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

            if (sqrDistance < 0.01f) 
            {
                rb.linearVelocity = Vector2.zero;
            }
            else
            {
                Vector2 direction = toTarget.normalized;

                if (direction.sqrMagnitude > 0.01f)
                {
                    lastMovementDirection = direction;
                }

                Vector2 newVelocity = direction * speed;
                Vector2 newPosition = (Vector2)transform.position + newVelocity * Time.fixedDeltaTime;

                newPosition = ClampToMapBounds(newPosition);

                rb.MovePosition(newPosition);
            }
        }

        private Vector2 ClampToMapBounds(Vector2 position)
        {
            if (TilemapRangeManager.Instance != null)
            {
                if (TilemapRangeManager.Instance.IsInMapRange(position))
                {
                    return position;
                }

                return transform.position;
            }

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

        public float GetEatingRadius()
        {
            return eatingRadius;
        }

        private void SetNewWanderTarget()
        {
            Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * wanderRadius;
            wanderTarget = (Vector2)transform.position + randomOffset;
        }

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

        private void MoveTowardsExhibitionTarget(Vector2 target, float speed)
        {
            if (controller == null || !controller.IsExhibitionPigeon || rb == null)
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

                if (direction.sqrMagnitude > 0.01f)
                {
                    lastMovementDirection = direction;
                }

                Vector2 newVelocity = direction * speed;
                Vector2 newPosition = (Vector2)transform.position + newVelocity * Time.fixedDeltaTime;

                newPosition = ClampToExhibitionBounds(newPosition);

                rb.MovePosition(newPosition);
            }
        }

        private Vector2 ClampToExhibitionBounds(Vector2 position)
        {
            if (controller == null || !controller.IsExhibitionPigeon)
                return position;

            if (TilemapRangeManager.Instance != null)
            {
                if (!TilemapRangeManager.Instance.IsInExhibitionArea(position))
                {
                    return transform.position;
                }
            }

            return position;
        }

        private void SetNewExhibitionWanderTarget()
        {
            if (controller == null || !controller.IsExhibitionPigeon)
            {
                SetNewWanderTarget();
                return;
            }

            if (TilemapRangeManager.Instance != null)
            {
                Vector3 randomPos = TilemapRangeManager.Instance.GetRandomPositionInExhibitionArea();
                if (randomPos != Vector3.zero)
                {
                    wanderTarget = randomPos;
                    return;
                }
            }

            wanderTarget = transform.position;
        }

        private bool IsPointInCollider(Vector2 point, Collider2D collider)
        {
            if (collider == null)
                return false;

            return collider.OverlapPoint(point);
        }

    }
}
