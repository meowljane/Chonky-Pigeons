using UnityEngine;
using System.Collections.Generic;
using PigeonGame.Data;

namespace PigeonGame.Gameplay
{
    /// <summary>
    /// 맵 정보를 담는 구조체
    /// </summary>
    [System.Serializable]
    public class MapInfo
    {
        public string mapName;
        public Collider2D mapCollider;
        public Collider2D bridgeCollider; // 다리 콜라이더 (Map2, 3, 4, 5에만 있음)
        public BridgeGate bridgeGate; // 게이트 오브젝트 (Map2, 3, 4, 5에만 있음)

        public MapInfo(string name, Collider2D collider)
        {
            mapName = name;
            mapCollider = collider;
        }
    }

    /// <summary>
    /// 맵 관리 시스템
    /// 맵 이름과 콜라이더, 다리를 관리
    /// </summary>
    public class MapManager : MonoBehaviour
    {
        public static MapManager Instance { get; private set; }

        [SerializeField] private MapInfo[] maps; // 맵 정보 배열 (다리와 게이트 포함)

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

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// 모든 맵 콜라이더 가져오기
        /// </summary>
        public Collider2D[] GetAllMapColliders()
        {
            if (maps == null || maps.Length == 0)
                return null;

            List<Collider2D> colliders = new List<Collider2D>(maps.Length);
            foreach (var map in maps)
            {
                if (map?.mapCollider != null)
                {
                    colliders.Add(map.mapCollider);
                }
            }
            return colliders.Count > 0 ? colliders.ToArray() : null;
        }

        /// <summary>
        /// 특정 콜라이더에 해당하는 맵 이름 가져오기
        /// </summary>
        public string GetMapName(Collider2D collider)
        {
            if (maps == null || collider == null)
                return "Unknown";

            foreach (var map in maps)
            {
                if (map != null && map.mapCollider == collider)
                {
                    return map.mapName;
                }
            }
            return "Unknown";
        }

        /// <summary>
        /// 특정 위치가 속한 맵 정보 가져오기
        /// </summary>
        public MapInfo GetMapAtPosition(Vector2 position)
        {
            if (maps == null)
                return null;

            foreach (var map in maps)
            {
                if (map?.mapCollider != null && 
                    ColliderUtility.IsPositionInsideCollider(position, map.mapCollider))
                {
                    return map;
                }
            }
            return null;
        }

        /// <summary>
        /// 특정 위치가 다리 위에 있는지 확인
        /// </summary>
        public bool IsPositionOnBridge(Vector2 position)
        {
            if (maps == null)
                return false;
            
            foreach (var map in maps)
            {
                if (map?.bridgeCollider != null && 
                    ColliderUtility.IsPositionInsideCollider(position, map.bridgeCollider))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 특정 위치의 다리에 접근 가능한지 확인 (해금 여부 체크)
        /// </summary>
        public bool CanAccessBridgeAtPosition(Vector2 position)
        {
            if (maps == null)
                return false;

            // 해당 위치가 속한 다리 찾기
            for (int i = 0; i < maps.Length; i++)
            {
                var map = maps[i];
                if (map?.bridgeCollider != null && 
                    ColliderUtility.IsPositionInsideCollider(position, map.bridgeCollider))
                {
                    // bridgeGate가 있으면 그 areaNumber를 사용, 없으면 배열 인덱스로 추정
                    int areaNumber;
                    if (map.bridgeGate != null)
                    {
                        // bridgeGate의 areaNumber를 직접 사용 (가장 정확)
                        areaNumber = map.bridgeGate.AreaNumber;
                    }
                    else
                    {
                        // bridgeGate가 없으면 배열 인덱스로 추정
                        areaNumber = i + 1; // Map1=1, Map2=2, ...
                    }
                    
                    // Map1은 다리가 없으므로 다리가 있는 맵(2, 3, 4, 5)만 체크
                    if (areaNumber >= 2 && areaNumber <= 5)
                    {
                        // 해금 여부 확인
                        return IsAreaUnlocked(areaNumber);
                    }
                    
                    // Map1이거나 다리가 없는 맵은 접근 가능
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 지역 번호로 맵 정보 가져오기 (Map1=1, Map2=2, ...)
        /// </summary>
        public MapInfo GetMapByAreaNumber(int areaNumber)
        {
            if (maps == null || areaNumber < 1)
                return null;

            // 배열 인덱스는 0부터 시작하므로 areaNumber - 1
            int index = areaNumber - 1;
            if (index >= 0 && index < maps.Length)
            {
                return maps[index];
            }
            return null;
        }

        /// <summary>
        /// 지역 번호로 지역 이름 가져오기
        /// </summary>
        public string GetAreaName(int areaNumber)
        {
            MapInfo mapInfo = GetMapByAreaNumber(areaNumber);
            if (mapInfo != null && !string.IsNullOrEmpty(mapInfo.mapName))
            {
                return mapInfo.mapName;
            }
            return null;
        }

        /// <summary>
        /// 지역 해금 여부 확인 (GameManager 래핑)
        /// </summary>
        public bool IsAreaUnlocked(int areaNumber)
        {
            if (GameManager.Instance != null)
            {
                return GameManager.Instance.IsAreaUnlocked(areaNumber);
            }
            return false;
        }

        /// <summary>
        /// 모든 다리 콜라이더 가져오기 (하위 호환성)
        /// </summary>
        public Collider2D[] GetAllBridgeColliders()
        {
            if (maps == null)
                return null;

            List<Collider2D> bridgeColliders = new List<Collider2D>();
            foreach (var map in maps)
            {
                if (map?.bridgeCollider != null)
                {
                    bridgeColliders.Add(map.bridgeCollider);
                }
            }
            return bridgeColliders.Count > 0 ? bridgeColliders.ToArray() : null;
        }

        /// <summary>
        /// 특정 위치의 지역 타입 반환 (TerrainType)
        /// </summary>
        public TerrainType GetTerrainTypeAtPosition(Vector3 position)
        {
            TerrainArea[] allTerrainAreas = FindObjectsByType<TerrainArea>(FindObjectsSortMode.None);
            
            // null 체크
            if (allTerrainAreas == null || allTerrainAreas.Length == 0)
                return TerrainType.SAND;
            
            foreach (var terrainArea in allTerrainAreas)
            {
                if (terrainArea != null && terrainArea.ContainsPosition(position))
                    return terrainArea.TerrainType;
            }
            return TerrainType.SAND;
        }
    }
}
