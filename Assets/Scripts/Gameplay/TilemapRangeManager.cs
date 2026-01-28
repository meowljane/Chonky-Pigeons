using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using PigeonGame.Data;

namespace PigeonGame.Gameplay
{
    /// <summary>
    /// 타일맵 기반 맵 범위 관리 시스템
    /// 각 타일맵 GameObject에 MapArea, TerrainArea, PlayerMovementArea, DoorTilemapArea 컴포넌트를 부착하여 타입 지정
    /// </summary>
    public class TilemapRangeManager : MonoBehaviour
    {
        public static TilemapRangeManager Instance { get; private set; }

        // 런타임 데이터: 셀 위치 -> 맵 타입, 지형 타입 (캐시)
        private Dictionary<Vector3Int, MapType> mapTypeCache = new Dictionary<Vector3Int, MapType>();
        private Dictionary<Vector3Int, TerrainType> terrainTypeCache = new Dictionary<Vector3Int, TerrainType>();
        private Dictionary<Vector3Int, bool> mapRangeCache = new Dictionary<Vector3Int, bool>();
        private Dictionary<Vector3Int, bool> playerMovementCache = new Dictionary<Vector3Int, bool>();

        // 타일맵 -> 컴포넌트 매핑 (초기화 시 한 번만 스캔)
        private Dictionary<Tilemap, MapArea> tilemapToMapArea = new Dictionary<Tilemap, MapArea>();
        private Dictionary<Tilemap, TerrainArea> tilemapToTerrainArea = new Dictionary<Tilemap, TerrainArea>();
        private Dictionary<Tilemap, DoorTilemapArea> tilemapToDoorArea = new Dictionary<Tilemap, DoorTilemapArea>();
        private List<Tilemap> playerMovementTilemaps = new List<Tilemap>();
        private List<Tilemap> exhibitionTilemaps = new List<Tilemap>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeTilemaps();
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
        /// 씬의 모든 타일맵을 스캔하여 MapArea/TerrainArea/PlayerMovementArea 컴포넌트 매핑
        /// </summary>
        private void InitializeTilemaps()
        {
            tilemapToMapArea.Clear();
            tilemapToTerrainArea.Clear();
            tilemapToDoorArea.Clear();
            playerMovementTilemaps.Clear();
            exhibitionTilemaps.Clear();
            mapTypeCache.Clear();
            terrainTypeCache.Clear();
            mapRangeCache.Clear();
            playerMovementCache.Clear();

            // 씬의 모든 타일맵 찾기
            Tilemap[] allTilemaps = FindObjectsByType<Tilemap>(FindObjectsSortMode.None);
            
            foreach (var tilemap in allTilemaps)
            {
                if (tilemap == null) continue;

                // MapArea 컴포넌트 확인
                MapArea mapArea = tilemap.GetComponent<MapArea>();
                if (mapArea != null)
                {
                    tilemapToMapArea[tilemap] = mapArea;
                }

                // PlayerMovementArea 컴포넌트 확인
                PlayerMovementArea playerMovementArea = tilemap.GetComponent<PlayerMovementArea>();
                if (playerMovementArea != null)
                {
                    playerMovementTilemaps.Add(tilemap);
                }

                // TerrainArea 컴포넌트 확인
                TerrainArea terrainArea = tilemap.GetComponent<TerrainArea>();
                if (terrainArea != null)
                {
                    tilemapToTerrainArea[tilemap] = terrainArea;
                }

                // DoorTilemapArea 컴포넌트 확인
                DoorTilemapArea doorArea = tilemap.GetComponent<DoorTilemapArea>();
                if (doorArea != null)
                {
                    tilemapToDoorArea[tilemap] = doorArea;
                }

                // ExhibitionArea 컴포넌트 확인
                ExhibitionArea exhibitionArea = tilemap.GetComponent<ExhibitionArea>();
                if (exhibitionArea != null)
                {
                    exhibitionTilemaps.Add(tilemap);
                }
            }

            Debug.Log($"TilemapRangeManager: {tilemapToMapArea.Count}개의 맵 타일맵, {tilemapToTerrainArea.Count}개의 지형 타일맵, {tilemapToDoorArea.Count}개의 문 타일맵, {playerMovementTilemaps.Count}개의 플레이어 이동 타일맵, {exhibitionTilemaps.Count}개의 전시 타일맵을 로드했습니다.");
        }

