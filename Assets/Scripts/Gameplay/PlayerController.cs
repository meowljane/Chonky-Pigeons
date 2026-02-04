using UnityEngine;
using UnityEngine.InputSystem;
using PigeonGame.UI;

namespace PigeonGame.Gameplay
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private MobileJoystick mobileJoystick; 
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
            if (mobileJoystick == null)
                Debug.LogWarning("MobileJoystick이 할당되지 않았습니다. 키보드 입력만 사용됩니다.", this);
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

            if (mobileJoystick != null && mobileJoystick.IsActive)
            {
                moveInput = mobileJoystick.InputVector;
            }
            else
            {
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
            Vector2 newVelocity = moveInput * moveSpeed;
            Vector2 newPosition = (Vector2)transform.position + newVelocity * Time.fixedDeltaTime;

            if (TilemapRangeManager.Instance != null)
            {
                if (!TilemapRangeManager.Instance.IsInPlayerMovementRange(newPosition))
                {
                    newPosition = transform.position;
                }
            }

            rb.MovePosition(newPosition);
        }
    }
}
