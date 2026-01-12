using UnityEngine;
using System.Collections.Generic;
using PigeonGame.Data;

namespace PigeonGame.Gameplay
{
    /// <summary>
    /// 월드 비둘기 관리 시스템
    /// 게임 시작 시 비둘기 스폰 및 랜덤 이동 관리
    /// </summary>
    public class WorldPigeonManager : MonoBehaviour
    {
        [SerializeField] private GameObject pigeonPrefab;
        [SerializeField] private int initialSpawnCount = 5;
        [SerializeField] private float spawnInterval = 10f; // 새 비둘기가 들어오는 간격
        [SerializeField] private float despawnChance = 0.1f; // 비둘기가 나가는 확률 (초당)
        [SerializeField] private float screenMargin = 2f; // 화면 밖 마진
        [SerializeField] private Collider2D[] mapColliders; // 맵 콜라이더 배열 (비둘기 스폰 영역 제한용)

        private List<PigeonController> worldPigeons = new List<PigeonController>();
        private Camera mainCamera;
        private float spawnTimer = 0f;

        private void Start()
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
                mainCamera = FindObjectOfType<Camera>();

            // 맵 콜라이더가 없으면 자동으로 찾기
            if (mapColliders == null || mapColliders.Length == 0)
            {
                mapColliders = FindMapColliders();
            }

            // 게임 시작 시 초기 비둘기 스폰
            SpawnInitialPigeons();
        }

        private void Update()
        {
            if (pigeonPrefab == null)
                return;

            // 화면 밖으로 나간 비둘기 제거
            CheckAndDespawnPigeons();

            // 랜덤하게 새 비둘기 스폰
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= spawnInterval)
            {
                spawnTimer = 0f;
                if (Random.value < 0.5f) // 50% 확률로 새 비둘기 스폰
                {
                    SpawnPigeonFromOffScreen();
                }
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
            
            if (foundColliders.Count > 0)
            {
                Debug.Log($"WorldPigeonManager: {foundColliders.Count}개의 맵 콜라이더를 자동으로 찾았습니다.");
            }
            
            return foundColliders.ToArray();
        }

        /// <summary>
        /// 게임 시작 시 초기 비둘기 스폰
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

