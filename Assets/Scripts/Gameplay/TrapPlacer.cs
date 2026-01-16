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
        public bool PlaceTrapAtPlayerPosition(TrapType trapType)
        {
            return PlaceTrapAtPlayerPosition(trapType, 0); // 기본 feedAmount 사용
        }

        /// <summary>
        /// 플레이어 위치에 덫 설치 (커스텀 feedAmount 포함)
        /// </summary>
        public bool PlaceTrapAtPlayerPosition(TrapType trapType, int feedAmount)
        {
            if (PlayerController.Instance == null)
                return false;

            Vector3 playerPos = PlayerController.Instance.Position;

            // 덫 해금 확인
            if (requirePurchase && GameManager.Instance != null)
            {
                // 해금되지 않은 덫은 설치 불가
                if (!GameManager.Instance.IsTrapUnlocked(trapType))
                {
                    return false;
                }

                var registry = GameDataRegistry.Instance;
                if (registry == null || registry.Traps == null)
                    return false;

                var trapData = registry.Traps.GetTrapById(trapType);
                if (trapData == null)
                    return false;

                // feedAmount가 0이면 기본값 사용
                int actualFeedAmount = feedAmount > 0 ? feedAmount : trapData.feedAmount;

                // 구매 처리 (설치 + 모이)
                if (!GameManager.Instance.PurchaseTrapInstallation(trapType, actualFeedAmount))
                {
                    return false; // 구매 실패
                }
            }

            // 덫 프리팹 생성
            GameObject trapObj = Instantiate(trapPrefab, playerPos, Quaternion.identity);
            FoodTrap trap = trapObj.GetComponent<FoodTrap>();
            
            if (trap != null)
            {
                var registry = GameDataRegistry.Instance;
                if (registry != null && registry.Traps != null)
                {
                    var trapData = registry.Traps.GetTrapById(trapType);
                    if (trapData != null)
                    {
                        // feedAmount가 0이면 기본값 사용
                        int actualFeedAmount = feedAmount > 0 ? feedAmount : trapData.feedAmount;
                        trap.SetTrapIdAndFeedAmount(trapType, actualFeedAmount);
                    }
                    else
                    {
                        trap.SetTrapId(trapType);
                    }
                }
                else
                {
                    trap.SetTrapId(trapType);
                }

                // 덫에 먹이 표시 UI 추가 (없으면)
                var foodDisplay = trapObj.GetComponent<UI.TrapFoodDisplay>();
                if (foodDisplay == null)
                {
                    foodDisplay = trapObj.AddComponent<UI.TrapFoodDisplay>();
                }

                // 덫 설치 시 비둘기 추가 스폰 (해당 맵 내 랜덤 위치)
                if (registry != null && registry.Traps != null && pigeonManager != null)
                {
                    var trapData = registry.Traps.GetTrapById(trapType);
                    if (trapData != null && trapData.pigeonSpawnCount > 0)
                    {
                        pigeonManager.SpawnPigeonsInMap(playerPos, trapData.pigeonSpawnCount, true);
                    }
                }

                return true;
            }

            return false;
        }

        // 포획 이벤트 핸들러 제거됨
        // 이제 TrapInteractionSystem에서 상호작용을 통해 비둘기를 수집함
        // private void OnPigeonCaptured(PigeonAI pigeon) { ... }
    }
}

