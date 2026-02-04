using UnityEngine;
using System.Collections.Generic;
using PigeonGame.Data;

namespace PigeonGame.Gameplay
{
    public class WorldPigeonManager : MonoBehaviour
    {
        [SerializeField] private GameObject pigeonPrefab;
        [SerializeField] private float spawnCheckInterval = 1f; 
        [SerializeField] private float spawnChance = 0.1f; 
        [SerializeField] private float despawnChance = 0.1f; 
        [SerializeField] private float forceFleeDespawnTime = 5f; 
        public static WorldPigeonManager Instance { get; private set; }

        private Dictionary<string, List<PigeonController>> pigeonsByMapName = new Dictionary<string, List<PigeonController>>();
        private Dictionary<PigeonController, string> pigeonToMapName = new Dictionary<PigeonController, string>();
        private Dictionary<PigeonController, PigeonAI> pigeonAICache = new Dictionary<PigeonController, PigeonAI>(); 
        private float spawnCheckTimer = 0f;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            UpdateMapNameCache();
        }

        private void Update()
        {
            if (pigeonPrefab == null || TilemapRangeManager.Instance == null)
                return;

            CheckAndDespawnPigeons();

            spawnCheckTimer += Time.deltaTime;
            if (spawnCheckTimer >= spawnCheckInterval)
            {
                spawnCheckTimer = 0f;
                CheckAndRefillPigeons();
            }
        }

        private int GetPigeonsPerMap()
        {
            return GameManager.Instance?.MaxPigeonsPerMap ?? 5;
        }

        private HashSet<string> cachedMapNames = new HashSet<string>();
        private float mapNameCacheUpdateTimer = 0f;
        private const float MAP_NAME_CACHE_UPDATE_INTERVAL = 5f; 

        private void UpdateMapNameCache()
        {
            if (TilemapRangeManager.Instance == null) return;
            
            cachedMapNames.Clear();
            var allPositions = TilemapRangeManager.Instance.GetAllValidPositionsInMapRange();
            foreach (var pos in allPositions)
            {
                string mapName = TilemapRangeManager.Instance.GetMapNameAtPosition(pos);
                if (!string.IsNullOrEmpty(mapName) && mapName != "Unknown")
                {
                    cachedMapNames.Add(mapName);
                }
            }
        }

        private void CheckAndRefillPigeons()
        {
            if (TilemapRangeManager.Instance == null) return;

            if (cachedMapNames.Count == 0)
            {
                UpdateMapNameCache();
            }

            mapNameCacheUpdateTimer += Time.deltaTime;
            if (mapNameCacheUpdateTimer >= MAP_NAME_CACHE_UPDATE_INTERVAL)
            {
                mapNameCacheUpdateTimer = 0f;
                UpdateMapNameCache();
            }

            HashSet<string> allMapNames = cachedMapNames;

            foreach (string mapName in allMapNames)
            {
                int currentCount = GetPigeonCountInMap(mapName);
                int pigeonsPerMap = GetPigeonsPerMap();

                if (currentCount < pigeonsPerMap)
                {
                    if (Random.value < spawnChance)
                    {
                        Vector3 spawnPos = GetRandomPositionInMap(mapName);
                        if (spawnPos != Vector3.zero)
                        {
                            SpawnPigeonAtPosition(spawnPos, mapName);
                        }
                    }
                }
                else if (currentCount > pigeonsPerMap)
                {
                    if (Random.value < despawnChance)
                    {
                        var validPigeons = GetValidPigeonsForManagement(mapName);
                        if (validPigeons.Count > 0)
                        {
                            var pigeon = validPigeons[0];
                            if (!pigeonAICache.TryGetValue(pigeon, out PigeonAI ai))
                            {
                                ai = pigeon.GetComponent<PigeonAI>();
                                if (ai != null)
                                    pigeonAICache[pigeon] = ai;
                            }
                            if (ai != null)
                            {
                                ai.ForceFlee();
                            }
                        }
                    }
                }
            }
        }

