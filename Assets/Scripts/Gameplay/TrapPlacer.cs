using UnityEngine;
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

        /// <summary>
        /// 플레이어 위치에 덫 설치
        /// </summary>
        public bool PlaceTrapAtPlayerPosition(string trapId)
        {
            if (PlayerController.Instance == null)
            {
                Debug.LogWarning("PlayerController를 찾을 수 없습니다!");
                return false;
            }

            Vector3 playerPos = PlayerController.Instance.Position;
            return PlaceTrapAtPosition(playerPos, trapId);
        }

        /// <summary>
        /// 지정된 위치에 덫 설치
        /// </summary>
        public bool PlaceTrapAtPosition(Vector3 position, string trapId)
        {

            // 덫 해금 확인
            if (requirePurchase && GameManager.Instance != null)
            {
                if (!GameManager.Instance.IsTrapUnlocked(trapId))
                {
                    Debug.Log($"해금되지 않은 덫: {trapId}");
                    return false;
                }
            }

            // 덫 프리팹 생성
            GameObject trapObj = Instantiate(trapPrefab, position, Quaternion.identity);
            FoodTrap trap = trapObj.GetComponent<FoodTrap>();
            
            if (trap != null)
            {
                // TrapId 설정 (리플렉션 사용)
                var trapType = typeof(FoodTrap).GetField("trapId", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (trapType != null)
                {
                    trapType.SetValue(trap, trapId);
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
                    spawner.SpawnPigeonsAtPosition(position, trap);
                }

                return true;
            }

            return false;
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

