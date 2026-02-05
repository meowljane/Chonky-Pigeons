using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using PigeonGame.Data;

namespace PigeonGame.Gameplay
{
    public class TilemapRangeManager : MonoBehaviour
    {
        public static TilemapRangeManager Instance { get; private set; }

        private Dictionary<Vector3Int, MapType> mapTypeCache = new Dictionary<Vector3Int, MapType>();
        private Dictionary<Vector3Int, TerrainType> terrainTypeCache = new Dictionary<Vector3Int, TerrainType>();
        private Dictionary<Vector3Int, bool> mapRangeCache = new Dictionary<Vector3Int, bool>();
        private Dictionary<Vector3Int, bool> playerMovementCache = new Dictionary<Vector3Int, bool>();

        private Dictionary<Tilemap, Area> tilemapToArea = new Dictionary<Tilemap, Area>();

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

        private void InitializeTilemaps()
        {
            tilemapToArea.Clear();
            mapTypeCache.Clear();
            terrainTypeCache.Clear();
            mapRangeCache.Clear();
            playerMovementCache.Clear();

            Area[] allAreas = FindObjectsByType<Area>(FindObjectsSortMode.None);
            foreach (var area in allAreas)
            {
                if (area == null) continue;

                Tilemap tilemap = area.GetComponent<Tilemap>();
                if (tilemap != null)
                    tilemapToArea[tilemap] = area;
            }
        }

        private Vector3Int WorldToCell(Vector3 worldPos, Tilemap tilemap)
        {
            if (tilemap != null)
                return tilemap.WorldToCell(worldPos);
            return Vector3Int.zero;
        }

        private Vector3Int WorldToGrid(Vector3 worldPos)
        {
            return new Vector3Int(
                Mathf.FloorToInt(worldPos.x),
                Mathf.FloorToInt(worldPos.y),
                0
            );
        }

        private Tilemap FindTilemapAtPosition(Vector3 position, AreaType areaType)
        {
            foreach (var kvp in tilemapToArea)
            {
                Tilemap tilemap = kvp.Key;
                Area area = kvp.Value;
                if (tilemap == null || area == null || area.AreaType != areaType) continue;

                Vector3Int cellPos = WorldToCell(position, tilemap);
                if (tilemap.HasTile(cellPos))
                    return tilemap;
            }
            return null;
        }

        private Tilemap FindMapTilemapAtPosition(Vector3 position) => FindTilemapAtPosition(position, AreaType.Map);
        private Tilemap FindTerrainTilemapAtPosition(Vector3 position) => FindTilemapAtPosition(position, AreaType.Terrain);

        private bool IsInPlayerMovementTilemap(Vector3 position)
        {
            return FindTilemapAtPosition(position, AreaType.PlayerMovement) != null;
        }

        public bool IsInMapRange(Vector3 position)
        {
            Vector3Int gridPos = WorldToGrid(position);

            if (mapRangeCache.TryGetValue(gridPos, out bool cached))
                return cached;

            Tilemap mapTilemap = FindMapTilemapAtPosition(position);
            bool inRange = mapTilemap != null;

            mapRangeCache[gridPos] = inRange;
            return inRange;
        }

        private bool IsBlockedByDoor(Vector3 position)
        {
            Tilemap doorTilemap = FindTilemapAtPosition(position, AreaType.Door);
            if (doorTilemap == null || !tilemapToArea.TryGetValue(doorTilemap, out Area doorArea))
                return false;

            return GameManager.Instance != null && !GameManager.Instance.IsDoorUnlocked(doorArea.DoorType);
        }

        public bool IsInPlayerMovementRange(Vector3 position)
        {
            Vector3Int gridPos = WorldToGrid(position);

            if (playerMovementCache.TryGetValue(gridPos, out bool cached))
                return cached;

            bool canMove = IsInPlayerMovementTilemap(position);

            if (canMove && IsBlockedByDoor(position))
            {
                canMove = false;
            }

            playerMovementCache[gridPos] = canMove;
            return canMove;
        }

        public MapType GetMapTypeAtPosition(Vector3 position)
        {
            Vector3Int gridPos = WorldToGrid(position);

            if (mapTypeCache.TryGetValue(gridPos, out MapType cachedType))
                return cachedType;

            MapType mapType = MapType.MAP1; 

            Tilemap mapTilemap = FindMapTilemapAtPosition(position);
            if (mapTilemap != null && tilemapToArea.TryGetValue(mapTilemap, out Area mapArea) && mapArea.AreaType == AreaType.Map)
            {
                mapType = mapArea.MapType;
            }

            mapTypeCache[gridPos] = mapType;
            return mapType;
        }

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

        public TerrainType GetTerrainTypeAtPosition(Vector3 position)
        {
            Vector3Int gridPos = WorldToGrid(position);

            if (terrainTypeCache.TryGetValue(gridPos, out TerrainType cachedType))
                return cachedType;

            TerrainType terrainType = TerrainType.SAND; 

            Tilemap terrainTilemap = FindTerrainTilemapAtPosition(position);
            if (terrainTilemap != null && tilemapToArea.TryGetValue(terrainTilemap, out Area terrainArea) && terrainArea.AreaType == AreaType.Terrain)
            {
                terrainType = terrainArea.TerrainType;
            }

            terrainTypeCache[gridPos] = terrainType;
            return terrainType;
        }

        private List<Vector3> GetValidPositionsForAreaType(AreaType areaType)
        {
            List<Vector3> positions = new List<Vector3>();

            foreach (var kvp in tilemapToArea)
            {
                Tilemap tilemap = kvp.Key;
                Area area = kvp.Value;
                if (tilemap == null || area == null || area.AreaType != areaType) continue;

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

        public List<Vector3> GetAllValidPositionsInMapRange()
        {
            return GetValidPositionsForAreaType(AreaType.Map);
        }

        public void InvalidatePlayerMovementCache(Vector3 position)
        {
            Vector3Int gridPos = WorldToGrid(position);
            playerMovementCache.Remove(gridPos);
        }

        public Tilemap GetDoorTilemapByType(DoorType doorType)
        {
            foreach (var kvp in tilemapToArea)
            {
                Area area = kvp.Value;
                if (area != null && area.AreaType == AreaType.Door && area.DoorType == doorType)
                    return kvp.Key;
            }
            return null;
        }

        public Vector3 GetRandomPositionInExhibitionArea()
        {
            List<Vector3> validPositions = GetValidPositionsForAreaType(AreaType.Exhibition);

            if (validPositions.Count == 0)
                return Vector3.zero;

            return validPositions[Random.Range(0, validPositions.Count)];
        }

        public bool IsInExhibitionArea(Vector3 position)
        {
            return FindTilemapAtPosition(position, AreaType.Exhibition) != null;
        }
    }
}