            for (int i = 0; i < initialSpawnCount; i++)
            {
                Vector3 spawnPos = GetRandomScreenPosition();
                SpawnPigeonAtPosition(spawnPos);
            }
        }

        /// <summary>
        /// 화면 밖에서 비둘기 스폰 (화면 안으로 날아들어옴)
        /// </summary>
        private void SpawnPigeonFromOffScreen()
        {
            if (mainCamera == null)
                return;

            // 화면 밖 위치 계산
            Vector3 spawnPos = GetRandomOffScreenPosition();
            SpawnPigeonAtPosition(spawnPos);
        }

        /// <summary>
        /// 덫 주변에 비둘기 스폰 (TrapPlacer에서 호출)
        /// </summary>
        public void SpawnPigeonsAtPosition(Vector3 position, FoodTrap trap, int spawnCount = 5, float spawnRadius = 3f)
        {
            if (pigeonPrefab == null)
            {
                Debug.LogError("WorldPigeonManager: Pigeon Prefab이 설정되지 않았습니다!");
                return;
            }

            var registry = GameDataRegistry.Instance;
            if (registry == null || registry.SpeciesSet == null || registry.Faces == null)
            {
                Debug.LogError("WorldPigeonManager: GameDataRegistry를 찾을 수 없습니다!");
                return;
            }

            // 랜덤 종 선택
            var allSpecies = registry.SpeciesSet.species;
            if (allSpecies.Length == 0)
                return;

            for (int i = 0; i < spawnCount; i++)
            {
                Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
                Vector3 spawnPos = position + (Vector3)randomOffset;
                SpawnPigeonAtPosition(spawnPos, allSpecies);
            }
        }

        /// <summary>
        /// 지정된 위치에 비둘기 스폰
        /// </summary>
        private void SpawnPigeonAtPosition(Vector3 position)
        {
            var registry = GameDataRegistry.Instance;
            if (registry == null || registry.SpeciesSet == null || registry.Faces == null)
                return;

            var allSpecies = registry.SpeciesSet.species;
            if (allSpecies.Length == 0)
                return;

            SpawnPigeonAtPosition(position, allSpecies);
        }

        /// <summary>
        /// 지정된 위치에 비둘기 스폰 (내부 메서드)
        /// </summary>
        private void SpawnPigeonAtPosition(Vector3 position, SpeciesDefinition[] allSpecies)
        {
            if (pigeonPrefab == null)
                return;

            var registry = GameDataRegistry.Instance;
            if (registry == null || registry.Faces == null)
                return;

            // 랜덤 종 선택
            var species = allSpecies[Random.Range(0, allSpecies.Length)];
            int obesity = Random.Range(species.defaultObesityRange.x, species.defaultObesityRange.y + 1);
            
            // 랜덤 얼굴 선택
            var allFaces = registry.Faces.faces;
            string faceId = allFaces[Random.Range(0, allFaces.Length)].id;

            // Z 위치를 0으로 명시적으로 설정 (2D 게임용)
            Vector3 spawnPosition = new Vector3(position.x, position.y, 0);
            GameObject pigeonObj = Instantiate(pigeonPrefab, spawnPosition, Quaternion.identity);
            
            // 게임 오브젝트가 활성화되어 있는지 확인
            if (!pigeonObj.activeSelf)
            {
                pigeonObj.SetActive(true);
            }
            
            PigeonController controller = pigeonObj.GetComponent<PigeonController>();
            
            if (controller != null)
            {
                var stats = PigeonInstanceFactory.CreateInstanceStats(
                    species.speciesId, 
                    obesity, 
                    faceId
                );
                
                controller.Initialize(stats);
                worldPigeons.Add(controller);
            }
            else
            {
                Debug.LogError($"WorldPigeonManager: PigeonController를 찾을 수 없습니다! GameObject: {pigeonObj.name}");
            }
        }

        /// <summary>
        /// 화면 내 랜덤 위치 반환 (맵 콜라이더 내부로 제한)
        /// </summary>
        private Vector3 GetRandomScreenPosition()
        {
            if (mainCamera == null)
                return Vector3.zero;

            float orthoSize = mainCamera.orthographicSize;
            float aspect = mainCamera.aspect;

            // 맵 콜라이더가 있으면 그 안에서만 스폰
            if (mapColliders != null && mapColliders.Length > 0)
            {
                return GetRandomPositionInMapColliders();
            }

            // 맵 콜라이더가 없으면 기존 방식 사용
            float x = Random.Range(-orthoSize * aspect, orthoSize * aspect);
            float y = Random.Range(-orthoSize, orthoSize);

            Vector3 worldPos = mainCamera.transform.position + new Vector3(x, y, 0);
            return worldPos;
        }

        /// <summary>
        /// 화면 밖 랜덤 위치 반환 (맵 콜라이더 내부로 제한)
        /// </summary>
        private Vector3 GetRandomOffScreenPosition()
        {
            if (mainCamera == null)
                return Vector3.zero;

            // 맵 콜라이더가 있으면 그 안에서만 스폰
            if (mapColliders != null && mapColliders.Length > 0)
            {
                return GetRandomPositionInMapColliders();
            }

            float orthoSize = mainCamera.orthographicSize;
            float aspect = mainCamera.aspect;
            Vector3 cameraPos = mainCamera.transform.position;

            // 화면 밖 위치 (4방향 중 하나)
            int side = Random.Range(0, 4);
            Vector3 spawnPos = Vector3.zero;

            switch (side)
            {
                case 0: // 위
                    spawnPos = cameraPos + new Vector3(
                        Random.Range(-orthoSize * aspect, orthoSize * aspect),
                        orthoSize + screenMargin,
                        0
                    );
                    break;
                case 1: // 아래
                    spawnPos = cameraPos + new Vector3(
                        Random.Range(-orthoSize * aspect, orthoSize * aspect),
                        -orthoSize - screenMargin,
                        0
                    );
                    break;
                case 2: // 왼쪽
                    spawnPos = cameraPos + new Vector3(
                        -orthoSize * aspect - screenMargin,
                        Random.Range(-orthoSize, orthoSize),
                        0
                    );
                    break;
                case 3: // 오른쪽
                    spawnPos = cameraPos + new Vector3(
                        orthoSize * aspect + screenMargin,
                        Random.Range(-orthoSize, orthoSize),
                        0
                    );
                    break;
            }

            return spawnPos;
        }

        /// <summary>
        /// 맵 콜라이더들 중 하나의 내부 랜덤 위치 반환
        /// </summary>
        private Vector3 GetRandomPositionInMapColliders()
        {
            if (mapColliders == null || mapColliders.Length == 0)
                return Vector3.zero;

            // 모든 콜라이더의 전체 bounds 계산
            Bounds combinedBounds = mapColliders[0].bounds;
            for (int i = 1; i < mapColliders.Length; i++)
            {
                if (mapColliders[i] != null)
                {
                    combinedBounds.Encapsulate(mapColliders[i].bounds);
                }
            }

            int maxAttempts = 100; // 최대 시도 횟수
            Vector3 randomPos = Vector3.zero;

            for (int i = 0; i < maxAttempts; i++)
            {
                // 전체 bounds 내에서 랜덤 위치 생성
                randomPos = new Vector3(
                    Random.Range(combinedBounds.min.x, combinedBounds.max.x),
                    Random.Range(combinedBounds.min.y, combinedBounds.max.y),
                    0
                );

                // 여러 콜라이더 중 하나라도 내부에 있으면 OK
                if (IsPositionInsideAnyCollider(randomPos))
                {
                    return randomPos;
                }
            }

            // 시도 실패 시 첫 번째 콜라이더의 bounds 중심 반환
            Debug.LogWarning("WorldPigeonManager: 맵 콜라이더 내부 위치를 찾지 못했습니다. 첫 번째 콜라이더 중심을 사용합니다.");
            return mapColliders[0] != null ? mapColliders[0].bounds.center : Vector3.zero;
        }

        /// <summary>
        /// 위치가 여러 콜라이더 중 하나라도 내부에 있는지 확인
        /// </summary>
        private bool IsPositionInsideAnyCollider(Vector3 position)
        {
            if (mapColliders == null || mapColliders.Length == 0)
                return false;

            foreach (Collider2D collider in mapColliders)
            {
                if (collider != null && IsPositionInsideCollider(position, collider))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 위치가 특정 콜라이더 내부인지 확인
        /// </summary>
        private bool IsPositionInsideCollider(Vector3 position, Collider2D collider)
        {
            if (collider == null)
                return false;

            // 대부분의 콜라이더는 bounds 체크로 충분
            if (collider is BoxCollider2D || collider is CircleCollider2D || collider is EdgeCollider2D)
            {
                return collider.bounds.Contains(position);
            }

            // PolygonCollider2D만 특별 처리
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
            // 월드 좌표로 변환
            Vector2[] worldPolygon = new Vector2[polygon.Length];
            for (int i = 0; i < polygon.Length; i++)
            {
                worldPolygon[i] = transform.TransformPoint(polygon[i]);
            }

            // Ray casting 알고리즘
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
        /// 화면 밖으로 나간 비둘기 제거
        /// </summary>
        private void CheckAndDespawnPigeons()
        {
            if (mainCamera == null)
                return;

            for (int i = worldPigeons.Count - 1; i >= 0; i--)
            {
                var pigeon = worldPigeons[i];
                if (pigeon == null || pigeon.gameObject == null)
                {
                    worldPigeons.RemoveAt(i);
                    continue;
                }

                // 랜덤하게 화면 밖으로 나가기
                if (Random.value < despawnChance * Time.deltaTime)
                {
                    // 화면 밖으로 이동시키기
                    Vector3 offScreenPos = GetRandomOffScreenPosition();
                    Vector2 direction = (offScreenPos - pigeon.transform.position).normalized;
                    
                    var movement = pigeon.GetComponent<PigeonMovement>();
                    if (movement != null)
                    {
                        // 일시적으로 빠른 속도로 화면 밖으로 이동
                        var rb = pigeon.GetComponent<Rigidbody2D>();
                        if (rb != null)
                        {
                            rb.linearVelocity = direction * 8f; // 빠른 속도
                        }
                    }
                }

                // 화면 밖으로 나갔는지 확인
                Vector2 viewportPos = mainCamera.WorldToViewportPoint(pigeon.transform.position);
                if (viewportPos.x < -0.2f || viewportPos.x > 1.2f || 
                    viewportPos.y < -0.2f || viewportPos.y > 1.2f)
                {
                    // 화면 밖으로 충분히 나갔으면 제거
                    if (Vector2.Distance(pigeon.transform.position, mainCamera.transform.position) > 
                        mainCamera.orthographicSize * 2f)
                    {
                        worldPigeons.RemoveAt(i);
                        Destroy(pigeon.gameObject);
                    }
                }
            }
        }

        /// <summary>
        /// 모든 월드 비둘기 제거
        /// </summary>
        public void ClearAllPigeons()
        {
            foreach (var pigeon in worldPigeons)
            {
                if (pigeon != null)
                    Destroy(pigeon.gameObject);
            }
            worldPigeons.Clear();
        }
    }
}

