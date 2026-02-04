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
        /// 현재 맵에 설치된 덫 개수 확인
        /// </summary>
        private int GetCurrentTrapCount(string mapName)
        {
            if (string.IsNullOrEmpty(mapName) || TilemapRangeManager.Instance == null)
                return 0;

            FoodTrap[] allTraps = FindObjectsByType<FoodTrap>(FindObjectsSortMode.None);
            if (allTraps == null)
                return 0;

            int count = 0;
            foreach (var trap in allTraps)
            {
                if (trap != null)
                {
                    string trapMapName = TilemapRangeManager.Instance.GetMapNameAtPosition(trap.transform.position);
                    if (trapMapName == mapName)
                        count++;
                }
            }
            return count;
        }

        /// <summary>
        /// 위치가 다른 InteractableBase 오브젝트(건물, 덫, 문 등)의 인식 범위 내에 있는지 확인
        /// 가장 가볍고 최적화된 방식: InteractionRadius + lossyScale 직접 계산 + 제곱 거리 비교
        /// Unity의 CircleCollider2D bounds 계산 방식과 동일하게 처리 (radius * max(scale.x, scale.y))
        /// </summary>
        private bool IsPositionTooCloseToOtherObjects(Vector3 position)
        {
            // InteractableBase를 상속받는 모든 오브젝트 확인 (WorldShop, Door, FoodTrap 등)
            InteractableBase[] interactables = FindObjectsByType<InteractableBase>(FindObjectsSortMode.None);
            if (interactables == null)
                return false;

            Vector2 pos2D = new Vector2(position.x, position.y);

            foreach (var interactable in interactables)
            {
                if (interactable == null)
                    continue;

                // Unity의 CircleCollider2D bounds 계산 방식과 동일:
                // bounds.extents.x = radius * max(lossyScale.x, lossyScale.y)
                // GetComponent나 bounds 계산 없이 직접 계산하여 최적화
                float interactionRadius = interactable.InteractionRadius;
                Vector3 lossyScale = interactable.transform.lossyScale;
                float scaledRadius = interactionRadius * Mathf.Max(Mathf.Abs(lossyScale.x), Mathf.Abs(lossyScale.y));
                
                // 제곱 거리 비교 (sqrt 없이, 가장 가벼운 방식)
                Vector2 interactablePos2D = new Vector2(interactable.transform.position.x, interactable.transform.position.y);
                float dx = pos2D.x - interactablePos2D.x;
                float dy = pos2D.y - interactablePos2D.y;
                float sqrDistance = dx * dx + dy * dy;
                float sqrRadius = scaledRadius * scaledRadius;
                
                if (sqrDistance < sqrRadius)
                    return true;
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

            Vector3 playerPos = PlayerController.Instance.Position;

            // 타일맵 기반 맵 범위 확인
            if (TilemapRangeManager.Instance == null || !TilemapRangeManager.Instance.IsInMapRange(playerPos))
            {
                ToastNotificationManager.ShowWarning("맵 범위를 벗어났습니다!");
                return false;
            }

            // 현재 맵 이름 가져오기
            string currentMapName = TilemapRangeManager.Instance.GetMapNameAtPosition(playerPos);
            if (string.IsNullOrEmpty(currentMapName) || currentMapName == "Unknown")
            {
                ToastNotificationManager.ShowWarning("맵 정보를 확인할 수 없습니다!");
                return false;
            }

            // 동시 덫 설치 개수 제한 확인 (현재 맵 기준)
            int maxTrapCount = UpgradeData.Instance?.MaxTrapCount ?? 0;
            if (maxTrapCount > 0)
            {
                int currentTrapCount = GetCurrentTrapCount(currentMapName);
                if (currentTrapCount >= maxTrapCount)
                {
                    ToastNotificationManager.ShowWarning($"덫 개수 제한에 도달했습니다! (최대 {maxTrapCount}개)");
                    return false;
                }
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
                if (!string.IsNullOrEmpty(currentMapName) && currentMapName != "Unknown")
                {
                    pigeonManager.SpawnPigeonAtPosition(playerPos, currentMapName, trapData.pigeonSpawnCount);
                }
            }

            ToastNotificationManager.ShowSuccess("덫 설치 완료!");
            return true;
        }
    }
}