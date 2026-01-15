using UnityEngine;
using PigeonGame.Data;

namespace PigeonGame.Gameplay
{
    public class TrapPlacer : MonoBehaviour
    {
        [SerializeField] private GameObject trapPrefab;
        [SerializeField] private WorldPigeonManager pigeonManager;
        [SerializeField] private bool requirePurchase = true; // 덫 설치 시 구매 필요 여부

        private void Start()
        {
            if (pigeonManager == null)
                pigeonManager = FindFirstObjectByType<WorldPigeonManager>();
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

            // 덫 해금 확인
            if (requirePurchase && GameManager.Instance != null)
            {
                if (!GameManager.Instance.IsTrapUnlocked(trapId))
                {
                    return false;
                }
            }

            // 덫 프리팹 생성
            GameObject trapObj = Instantiate(trapPrefab, playerPos, Quaternion.identity);
            FoodTrap trap = trapObj.GetComponent<FoodTrap>();
            
            if (trap != null)
            {
                // TrapId 설정
                trap.SetTrapId(trapId);

                // 덫에 먹이 표시 UI 추가 (없으면)
                var foodDisplay = trapObj.GetComponent<UI.TrapFoodDisplay>();
                if (foodDisplay == null)
                {
                    foodDisplay = trapObj.AddComponent<UI.TrapFoodDisplay>();
                }

                // 덫 설치 시 비둘기 추가 스폰 (해당 맵 내 랜덤 위치)
                var registry = GameDataRegistry.Instance;
                if (registry != null && registry.Traps != null && pigeonManager != null)
                {
                    var trapData = registry.Traps.GetTrapById(trapId);
                    if (trapData != null && trapData.pigeonSpawnCount > 0)
                    {
                        pigeonManager.SpawnPigeonsInMap(playerPos, trapData.pigeonSpawnCount, true);
                    }
                }

                // 포획 이벤트는 더 이상 즉시 등록하지 않음
                // 상호작용을 통해 수집하도록 변경됨
                // trap.OnCaptured += OnPigeonCaptured;

                return true;
            }

            return false;
        }

        // 포획 이벤트 핸들러 제거됨
        // 이제 TrapInteractionSystem에서 상호작용을 통해 비둘기를 수집함
        // private void OnPigeonCaptured(PigeonAI pigeon) { ... }
    }
}