        /// <summary>
        /// 월드 좌표를 타일맵 셀 좌표로 변환
        /// </summary>
        private Vector3Int WorldToCell(Vector3 worldPos, Tilemap tilemap)
        {
            if (tilemap != null)
                return tilemap.WorldToCell(worldPos);
            return Vector3Int.zero;
        }

        /// <summary>
        /// 월드 좌표를 그리드 좌표로 변환 (캐싱용)
        /// </summary>
        private Vector3Int WorldToGrid(Vector3 worldPos)
        {
            return new Vector3Int(
                Mathf.FloorToInt(worldPos.x),
                Mathf.FloorToInt(worldPos.y),
                0
            );
        }

        /// <summary>
        /// 특정 위치에서 맵 타입을 가진 타일맵 찾기
        /// </summary>
        private Tilemap FindMapTilemapAtPosition(Vector3 position)
        {
            foreach (var kvp in tilemapToMapArea)
            {
                Tilemap tilemap = kvp.Key;
                if (tilemap == null) continue;

                Vector3Int cellPos = WorldToCell(position, tilemap);
                if (tilemap.HasTile(cellPos))
                {
                    return tilemap;
                }
            }

            return null;
        }

        /// <summary>
        /// 특정 위치에서 지형 타입을 가진 타일맵 찾기
        /// </summary>
        private Tilemap FindTerrainTilemapAtPosition(Vector3 position)
        {
            foreach (var kvp in tilemapToTerrainArea)
            {
                Tilemap tilemap = kvp.Key;
                if (tilemap == null) continue;

                Vector3Int cellPos = WorldToCell(position, tilemap);
                if (tilemap.HasTile(cellPos))
                {
                    return tilemap;
                }
            }

            return null;
        }

