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

        // 맵 콜라이더별 비둘기 관리
        private Dictionary<Collider2D, List<PigeonController>> pigeonsByMap = new Dictionary<Collider2D, List<PigeonController>>();
        private Dictionary<PigeonController, Collider2D> pigeonToMap = new Dictionary<PigeonController, Collider2D>();
        private Camera mainCamera;
        private float spawnCheckTimer = 0f;
        private TerrainArea[] allTerrainAreas; // 모든 terrain 영역

        private void Start()
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
                mainCamera = FindFirstObjectByType<Camera>();

            // 맵 콜라이더가 없으면 자동으로 찾기
            if (mapColliders == null || mapColliders.Length == 0)
            {
                mapColliders = FindMapColliders();
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
        /// 맵 콜라이더 자동 찾기
        /// </summary>
        private Collider2D[] FindMapColliders()
        {
            List<Collider2D> foundColliders = new List<Collider2D>();
            
            // "Map" 태그로 찾기
            GameObject[] mapObjects = GameObject.FindGameObjectsWithTag("Map");
            foreach (GameObject mapObj in mapObjects)
            {
                Collider2D col = mapObj.GetComponent<Collider2D>() ?? mapObj.GetComponentInChildren<Collider2D>();
                if (col != null && !foundColliders.Contains(col))
                    foundColliders.Add(col);
            }
            
            // 이름으로 찾기 (Map1, Map2 등)
            if (foundColliders.Count == 0)
            {
                for (int i = 1; i <= 10; i++)
                {
                    GameObject mapObj = GameObject.Find($"Map{i}");
                    if (mapObj != null)
                    {
                        Collider2D col = mapObj.GetComponent<Collider2D>() ?? mapObj.GetComponentInChildren<Collider2D>();
                        if (col != null && !foundColliders.Contains(col))
                            foundColliders.Add(col);
                    }
                }
            }
            
            return foundColliders.ToArray();
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

            // 위치의 terrain 타입 확인
            string terrainType = GetTerrainTypeAtPosition(position);

            // 각 종의 가중치 계산
            List<float> weights = new List<float>();
            foreach (var species in allSpecies)
            {
                float weight = species.baseSpawnWeight; // 종별 초기 확률 가중치

                // 덫 종류 선호도 보정
                float trapPreferenceSum = 0f;
                int trapCount = 0;
                foreach (var trap in activeTraps)
                {
                    int preference = GetTrapPreference(species, trap.TrapId);
                    trapPreferenceSum += preference;
                    trapCount++;
                }
                if (trapCount > 0)
                {
                    weight *= (trapPreferenceSum / trapCount) / 100f; // 0-1 범위로 정규화
                }

                // Terrain 타입 선호도 보정
                if (species.terrainPreference != null && !string.IsNullOrEmpty(terrainType))
                {
                    int terrainPreference = GetTerrainPreference(species, terrainType);
                    weight *= terrainPreference / 100f; // 0-1 범위로 정규화
                }

                weights.Add(weight);
            }

            // 가중치 기반 랜덤 선택
            float totalWeight = weights.Sum();
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
        /// 덫 종류 선호도 가져오기
        /// </summary>
        private int GetTrapPreference(SpeciesDefinition species, string trapId)
        {
            if (species.trapPreference == null)
                return 50; // 기본값

            return trapId switch
            {
                "BREAD" => species.trapPreference.BREAD,
                "SEEDS" => species.trapPreference.SEEDS,
                "CORN" => species.trapPreference.CORN,
                "PELLET" => species.trapPreference.PELLET,
                "SHINY" => species.trapPreference.SHINY,
                _ => 50
            };
        }

        /// <summary>
        /// Terrain 타입 선호도 가져오기
        /// </summary>
        private int GetTerrainPreference(SpeciesDefinition species, string terrainType)
        {
            if (species.terrainPreference == null)
                return 50; // 기본값

            return terrainType switch
            {
                "default" => species.terrainPreference.sand, // default는 sand로 매핑
                "sand" => species.terrainPreference.sand, // sand는 기본값
                "grass" => species.terrainPreference.grass,
                "water" => species.terrainPreference.water,
                "flower" => species.terrainPreference.flower, // 기존 sand는 flower로
                "wetland" => species.terrainPreference.wetland,
                _ => species.terrainPreference.sand // 알 수 없는 타입도 sand(기본값)로 처리
            };
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
        /// 현재 맵의 종별 스폰 확률 계산 (UI 표시용)
        /// </summary>
        public Dictionary<string, float> GetSpeciesSpawnProbabilities(Vector3 position)
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

            // 위치의 terrain 타입 확인
            string terrainType = GetTerrainTypeAtPosition(position);

            // 각 종의 가중치 계산
            Dictionary<string, float> weights = new Dictionary<string, float>();
            foreach (var species in allSpecies)
            {
                if (species == null || string.IsNullOrEmpty(species.speciesId))
                {
                    Debug.LogWarning("WorldPigeonManager: species 또는 speciesId가 null입니다.");
                    continue;
                }

                float weight = species.baseSpawnWeight;
                Debug.Log($"[DEBUG] {species.name}: baseSpawnWeight = {weight}");

                // 덫 종류 선호도 보정
                if (activeTraps.Count > 0)
                {
                    float trapPreferenceSum = 0f;
                    foreach (var trap in activeTraps)
                    {
                        int preference = GetTrapPreference(species, trap.TrapId);
                        trapPreferenceSum += preference;
                    }
                    float trapMultiplier = (trapPreferenceSum / activeTraps.Count) / 100f;
                    Debug.Log($"[DEBUG] {species.name}: trapPreferenceSum = {trapPreferenceSum}, trapCount = {activeTraps.Count}, trapMultiplier = {trapMultiplier}");
                    weight *= trapMultiplier;
                }

                // Terrain 타입 선호도 보정
                if (species.terrainPreference != null && !string.IsNullOrEmpty(terrainType))
                {
                    int terrainPreference = GetTerrainPreference(species, terrainType);
                    float terrainMultiplier = terrainPreference / 100f;
                    Debug.Log($"[DEBUG] {species.name}: terrainType = {terrainType}, terrainPreference = {terrainPreference}, terrainMultiplier = {terrainMultiplier}");
                    Debug.Log($"[DEBUG] {species.name}: terrainPreference object = {(species.terrainPreference != null ? "not null" : "null")}, sand = {species.terrainPreference?.sand}, flower = {species.terrainPreference?.flower}");
                    weight *= terrainMultiplier;
                }
                else
                {
                    Debug.Log($"[DEBUG] {species.name}: terrainPreference is null or terrainType is empty. terrainPreference = {(species.terrainPreference != null ? "not null" : "null")}, terrainType = {terrainType}");
                }

                Debug.Log($"[DEBUG] {species.name}: final weight = {weight}");
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
