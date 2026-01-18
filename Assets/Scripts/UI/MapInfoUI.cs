using UnityEngine;
using UnityEngine.UI;
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
        [SerializeField] private Text terrainTypeText; // 현재 terrain 타입 표시 (Legacy Text)
        [SerializeField] private TextMeshProUGUI terrainTypeTextMesh; // 현재 terrain 타입 표시 (TextMeshPro)
        [SerializeField] private Text mapNameText; // 현재 맵 이름 표시 (Legacy Text)
        [SerializeField] private TextMeshProUGUI mapNameTextMesh; // 현재 맵 이름 표시 (TextMeshPro)
        [SerializeField] private Transform speciesProbabilityContainer; // 종별 확률 표시 컨테이너
        [SerializeField] private GameObject speciesProbabilityItemPrefab; // 종별 확률 아이템 프리팹
        [SerializeField] private float updateInterval = 0.5f; // 업데이트 간격 (초)

        private WorldPigeonManager pigeonManager;
        private float updateTimer = 0f;
        private List<GameObject> probabilityItemObjects = new List<GameObject>();

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
            if (mapNameTextMesh != null)
            {
                mapNameTextMesh.text = mapDisplay;
            }

            // 현재 플레이어 위치의 terrain 타입 표시
            TerrainType currentTerrain = pigeonManager.GetTerrainTypeAtPosition(PlayerController.Instance.Position);
            string terrainDisplay = $"Terrain: {currentTerrain}";
            
            if (terrainTypeText != null)
            {
                terrainTypeText.text = terrainDisplay;
            }
            if (terrainTypeTextMesh != null)
            {
                terrainTypeTextMesh.text = terrainDisplay;
            }

            // 현재 맵의 종별 스폰 확률 표시
            UpdateSpeciesProbabilities();
        }

        private void UpdateSpeciesProbabilities()
        {
            if (PlayerController.Instance == null || pigeonManager == null || speciesProbabilityContainer == null)
                return;

            UIHelper.ClearSlotList(probabilityItemObjects);

            var probabilities = pigeonManager.GetSpeciesSpawnProbabilities(PlayerController.Instance.CurrentMapCollider);
            if (probabilities == null || probabilities.Count == 0) return;

            var registry = GameDataRegistry.Instance;
            if (registry?.SpeciesSet == null) return;

            // 확률이 높은 순으로 정렬
            List<KeyValuePair<PigeonSpecies, float>> sortedProbabilities = new List<KeyValuePair<PigeonSpecies, float>>(probabilities);
            sortedProbabilities.Sort((a, b) => b.Value.CompareTo(a.Value));

            foreach (var kvp in sortedProbabilities)
            {
                var species = registry.SpeciesSet.GetSpeciesById(kvp.Key);
                if (species == null) continue;

                GameObject itemObj = speciesProbabilityItemPrefab != null
                    ? Instantiate(speciesProbabilityItemPrefab, speciesProbabilityContainer, false)
                    : CreateProbabilityItemFallback(species.name, kvp.Value);
                
                if (itemObj != null)
                {
                    if (speciesProbabilityItemPrefab == null)
                        itemObj.transform.SetParent(speciesProbabilityContainer, false);
                    SetupProbabilityItem(itemObj, species.name, kvp.Value);
                    probabilityItemObjects.Add(itemObj);
                }
            }
        }

        private void SetupProbabilityItem(GameObject itemObj, string speciesName, float probability)
        {
            // TextMeshProUGUI 우선, 없으면 Legacy Text 사용
            TextMeshProUGUI textMesh = itemObj.GetComponentInChildren<TextMeshProUGUI>();
            if (textMesh != null)
            {
                textMesh.text = $"{speciesName}: {probability:F0}%";
                return;
            }

            Text textComponent = itemObj.GetComponentInChildren<Text>();
            if (textComponent != null)
            {
                textComponent.text = $"{speciesName}: {probability:F0}%";
            }
        }

        /// <summary>
        /// 프리팹이 없을 때 대체 아이템 생성
        /// </summary>
        private GameObject CreateProbabilityItemFallback(string speciesName, float probability)
        {
            GameObject itemObj = new GameObject($"ProbabilityItem_{speciesName}");
            RectTransform rect = itemObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(290f, 25f);

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(itemObj.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;

            TextMeshProUGUI textMesh = textObj.AddComponent<TextMeshProUGUI>();
            textMesh.text = $"{speciesName}: {probability:F0}%";
            textMesh.fontSize = 14f;
            textMesh.color = Color.white;

            return itemObj;
        }
    }
}
