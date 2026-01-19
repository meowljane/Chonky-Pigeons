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
        private Collider2D myMapCollider; // 현재 위치한 맵 콜라이더

        public static PlayerController Instance { get; private set; }
        public Vector2 Position => (Vector2)transform.position;
        public Collider2D CurrentMapCollider => myMapCollider;
        public string CurrentMapName
        {
            get
            {
                if (myMapCollider != null && MapManager.Instance != null)
                {
                    return MapManager.Instance.GetMapName(myMapCollider);
                }
                return "Unknown";
            }
        }

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
            
            // 현재 위치한 맵 콜라이더 찾기
            FindMyMapCollider();
        }
        
        private void FindMyMapCollider()
        {
            // MapManager를 통해 현재 위치가 속한 맵 찾기
            if (MapManager.Instance != null)
            {
                var mapInfo = MapManager.Instance.GetMapAtPosition(transform.position);
                if (mapInfo != null && mapInfo.mapCollider != null)
                {
                    myMapCollider = mapInfo.mapCollider;
                    return;
                }
            }
            
            // 폴백: WorldPigeonManager 사용
            if (WorldPigeonManager.Instance != null)
            {
                var mapColliders = WorldPigeonManager.Instance.MapColliders;
                if (mapColliders != null)
                {
                    foreach (var collider in mapColliders)
                    {
                        if (collider != null && ColliderUtility.IsPositionInsideCollider(transform.position, collider))
                        {
                            myMapCollider = collider;
                            return;
                        }
                    }
                }
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
            
            // 다리 위에 있는지 확인
            bool isOnBridge = IsOnBridge(newPosition);
            
            if (isOnBridge)
            {
                // 다리 접근 가능 여부 확인 (해금 여부 체크)
                bool canAccessBridge = CanAccessBridge(newPosition);
                
                if (canAccessBridge)
                {
                    // 다리 위에서는 자유롭게 이동 가능 (맵 경계 제한 없음)
                    // 다리 위에서 벗어나면 다시 맵 경계로 제한되도록 맵 업데이트
                    UpdateMapIfNeeded(newPosition);
                }
                else
                {
                    // 해금되지 않은 다리로는 이동 불가 (현재 위치 유지)
                    newPosition = transform.position;
                }
            }
            else
            {
                // 다리 위가 아니면 맵 경계 내로 제한
                Collider2D targetMapCollider = FindMapColliderForPosition(newPosition);
                
                if (targetMapCollider != null)
                {
                    myMapCollider = targetMapCollider;
                    newPosition = ClampToMapBounds(newPosition, targetMapCollider);
                }
                else if (myMapCollider != null)
                {
                    // 어떤 맵에도 속하지 않으면 현재 맵의 경계로 제한
                    newPosition = ClampToMapBounds(newPosition, myMapCollider);
                }
            }
            
            // 위치 직접 설정
            rb.MovePosition(newPosition);
        }
        
        /// <summary>
        /// 특정 위치가 속한 맵 콜라이더 찾기
        /// 겹치는 영역에서는 현재 맵이 아닌 다른 맵을 우선적으로 선택 (맵 전환 용이)
        /// </summary>
        private Collider2D FindMapColliderForPosition(Vector2 position)
        {
            Collider2D[] allMaps = null;
            
            // MapManager를 통해 맵 찾기
            if (MapManager.Instance != null)
            {
                allMaps = MapManager.Instance.GetAllMapColliders();
            }
            
            // 폴백: WorldPigeonManager 사용
            if (allMaps == null && WorldPigeonManager.Instance != null)
            {
                allMaps = WorldPigeonManager.Instance.MapColliders;
            }
            
            if (allMaps == null)
                return null;
            
            // 먼저 현재 맵이 아닌 다른 맵을 찾기 (맵 전환 우선)
            foreach (var collider in allMaps)
            {
                if (collider != null && collider != myMapCollider && 
                    ColliderUtility.IsPositionInsideCollider(position, collider))
                {
                    return collider;
                }
            }
            
            // 다른 맵을 찾지 못했으면 현재 맵 확인
            if (myMapCollider != null && ColliderUtility.IsPositionInsideCollider(position, myMapCollider))
            {
                return myMapCollider;
            }
            
            // 현재 맵에도 없으면 다른 맵 중 하나라도 찾기
            foreach (var collider in allMaps)
            {
                if (collider != null && ColliderUtility.IsPositionInsideCollider(position, collider))
                {
                    return collider;
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// 위치가 다리 위에 있는지 확인
        /// </summary>
        private bool IsOnBridge(Vector2 position)
        {
            if (MapManager.Instance == null)
                return false;
            
            return MapManager.Instance.IsPositionOnBridge(position);
        }

        /// <summary>
        /// 다리에 접근 가능한지 확인 (해금 여부 체크)
        /// </summary>
        private bool CanAccessBridge(Vector2 position)
        {
            if (MapManager.Instance == null)
                return false;
            
            return MapManager.Instance.CanAccessBridgeAtPosition(position);
        }
        
        /// <summary>
        /// 다리 위에서 벗어날 때 맵 업데이트 (다리에서 나가면 다시 맵 경계로 제한)
        /// </summary>
        private void UpdateMapIfNeeded(Vector2 position)
        {
            // 다리 위에서 벗어나려고 할 때, 목적지 맵 확인
            var targetMap = FindMapColliderForPosition(position);
            if (targetMap != null)
            {
                myMapCollider = targetMap;
            }
        }
        
        /// <summary>
        /// 특정 맵 콜라이더의 bounds로 위치 제한
        /// </summary>
        private Vector2 ClampToMapBounds(Vector2 position, Collider2D mapCollider)
        {
            if (mapCollider == null)
                return position;
            
            Bounds bounds = mapCollider.bounds;
            position.x = Mathf.Clamp(position.x, bounds.min.x, bounds.max.x);
            position.y = Mathf.Clamp(position.y, bounds.min.y, bounds.max.y);
            return position;
        }
    }
}

