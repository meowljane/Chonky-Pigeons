using UnityEngine;
using UnityEngine.InputSystem;
using PigeonGame.UI;

namespace PigeonGame.Gameplay
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private MobileJoystick mobileJoystick; // 모바일 조이스틱 참조
        private Rigidbody2D rb;
        private Vector2 moveInput;

        public static PlayerController Instance { get; private set; }
        public Vector2 Position => (Vector2)transform.position;
        public string CurrentMapName => TilemapRangeManager.Instance?.GetMapNameAtPosition(transform.position) ?? "Unknown";

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.linearDamping = 10f;
        }

        private void Start()
        {
            // 조이스틱 자동 찾기 (씬에 하나만 있다고 가정)
            if (mobileJoystick == null)
            {
                mobileJoystick = FindFirstObjectByType<MobileJoystick>();
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Update()
        {
            moveInput = Vector2.zero;
            
            // 모바일 조이스틱 입력 (우선순위)
            if (mobileJoystick != null && mobileJoystick.IsActive)
            {
                moveInput = mobileJoystick.InputVector;
            }
            else
            {
                // 키보드 입력 (조이스틱이 없을 때)
                Keyboard keyboard = Keyboard.current;
                if (keyboard != null)
                {
                    if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
                        moveInput.y += 1f;
                    if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
                        moveInput.y -= 1f;
                    if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
                        moveInput.x += 1f;
                    if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
                        moveInput.x -= 1f;
                    
                    moveInput.Normalize();
                }
            }
        }

        private void FixedUpdate()
        {
            // 이동
            Vector2 newVelocity = moveInput * moveSpeed;
            Vector2 newPosition = (Vector2)transform.position + newVelocity * Time.fixedDeltaTime;
            
            // 타일맵 기반 이동 범위 체크
            if (TilemapRangeManager.Instance != null)
            {
                // 플레이어 이동 가능 범위 내에 있는지 확인
                if (!TilemapRangeManager.Instance.IsInPlayerMovementRange(newPosition))
                {
                    // 이동 불가능한 위치면 현재 위치 유지
                    newPosition = transform.position;
                }
            }
            
            // 위치 직접 설정
            rb.MovePosition(newPosition);
        }
    }
}