        private List<PigeonController> GetCleanedPigeonList(string mapName)
        {
            if (!pigeonsByMapName.ContainsKey(mapName))
                pigeonsByMapName[mapName] = new List<PigeonController>();

            var pigeons = pigeonsByMapName[mapName];
            pigeons.RemoveAll(p => p == null || p.gameObject == null);
            return pigeons;
        }

        private List<PigeonController> GetValidPigeonsForManagement(string mapName)
        {
            var pigeons = GetCleanedPigeonList(mapName);
            var validPigeons = new List<PigeonController>();

            foreach (var pigeon in pigeons)
            {
                if (pigeon.IsExhibitionPigeon)
                    continue;

                if (!pigeonAICache.TryGetValue(pigeon, out PigeonAI ai))
                {
                    ai = pigeon.GetComponent<PigeonAI>();
                    if (ai != null)
                        pigeonAICache[pigeon] = ai;
                }
                if (ai?.CurrentState == PigeonState.Flee)
                    continue;

                validPigeons.Add(pigeon);
            }

            return validPigeons;
        }

        public int GetPigeonCountInMap(string mapName)
        {
            if (string.IsNullOrEmpty(mapName))
                return 0;

            var validPigeons = GetValidPigeonsForManagement(mapName);
            return validPigeons.Count;
        }

        private List<SpeciesDefinition> reusableUnlockedSpeciesList = new List<SpeciesDefinition>();

        private List<SpeciesDefinition> GetUnlockedSpecies(SpeciesDefinition[] allSpecies)
        {
            reusableUnlockedSpeciesList.Clear();
            foreach (var species in allSpecies)
            {
                if (GameManager.Instance.IsSpeciesUnlocked(species.speciesType))
                {
                    reusableUnlockedSpeciesList.Add(species);
                }
            }
            return reusableUnlockedSpeciesList;
        }

        private float CalculateSpeciesWeight(SpeciesDefinition species, List<FoodTrap> activeTraps)
        {
            float baseWeight = species.baseSpawnWeight;

            float upgradeFactor = 1.0f;
            if (GameManager.Instance != null && UpgradeData.Instance != null)
            {
                upgradeFactor = UpgradeData.Instance.GetSpeciesWeightMultiplier(species.speciesType);
            }

            if (activeTraps == null || activeTraps.Count == 0)
            {
                return baseWeight * upgradeFactor;
            }

            int matchingTrapCount = 0; 
            int matchingTerrainCount = 0; 

            foreach (var trap in activeTraps)
            {
                bool isFavoriteTrap = trap.TrapId == species.favoriteTrapType;
                TerrainType terrainType = MapManager.Instance != null ? MapManager.Instance.GetTerrainTypeAtPosition(trap.transform.position) : TerrainType.SAND;
                bool isFavoriteTerrain = terrainType == species.favoriteTerrain;

                if (isFavoriteTrap)
                    matchingTrapCount++;
                if (isFavoriteTerrain)
                    matchingTerrainCount++;
            }

            float trapBonus = 1.0f + (matchingTrapCount * 0.2f) + (matchingTerrainCount * 0.2f);
            float finalWeight = baseWeight * trapBonus * upgradeFactor;

            return Mathf.Max(0f, finalWeight);
        }

        private List<FoodTrap> cachedTraps = new List<FoodTrap>();
        private List<FoodTrap> reusableActiveTrapsList = new List<FoodTrap>(); 
        private float trapCacheUpdateTimer = 0f;
        private const float TRAP_CACHE_UPDATE_INTERVAL = 2f; 

        private List<FoodTrap> GetActiveTrapsInMap(string mapName)
        {
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

            reusableActiveTrapsList.Clear();
            if (TilemapRangeManager.Instance == null)
                return reusableActiveTrapsList;

            foreach (var trap in cachedTraps)
            {
                if (trap == null) continue;

                string trapMapName = TilemapRangeManager.Instance.GetMapNameAtPosition(trap.transform.position);
                if (trapMapName == mapName)
                {
                    reusableActiveTrapsList.Add(trap);
                }
            }
            return reusableActiveTrapsList;
        }

