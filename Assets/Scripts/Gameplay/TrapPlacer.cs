using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using PigeonGame.Data;

namespace PigeonGame.Gameplay
{
    public class TrapPlacer : MonoBehaviour
    {
        [SerializeField] private Camera mainCamera;
        [SerializeField] private GameObject trapPrefab;
        [SerializeField] private PigeonSpawner spawner;
        [SerializeField] private string defaultTrapId = "BREAD";
        [SerializeField] private bool requirePurchase = true; // 덫 설치 시 구매 필요 여부

        private void Start()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;
            
            if (spawner == null)
                spawner = FindObjectOfType<PigeonSpawner>();
        }

        private void Update()
        {
            // 마우스 클릭으로 덫 설치 (새 Input System)
            Mouse mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                // UI 요소를 클릭한 경우 덫 설치하지 않음
                if (!IsPointerOverUI())
                {
                    PlaceTrap();
                }
            }
        }

        /// <summary>
        /// 마우스 포인터가 UI 위에 있는지 확인
        /// </summary>
        private bool IsPointerOverUI()
        {
            if (EventSystem.current == null)
                return false;

            // EventSystem을 사용한 기본 체크 (마우스와 터치 모두 지원)
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return true;
            }

            // 새 Input System용 추가 체크 (터치 입력 명시적 처리)
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            Vector2 screenPosition = Vector2.zero;
            bool hasInput = false;

            // 마우스 포인터 위치
            Mouse mouse = Mouse.current;
            if (mouse != null)
            {
                screenPosition = mouse.position.ReadValue();
                hasInput = true;
            }
            else
            {
                // 터치 입력
                Touchscreen touch = Touchscreen.current;
                if (touch != null && touch.primaryTouch.isInProgress)
                {
                    screenPosition = touch.primaryTouch.position.ReadValue();
                    hasInput = true;
                }
            }

            if (!hasInput)
                return false;

            pointerData.position = screenPosition;

            // UI 레이어에 Raycast
            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);
            
            return results.Count > 0;
        }

        private void PlaceTrap()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null) return;

            Vector3 mousePos = mainCamera.ScreenToWorldPoint(mouse.position.ReadValue());
            mousePos.z = 0;

            // 덫 해금 확인
            if (requirePurchase && GameManager.Instance != null)
            {
                if (!GameManager.Instance.IsTrapUnlocked(defaultTrapId))
                {
                    Debug.Log($"해금되지 않은 덫: {defaultTrapId}");
                    return;
                }
            }

            // 덫 구매 (필요한 경우)
            if (requirePurchase && GameManager.Instance != null)
            {
                var registry = GameDataRegistry.Instance;
                if (registry != null && registry.Traps != null)
                {
                    var trapData = registry.Traps.GetTrapById(defaultTrapId);
                    // 덫이 이미 해금되어 있으면 구매 불필요 (한 번만 구매하면 계속 사용 가능)
                }
            }

            // 덫 프리팹 생성
            GameObject trapObj = Instantiate(trapPrefab, mousePos, Quaternion.identity);
            FoodTrap trap = trapObj.GetComponent<FoodTrap>();
            
            if (trap != null)
            {
                // TrapId 설정 (리플렉션 사용)
                var trapType = typeof(FoodTrap).GetField("trapId", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (trapType != null)
                {
                    trapType.SetValue(trap, defaultTrapId);
                }

                // 덫에 먹이 표시 UI 추가 (없으면)
                var foodDisplay = trapObj.GetComponent<UI.TrapFoodDisplay>();
                if (foodDisplay == null)
                {
                    foodDisplay = trapObj.AddComponent<UI.TrapFoodDisplay>();
                }

                // 포획 이벤트 연결
                trap.OnCaptured += OnPigeonCaptured;

                // 비둘기 스폰
                if (spawner != null)
                {
                    spawner.SpawnPigeonsAtPosition(mousePos, trap);
                }
            }
        }

        private void OnPigeonCaptured(PigeonAI pigeon)
        {
            Debug.Log("비둘기 포획 성공!");
            
            // 인벤토리에 추가
            var controller = pigeon.GetComponent<PigeonController>();
            if (controller != null && controller.Stats != null)
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.AddPigeonToInventory(controller.Stats);
                }
                else
                {
                    Debug.LogWarning("GameManager를 찾을 수 없습니다!");
                }
            }
        }
    }
}