        /// <summary>
        /// 특정 위치가 플레이어 이동 가능 타일맵에 있는지 확인
        /// </summary>
        private bool IsInPlayerMovementTilemap(Vector3 position)
        {
            foreach (var tilemap in playerMovementTilemaps)
            {
                if (tilemap == null) continue;

                Vector3Int cellPos = WorldToCell(position, tilemap);
                if (tilemap.HasTile(cellPos))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 특정 위치가 맵 범위 내에 있는지 확인
        /// </summary>
        public bool IsInMapRange(Vector3 position)
        {
            Vector3Int gridPos = WorldToGrid(position);
            
            // 캐시 확인
            if (mapRangeCache.TryGetValue(gridPos, out bool cached))
                return cached;

            // 해당 위치에 타일이 있는 맵 타일맵 찾기
            Tilemap mapTilemap = FindMapTilemapAtPosition(position);
            bool inRange = mapTilemap != null;
            
            mapRangeCache[gridPos] = inRange;
            return inRange;
        }

        /// <summary>
        /// 특정 위치에 문 타일이 있고 막고 있는지 확인
        /// </summary>
        private bool IsBlockedByDoor(Vector3 position)
        {
            foreach (var kvp in tilemapToDoorArea)
            {
                Tilemap tilemap = kvp.Key;
                DoorTilemapArea doorArea = kvp.Value;
                if (tilemap == null || doorArea == null) continue;

                Vector3Int cellPos = WorldToCell(position, tilemap);
                if (tilemap.HasTile(cellPos))
                {
                    // 문 타일이 있고, 해당 문이 해금되지 않았으면 막힘
                    if (GameManager.Instance != null && !GameManager.Instance.IsDoorUnlocked(doorArea.DoorType))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 특정 위치가 플레이어 이동 가능 범위 내에 있는지 확인
        /// </summary>
        public bool IsInPlayerMovementRange(Vector3 position)
        {
            Vector3Int gridPos = WorldToGrid(position);
            
            // 캐시 확인
            if (playerMovementCache.TryGetValue(gridPos, out bool cached))
                return cached;

            // PlayerMovementArea 컴포넌트가 있는 타일맵에만 있으면 이동 가능
            bool canMove = IsInPlayerMovementTilemap(position);
            
            // 문 타일맵에 막혀있으면 이동 불가
            if (canMove && IsBlockedByDoor(position))
            {
                canMove = false;
            }
            
            playerMovementCache[gridPos] = canMove;
            return canMove;
        }

        /// <summary>
        /// 특정 위치의 맵 타입 가져오기
        /// </summary>
        public MapType GetMapTypeAtPosition(Vector3 position)
        {
            Vector3Int gridPos = WorldToGrid(position);
            
            // 캐시 확인
            if (mapTypeCache.TryGetValue(gridPos, out MapType cachedType))
                return cachedType;

            MapType mapType = MapType.MAP1; // 기본값

            // 해당 위치에 타일이 있는 맵 타일맵 찾기
            Tilemap mapTilemap = FindMapTilemapAtPosition(position);
            if (mapTilemap != null && tilemapToMapArea.TryGetValue(mapTilemap, out MapArea mapArea))
            {
                mapType = mapArea.MapType;
            }

            mapTypeCache[gridPos] = mapType;
            return mapType;
        }

        /// <summary>
        /// 특정 위치의 맵 이름 가져오기 (표시용)
        /// </summary>
        public string GetMapNameAtPosition(Vector3 position)
        {
            MapType mapType = GetMapTypeAtPosition(position);
            
            var registry = GameDataRegistry.Instance;
            if (registry?.MapTypes != null)
            {
                var mapDef = registry.MapTypes.GetMapById(mapType);
                if (mapDef != null)
                {
                    return mapDef.displayName;
                }
            }
            
            return mapType.ToString();
        }

        /// <summary>
        /// 특정 위치의 지형 타입 가져오기
        /// </summary>
        public TerrainType GetTerrainTypeAtPosition(Vector3 position)
        {
            Vector3Int gridPos = WorldToGrid(position);
            
            // 캐시 확인
            if (terrainTypeCache.TryGetValue(gridPos, out TerrainType cachedType))
                return cachedType;

            TerrainType terrainType = TerrainType.SAND; // 기본값

            // 해당 위치에 타일이 있는 지형 타일맵 찾기
            Tilemap terrainTilemap = FindTerrainTilemapAtPosition(position);
            if (terrainTilemap != null && tilemapToTerrainArea.TryGetValue(terrainTilemap, out TerrainArea terrainArea))
            {
                terrainType = terrainArea.TerrainType;
            }

            terrainTypeCache[gridPos] = terrainType;
            return terrainType;
        }

        /// <summary>
        /// 특정 범위 내의 모든 유효한 위치 가져오기 (맵 타일맵의 모든 타일 위치)
        /// </summary>
        public List<Vector3> GetAllValidPositionsInMapRange()
        {
            List<Vector3> positions = new List<Vector3>();
            
            foreach (var tilemap in tilemapToMapArea.Keys)
            {
                if (tilemap == null) continue;

                BoundsInt bounds = tilemap.cellBounds;
                foreach (var pos in bounds.allPositionsWithin)
                {
                    if (tilemap.HasTile(pos))
                    {
                        Vector3 worldPos = tilemap.CellToWorld(pos) + tilemap.cellSize * 0.5f;
                        positions.Add(worldPos);
                    }
                }
            }

            return positions;
        }

        /// <summary>
        /// 특정 위치의 플레이어 이동 캐시 무효화 (문 구매 후 호출)
        /// </summary>
        public void InvalidatePlayerMovementCache(Vector3 position)
        {
            Vector3Int gridPos = WorldToGrid(position);
            playerMovementCache.Remove(gridPos);
        }

        /// <summary>
        /// 전시 타일맵에서 랜덤 위치 가져오기
        /// </summary>
        public Vector3 GetRandomPositionInExhibitionArea()
        {
            if (exhibitionTilemaps.Count == 0)
                return Vector3.zero;

            // 모든 전시 타일맵에서 타일이 있는 위치 수집
            List<Vector3> validPositions = new List<Vector3>();
            
            foreach (var tilemap in exhibitionTilemaps)
            {
                if (tilemap == null) continue;

                BoundsInt bounds = tilemap.cellBounds;
                foreach (var pos in bounds.allPositionsWithin)
                {
                    if (tilemap.HasTile(pos))
                    {
                        Vector3 worldPos = tilemap.CellToWorld(pos) + tilemap.cellSize * 0.5f;
                        validPositions.Add(worldPos);
                    }
                }
            }

            if (validPositions.Count == 0)
                return Vector3.zero;

            // 랜덤 위치 반환
            return validPositions[Random.Range(0, validPositions.Count)];
        }

        /// <summary>
        /// 특정 위치가 전시 영역 내에 있는지 확인
        /// </summary>
        public bool IsInExhibitionArea(Vector3 position)
        {
            foreach (var tilemap in exhibitionTilemaps)
            {
                if (tilemap == null) continue;

                Vector3Int cellPos = WorldToCell(position, tilemap);
                if (tilemap.HasTile(cellPos))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