        private List<float> reusableWeightsList = new List<float>();

        private SpeciesDefinition SelectSpeciesWithPreference(SpeciesDefinition[] allSpecies, string mapName)
        {
            List<FoodTrap> activeTraps = GetActiveTrapsInMap(mapName);

            reusableWeightsList.Clear();
            float totalWeight = 0f;
            foreach (var species in allSpecies)
            {
                float weight = CalculateSpeciesWeight(species, activeTraps);
                reusableWeightsList.Add(weight);
                totalWeight += weight;
            }

            float randomValue = Random.Range(0f, totalWeight);
            float currentWeight = 0f;
            for (int i = 0; i < allSpecies.Length; i++)
            {
                currentWeight += reusableWeightsList[i];
                if (randomValue <= currentWeight)
                    return allSpecies[i];
            }
            return allSpecies[allSpecies.Length - 1];
        }

        public void SpawnPigeonAtPosition(Vector3 position, string mapName, int count = 1)
        {
            if (pigeonPrefab == null || string.IsNullOrEmpty(mapName))
                return;

            int currentCount = GetPigeonCountInMap(mapName);
            int pigeonsPerMap = GetPigeonsPerMap();
            int availableSlots = pigeonsPerMap - currentCount;
            int spawnCount = Mathf.Min(count, availableSlots);

            if (spawnCount <= 0)
                return;

            var registry = GameDataRegistry.Instance;
            var allSpecies = registry?.SpeciesSet?.species;
            if (allSpecies == null || allSpecies.Length == 0)
                return;

            List<SpeciesDefinition> unlockedSpecies = GetUnlockedSpecies(allSpecies);
            if (unlockedSpecies.Count == 0)
                return;

            for (int i = 0; i < spawnCount; i++)
            {
                Vector3 spawnPos = GetRandomPositionInMap(mapName);
                if (spawnPos == Vector3.zero)
                    spawnPos = position; 

                SpeciesDefinition selectedSpecies = SelectSpeciesWithPreference(unlockedSpecies.ToArray(), mapName);
                CreateAndRegisterPigeon(selectedSpecies, spawnPos, mapName);
            }
        }

        private void CreateAndRegisterPigeon(SpeciesDefinition species, Vector3 position, string mapName)
        {
            var registry = GameDataRegistry.Instance;

            float weightKg = Random.Range(1.0f, 5.1f); 
            weightKg = Mathf.Round(weightKg * 10f) / 10f; 
            int obesity = Mathf.RoundToInt(weightKg); 
            var allFaces = registry.Faces.faces;
            var selectedFace = allFaces[Random.Range(0, allFaces.Length)];
            FaceType faceType = selectedFace.faceType;

            PigeonSpecies speciesType = species.speciesType;
            Vector3 spawnPosition = new Vector3(position.x, position.y, 0);
            GameObject pigeonObj = Instantiate(pigeonPrefab, spawnPosition, Quaternion.identity);
            pigeonObj.SetActive(true);

            PigeonController controller = pigeonObj.GetComponent<PigeonController>();
            controller.Initialize(PigeonInstanceFactory.CreateInstanceStats(speciesType, obesity, weightKg, faceType));

            if (!pigeonsByMapName.ContainsKey(mapName))
                pigeonsByMapName[mapName] = new List<PigeonController>();

            pigeonsByMapName[mapName].Add(controller);
            pigeonToMapName[controller] = mapName;
        }

        private List<Vector3> reusableValidPositionsList = new List<Vector3>();

