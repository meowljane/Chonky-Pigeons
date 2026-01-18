using UnityEngine;
using PigeonGame.Data;

namespace PigeonGame.Gameplay
{
    public class TrapPlacer : MonoBehaviour
    {
        [SerializeField] private GameObject trapPrefab;
        [SerializeField] private WorldPigeonManager pigeonManager;

        private void Start()
        {
            if (pigeonManager == null)
                pigeonManager = FindFirstObjectByType<WorldPigeonManager>();
        }

        /// <summary>
        /// 현재 설치된 덫 개수 확인
        /// </summary>
        private int GetCurrentTrapCount()
        {
            FoodTrap[] allTraps = FindObjectsByType<FoodTrap>(FindObjectsSortMode.None);
            int count = 0;
            foreach (var trap in allTraps)
            {
                if (trap != null)
                {
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// 플레이어 위치에 덫 설치 (feedAmount가 0이면 기본값 사용)
        /// </summary>
        public bool PlaceTrapAtPlayerPosition(TrapType trapType, int feedAmount = 0)
        {
            if (PlayerController.Instance == null)
                return false;

            // 동시 덫 설치 개수 제한 확인ㄴ
            if (GameManager.Instance != null)
            {
                int maxTrapCount = UpgradeData.Instance.MaxTrapCount;
                if (maxTrapCount > 0) // 0 이하면 제한 없음
                {
                    int currentTrapCount = GetCurrentTrapCount();
                    if (currentTrapCount >= maxTrapCount)
                    {
                        return false; // 제한 초과
                    }
                }
            }

            Vector3 playerPos = PlayerController.Instance.Position;

            // 덫 해금 확인 및 구매 처리
            if (GameManager.Instance != null)
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
                        // 위치로 맵 콜라이더 찾기
                        Collider2D mapCollider = null;
                        if (MapManager.Instance != null)
                        {
                            var mapInfo = MapManager.Instance.GetMapAtPosition(playerPos);
                            if (mapInfo != null)
                            {
                                mapCollider = mapInfo.mapCollider;
                            }
                        }
                        
                        if (mapCollider != null)
                        {
                            pigeonManager.SpawnPigeonAtPosition(playerPos, mapCollider, trapData.pigeonSpawnCount);
                        }
                    }
                }

                return true;
            }

            return false;
        }
    }
}