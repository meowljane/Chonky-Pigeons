using UnityEngine;
using System.Collections.Generic;
using PigeonGame.Data;

namespace PigeonGame.Gameplay
{
    /// <summary>
    /// 월드 비둘기 관리 시스템
    /// 맵 콜라이더별로 비둘기 수를 관리하고 자동 보충
    /// </summary>
    public class WorldPigeonManager : MonoBehaviour
    {
        [SerializeField] private GameObject pigeonPrefab;
        [SerializeField] private int pigeonsPerMap = 20; // 각 맵 콜라이더당 초기 비둘기 수
        [SerializeField] private float spawnCheckInterval = 5f; // 비둘기 수 체크 간격 (초)
        [SerializeField] private float spawnChance = 0.3f; // 비둘기 수가 부족할 때 스폰 확률
        [SerializeField] private float despawnChance = 0.1f; // 비둘기가 나가는 확률 (초당)
        [SerializeField] private float forceFleeDespawnTime = 5f; // forceFlee 상태인 비둘기가 제거되기까지의 시간 (초)
        public static WorldPigeonManager Instance { get; private set; }
        
        /// <summary>
        /// MapManager에서 맵 콜라이더 가져오기
        /// </summary>
        public Collider2D[] MapColliders
        {
            get
            {
                if (MapManager.Instance != null)
                {
                    var colliders = MapManager.Instance.GetAllMapColliders();
                    if (colliders != null && colliders.Length > 0)
                        return colliders;
                }
                return null;
            }
        }

        // 맵 콜라이더별 비둘기 관리
        private Dictionary<Collider2D, List<PigeonController>> pigeonsByMap = new Dictionary<Collider2D, List<PigeonController>>();
        private Dictionary<PigeonController, Collider2D> pigeonToMap = new Dictionary<PigeonController, Collider2D>();
        private float spawnCheckTimer = 0f;
        private TerrainArea[] allTerrainAreas; // 모든 terrain 영역

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
            // 모든 terrain 영역 찾기
            allTerrainAreas = FindObjectsByType<TerrainArea>(FindObjectsSortMode.None);

            // 맵 콜라이더별 비둘기 리스트 초기화
            var mapColliders = MapColliders;
            if (mapColliders != null)
            {
                foreach (var mapCollider in mapColliders)
                {
                    if (mapCollider != null)
                    {
                        pigeonsByMap[mapCollider] = new List<PigeonController>();
                    }
                }
            }

            // 게임 시작 시 각 맵에 초기 비둘기 스폰
            SpawnInitialPigeons();
        }

