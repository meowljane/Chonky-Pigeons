using UnityEngine;
using System.Collections.Generic;
using PigeonGame.Data;

namespace PigeonGame.Gameplay
{
    /// <summary>
    /// 월드 비둘기 관리 시스템 (타일맵 기반)
    /// 맵 이름별로 비둘기 수를 관리하고 자동 보충
    /// </summary>
    public class WorldPigeonManager : MonoBehaviour
    {
        [SerializeField] private GameObject pigeonPrefab;
        [SerializeField] private float spawnCheckInterval = 1f; // 비둘기 수 체크 간격 (초)
        [SerializeField] private float spawnChance = 0.1f; // 비둘기 수가 부족할 때 스폰 확률
        [SerializeField] private float despawnChance = 0.1f; // 비둘기가 나가는 확률 (초당)
        [SerializeField] private float forceFleeDespawnTime = 5f; // forceFlee 상태인 비둘기가 제거되기까지의 시간 (초)
        public static WorldPigeonManager Instance { get; private set; }

        // 맵 이름별 비둘기 관리
        private Dictionary<string, List<PigeonController>> pigeonsByMapName = new Dictionary<string, List<PigeonController>>();
        private Dictionary<PigeonController, string> pigeonToMapName = new Dictionary<PigeonController, string>();
        private float spawnCheckTimer = 0f;

        /// <summary>
        /// 싱글톤 인스턴스 초기화
        /// </summary>
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

        /// <summary>
        /// 게임 시작 시 초기화
        /// </summary>
        private void Start()
        {
            // 초기화는 필요시 자동으로 처리됨
        }

        /// <summary>
        /// 매 프레임마다 비둘기 제거 및 보충 체크
        /// </summary>
        private void Update()
        {
            if (pigeonPrefab == null || TilemapRangeManager.Instance == null)
                return;

            // forceFlee 상태인 비둘기 제거 관리
            CheckAndDespawnPigeons();

            // 주기적으로 비둘기 수 체크 및 보충
            spawnCheckTimer += Time.deltaTime;
            if (spawnCheckTimer >= spawnCheckInterval)
            {
                spawnCheckTimer = 0f;
                CheckAndRefillPigeons();
            }
        }

        /// <summary>
        /// 현재 맵당 비둘기 수 반환 (UpgradeData에서 계산된 최종 값 사용)
        /// </summary>
        private int GetPigeonsPerMap()
        {
            return GameManager.Instance?.MaxPigeonsPerMap ?? 5;
        }

