using UnityEngine;

namespace PigeonGame.Data
{
    /// <summary>
    /// 덫 타입 Enum
    /// </summary>
    public enum TrapType
    {
        BREAD,
        SEEDS,
        CORN,
        PELLET,
        SHINY
    }

    [System.Serializable]
    public class TrapDefinition
    {
        public TrapType trapType;
        public string name;
        public Sprite icon; // 에디터에서 직접 할당
        public int unlockCost; // 해금에 필요한 골드
        public int installCost; // 해금 후 덫을 실제로 설치할 때마다 드는 골드
        public int feedCostPerUnit; // 설치 시 넣을 모이 마다 추가되는 골드 (기본 양은 20)
        public int feedAmount; // 기본 모이 양
        public int pigeonSpawnCount; // 덫 설치 시 추가로 스폰되는 비둘기 수
    }

    [CreateAssetMenu(fileName = "Traps", menuName = "PigeonGame/Traps")]
    public class TrapTypeSet : ScriptableObject
    {
        public int version;
        public TrapDefinition[] traps;

        private void OnEnable()
        {
            InitializeData();
        }

        private void InitializeData()
        {
            if (traps == null || traps.Length == 0)
            {
                version = 1;
                traps = new TrapDefinition[]
                {
                    new TrapDefinition { trapType = TrapType.BREAD, name = "바삭빵덫", unlockCost = 0, installCost = 0, feedCostPerUnit = 1, feedAmount = 20, pigeonSpawnCount = 3 },
                    new TrapDefinition { trapType = TrapType.SEEDS, name = "톡톡씨앗덫", unlockCost = 15, installCost = 8, feedCostPerUnit = 1, feedAmount = 20, pigeonSpawnCount = 4 },
                    new TrapDefinition { trapType = TrapType.CORN, name = "노랑옥수수덫", unlockCost = 25, installCost = 12, feedCostPerUnit = 2, feedAmount = 20, pigeonSpawnCount = 5 },
                    new TrapDefinition { trapType = TrapType.PELLET, name = "프리미엄알갱이덫", unlockCost = 50, installCost = 25, feedCostPerUnit = 3, feedAmount = 20, pigeonSpawnCount = 6 },
                    new TrapDefinition { trapType = TrapType.SHINY, name = "반짝간식덫", unlockCost = 100, installCost = 50, feedCostPerUnit = 5, feedAmount = 20, pigeonSpawnCount = 8 }
                };
            }
        }

        public TrapDefinition GetTrapById(TrapType trapType)
        {
            foreach (var trap in traps)
            {
                if (trap.trapType == trapType)
                    return trap;
            }
            return null;
        }
    }
}
