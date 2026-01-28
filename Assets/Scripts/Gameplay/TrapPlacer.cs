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
            return allTraps?.Length ?? 0;
        }

        /// <summary>
        /// 위치가 다른 건물이나 덫의 중심점이 2f 이내에 있는지, 또는 문 영역 내에 있는지 확인
        /// </summary>
        private bool IsPositionTooCloseToOtherObjects(Vector3 position)
        {
            Vector2 pos2D = new Vector2(position.x, position.y);
            float minDistance = INTERACTION_RADIUS;

            // 건물 중심점 거리 확인
            if (IsTooCloseToObjects<WorldShop>(pos2D, minDistance))
                return true;

            // 덫 중심점 거리 확인
            if (IsTooCloseToObjects<FoodTrap>(pos2D, minDistance))
                return true;

            // 게이트 중심점 거리 확인 (해금된 게이트는 콜라이더가 비활성화되어 있으므로 자동으로 제외됨)
            BridgeGate[] allGates = FindObjectsByType<BridgeGate>(FindObjectsSortMode.None);
            if (allGates != null)
            {
                foreach (var gate in allGates)
                {
                    if (gate?.GateCollider?.enabled == true)
                    {
                        float distance = Vector2.Distance(pos2D, gate.transform.position);
                        if (distance < minDistance)
                            return true;
                    }
                }
            }

            return false;
        }

        private bool IsTooCloseToObjects<T>(Vector2 pos2D, float minDistance) where T : MonoBehaviour
        {
            T[] objects = FindObjectsByType<T>(FindObjectsSortMode.None);
            if (objects == null)
                return false;

            foreach (var obj in objects)
            {
                if (obj != null)
                {
                    float distance = Vector2.Distance(pos2D, obj.transform.position);
                    if (distance < minDistance)
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

            // 동시 덫 설치 개수 제한 확인
            int maxTrapCount = UpgradeData.Instance?.MaxTrapCount ?? 0;
            if (maxTrapCount > 0)
            {
                int currentTrapCount = GetCurrentTrapCount();
                if (currentTrapCount >= maxTrapCount)
                {
                    ToastNotificationManager.ShowWarning($"덫 개수 제한에 도달했습니다! (최대 {maxTrapCount}개)");
                    return false;
                }
            }

            Vector3 playerPos = PlayerController.Instance.Position;

            // 타일맵 기반 맵 범위 확인
            if (TilemapRangeManager.Instance == null || !TilemapRangeManager.Instance.IsInMapRange(playerPos))
            {
                ToastNotificationManager.ShowWarning("맵 범위를 벗어났습니다!");
                return false;
            }

            // 다른 건물이나 덫의 interactionRadius 내에 있는지, 또는 문 영역 내에 있는지 확인
            if (IsPositionTooCloseToOtherObjects(playerPos))
            {
                ToastNotificationManager.ShowWarning("다른 사물이 너무 가까이 있습니다!");
                return false;
            }

            // 덫 해금 확인 및 구매 처리
            if (GameManager.Instance == null)
                return false;

            // 해금되지 않은 덫은 설치 불가
            if (!GameManager.Instance.IsTrapUnlocked(trapType))
            {
                ToastNotificationManager.ShowWarning("아직 해금되지 않은 덫입니다!");
                return false;
            }

            var registry = GameDataRegistry.Instance;
            if (registry?.Traps == null)
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
                return false;
            }

            // 덫 프리팹 생성
            GameObject trapObj = Instantiate(trapPrefab, playerPos, Quaternion.identity);
            FoodTrap trap = trapObj.GetComponent<FoodTrap>();
            
            if (trap == null)
                return false;

            if (trapData != null)
            {
                trap.SetTrapIdAndFeedAmount(trapType, actualFeedAmount);
            }
            else
            {
                trap.SetTrapId(trapType);
            }

            // 덫에 먹이 표시 UI 추가 (없으면)
            if (trapObj.GetComponent<UI.TrapFoodDisplay>() == null)
            {
                trapObj.AddComponent<UI.TrapFoodDisplay>();
            }

            // 덫 설치 시 비둘기 추가 스폰 (해당 맵 내 랜덤 위치)
            if (trapData != null && trapData.pigeonSpawnCount > 0 && pigeonManager != null)
            {
                string mapName = TilemapRangeManager.Instance?.GetMapNameAtPosition(playerPos);
                if (!string.IsNullOrEmpty(mapName) && mapName != "Unknown")
                {
                    pigeonManager.SpawnPigeonAtPosition(playerPos, mapName, trapData.pigeonSpawnCount);
                }
            }

            ToastNotificationManager.ShowSuccess("덫 설치 완료!");
            return true;
        }
    }
}