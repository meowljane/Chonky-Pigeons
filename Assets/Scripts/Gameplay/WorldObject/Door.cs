using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using PigeonGame.Data;

namespace PigeonGame.Gameplay
{
    /// <summary>
    /// 문 오브젝트 (타일맵 기반)
    /// 타일맵의 특정 위치에 타일을 배치하여 길을 막고, 구매 후 해당 타일을 제거
    /// </summary>
    public class Door : InteractableBase
    {
        [Header("Door Settings")]
        [SerializeField] private DoorType doorType = DoorType.DOOR1; // 문 타입
        [SerializeField] private Tilemap doorTilemap; // 문 타일맵 (자동 감지)

        private DoorDefinition doorDefinition;
        private Vector3Int doorTilePosition; // 문 타일 위치

        protected override void Start()
        {
            base.Start();

            // DoorSet에서 문 데이터 가져오기
            LoadDoorData();

            // 문 타일맵 자동 찾기
            if (doorTilemap == null)
            {
                // DoorTilemapArea 컴포넌트가 있는 타일맵 찾기
                DoorTilemapArea[] doorAreas = FindObjectsByType<DoorTilemapArea>(FindObjectsSortMode.None);
                foreach (var doorArea in doorAreas)
                {
                    if (doorArea.DoorType == doorType)
                    {
                        doorTilemap = doorArea.GetComponent<Tilemap>();
                        break;
                    }
                }
            }

            // 문 타일 위치 저장
            if (doorTilemap != null)
            {
                doorTilePosition = doorTilemap.WorldToCell(transform.position);
            }

            // 이미 구매된 문이면 타일 제거
            if (GameManager.Instance?.IsDoorUnlocked(doorType) == true)
            {
                UnlockDoor();
            }
        }

        /// <summary>
        /// DoorSet에서 문 데이터 로드
        /// </summary>
        private void LoadDoorData()
        {
            var registry = GameDataRegistry.Instance;
            if (registry?.DoorSet != null)
            {
                doorDefinition = registry.DoorSet.GetDoorById(doorType);
            }

            // DoorSet에서 데이터를 찾지 못한 경우 기본값 사용
            if (doorDefinition == null)
            {
                Debug.LogWarning($"Door: DoorType {doorType}에 대한 데이터를 DoorSet에서 찾을 수 없습니다. 기본값을 사용합니다.");
            }
        }

        public override void OnInteract()
        {
            if (!CanInteract())
                return;

            // 이미 구매된 문이면 상호작용 불가
            if (GameManager.Instance?.IsDoorUnlocked(doorType) == true)
            {
                return;
            }

            // InteractionSystem을 통해 구매 패널 열기
            int cost = doorDefinition?.unlockCost ?? 100;
            MapType mapToUnlock = doorDefinition?.unlocksMap ?? MapType.MAP1;
            InteractionSystem.Instance?.OpenDoorPurchase(this, doorType, cost, mapToUnlock);
        }

        public override bool CanInteract()
        {
            // 이미 구매된 문이면 상호작용 불가
            if (GameManager.Instance?.IsDoorUnlocked(doorType) == true)
            {
                return false;
            }

            return base.CanInteract();
        }

        /// <summary>
        /// 문 해제 (구매 후 호출)
        /// </summary>
        public void UnlockDoor()
        {
            // 타일맵에서 해당 문 타입의 모든 타일 제거
            if (doorTilemap != null)
            {
                // 문 타일맵의 모든 타일 제거
                BoundsInt bounds = doorTilemap.cellBounds;
                List<Vector3Int> positionsToClear = new List<Vector3Int>();
                
                foreach (var pos in bounds.allPositionsWithin)
                {
                    if (doorTilemap.HasTile(pos))
                    {
                        positionsToClear.Add(pos);
                    }
                }
                
                // 모든 타일 제거
                foreach (var pos in positionsToClear)
                {
                    doorTilemap.SetTile(pos, null);
                }
                
                // 캐시 무효화 (TilemapRangeManager) - 모든 제거된 위치에 대해
                if (TilemapRangeManager.Instance != null)
                {
                    foreach (var pos in positionsToClear)
                    {
                        Vector3 worldPos = doorTilemap.CellToWorld(pos) + doorTilemap.cellSize * 0.5f;
                        TilemapRangeManager.Instance.InvalidatePlayerMovementCache(worldPos);
                    }
                }
            }
            
            // 문 오브젝트 삭제
            Destroy(gameObject);
        }

        /// <summary>
        /// 문 타입 가져오기
        /// </summary>
        public DoorType DoorType => doorType;

        /// <summary>
        /// 해금 비용 가져오기
        /// </summary>
        public int UnlockCost => doorDefinition?.unlockCost ?? 100;

        /// <summary>
        /// 해금하는 맵 타입 가져오기
        /// </summary>
        public MapType UnlocksMap => doorDefinition?.unlocksMap ?? MapType.MAP1;
    }
}
