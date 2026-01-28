using UnityEngine;

namespace PigeonGame.Data
{
    /// <summary>
    /// 문 타입 Enum
    /// </summary>
    public enum DoorType
    {
        DOOR1,
        DOOR2,
        DOOR3,
        DOOR4,
    }

    [System.Serializable]
    public class DoorDefinition
    {
        public DoorType doorType;
        public int unlockCost; // 해금 비용
        public MapType unlocksMap; // 이 문이 해금하는 맵
    }

    [CreateAssetMenu(fileName = "Doors", menuName = "PigeonGame/Door Set")]
    public class DoorSet : ScriptableObject
    {
        public int version;
        public DoorDefinition[] doors;

        private void OnEnable()
        {
            InitializeData();
        }

        private void InitializeData()
        {
            if (doors == null || doors.Length == 0)
            {
                version = 1;
                doors = new DoorDefinition[]
                {
                    new DoorDefinition { doorType = DoorType.DOOR1, unlockCost = 200, unlocksMap = MapType.MAP2 },
                    new DoorDefinition { doorType = DoorType.DOOR2, unlockCost = 550, unlocksMap = MapType.MAP3 },
                    new DoorDefinition { doorType = DoorType.DOOR3, unlockCost = 1200, unlocksMap = MapType.MAP4 },
                    new DoorDefinition { doorType = DoorType.DOOR4, unlockCost = 2500, unlocksMap = MapType.MAP5 },
                };
            }
        }

        /// <summary>
        /// 문 타입으로 정의 가져오기
        /// </summary>
        public DoorDefinition GetDoorById(DoorType doorType)
        {
            foreach (var door in doors)
            {
                if (door.doorType == doorType)
                    return door;
            }
            return null;
        }
    }
}
