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
        [SerializeField] private TrapPlacer trapPlacer;

        private float updateTimer = 0f;

        private void Start()
        {
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

            string currentMapName = GetCurrentMapName();

            if (mapNameText != null)
                mapNameText.text = $"현재 맵: {currentMapName}";

            TerrainType currentTerrain = TilemapRangeManager.Instance?.GetTerrainTypeAtPosition(PlayerController.Instance.Position) ?? TerrainType.SAND;
            if (terrainTypeText != null)
                terrainTypeText.text = $"현재 지형: {UIHelper.GetTerrainName(currentTerrain)}";

            UpdateTrapAndPigeonCount(currentMapName);

            UpdateSpeciesProbabilities();
        }

        private string GetCurrentMapName()
        {
            if (TilemapRangeManager.Instance == null)
                return "없음";

            string mapName = TilemapRangeManager.Instance.GetMapNameAtPosition(PlayerController.Instance.Position);
            if (string.IsNullOrEmpty(mapName) || mapName == "Unknown")
                return "없음";

            return mapName;
        }

        private void UpdateTrapAndPigeonCount(string currentMapName)
        {
            if (currentMapName == "없음")
            {
                if (trapCountText != null) trapCountText.text = "덫: 없음";
                if (pigeonCountText != null) pigeonCountText.text = "비둘기: 없음";
                return;
            }

            int activeTrapCount = GetActiveTrapCountInMap(currentMapName);
            int maxTrapCount = UpgradeData.Instance?.MaxTrapCount ?? 2;
            if (trapCountText != null)
                trapCountText.text = $"덫: {activeTrapCount}/{maxTrapCount}개";

            int currentPigeonCount = GetPigeonCountInMap(currentMapName);
            int maxPigeonCount = GameManager.Instance?.MaxPigeonsPerMap ?? 5;
            if (pigeonCountText != null)
                pigeonCountText.text = $"비둘기: {currentPigeonCount}/{maxPigeonCount}마리";
        }

        private int GetActiveTrapCountInMap(string mapName)
        {
            if (string.IsNullOrEmpty(mapName))
                return 0;

            if (trapPlacer != null)
                return trapPlacer.GetActiveTrapCountInMap(mapName);

            if (TrapPlacer.Instance != null)
                return TrapPlacer.Instance.GetActiveTrapCountInMap(mapName);

            return 0;
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