        /// <summary>
        /// 비둘기 수 체크 및 보충/감소 관리
        /// </summary>
        private void CheckAndRefillPigeons()
        {
            if (TilemapRangeManager.Instance == null) return;

            // 모든 맵 이름 수집 (타일맵에서)
            HashSet<string> allMapNames = new HashSet<string>();
            var allPositions = TilemapRangeManager.Instance.GetAllValidPositionsInMapRange();
            foreach (var pos in allPositions)
            {
                string mapName = TilemapRangeManager.Instance.GetMapNameAtPosition(pos);
                if (!string.IsNullOrEmpty(mapName) && mapName != "Unknown")
                {
                    allMapNames.Add(mapName);
                }
            }

            // 각 맵별로 체크
            foreach (string mapName in allMapNames)
            {
                // 정확한 비둘기 수 계산 (전시관, Flee 상태 제외)
                int currentCount = GetPigeonCountInMap(mapName);
                int pigeonsPerMap = GetPigeonsPerMap();

                // 비둘기 수가 부족하면 보충
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
                // 비둘기 수가 넘치면 despawnChance에 따라 랜덤하게 한 마리만 flee 처리
                else if (currentCount > pigeonsPerMap)
                {
                    // 확률 체크로 한 마리만 flee 처리
                    if (Random.value < despawnChance)
                    {
                        var validPigeons = GetValidPigeonsForManagement(mapName);
                        if (validPigeons.Count > 0)
                        {
                            // 첫 번째 유효한 비둘기를 flee 처리
                            var ai = validPigeons[0].GetComponent<PigeonAI>();
                            if (ai != null)
                            {
                                ai.ForceFlee();
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 맵 이름의 비둘기 리스트 정리 (null 제거) 및 반환
        /// </summary>
        private List<PigeonController> GetCleanedPigeonList(string mapName)
        {
            if (!pigeonsByMapName.ContainsKey(mapName))
                pigeonsByMapName[mapName] = new List<PigeonController>();

            var pigeons = pigeonsByMapName[mapName];
            pigeons.RemoveAll(p => p == null || p.gameObject == null);
            return pigeons;
        }

        /// <summary>
        /// 유효한 비둘기만 필터링하여 반환 (전시관, Flee 상태 제외)
        /// </summary>
        private List<PigeonController> GetValidPigeonsForManagement(string mapName)
        {
            var pigeons = GetCleanedPigeonList(mapName);
            var validPigeons = new List<PigeonController>();
            
            foreach (var pigeon in pigeons)
            {
                if (pigeon.IsExhibitionPigeon)
                    continue;
                    
                var ai = pigeon.GetComponent<PigeonAI>();
                if (ai?.CurrentState == PigeonState.Flee)
                    continue;
                    
                validPigeons.Add(pigeon);
            }
            
            return validPigeons;
        }

        /// <summary>
        /// 특정 맵의 비둘기 수 반환 (Flee 상태 제외, 전시관 비둘기 제외)
        /// </summary>
        public int GetPigeonCountInMap(string mapName)
        {
            if (string.IsNullOrEmpty(mapName))
                return 0;

            var validPigeons = GetValidPigeonsForManagement(mapName);
            return validPigeons.Count;
        }

        /// <summary>
        /// 해금된 종만 필터링하여 반환
        /// </summary>
        private List<SpeciesDefinition> GetUnlockedSpecies(SpeciesDefinition[] allSpecies)
        {
            List<SpeciesDefinition> unlockedSpecies = new List<SpeciesDefinition>();
            foreach (var species in allSpecies)
            {
                if (GameManager.Instance.IsSpeciesUnlocked(species.speciesType))
                {
                    unlockedSpecies.Add(species);
                }
            }
            return unlockedSpecies;
        }

        /// <summary>
        /// 종별 가중치 계산
        /// 공식: w_i_final = w_i × (1 + 0.2⋅trapMatch_i + 0.2⋅terrainMatch_i) × upgradeFactor_i
        /// </summary>
        private float CalculateSpeciesWeight(SpeciesDefinition species, List<FoodTrap> activeTraps)
        {
            // 기본 가중치
            float baseWeight = species.baseSpawnWeight;
            
            // 업그레이드 배율 (upgradeFactor_i)
            float upgradeFactor = 1.0f;
            if (GameManager.Instance != null && UpgradeData.Instance != null)
            {
                upgradeFactor = UpgradeData.Instance.GetSpeciesWeightMultiplier(species.speciesType);
            }

            if (activeTraps == null || activeTraps.Count == 0)
            {
                // 덫이 없으면: w_i × upgradeFactor_i
                return baseWeight * upgradeFactor;
            }

            // 덫/지역 매칭 개수 계산
            int matchingTrapCount = 0; // 선호하는 덫 개수
            int matchingTerrainCount = 0; // 선호하는 지역 개수
            
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
            
            // 공식: w_i × (1 + 0.2⋅trapMatch_i + 0.2⋅terrainMatch_i) × upgradeFactor_i
            float trapBonus = 1.0f + (matchingTrapCount * 0.2f) + (matchingTerrainCount * 0.2f);
            float finalWeight = baseWeight * trapBonus * upgradeFactor;
            
            // 최소 0 이상 보장 (음수가 되지 않도록)
            return Mathf.Max(0f, finalWeight);
        }

        /// <summary>
        /// 맵 이름 내의 활성 덫 수집 (포획된 덫 포함)
        /// </summary>
        private List<FoodTrap> GetActiveTrapsInMap(string mapName)
        {
            FoodTrap[] allTraps = FindObjectsByType<FoodTrap>(FindObjectsSortMode.None);
            List<FoodTrap> activeTraps = new List<FoodTrap>();
            foreach (var trap in allTraps)
            {
                if (trap != null)
                {
                    if (TilemapRangeManager.Instance != null)
                    {
                        string trapMapName = TilemapRangeManager.Instance.GetMapNameAtPosition(trap.transform.position);
                        if (trapMapName == mapName)
                        {
                            activeTraps.Add(trap);
                        }
                    }
                }
            }
            return activeTraps;
        }

        /// <summary>
        /// 덫 선호도 및 지역 선호도를 고려하여 가중치 기반으로 비둘기 종 선택
        /// </summary>
        private SpeciesDefinition SelectSpeciesWithPreference(SpeciesDefinition[] allSpecies, string mapName)
        {
            List<FoodTrap> activeTraps = GetActiveTrapsInMap(mapName);

            List<float> weights = new List<float>();
            float totalWeight = 0f;
            foreach (var species in allSpecies)
            {
                float weight = CalculateSpeciesWeight(species, activeTraps);
                weights.Add(weight);
                totalWeight += weight;
            }

            float randomValue = Random.Range(0f, totalWeight);
            float currentWeight = 0f;
            for (int i = 0; i < allSpecies.Length; i++)
            {
                currentWeight += weights[i];
                if (randomValue <= currentWeight)
                    return allSpecies[i];
            }
            return allSpecies[allSpecies.Length - 1];
        }

        /// <summary>
        /// 지정된 위치에 비둘기 스폰 (맵 이름 필수, 최대 수 제한 적용)
        /// </summary>
        public void SpawnPigeonAtPosition(Vector3 position, string mapName, int count = 1)
        {
            if (pigeonPrefab == null || string.IsNullOrEmpty(mapName))
                return;

            // 현재 맵의 비둘기 수 확인 (정확한 카운트)
            int currentCount = GetPigeonCountInMap(mapName);

            // 최대 수를 넘지 않도록 스폰할 수 있는 수 계산
            int pigeonsPerMap = GetPigeonsPerMap();
            int availableSlots = pigeonsPerMap - currentCount;
            int spawnCount = Mathf.Min(count, availableSlots);

            if (spawnCount <= 0)
                return;

            var registry = GameDataRegistry.Instance;
            var allSpecies = registry?.SpeciesSet?.species;
            if (allSpecies == null || allSpecies.Length == 0)
                return;

            // 해금된 종만 필터링
            List<SpeciesDefinition> unlockedSpecies = GetUnlockedSpecies(allSpecies);
            if (unlockedSpecies.Count == 0)
                return;

            // 지정된 개수만큼 스폰
            for (int i = 0; i < spawnCount; i++)
            {
                Vector3 spawnPos = GetRandomPositionInMap(mapName);
                if (spawnPos == Vector3.zero)
                    spawnPos = position; // 랜덤 위치를 찾지 못하면 지정된 위치 사용
                    
                SpeciesDefinition selectedSpecies = SelectSpeciesWithPreference(unlockedSpecies.ToArray(), mapName);
                CreateAndRegisterPigeon(selectedSpecies, spawnPos, mapName);
            }
        }

        /// <summary>
        /// 비둘기 생성 및 맵에 등록
        /// </summary>
        private void CreateAndRegisterPigeon(SpeciesDefinition species, Vector3 position, string mapName)
        {
            var registry = GameDataRegistry.Instance;
            
            // 무게를 소수점 첫째 자리까지 랜덤 생성 (1.0~5.0), 내부 저장은 정수로 (1~5)
            float weightKg = Random.Range(1.0f, 5.1f); // 5.1은 exclusive이므로 5.0까지
            weightKg = Mathf.Round(weightKg * 10f) / 10f; // 소수점 첫째 자리로 반올림
            int obesity = Mathf.RoundToInt(weightKg); // 정수 부분만 저장 (1~5)
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

        /// <summary>
        /// 특정 맵 내의 랜덤 위치 반환
        /// </summary>
        private Vector3 GetRandomPositionInMap(string mapName)
        {
            if (TilemapRangeManager.Instance == null) return Vector3.zero;

            var allPositions = TilemapRangeManager.Instance.GetAllValidPositionsInMapRange();
            List<Vector3> validPositions = new List<Vector3>();
            
            foreach (var pos in allPositions)
            {
                string posMapName = TilemapRangeManager.Instance.GetMapNameAtPosition(pos);
                if (posMapName == mapName)
                {
                    validPositions.Add(pos);
                }
            }

            if (validPositions.Count == 0) return Vector3.zero;

            return validPositions[Random.Range(0, validPositions.Count)];
        }

        /// <summary>
        /// Flee 상태인 비둘기 제거 관리 (forceFleeDespawnTime 경과 후 제거)
        /// </summary>
        private void CheckAndDespawnPigeons()
        {
            // 모든 맵의 비둘기 리스트를 복사해서 순회 (순회 중 수정 방지)
            var allPigeons = new List<PigeonController>();
            foreach (var pigeons in pigeonsByMapName.Values)
            {
                allPigeons.AddRange(pigeons);
            }

            foreach (var pigeon in allPigeons)
            {
                if (pigeon == null || pigeon.gameObject == null)
                {
                    RemovePigeonFromMap(pigeon);
                    continue;
                }

                var ai = pigeon.GetComponent<PigeonAI>();
                if (ai == null)
                    continue;
                
                // Flee 상태인 비둘기가 forceFleeDespawnTime 경과 후 제거
                if (ai.CurrentState == PigeonState.Flee && ai.FleeElapsedTime >= forceFleeDespawnTime)
                {
                    RemovePigeonFromMap(pigeon);
                    Destroy(pigeon.gameObject);
                }
            }
        }

        /// <summary>
        /// 비둘기가 속한 맵 이름 가져오기
        /// </summary>
        public string GetMapNameForPigeon(PigeonController pigeon)
        {
            if (pigeon != null && pigeonToMapName.ContainsKey(pigeon))
            {
                return pigeonToMapName[pigeon];
            }
            return null;
        }

        /// <summary>
        /// 비둘기를 맵 관리 딕셔너리에서 제거
        /// </summary>
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
        }

        /// <summary>
        /// 현재 플레이어 위치의 맵에서 종별 스폰 확률 계산 (UI 표시용, 덫/지역 보너스 및 업그레이드 보너스 반영)
        /// </summary>
        public Dictionary<PigeonSpecies, float> GetSpeciesSpawnProbabilities()
        {
            var registry = GameDataRegistry.Instance;
            var allSpecies = registry?.SpeciesSet?.species;
            if (allSpecies == null)
                return new Dictionary<PigeonSpecies, float>();
            
            // 현재 플레이어 위치의 맵 이름 가져오기
            string currentMapName = TilemapRangeManager.Instance?.GetMapNameAtPosition(PlayerController.Instance?.Position ?? Vector3.zero);
            if (string.IsNullOrEmpty(currentMapName) || currentMapName == "Unknown")
            {
                return new Dictionary<PigeonSpecies, float>();
            }
            
            List<FoodTrap> activeTraps = GetActiveTrapsInMap(currentMapName);

            // 해금된 종만 필터링
            List<SpeciesDefinition> unlockedSpecies = GetUnlockedSpecies(allSpecies);

            Dictionary<PigeonSpecies, float> weights = new Dictionary<PigeonSpecies, float>();
            float totalWeight = 0f;
            
            foreach (var species in unlockedSpecies)
            {
                // 항상 CalculateSpeciesWeight를 호출하여 업그레이드 보너스도 적용
                float weight = CalculateSpeciesWeight(species, activeTraps);
                weights[species.speciesType] = weight;
                totalWeight += weight;
            }

            Dictionary<PigeonSpecies, float> probabilities = new Dictionary<PigeonSpecies, float>();
            foreach (var kvp in weights)
            {
                float prob = (kvp.Value / totalWeight) * 100f;
                probabilities[kvp.Key] = prob;
            }

            return probabilities;
        }
    }
}