        private void Update()
        {
            if (pigeonPrefab == null)
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
        /// 게임 시작 시 각 맵에 초기 비둘기 스폰
        /// </summary>
        private void SpawnInitialPigeons()
        {
            if (pigeonPrefab == null)
                return;

            var registry = GameDataRegistry.Instance;
            if (registry == null || registry.SpeciesSet == null)
                return;

            // 각 맵 콜라이더마다 비둘기 스폰
            var mapColliders = MapColliders;
            if (mapColliders != null)
            {
                foreach (var mapCollider in mapColliders)
                {
                    if (mapCollider == null)
                        continue;

                    for (int i = 0; i < pigeonsPerMap; i++)
                    {
                        Vector3 spawnPos = GetRandomPositionInCollider(mapCollider);
                        SpawnPigeonAtPosition(spawnPos, mapCollider);
                    }
                }
            }
        }

        /// <summary>
        /// 비둘기 수 체크 및 보충
        /// </summary>
        private void CheckAndRefillPigeons()
        {
            var registry = GameDataRegistry.Instance;
            if (registry == null || registry.SpeciesSet == null)
                return;

            // 각 맵 콜라이더별로 체크
            foreach (var kvp in pigeonsByMap)
            {
                var mapCollider = kvp.Key;
                var pigeons = kvp.Value;

                if (mapCollider == null)
                    continue;

                // null인 비둘기 제거
                pigeons.RemoveAll(p => p == null || p.gameObject == null);

                // 비둘기 수가 부족하면 보충
                int currentCount = pigeons.Count;
                if (currentCount < pigeonsPerMap)
                {
                    if (Random.value < spawnChance)
                    {
                        Vector3 spawnPos = GetRandomPositionInCollider(mapCollider);
                        SpawnPigeonAtPosition(spawnPos, mapCollider, true); // 보충 스폰은 확률 보정 적용
                    }
                }
            }
        }

        /// <summary>
        /// 지정된 위치에 비둘기 스폰
        /// </summary>
        private void SpawnPigeonAtPosition(Vector3 position, Collider2D mapCollider, bool applyPreferenceBonus = false)
        {
            if (pigeonPrefab == null || mapCollider == null)
                return;

            var registry = GameDataRegistry.Instance;
            if (registry == null || registry.SpeciesSet == null || registry.Faces == null)
                return;

            var allSpecies = registry.SpeciesSet.species;
            if (allSpecies.Length == 0)
                return;

            // 해금된 종만 필터링
            List<SpeciesDefinition> unlockedSpecies = new List<SpeciesDefinition>();
            if (GameManager.Instance != null)
            {
                foreach (var species in allSpecies)
                {
                    if (species != null && GameManager.Instance.IsSpeciesUnlocked(species.speciesType))
                    {
                        unlockedSpecies.Add(species);
                    }
                }
            }

            // 해금된 종이 없으면 스폰하지 않음
            if (unlockedSpecies.Count == 0)
                return;

            // 비둘기 종 선택 (덫 선호도 기반 확률 보정) - 해당 맵의 덫만 고려
            SpeciesDefinition selectedSpecies = SelectSpeciesWithPreference(unlockedSpecies.ToArray(), position, mapCollider, applyPreferenceBonus);
            if (selectedSpecies == null && unlockedSpecies.Count > 0)
                selectedSpecies = unlockedSpecies[Random.Range(0, unlockedSpecies.Count)]; // 폴백

            int obesity = Random.Range(1, 6); // 모든 종류는 1~5 비만도 중 랜덤
            
            // 랜덤 얼굴 선택
            var allFaces = registry.Faces.faces;
            FaceType faceType = FaceType.F00; // 기본값
            if (allFaces != null && allFaces.Length > 0)
            {
                var selectedFace = allFaces[Random.Range(0, allFaces.Length)];
                if (selectedFace != null)
                {
                    faceType = selectedFace.faceType;
                }
            }

            // speciesType 사용
            PigeonSpecies speciesType = selectedSpecies != null ? selectedSpecies.speciesType : PigeonSpecies.SP01;

            // Z 위치를 0으로 명시적으로 설정 (2D 게임용)
            Vector3 spawnPosition = new Vector3(position.x, position.y, 0);
            GameObject pigeonObj = Instantiate(pigeonPrefab, spawnPosition, Quaternion.identity);
            
            if (!pigeonObj.activeSelf)
            {
                pigeonObj.SetActive(true);
            }
            
            PigeonController controller = pigeonObj.GetComponent<PigeonController>();
            
            if (controller != null)
            {
                var stats = PigeonInstanceFactory.CreateInstanceStats(
                    speciesType, 
                    obesity, 
                    faceType
                );
                
                controller.Initialize(stats);
                
                // 맵별 비둘기 리스트에 추가
                if (!pigeonsByMap.ContainsKey(mapCollider))
                {
                    pigeonsByMap[mapCollider] = new List<PigeonController>();
                }
                pigeonsByMap[mapCollider].Add(controller);
                pigeonToMap[controller] = mapCollider;
            }
        }

        /// <summary>
        /// 종별 가중치 계산 (단순화된 로직: 각 종마다 좋아하는 덫과 terrain 하나씩만)
        /// </summary>
        private float CalculateSpeciesWeight(SpeciesDefinition species, List<FoodTrap> activeTraps)
        {
            if (species == null || activeTraps == null || activeTraps.Count == 0)
                return species != null ? species.baseSpawnWeight : 0f;

            float weight = species.baseSpawnWeight;
            
            // 각 덫을 확인하여 가중치 증가
            int matchingTrapCount = 0; // 좋아하는 덫 개수
            int matchingTerrainCount = 0; // 좋아하는 terrain 개수
            int perfectMatchCount = 0; // 덫과 terrain 둘 다 맞는 개수
            
            foreach (var trap in activeTraps)
            {
                // favoriteTrapType enum 사용
                bool isFavoriteTrap = trap.TrapId == species.favoriteTrapType;
                
                // favoriteTerrain enum 사용
                TerrainType terrainType = GetTerrainTypeAtPosition(trap.transform.position);
                bool isFavoriteTerrain = terrainType == species.favoriteTerrain;
                
                if (isFavoriteTrap)
                    matchingTrapCount++;
                if (isFavoriteTerrain)
                    matchingTerrainCount++;
                if (isFavoriteTrap && isFavoriteTerrain)
                    perfectMatchCount++;
            }
            
            // 가중치 증가: 덫 하나당 2배, terrain 하나당 2배, 둘 다 맞으면 3배
            // 예: 좋아하는 덫 3개, 좋아하는 terrain 2개, 둘 다 맞는 것 1개
            // → 가중치 = baseWeight × (1 + 3×2 + 2×2 + 1×3) = baseWeight × 14
            float bonus = 1f + (matchingTrapCount * 2f) + (matchingTerrainCount * 2f) + (perfectMatchCount * 3f);
            weight *= bonus;

            return weight;
        }

        /// <summary>
        /// 덫 선호도 기반으로 비둘기 종 선택 (확률 보정)
        /// </summary>
        private SpeciesDefinition SelectSpeciesWithPreference(SpeciesDefinition[] allSpecies, Vector3 position, Collider2D mapCollider, bool applyBonus)
        {
            if (!applyBonus)
            {
                // 보정 없이 랜덤 선택 (해금된 종만)
                if (allSpecies.Length == 0)
                    return null;
                return allSpecies[Random.Range(0, allSpecies.Length)];
            }

            // 덫 리스트 가져오기 (포획된 덫 제외)
            FoodTrap[] allTraps = FindObjectsByType<FoodTrap>(FindObjectsSortMode.None);
            List<FoodTrap> activeTraps = new List<FoodTrap>();
            foreach (var trap in allTraps)
            {
                if (trap != null && !trap.HasCapturedPigeon && !trap.IsDepleted)
                {
                    // 특정 맵이 지정되었으면 해당 맵 내부의 덫만 필터링
                    if (mapCollider == null || ColliderUtility.IsPositionInsideCollider(trap.transform.position, mapCollider))
                    {
                        activeTraps.Add(trap);
                    }
                }
            }

            if (activeTraps.Count == 0)
            {
                // 덫이 없으면 랜덤 선택 (해금된 종만)
                if (allSpecies.Length == 0)
                    return null;
                return allSpecies[Random.Range(0, allSpecies.Length)];
            }

            // 각 종의 가중치 계산 (공통 로직 사용)
            List<float> weights = new List<float>();
            float totalWeight = 0f;
            foreach (var species in allSpecies)
            {
                float weight = CalculateSpeciesWeight(species, activeTraps);
                weights.Add(weight);
                totalWeight += weight;
            }

            // 가중치 기반 랜덤 선택
            if (totalWeight <= 0f)
            {
                // 폴백 (해금된 종만)
                if (allSpecies.Length == 0)
                    return null;
                return allSpecies[Random.Range(0, allSpecies.Length)];
            }

            float randomValue = Random.Range(0f, totalWeight);
            float currentWeight = 0f;

            for (int i = 0; i < allSpecies.Length; i++)
            {
                currentWeight += weights[i];
                if (randomValue <= currentWeight)
                {
                    return allSpecies[i];
                }
            }

            // 폴백
            return allSpecies[Random.Range(0, allSpecies.Length)];
        }


        /// <summary>
        /// 위치의 terrain 타입 확인 (public 메서드)
        /// </summary>
        public TerrainType GetTerrainTypeAtPosition(Vector3 position)
        {
            if (allTerrainAreas == null || allTerrainAreas.Length == 0)
                return TerrainType.SAND; // terrain 영역이 없으면 SAND (기본값)

            // 가장 먼저 발견된 terrain 영역의 타입 반환
            foreach (var terrainArea in allTerrainAreas)
            {
                if (terrainArea != null && terrainArea.ContainsPosition(position))
                {
                    return terrainArea.TerrainType;
                }
            }

            return TerrainType.SAND; // terrain 영역을 찾지 못하면 SAND (기본값)
        }

        /// <summary>
        /// 콜라이더 내부의 랜덤 위치 반환
        /// </summary>
        private Vector3 GetRandomPositionInCollider(Collider2D collider)
        {
            if (collider == null)
                return Vector3.zero;

            Bounds bounds = collider.bounds;
            int maxAttempts = 100;

            for (int i = 0; i < maxAttempts; i++)
            {
                Vector3 randomPos = new Vector3(
                    Random.Range(bounds.min.x, bounds.max.x),
                    Random.Range(bounds.min.y, bounds.max.y),
                    0
                );

                if (ColliderUtility.IsPositionInsideCollider(randomPos, collider))
                {
                    return randomPos;
                }
            }

            // 시도 실패 시 bounds 중심 반환
            return bounds.center;
        }


        /// <summary>
        /// forceFlee 상태인 비둘기 제거 관리
        /// </summary>
        private void CheckAndDespawnPigeons()
        {
            // 모든 맵의 비둘기 리스트를 복사해서 순회 (순회 중 수정 방지)
            var allPigeons = new List<PigeonController>();
            foreach (var pigeons in pigeonsByMap.Values)
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
                
                // 랜덤하게 화면 밖으로 나가기 (Flee 상태로 전환)
                if (ai.CurrentState != PigeonState.Flee && Random.value < despawnChance * Time.deltaTime)
                {
                    ai.ForceFlee();
                }

                // Flee 상태인 비둘기는 일정 시간이 지나면 제거
                if (ai.CurrentState == PigeonState.Flee)
                {
                    float elapsedTime = ai.FleeElapsedTime;
                    if (elapsedTime >= forceFleeDespawnTime)
                    {
                        RemovePigeonFromMap(pigeon);
                        if (pigeon != null && pigeon.gameObject != null)
                        {
                            Destroy(pigeon.gameObject);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 비둘기가 속한 맵 콜라이더 가져오기
        /// </summary>
        public Collider2D GetMapColliderForPigeon(PigeonController pigeon)
        {
            if (pigeon != null && pigeonToMap.ContainsKey(pigeon))
            {
                return pigeonToMap[pigeon];
            }
            return null;
        }

        /// <summary>
        /// 비둘기를 맵에서 제거
        /// </summary>
        private void RemovePigeonFromMap(PigeonController pigeon)
        {
            if (pigeon == null)
                return;

            foreach (var kvp in pigeonsByMap)
            {
                var pigeons = kvp.Value;
                if (pigeons != null && pigeons.Contains(pigeon))
                {
                    pigeons.Remove(pigeon);
                    break;
                }
            }

            if (pigeonToMap.ContainsKey(pigeon))
            {
                pigeonToMap.Remove(pigeon);
            }
        }

        /// <summary>
        /// 덫 설치 시 해당 맵에 비둘기 추가 스폰
        /// </summary>
        public void SpawnPigeonsInMap(Vector3 position, int count, bool applyPreferenceBonus = true)
        {
            if (pigeonPrefab == null)
                return;

            // 위치가 속한 맵 콜라이더 찾기
            Collider2D targetMapCollider = null;
            var mapColliders = MapColliders;
            if (mapColliders != null)
            {
                foreach (var mapCollider in mapColliders)
                {
                    if (mapCollider != null && ColliderUtility.IsPositionInsideCollider(position, mapCollider))
                    {
                        targetMapCollider = mapCollider;
                        break;
                    }
                }

                // 맵 콜라이더를 찾지 못했으면 첫 번째 맵 콜라이더 사용
                if (targetMapCollider == null && mapColliders.Length > 0)
                {
                    targetMapCollider = mapColliders[0];
                }
            }

            if (targetMapCollider == null)
                return;

            // 지정된 수만큼 비둘기를 맵 내 랜덤 위치에 스폰
            for (int i = 0; i < count; i++)
            {
                Vector3 spawnPos = GetRandomPositionInCollider(targetMapCollider);
                SpawnPigeonAtPosition(spawnPos, targetMapCollider, applyPreferenceBonus);
            }
        }

        /// <summary>
        /// 특정 맵의 종별 스폰 확률 계산 (UI 표시용) - 덫 위치 기반
        /// </summary>
        public Dictionary<PigeonSpecies, float> GetSpeciesSpawnProbabilities(Collider2D mapCollider = null)
        {
            var registry = GameDataRegistry.Instance;
            if (registry == null)
                return null;

            if (registry.SpeciesSet == null)
                return null;

            var allSpecies = registry.SpeciesSet.species;
            if (allSpecies == null || allSpecies.Length == 0)
                return null;

            // 덫 리스트 가져오기 (포획된 덫 제외)
            FoodTrap[] allTraps = FindObjectsByType<FoodTrap>(FindObjectsSortMode.None);
            List<FoodTrap> activeTraps = new List<FoodTrap>();
            foreach (var trap in allTraps)
            {
                if (trap != null && !trap.HasCapturedPigeon && !trap.IsDepleted)
                {
                    // 특정 맵이 지정되었으면 해당 맵 내부의 덫만 필터링
                    if (mapCollider == null || ColliderUtility.IsPositionInsideCollider(trap.transform.position, mapCollider))
                    {
                        activeTraps.Add(trap);
                    }
                }
            }

            if (activeTraps.Count == 0)
            {
                // 덫이 없으면 기본 확률 반환 (baseSpawnWeight 기반)
                Dictionary<PigeonSpecies, float> defaultProbabilities = new Dictionary<PigeonSpecies, float>();
                float defaultTotalWeight = 0f;
                foreach (var species in allSpecies)
                {
                    if (species != null)
                    {
                        defaultTotalWeight += species.baseSpawnWeight;
                    }
                }
                
                if (defaultTotalWeight > 0f)
                {
                    foreach (var species in allSpecies)
                    {
                        if (species != null)
                        {
                            defaultProbabilities[species.speciesType] = (species.baseSpawnWeight / defaultTotalWeight) * 100f;
                        }
                    }
                }
                return defaultProbabilities;
            }

            // 각 종의 가중치 계산 (공통 로직 사용)
            Dictionary<PigeonSpecies, float> weights = new Dictionary<PigeonSpecies, float>();
            foreach (var species in allSpecies)
            {
                if (species == null)
                    continue;

                float weight = CalculateSpeciesWeight(species, activeTraps);
                weights[species.speciesType] = weight;
            }

            // 가중치를 확률(%)로 변환
            float totalWeight = 0f;
            foreach (var weight in weights.Values)
            {
                totalWeight += weight;
            }
            
            if (totalWeight <= 0f)
                return null;

            Dictionary<PigeonSpecies, float> probabilities = new Dictionary<PigeonSpecies, float>();
            foreach (var kvp in weights)
            {
                probabilities[kvp.Key] = (kvp.Value / totalWeight) * 100f;
            }

            return probabilities;
        }

    }
}
