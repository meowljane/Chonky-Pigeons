using UnityEngine;
using UnityEngine.InputSystem;
using PigeonGame.UI;

namespace PigeonGame.Gameplay
{
    public enum PlayerDirection
    {
        Up = 0,
        Down = 1,
        Left = 2,
        Right = 3
    }

    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Animator))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private MobileJoystick mobileJoystick;
        
        private Rigidbody2D rb;
        private Animator animator;
        private Vector2 moveInput;
        private PlayerDirection lastDirection = PlayerDirection.Down;
        private bool isMoving = false;

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
                Destroy(gameObject);
            }

            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.linearDamping = 10f;
            
            animator = GetComponent<Animator>();
        }

        private void Start()
        {
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
            moveInput = ReadMoveInput();
            UpdateAnimation();
        }

        private Vector2 ReadMoveInput()
        {
            Vector2 input = Vector2.zero;

            if (mobileJoystick != null && mobileJoystick.IsActive)
            {
                input += mobileJoystick.InputVector;
            }

            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
                    input.y += 1f;
                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
                    input.y -= 1f;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
                    input.x += 1f;
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
                    input.x -= 1f;
            }

            if (input.sqrMagnitude <= 0f)
                return Vector2.zero;

            return input.normalized;
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

        private void UpdateAnimation()
        {
            if (animator == null || animator.runtimeAnimatorController == null)
                return;

            isMoving = moveInput.sqrMagnitude > 0f;

            if (isMoving)
            {
                PlayerDirection currentDirection = DetermineDirection(moveInput);
                if (currentDirection != lastDirection)
                {
                    lastDirection = currentDirection;
                }
            }
            
            string stateName = GetStateName(lastDirection, isMoving);
            
            try
            {
                animator.Play(stateName);
            }
            catch (System.Exception)
            {
            }
        }

        private string GetStateName(PlayerDirection direction, bool moving)
        {
            string prefix = moving ? "Walk" : "Idle";
            string directionName;
            
            switch (direction)
            {
                case PlayerDirection.Up:
                    directionName = "Up";
                    break;
                case PlayerDirection.Down:
                    directionName = "Down";
                    break;
                case PlayerDirection.Left:
                    directionName = "Left";
                    break;
                case PlayerDirection.Right:
                    directionName = "Right";
                    break;
                default:
                    directionName = "Down";
                    break;
            }
            
            return prefix + directionName;
        }

        private PlayerDirection DetermineDirection(Vector2 input)
        {
            if (Mathf.Abs(input.y) > Mathf.Abs(input.x))
            {
                return input.y > 0 ? PlayerDirection.Up : PlayerDirection.Down;
            }
            else
            {
                return input.x > 0 ? PlayerDirection.Right : PlayerDirection.Left;
            }
        }
    }
}
