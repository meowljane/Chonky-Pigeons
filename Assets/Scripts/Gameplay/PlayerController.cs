using UnityEngine;
using UnityEngine.InputSystem;

namespace PigeonGame.Gameplay
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;
        private Rigidbody2D rb;
        private Vector2 moveInput;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.linearDamping = 10f;
        }

        private void Update()
        {
            // 입력 받기 (새 Input System)
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return;

            moveInput = Vector2.zero;
            
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

        private void FixedUpdate()
        {
            // 이동
            rb.linearVelocity = moveInput * moveSpeed;
        }
    }
}

