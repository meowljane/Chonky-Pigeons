using UnityEngine;
using System.Collections.Generic;
using System.Linq;
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
        [SerializeField] private Collider2D[] mapColliders; // 맵 콜라이더 배열
        
        public static WorldPigeonManager Instance { get; private set; }
        public Collider2D[] MapColliders => mapColliders;

        // 맵 콜라이더별 비둘기 관리
        private Dictionary<Collider2D, List<PigeonController>> pigeonsByMap = new Dictionary<Collider2D, List<PigeonController>>();
        private Dictionary<PigeonController, Collider2D> pigeonToMap = new Dictionary<PigeonController, Collider2D>();
        private Camera mainCamera;
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
                Debug.LogWarning("Multiple WorldPigeonManager instances detected!");
            }
        }

        private void Start()
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
                mainCamera = FindFirstObjectByType<Camera>();

            // 맵 콜라이더가 설정되지 않았으면 경고
            if (mapColliders == null || mapColliders.Length == 0)
            {
                Debug.LogError("WorldPigeonManager: 맵 콜라이더가 설정되지 않았습니다! Inspector에서 Map Colliders를 할당해주세요.");
            }

            // 모든 terrain 영역 찾기
            allTerrainAreas = FindObjectsByType<TerrainArea>(FindObjectsSortMode.None);

            // 맵 콜라이더별 비둘기 리스트 초기화
            foreach (var mapCollider in mapColliders)
            {
                if (mapCollider != null)
                {
                    pigeonsByMap[mapCollider] = new List<PigeonController>();
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
            {
                Debug.LogError("WorldPigeonManager: Pigeon Prefab이 설정되지 않았습니다!");
                return;
            }

            var registry = GameDataRegistry.Instance;
            if (registry == null || registry.SpeciesSet == null)
            {
                Debug.LogError("WorldPigeonManager: GameDataRegistry를 찾을 수 없습니다!");
                return;
            }

            // 각 맵 콜라이더마다 비둘기 스폰
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

            // 비둘기 종 선택 (덫 선호도 기반 확률 보정)
            SpeciesDefinition selectedSpecies = SelectSpeciesWithPreference(allSpecies, position, applyPreferenceBonus);
            if (selectedSpecies == null)
                selectedSpecies = allSpecies[Random.Range(0, allSpecies.Length)]; // 폴백

            int obesity = Random.Range(selectedSpecies.defaultObesityRange.x, selectedSpecies.defaultObesityRange.y + 1);
            
            // 랜덤 얼굴 선택
            var allFaces = registry.Faces.faces;
            string faceId = allFaces[Random.Range(0, allFaces.Length)].id;

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
                    selectedSpecies.speciesId, 
                    obesity, 
                    faceId
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
            else
            {
                Debug.LogError($"WorldPigeonManager: PigeonController를 찾을 수 없습니다! GameObject: {pigeonObj.name}");
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
                bool isFavoriteTrap = !string.IsNullOrEmpty(species.favoriteTrap) && 
                                      trap.TrapId == species.favoriteTrap;
                bool isFavoriteTerrain = false;
                
                string terrainType = GetTerrainTypeAtPosition(trap.transform.position);
                if (!string.IsNullOrEmpty(species.favoriteTerrain) && 
                    !string.IsNullOrEmpty(terrainType) && 
                    terrainType == species.favoriteTerrain)
                {
                    isFavoriteTerrain = true;
                }
                
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
        private SpeciesDefinition SelectSpeciesWithPreference(SpeciesDefinition[] allSpecies, Vector3 position, bool applyBonus)
        {
            if (!applyBonus)
            {
                // 보정 없이 랜덤 선택
                return allSpecies[Random.Range(0, allSpecies.Length)];
            }

            // 덫 리스트 가져오기 (포획된 덫 제외)
            FoodTrap[] allTraps = FindObjectsByType<FoodTrap>(FindObjectsSortMode.None);
            List<FoodTrap> activeTraps = allTraps.Where(t => t != null && !t.HasCapturedPigeon && !t.IsDepleted).ToList();

            if (activeTraps.Count == 0)
            {
                // 덫이 없으면 랜덤 선택
                return allSpecies[Random.Range(0, allSpecies.Length)];
            }

            // 각 종의 가중치 계산 (공통 로직 사용)
            List<float> weights = new List<float>();
            foreach (var species in allSpecies)
            {
                float weight = CalculateSpeciesWeight(species, activeTraps);
                weights.Add(weight);
            }

            // 가중치 기반 랜덤 선택
            float totalWeight = weights.Sum();
            if (totalWeight <= 0f)
            {
                // 폴백
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
        public string GetTerrainTypeAtPosition(Vector3 position)
        {
            if (allTerrainAreas == null || allTerrainAreas.Length == 0)
                return "sand"; // terrain 영역이 없으면 sand (기본값)

            // 가장 먼저 발견된 terrain 영역의 타입 반환
            foreach (var terrainArea in allTerrainAreas)
            {
                if (terrainArea != null && terrainArea.ContainsPosition(position))
                {
                    return terrainArea.TerrainType;
                }
            }

            return "sand"; // terrain 영역을 찾지 못하면 sand (기본값)
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

                if (IsPositionInsideCollider(randomPos, collider))
                {
                    return randomPos;
                }
            }

            // 시도 실패 시 bounds 중심 반환
            return bounds.center;
        }

        /// <summary>
        /// 위치가 특정 콜라이더 내부인지 확인
        /// </summary>
        private bool IsPositionInsideCollider(Vector3 position, Collider2D collider)
        {
            if (collider == null)
                return false;

            if (collider is BoxCollider2D || collider is CircleCollider2D || collider is EdgeCollider2D)
            {
                return collider.bounds.Contains(position);
            }

            if (collider is PolygonCollider2D polygonCollider)
            {
                Vector2 pos2D = new Vector2(position.x, position.y);
                return IsPointInPolygon(pos2D, polygonCollider.points, polygonCollider.transform);
            }

            return collider.bounds.Contains(position);
        }

        /// <summary>
        /// 점이 폴리곤 내부에 있는지 확인 (PolygonCollider2D용)
        /// </summary>
        private bool IsPointInPolygon(Vector2 point, Vector2[] polygon, Transform transform)
        {
            Vector2[] worldPolygon = new Vector2[polygon.Length];
            for (int i = 0; i < polygon.Length; i++)
            {
                worldPolygon[i] = transform.TransformPoint(polygon[i]);
            }

            bool inside = false;
            int j = worldPolygon.Length - 1;

            for (int i = 0; i < worldPolygon.Length; i++)
            {
                Vector2 vi = worldPolygon[i];
                Vector2 vj = worldPolygon[j];

                if (((vi.y > point.y) != (vj.y > point.y)) &&
                    (point.x < (vj.x - vi.x) * (point.y - vi.y) / (vj.y - vi.y) + vi.x))
                {
                    inside = !inside;
                }
                j = i;
            }

            return inside;
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
            foreach (var mapCollider in mapColliders)
            {
                if (mapCollider != null && IsPositionInsideCollider(position, mapCollider))
                    {
                    targetMapCollider = mapCollider;
                    break;
                }
            }

            // 맵 콜라이더를 찾지 못했으면 첫 번째 맵 콜라이더 사용
            if (targetMapCollider == null && mapColliders != null && mapColliders.Length > 0)
            {
                targetMapCollider = mapColliders[0];
            }

            if (targetMapCollider == null)
                        {
                Debug.LogWarning("WorldPigeonManager: 맵 콜라이더를 찾을 수 없어 비둘기를 스폰할 수 없습니다.");
                return;
            }

            // 지정된 수만큼 비둘기를 맵 내 랜덤 위치에 스폰
            for (int i = 0; i < count; i++)
            {
                Vector3 spawnPos = GetRandomPositionInCollider(targetMapCollider);
                SpawnPigeonAtPosition(spawnPos, targetMapCollider, applyPreferenceBonus);
                        }
                    }

        /// <summary>
        /// 현재 맵의 종별 스폰 확률 계산 (UI 표시용) - 덫 위치 기반
        /// </summary>
        public Dictionary<string, float> GetSpeciesSpawnProbabilities()
        {
            var registry = GameDataRegistry.Instance;
            if (registry == null)
            {
                Debug.LogWarning("WorldPigeonManager: GameDataRegistry.Instance가 null입니다.");
                return null;
            }

            if (registry.SpeciesSet == null)
            {
                Debug.LogWarning("WorldPigeonManager: SpeciesSet이 null입니다.");
                return null;
            }

            var allSpecies = registry.SpeciesSet.species;
            if (allSpecies == null || allSpecies.Length == 0)
            {
                Debug.LogWarning("WorldPigeonManager: species 배열이 비어있습니다.");
                return null;
            }

            // 덫 리스트 가져오기 (포획된 덫 제외)
            FoodTrap[] allTraps = FindObjectsByType<FoodTrap>(FindObjectsSortMode.None);
            List<FoodTrap> activeTraps = allTraps.Where(t => t != null && !t.HasCapturedPigeon && !t.IsDepleted).ToList();

            if (activeTraps.Count == 0)
            {
                // 덫이 없으면 기본 확률 반환 (baseSpawnWeight 기반)
                Dictionary<string, float> defaultProbabilities = new Dictionary<string, float>();
                float defaultTotalWeight = allSpecies.Sum(s => s != null ? s.baseSpawnWeight : 0f);
                if (defaultTotalWeight > 0f)
                {
                    foreach (var species in allSpecies)
                    {
                        if (species != null && !string.IsNullOrEmpty(species.speciesId))
                        {
                            defaultProbabilities[species.speciesId] = (species.baseSpawnWeight / defaultTotalWeight) * 100f;
                        }
                    }
                }
                return defaultProbabilities;
            }

            // 각 종의 가중치 계산 (공통 로직 사용)
            Dictionary<string, float> weights = new Dictionary<string, float>();
            foreach (var species in allSpecies)
            {
                if (species == null || string.IsNullOrEmpty(species.speciesId))
                {
                    Debug.LogWarning("WorldPigeonManager: species 또는 speciesId가 null입니다.");
                    continue;
                }

                float weight = CalculateSpeciesWeight(species, activeTraps);
                weights[species.speciesId] = weight;
            }

            // 가중치를 확률(%)로 변환
            float totalWeight = weights.Values.Sum();
            if (totalWeight <= 0f)
            {
                Debug.LogWarning("WorldPigeonManager: 총 가중치가 0 이하입니다.");
                return null;
            }

            Dictionary<string, float> probabilities = new Dictionary<string, float>();
            foreach (var kvp in weights)
            {
                probabilities[kvp.Key] = (kvp.Value / totalWeight) * 100f;
            }

            return probabilities;
        }

        /// <summary>
        /// 모든 월드 비둘기 제거
        /// </summary>
        public void ClearAllPigeons()
        {
            foreach (var pigeons in pigeonsByMap.Values)
            {
                foreach (var pigeon in pigeons)
            {
                if (pigeon != null)
                    Destroy(pigeon.gameObject);
                }
                pigeons.Clear();
            }
            pigeonsByMap.Clear();
            pigeonToMap.Clear();
        }
    }
}
