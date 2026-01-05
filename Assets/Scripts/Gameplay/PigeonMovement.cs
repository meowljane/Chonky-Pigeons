using UnityEngine;

namespace PigeonGame.Gameplay
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PigeonMovement : MonoBehaviour
    {
        [SerializeField] private float wanderSpeed = 2f;
        [SerializeField] private float wanderRadius = 1f;
        [SerializeField] private float wanderInterval = 2f;
        [SerializeField] private float foodDetectionRadius = 5f;
        
        private Rigidbody2D rb;
        private Vector2 wanderTarget;
        private float wanderTimer;
        private FoodTrap targetFoodTrap;

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
        }

        private void Start()
        {
            SetNewWanderTarget();
        }

        private void Update()
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
    }
}

