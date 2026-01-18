using UnityEngine;
using PigeonGame.Data;
using PigeonGame.UI;

namespace PigeonGame.Gameplay
{
    public class TrapPlacer : MonoBehaviour
    {
        private const float INTERACTION_RADIUS = 2f; // 모든 건물과 덫의 통일된 interactionRadius

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
        /// 위치가 다른 건물이나 덫의 interactionRadius 내에 있는지 확인
        /// </summary>
        private bool IsPositionTooCloseToOtherObjects(Vector3 position)
        {
            Vector2 pos2D = new Vector2(position.x, position.y);

            // Physics2D를 사용하여 interactionRadius 내의 모든 콜라이더 검사
            Collider2D[] colliders = Physics2D.OverlapCircleAll(pos2D, INTERACTION_RADIUS);
            
            foreach (var col in colliders)
            {
                if (col == null)
                    continue;

                // 건물 또는 덫인지 확인
                if (col.GetComponent<WorldShop>() != null || col.GetComponent<FoodTrap>() != null)
                {
                    return true;
                }
            }

            return false;
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
                        // 토스트 알림 표시
                        ToastNotificationManager.ShowWarning($"덫 개수 제한에 도달했습니다! (최대 {maxTrapCount}개)");
                        return false; // 제한 초과
                    }
                }
            }

            Vector3 playerPos = PlayerController.Instance.Position;

            // 다른 건물이나 덫의 interactionRadius 내에 있는지 확인
            if (IsPositionTooCloseToOtherObjects(playerPos))
            {
                ToastNotificationManager.ShowWarning("다른 사물이 너무 가까이 있습니다!");
                return false;
            }

            // 덫 해금 확인 및 구매 처리
            if (GameManager.Instance != null)
            {
                // 해금되지 않은 덫은 설치 불가
                if (!GameManager.Instance.IsTrapUnlocked(trapType))
                {
                    ToastNotificationManager.ShowWarning("아직 해금되지 않은 덫입니다!");
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
                    ToastNotificationManager.ShowWarning("골드가 부족합니다!");
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

                ToastNotificationManager.ShowSuccess("덫 설치 완료!");
                return true;
            }

            return false;
        }
    }
}