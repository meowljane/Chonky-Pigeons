using UnityEngine;
using UnityEngine.InputSystem;
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
                PlaceTrap();
            }
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
                    if (trapData != null)
                    {
                        // 덫이 이미 해금되어 있으면 구매 불필요 (한 번만 구매하면 계속 사용 가능)
                        // 또는 매번 구매하려면 아래 주석 해제
                        // if (!GameManager.Instance.SpendMoney(trapData.cost))
                        // {
                        //     Debug.Log($"덫 구매 실패: 돈 부족 (필요: {trapData.cost})");
                        //     return;
                        // }
                    }
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

