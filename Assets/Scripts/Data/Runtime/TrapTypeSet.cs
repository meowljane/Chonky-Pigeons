using UnityEngine;

namespace PigeonGame.Data
{
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
