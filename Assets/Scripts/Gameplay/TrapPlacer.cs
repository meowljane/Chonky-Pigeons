using UnityEngine;
using System.Collections.Generic;
using PigeonGame.Data;
using PigeonGame.UI;

namespace PigeonGame.Gameplay
{
    public class TrapPlacer : MonoBehaviour
    {
        private const float INTERACTION_RADIUS = 2f; 

        [SerializeField] private GameObject trapPrefab;
        [SerializeField] private WorldPigeonManager pigeonManager;

        private void Start()
        {
            if (pigeonManager == null)
                Debug.LogError("WorldPigeonManager가 할당되지 않았습니다!", this);
        }

        private List<FoodTrap> cachedTraps = new List<FoodTrap>();
        private float trapCacheUpdateTimer = 0f;
        private const float TRAP_CACHE_UPDATE_INTERVAL = 1f;

        private int GetCurrentTrapCount(string mapName)
        {
            if (string.IsNullOrEmpty(mapName) || TilemapRangeManager.Instance == null)
                return 0;

            trapCacheUpdateTimer += Time.deltaTime;
            if (trapCacheUpdateTimer >= TRAP_CACHE_UPDATE_INTERVAL)
            {
                trapCacheUpdateTimer = 0f;
                cachedTraps.Clear();
                FoodTrap[] allTraps = FindObjectsByType<FoodTrap>(FindObjectsSortMode.None);
                if (allTraps != null)
                {
                    cachedTraps.AddRange(allTraps);
                }
            }

            int count = 0;
            foreach (var trap in cachedTraps)
            {
                if (trap == null) continue;

                string trapMapName = TilemapRangeManager.Instance.GetMapNameAtPosition(trap.transform.position);
                if (trapMapName == mapName)
                    count++;
            }
            return count;
        }

        private List<InteractableBase> cachedInteractables = new List<InteractableBase>();
        private float interactableCacheUpdateTimer = 0f;
        private const float INTERACTABLE_CACHE_UPDATE_INTERVAL = 1f;

        private bool IsPositionTooCloseToOtherObjects(Vector3 position)
        {
            interactableCacheUpdateTimer += Time.deltaTime;
            if (interactableCacheUpdateTimer >= INTERACTABLE_CACHE_UPDATE_INTERVAL)
            {
                interactableCacheUpdateTimer = 0f;
                cachedInteractables.Clear();
                InteractableBase[] interactables = FindObjectsByType<InteractableBase>(FindObjectsSortMode.None);
                if (interactables != null)
                {
                    cachedInteractables.AddRange(interactables);
                }
            }

            if (cachedInteractables.Count == 0)
                return false;

            Vector2 pos2D = new Vector2(position.x, position.y);

            foreach (var interactable in cachedInteractables)
            {
                if (interactable == null)
                    continue;

                float interactionRadius = interactable.InteractionRadius;
                Vector3 lossyScale = interactable.transform.lossyScale;
                float scaledRadius = interactionRadius * Mathf.Max(Mathf.Abs(lossyScale.x), Mathf.Abs(lossyScale.y));

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

        public bool PlaceTrapAtPlayerPosition(TrapType trapType, int feedAmount = 0)
        {
            if (PlayerController.Instance == null)
                return false;

            Vector3 playerPos = PlayerController.Instance.Position;

            if (TilemapRangeManager.Instance == null || !TilemapRangeManager.Instance.IsInMapRange(playerPos))
            {
                ToastNotificationManager.ShowWarning("맵 범위를 벗어났습니다!");
                return false;
            }

            string currentMapName = TilemapRangeManager.Instance.GetMapNameAtPosition(playerPos);
            if (string.IsNullOrEmpty(currentMapName) || currentMapName == "Unknown")
            {
                ToastNotificationManager.ShowWarning("맵 정보를 확인할 수 없습니다!");
                return false;
            }

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

            if (IsPositionTooCloseToOtherObjects(playerPos))
            {
                ToastNotificationManager.ShowWarning("다른 사물이 너무 가까이 있습니다!");
                return false;
            }

            if (GameManager.Instance == null)
                return false;

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

            int actualFeedAmount = feedAmount > 0 ? feedAmount : trapData.feedAmount;

            if (!GameManager.Instance.PurchaseTrapInstallation(trapType, actualFeedAmount))
            {
                ToastNotificationManager.ShowWarning("골드가 부족합니다!");
                return false;
            }

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

            if (trapObj.GetComponent<UI.TrapFoodDisplay>() == null)
            {
                trapObj.AddComponent<UI.TrapFoodDisplay>();
            }

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