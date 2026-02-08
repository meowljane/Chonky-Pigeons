using UnityEngine;

namespace PigeonGame.Data
{
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
        public Sprite icon; 
        public Sprite capturedSprite; 
        public int unlockCost; 
        public int installCost; 
        public int feedCostPerUnit; 
        public int feedAmount; 
        public int pigeonSpawnCount; 
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
                    new TrapDefinition { trapType = TrapType.CORN, name = "옥수수", unlockCost = 80, installCost = 5, feedCostPerUnit = 2, feedAmount = 20, pigeonSpawnCount = 1 },
                    new TrapDefinition { trapType = TrapType.BREAD, name = "빵", unlockCost = 180, installCost = 10, feedCostPerUnit = 3, feedAmount = 20, pigeonSpawnCount = 2 },
                    new TrapDefinition { trapType = TrapType.LUXURY, name = "고급먹이", unlockCost = 380, installCost = 20, feedCostPerUnit = 5, feedAmount = 20, pigeonSpawnCount = 2 },
                    new TrapDefinition { trapType = TrapType.SHINY, name = "반짝먹이", unlockCost = 750, installCost = 40, feedCostPerUnit = 8, feedAmount = 20, pigeonSpawnCount = 3 }
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
