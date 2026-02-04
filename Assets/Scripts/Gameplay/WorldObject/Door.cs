using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using PigeonGame.Data;

namespace PigeonGame.Gameplay
{
    public class Door : InteractableBase
    {
        [Header("Door Settings")]
        [SerializeField] private DoorType doorType = DoorType.DOOR1; 
        [SerializeField] private Tilemap doorTilemap; 

        private DoorDefinition doorDefinition;
        private Vector3Int doorTilePosition; 

        protected override void Start()
        {
            base.Start();

            LoadDoorData();

            if (doorTilemap == null)
            {
                DoorTilemapArea[] doorAreas = FindObjectsByType<DoorTilemapArea>(FindObjectsSortMode.None);
                foreach (var doorArea in doorAreas)
                {
                    if (doorArea.DoorType == doorType)
                    {
                        doorTilemap = doorArea.GetComponent<Tilemap>();
                        break;
                    }
                }

                if (doorTilemap == null)
                    Debug.LogError($"DoorType {doorType}에 해당하는 DoorTilemapArea를 찾을 수 없습니다!", this);
            }

            if (doorTilemap != null)
            {
                doorTilePosition = doorTilemap.WorldToCell(transform.position);
            }

            if (GameManager.Instance?.IsDoorUnlocked(doorType) == true)
            {
                UnlockDoor();
            }
        }

        private void LoadDoorData()
        {
            var registry = GameDataRegistry.Instance;
            if (registry?.DoorSet != null)
            {
                doorDefinition = registry.DoorSet.GetDoorById(doorType);
            }

        }

        public override void OnInteract()
        {
            if (!CanInteract())
                return;

            if (GameManager.Instance?.IsDoorUnlocked(doorType) == true)
            {
                return;
            }

            int cost = doorDefinition?.unlockCost ?? 100;
            MapType mapToUnlock = doorDefinition?.unlocksMap ?? MapType.MAP1;
            InteractionSystem.Instance?.OpenDoorPurchase(this, doorType, cost, mapToUnlock);
        }

        public override bool CanInteract()
        {
            if (GameManager.Instance?.IsDoorUnlocked(doorType) == true)
            {
                return false;
            }

            return base.CanInteract();
        }

        public void UnlockDoor()
        {
            if (doorTilemap != null)
            {
                BoundsInt bounds = doorTilemap.cellBounds;
                List<Vector3Int> positionsToClear = new List<Vector3Int>();

                foreach (var pos in bounds.allPositionsWithin)
                {
                    if (doorTilemap.HasTile(pos))
                    {
                        positionsToClear.Add(pos);
                    }
                }

                foreach (var pos in positionsToClear)
                {
                    doorTilemap.SetTile(pos, null);
                }

                if (TilemapRangeManager.Instance != null)
                {
                    foreach (var pos in positionsToClear)
                    {
                        Vector3 worldPos = doorTilemap.CellToWorld(pos) + doorTilemap.cellSize * 0.5f;
                        TilemapRangeManager.Instance.InvalidatePlayerMovementCache(worldPos);
                    }
                }
            }

            Destroy(gameObject);
        }

        public DoorType DoorType => doorType;

        public int UnlockCost => doorDefinition?.unlockCost ?? 100;

        public MapType UnlocksMap => doorDefinition?.unlocksMap ?? MapType.MAP1;
    }
}