        private Vector3 GetRandomPositionInMap(string mapName)
        {
            if (TilemapRangeManager.Instance == null) return Vector3.zero;

            var allPositions = TilemapRangeManager.Instance.GetAllValidPositionsInMapRange();
            reusableValidPositionsList.Clear();

            foreach (var pos in allPositions)
            {
                string posMapName = TilemapRangeManager.Instance.GetMapNameAtPosition(pos);
                if (posMapName == mapName)
                {
                    reusableValidPositionsList.Add(pos);
                }
            }

            if (reusableValidPositionsList.Count == 0) return Vector3.zero;

            return reusableValidPositionsList[Random.Range(0, reusableValidPositionsList.Count)];
        }

        private List<PigeonController> reusableAllPigeonsList = new List<PigeonController>();

        private void CheckAndDespawnPigeons()
        {
            reusableAllPigeonsList.Clear();
            foreach (var pigeons in pigeonsByMapName.Values)
            {
                reusableAllPigeonsList.AddRange(pigeons);
            }

            foreach (var pigeon in reusableAllPigeonsList)
            {
                if (pigeon == null || pigeon.gameObject == null)
                {
                    RemovePigeonFromMap(pigeon);
                    continue;
                }

                if (!pigeonAICache.TryGetValue(pigeon, out PigeonAI ai))
                {
                    ai = pigeon.GetComponent<PigeonAI>();
                    if (ai == null)
                        continue;
                    pigeonAICache[pigeon] = ai;
                }

                if (ai.CurrentState == PigeonState.Flee && ai.FleeElapsedTime >= forceFleeDespawnTime)
                {
                    RemovePigeonFromMap(pigeon);
                    pigeonAICache.Remove(pigeon);
                    Destroy(pigeon.gameObject);
                }
            }
        }

        public string GetMapNameForPigeon(PigeonController pigeon)
        {
            if (pigeon != null && pigeonToMapName.ContainsKey(pigeon))
            {
                return pigeonToMapName[pigeon];
            }
            return null;
        }

        private void RemovePigeonFromMap(PigeonController pigeon)
        {
            if (pigeon == null)
                return;

            if (pigeonToMapName.TryGetValue(pigeon, out string mapName))
            {
                if (pigeonsByMapName.ContainsKey(mapName))
                {
                    pigeonsByMapName[mapName].Remove(pigeon);
                }
                pigeonToMapName.Remove(pigeon);
            }
            pigeonAICache.Remove(pigeon);
        }

        private Dictionary<PigeonSpecies, float> reusableWeightsDict = new Dictionary<PigeonSpecies, float>();
        private Dictionary<PigeonSpecies, float> reusableProbabilitiesDict = new Dictionary<PigeonSpecies, float>();

        public Dictionary<PigeonSpecies, float> GetSpeciesSpawnProbabilities()
        {
            var registry = GameDataRegistry.Instance;
            var allSpecies = registry?.SpeciesSet?.species;
            if (allSpecies == null)
            {
                reusableProbabilitiesDict.Clear();
                return reusableProbabilitiesDict;
            }

            string currentMapName = TilemapRangeManager.Instance?.GetMapNameAtPosition(PlayerController.Instance?.Position ?? Vector3.zero);
            if (string.IsNullOrEmpty(currentMapName) || currentMapName == "Unknown")
            {
                reusableProbabilitiesDict.Clear();
                return reusableProbabilitiesDict;
            }

            List<FoodTrap> activeTraps = GetActiveTrapsInMap(currentMapName);

            List<SpeciesDefinition> unlockedSpecies = GetUnlockedSpecies(allSpecies);

            reusableWeightsDict.Clear();
            float totalWeight = 0f;

            foreach (var species in unlockedSpecies)
            {
                float weight = CalculateSpeciesWeight(species, activeTraps);
                reusableWeightsDict[species.speciesType] = weight;
                totalWeight += weight;
            }

            reusableProbabilitiesDict.Clear();
            foreach (var kvp in reusableWeightsDict)
            {
                float prob = (kvp.Value / totalWeight) * 100f;
                reusableProbabilitiesDict[kvp.Key] = prob;
            }

            return reusableProbabilitiesDict;
        }
    }
}
