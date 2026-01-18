using UnityEngine;
using TMPro;
using System.Collections.Generic;
using PigeonGame.Data;
using PigeonGame.Gameplay;

namespace PigeonGame.UI
{
    /// <summary>
    /// 상단바에 현재 맵 정보 표시 (종별 스폰 확률, 현재 terrain 타입)
    /// </summary>
    public class MapInfoUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI terrainTypeText; // 현재 terrain 타입 표시
        [SerializeField] private TextMeshProUGUI mapNameText; // 현재 맵 이름 표시
        [SerializeField] private TextMeshProUGUI trapCountText; // 덫 수 표시
        [SerializeField] private TextMeshProUGUI pigeonCountText; // 비둘기 수 표시
        [SerializeField] private TextMeshProUGUI speciesProbabilityText; // 종별 확률 표시 텍스트
        [SerializeField] private float updateInterval = 0.5f; // 업데이트 간격 (초)

        private WorldPigeonManager pigeonManager;
        private float updateTimer = 0f;

        private void Start()
        {
            pigeonManager = FindFirstObjectByType<WorldPigeonManager>();
        }

        private void Update()
        {
            updateTimer += Time.deltaTime;
            if (updateTimer >= updateInterval)
            {
                updateTimer = 0f;
                UpdateMapInfo();
            }
        }

        private void UpdateMapInfo()
        {
            if (PlayerController.Instance == null || pigeonManager == null)
                return;

            // 현재 맵 이름 표시
            string currentMapName = PlayerController.Instance.CurrentMapName;
            string mapDisplay = $"Map: {currentMapName}";
            
            if (mapNameText != null)
            {
                mapNameText.text = mapDisplay;
            }

            // 현재 플레이어 위치의 terrain 타입 표시
            TerrainType currentTerrain = MapManager.Instance != null ? MapManager.Instance.GetTerrainTypeAtPosition(PlayerController.Instance.Position) : TerrainType.SAND;
            string terrainDisplay = $"Terrain: {currentTerrain}";
            
            if (terrainTypeText != null)
            {
                terrainTypeText.text = terrainDisplay;
            }

            // 현재 맵의 덫 수 및 비둘기 수 표시
            UpdateTrapAndPigeonCount();

            // 현재 맵의 종별 스폰 확률 표시
            UpdateSpeciesProbabilities();
        }

        private void UpdateTrapAndPigeonCount()
        {
            if (PlayerController.Instance == null || MapManager.Instance == null)
                return;

            // 현재 맵 정보 가져오기
            var mapInfo = MapManager.Instance.GetMapAtPosition(PlayerController.Instance.Position);
            if (mapInfo == null || mapInfo.mapCollider == null)
                return;

            // 현재 맵의 활성 덫 수 계산
            int activeTrapCount = GetActiveTrapCountInMap(mapInfo.mapCollider);
            int maxTrapCount = UpgradeData.Instance != null ? UpgradeData.Instance.MaxTrapCount : 2;
            string trapDisplay = $"덫: {activeTrapCount}/{maxTrapCount}개";

            if (trapCountText != null)
            {
                trapCountText.text = trapDisplay;
            }

            // 현재 맵의 비둘기 수 계산
            int currentPigeonCount = GetPigeonCountInMap(mapInfo.mapCollider);
            int maxPigeonCount = GameManager.Instance != null ? GameManager.Instance.MaxPigeonsPerMap : 5;
            string pigeonDisplay = $"비둘기: {currentPigeonCount}/{maxPigeonCount}마리";

            if (pigeonCountText != null)
            {
                pigeonCountText.text = pigeonDisplay;
            }
        }

        /// <summary>
        /// 현재 맵의 활성 덫 수 계산 (포획된 덫 포함)
        /// </summary>
        private int GetActiveTrapCountInMap(Collider2D mapCollider)
        {
            if (mapCollider == null)
                return 0;

            FoodTrap[] allTraps = FindObjectsByType<FoodTrap>(FindObjectsSortMode.None);
            int count = 0;
            foreach (var trap in allTraps)
            {
                if (trap != null)
                {
                    if (ColliderUtility.IsPositionInsideCollider(trap.transform.position, mapCollider))
                        count++;
                }
            }
            return count;
        }

        /// <summary>
        /// 현재 맵의 비둘기 수 계산 (WorldPigeonManager에서 가져옴)
        /// </summary>
        private int GetPigeonCountInMap(Collider2D mapCollider)
        {
            if (mapCollider == null || pigeonManager == null)
                return 0;

            return pigeonManager.GetPigeonCountInMap(mapCollider);
        }

        private void UpdateSpeciesProbabilities()
        {
            if (PlayerController.Instance == null || pigeonManager == null || speciesProbabilityText == null)
                return;

            var probabilities = pigeonManager.GetSpeciesSpawnProbabilities();
            if (probabilities == null || probabilities.Count == 0)
            {
                speciesProbabilityText.text = "";
                return;
            }

            var registry = GameDataRegistry.Instance;
            if (registry?.SpeciesSet == null)
            {
                speciesProbabilityText.text = "";
                return;
            }

            // 확률이 높은 순으로 정렬
            List<KeyValuePair<PigeonSpecies, float>> sortedProbabilities = new List<KeyValuePair<PigeonSpecies, float>>(probabilities);
            sortedProbabilities.Sort((a, b) => b.Value.CompareTo(a.Value));

            // 줄바꿈으로 모든 종의 확률 표시
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (var kvp in sortedProbabilities)
            {
                var species = registry.SpeciesSet.GetSpeciesById(kvp.Key);
                if (species == null) continue;

                if (sb.Length > 0)
                    sb.Append("\n");
                
                sb.Append($"{species.name}: {kvp.Value:F1}%");
            }

            speciesProbabilityText.text = sb.ToString();
        }
    }
}
