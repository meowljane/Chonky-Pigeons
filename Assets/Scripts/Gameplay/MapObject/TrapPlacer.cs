using UnityEngine;
using System.Collections.Generic;
using PigeonGame.Data;
using PigeonGame.UI;

namespace PigeonGame.Gameplay
{
    public class TrapPlacer : MonoBehaviour
    {
        [SerializeField] private GameObject trapPrefab;

        private List<FoodTrap> cachedTraps = new List<FoodTrap>();
        private List<InteractableBase> cachedInteractables = new List<InteractableBase>();
        private float cacheUpdateTimer = 0f;
        private const float CACHE_UPDATE_INTERVAL = 1f;

        private void UpdateCache()
        {
            cacheUpdateTimer += Time.deltaTime;
            if (cacheUpdateTimer >= CACHE_UPDATE_INTERVAL)
            {
                cacheUpdateTimer = 0f;
                cachedTraps.Clear();
                cachedInteractables.Clear();

                FoodTrap[] allTraps = FindObjectsByType<FoodTrap>(FindObjectsSortMode.None);
                if (allTraps != null)
                    cachedTraps.AddRange(allTraps);

                InteractableBase[] interactables = FindObjectsByType<InteractableBase>(FindObjectsSortMode.None);
                if (interactables != null)
                    cachedInteractables.AddRange(interactables);
            }
        }

        private int GetCurrentTrapCount(string mapName)
        {
            if (string.IsNullOrEmpty(mapName) || TilemapRangeManager.Instance == null)
                return 0;

            UpdateCache();

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

        private bool IsPositionTooCloseToOtherObjects(Vector3 position)
        {
            UpdateCache();

            if (cachedInteractables.Count == 0)
                return false;

            Vector2 pos2D = new Vector2(position.x, position.y);

            foreach (var interactable in cachedInteractables)
            {
                if (interactable == null) continue;

                float interactionRadius = interactable.InteractionRadius;
                Vector3 lossyScale = interactable.transform.lossyScale;
                float scaledRadius = interactionRadius * Mathf.Max(Mathf.Abs(lossyScale.x), Mathf.Abs(lossyScale.y));
                float sqrRadius = scaledRadius * scaledRadius;

                Vector2 interactablePos2D = new Vector2(interactable.transform.position.x, interactable.transform.position.y);
                float sqrDistance = Vector2.SqrMagnitude(pos2D - interactablePos2D);

                if (sqrDistance < sqrRadius)
                    return true;
            }

            return false;
        }

        private bool ValidatePlacement(Vector3 position, out string mapName)
        {
            mapName = null;

            if (PlayerController.Instance == null || TilemapRangeManager.Instance == null)
                return false;

            if (!TilemapRangeManager.Instance.IsInMapRange(position))
            {
                ToastNotificationManager.ShowWarning("맵 범위를 벗어났습니다!");
                return false;
            }

            mapName = TilemapRangeManager.Instance.GetMapNameAtPosition(position);
            if (string.IsNullOrEmpty(mapName) || mapName == "Unknown")
            {
                ToastNotificationManager.ShowWarning("맵 정보를 확인할 수 없습니다!");
                return false;
            }

            int maxTrapCount = UpgradeData.Instance?.MaxTrapCount ?? 0;
            if (maxTrapCount > 0 && GetCurrentTrapCount(mapName) >= maxTrapCount)
            {
                ToastNotificationManager.ShowWarning($"덫 개수 제한에 도달했습니다! (최대 {maxTrapCount}개)");
                return false;
            }

            if (IsPositionTooCloseToOtherObjects(position))
            {
                ToastNotificationManager.ShowWarning("다른 사물이 너무 가까이 있습니다!");
                return false;
            }

            return true;
        }

        public bool PlaceTrapAtPlayerPosition(TrapType trapType, int feedAmount)
        {
            if (PlayerController.Instance == null || GameManager.Instance == null)
                return false;

            Vector3 playerPos = PlayerController.Instance.Position;

            if (!ValidatePlacement(playerPos, out string mapName))
                return false;

            if (!GameManager.Instance.IsTrapUnlocked(trapType))
            {
                ToastNotificationManager.ShowWarning("아직 해금되지 않은 덫입니다!");
                return false;
            }

            var registry = GameDataRegistry.Instance;
            var trapData = registry?.Traps?.GetTrapById(trapType);
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

            trap.SetTrapIdAndFeedAmount(trapType, actualFeedAmount);

            if (trapData.pigeonSpawnCount > 0 && WorldPigeonManager.Instance != null)
                WorldPigeonManager.Instance.SpawnPigeonAtPosition(playerPos, mapName, trapData.pigeonSpawnCount);

            ToastNotificationManager.ShowSuccess("덫 설치 완료!");
            return true;
        }
    }
}