using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
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
        private Collider2D[] mapColliders; // 맵 경계 체크용

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
                mobileJoystick = FindFirstObjectByType<MobileJoystick>();
            }
            
            // 맵 콜라이더 찾기
            FindMapColliders();
        }
        
        private void FindMapColliders()
        {
            // WorldPigeonManager에서 맵 콜라이더 가져오기
            if (WorldPigeonManager.Instance != null)
            {
                mapColliders = WorldPigeonManager.Instance.MapColliders;
            }
            
            if (mapColliders == null || mapColliders.Length == 0)
            {
                Debug.LogWarning("PlayerController: 맵 콜라이더를 찾을 수 없습니다!");
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
            
            // 맵 경계 체크
            newPosition = ClampToMapBounds(newPosition);
            
            // 위치 직접 설정 (경계를 벗어나지 않도록)
            rb.MovePosition(newPosition);
        }
        
        private Vector2 ClampToMapBounds(Vector2 position)
        {
            if (mapColliders == null || mapColliders.Length == 0)
                return position;
            
            // 모든 맵 콜라이더의 bounds를 합친 영역 계산
            Bounds? combinedBounds = null;
            foreach (var col in mapColliders)
            {
                if (col != null)
                {
                    if (combinedBounds == null)
                        combinedBounds = col.bounds;
                    else
                    {
                        Bounds bounds = combinedBounds.Value;
                        bounds.Encapsulate(col.bounds);
                        combinedBounds = bounds;
                    }
                }
            }
            
            if (combinedBounds.HasValue)
            {
                Bounds bounds = combinedBounds.Value;
                position.x = Mathf.Clamp(position.x, bounds.min.x, bounds.max.x);
                position.y = Mathf.Clamp(position.y, bounds.min.y, bounds.max.y);
            }
            
            return position;
        }
    }
}

