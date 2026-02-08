using UnityEngine;

namespace PigeonGame.Data
{
    public enum UpgradeType
    {
        InventorySlots,      
        PigeonsPerMap,       
        MaxTrapCount,        
        SpeciesWeightAdjust  
    }

    [System.Serializable]
    public class UpgradeDefinition
    {
        public string upgradeName;
        public int[] costs; 
        public int[] values; 
        public UpgradeType upgradeType;
    }

    [CreateAssetMenu(fileName = "UpgradeDefinitions", menuName = "PigeonGame/Upgrade Definitions")]
    public class UpgradeDefinitionSet : ScriptableObject
    {
        public int version;
        public UpgradeDefinition[] upgrades;

        private void OnEnable()
        {
            InitializeData();
        }

        private void InitializeData()
        {
            if (upgrades == null || upgrades.Length == 0)
            {
                version = 1;
                upgrades = new UpgradeDefinition[]
                {
                    new UpgradeDefinition
                    {
                        upgradeName = "인벤토리 확장",
                        costs = new int[] { 80, 200, 450 },
                        values = new int[] { 15, 20, 25 },
                        upgradeType = UpgradeType.InventorySlots
                    },
                    new UpgradeDefinition
                    {
                        upgradeName = "비둘기 스폰 증가",
                        costs = new int[] { 60, 180, 400 },
                        values = new int[] { 10, 15, 20 },
                        upgradeType = UpgradeType.PigeonsPerMap
                    },
                    new UpgradeDefinition
                    {
                        upgradeName = "덫 설치 개수 증가",
                        costs = new int[] { 100, 280, 650 },
                        values = new int[] { 5, 7, 10 },
                        upgradeType = UpgradeType.MaxTrapCount
                    }
                };
            }
        }

        public UpgradeDefinition GetUpgradeByType(UpgradeType upgradeType)
        {
            if (upgrades == null)
                return null;

            foreach (var upgrade in upgrades)
            {
                if (upgrade != null && upgrade.upgradeType == upgradeType)
                    return upgrade;
            }
            return null;
        }
    }
}
