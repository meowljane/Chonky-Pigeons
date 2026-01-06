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

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogWarning("Multiple PlayerController instances detected!");
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
                mobileJoystick = FindObjectOfType<MobileJoystick>();
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
                // 테스트용 WASD 입력 (조이스틱이 없을 때만)
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
            rb.linearVelocity = moveInput * moveSpeed;
        }
    }
}

