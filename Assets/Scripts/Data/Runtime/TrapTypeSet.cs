using UnityEngine;

namespace PigeonGame.Data
{
    /// <summary>
    /// 덫 타입 Enum
    /// </summary>
    public enum TrapType
    {
        SEED,
        CORN,
        BREAD,
        LUXURY,
        SHINY
    }

    [System.Serializable]
    public class TrapDefinition
    {
        public TrapType trapType;
        public string name;
        public Sprite icon; // 설치되었을 때의 이미지 (UI 아이콘으로도 사용)
        public Sprite capturedSprite; // 포획되었을 때의 이미지
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
                    new TrapDefinition { trapType = TrapType.SEED, name = "씨앗", unlockCost = 0, installCost = 0, feedCostPerUnit = 1, feedAmount = 20, pigeonSpawnCount = 1 },
                    new TrapDefinition { trapType = TrapType.CORN, name = "옥수수", unlockCost = 240, installCost = 12, feedCostPerUnit = 2, feedAmount = 20, pigeonSpawnCount = 1 },
                    new TrapDefinition { trapType = TrapType.BREAD, name = "빵", unlockCost = 420, installCost = 22, feedCostPerUnit = 3, feedAmount = 20, pigeonSpawnCount = 2 },
                    new TrapDefinition { trapType = TrapType.LUXURY, name = "고급먹이", unlockCost = 780, installCost = 40, feedCostPerUnit = 5, feedAmount = 20, pigeonSpawnCount = 2 },
                    new TrapDefinition { trapType = TrapType.SHINY, name = "반짝먹이", unlockCost = 1400, installCost = 75, feedCostPerUnit = 8, feedAmount = 20, pigeonSpawnCount = 3 }
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
