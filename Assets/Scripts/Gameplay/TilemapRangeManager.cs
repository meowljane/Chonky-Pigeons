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

            Tilemap[] allTilemaps = FindObjectsByType<Tilemap>(FindObjectsSortMode.None);

            foreach (var tilemap in allTilemaps)
            {
                if (tilemap == null) continue;

                MapArea mapArea = tilemap.GetComponent<MapArea>();
                if (mapArea != null)
                {
                    tilemapToMapArea[tilemap] = mapArea;
                }

                PlayerMovementArea playerMovementArea = tilemap.GetComponent<PlayerMovementArea>();
                if (playerMovementArea != null)
                {
                    playerMovementTilemaps.Add(tilemap);
                }

                TerrainArea terrainArea = tilemap.GetComponent<TerrainArea>();
                if (terrainArea != null)
                {
                    tilemapToTerrainArea[tilemap] = terrainArea;
                }

                DoorTilemapArea doorArea = tilemap.GetComponent<DoorTilemapArea>();
                if (doorArea != null)
                {
                    tilemapToDoorArea[tilemap] = doorArea;
                }

                ExhibitionArea exhibitionArea = tilemap.GetComponent<ExhibitionArea>();
                if (exhibitionArea != null)
                {
                    exhibitionTilemaps.Add(tilemap);
                }
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
            foreach (var kvp in tilemapToDoorArea)
            {
                Tilemap tilemap = kvp.Key;
                DoorTilemapArea doorArea = kvp.Value;
                if (tilemap == null || doorArea == null) continue;

                Vector3Int cellPos = WorldToCell(position, tilemap);
                if (tilemap.HasTile(cellPos))
                {
                    if (GameManager.Instance != null && !GameManager.Instance.IsDoorUnlocked(doorArea.DoorType))
                    {
                        return true;
                    }
                }
            }

            return false;
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
            if (mapTilemap != null && tilemapToMapArea.TryGetValue(mapTilemap, out MapArea mapArea))
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
            if (terrainTilemap != null && tilemapToTerrainArea.TryGetValue(terrainTilemap, out TerrainArea terrainArea))
            {
                terrainType = terrainArea.TerrainType;
            }

            terrainTypeCache[gridPos] = terrainType;
            return terrainType;
        }

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

        public void InvalidatePlayerMovementCache(Vector3 position)
        {
            Vector3Int gridPos = WorldToGrid(position);
            playerMovementCache.Remove(gridPos);
        }

        public Vector3 GetRandomPositionInExhibitionArea()
        {
            if (exhibitionTilemaps.Count == 0)
                return Vector3.zero;

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

            return validPositions[Random.Range(0, validPositions.Count)];
        }

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
