using UnityEngine;
using TMPro;
using System.Collections.Generic;
using PigeonGame.Data;
using PigeonGame.Gameplay;

namespace PigeonGame.UI
{
    public class MapInfoUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI terrainTypeText; 
        [SerializeField] private TextMeshProUGUI mapNameText; 
        [SerializeField] private TextMeshProUGUI trapCountText; 
        [SerializeField] private TextMeshProUGUI pigeonCountText; 
        [SerializeField] private TextMeshProUGUI speciesProbabilityText; 
        [SerializeField] private float updateInterval = 0.5f; 

        [Header("References")]
        [SerializeField] private WorldPigeonManager pigeonManager;

        private float updateTimer = 0f;

        private void Start()
        {
            if (pigeonManager == null)
                Debug.LogError("WorldPigeonManager가 할당되지 않았습니다!", this);
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

            string currentMapName = "없음";
            if (TilemapRangeManager.Instance != null)
            {
                currentMapName = TilemapRangeManager.Instance.GetMapNameAtPosition(PlayerController.Instance.Position);
                if (string.IsNullOrEmpty(currentMapName) || currentMapName == "Unknown")
                {
                    currentMapName = "없음";
                }
            }

            string mapDisplay = $"현재 맵: {currentMapName}";
            if (mapNameText != null)
            {
                mapNameText.text = mapDisplay;
            }

            TerrainType currentTerrain = TilemapRangeManager.Instance?.GetTerrainTypeAtPosition(PlayerController.Instance.Position) ?? TerrainType.SAND;
            string terrainName = currentTerrain.ToString();
            var registry = GameDataRegistry.Instance;
            if (registry?.TerrainTypes != null)
            {
                var terrainDef = registry.TerrainTypes.GetTerrainById(currentTerrain);
                if (terrainDef != null)
                {
                    terrainName = terrainDef.koreanName;
                }
            }
            string terrainDisplay = $"현재 지형: {terrainName}";

            if (terrainTypeText != null)
            {
                terrainTypeText.text = terrainDisplay;
            }

            UpdateTrapAndPigeonCount();

            UpdateSpeciesProbabilities();
        }

        private void UpdateTrapAndPigeonCount()
        {
            if (PlayerController.Instance == null || TilemapRangeManager.Instance == null)
                return;

            string currentMapName = TilemapRangeManager.Instance.GetMapNameAtPosition(PlayerController.Instance.Position);
            if (string.IsNullOrEmpty(currentMapName) || currentMapName == "Unknown")
            {
                if (trapCountText != null)
                {
                    trapCountText.text = "덫: 없음";
                }
                if (pigeonCountText != null)
                {
                    pigeonCountText.text = "비둘기: 없음";
                }
                return;
            }

            int activeTrapCount = GetActiveTrapCountInMap(currentMapName);
            int maxTrapCount = UpgradeData.Instance != null ? UpgradeData.Instance.MaxTrapCount : 2;
            string trapDisplay = $"덫: {activeTrapCount}/{maxTrapCount}개";

            if (trapCountText != null)
            {
                trapCountText.text = trapDisplay;
            }

            int currentPigeonCount = GetPigeonCountInMap(currentMapName);
            int maxPigeonCount = GameManager.Instance != null ? GameManager.Instance.MaxPigeonsPerMap : 5;
            string pigeonDisplay = $"비둘기: {currentPigeonCount}/{maxPigeonCount}마리";

            if (pigeonCountText != null)
            {
                pigeonCountText.text = pigeonDisplay;
            }
        }

        private int GetActiveTrapCountInMap(string mapName)
        {
            if (string.IsNullOrEmpty(mapName) || TilemapRangeManager.Instance == null)
                return 0;

            FoodTrap[] allTraps = FindObjectsByType<FoodTrap>(FindObjectsSortMode.None);
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

        private int GetPigeonCountInMap(string mapName)
        {
            if (string.IsNullOrEmpty(mapName) || pigeonManager == null)
                return 0;

            return pigeonManager.GetPigeonCountInMap(mapName);
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

            List<KeyValuePair<PigeonSpecies, float>> sortedProbabilities = new List<KeyValuePair<PigeonSpecies, float>>(probabilities);
            sortedProbabilities.Sort((a, b) => b.Value.CompareTo(a.Value));

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
