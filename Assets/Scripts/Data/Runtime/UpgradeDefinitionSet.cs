using UnityEngine;

namespace PigeonGame.Data
{
    /// <summary>
    /// 업그레이드 타입
    /// </summary>
    public enum UpgradeType
    {
        InventorySlots,      // 인벤토리 슬롯
        PigeonsPerMap,       // 맵당 비둘기 수
        MaxTrapCount,        // 동시 덫 설치 개수
        SpeciesWeightAdjust  // 비둘기 확률 조정 (해금만)
    }

    /// <summary>
    /// 업그레이드 정의 (단계별 정보)
    /// </summary>
    [System.Serializable]
    public class UpgradeDefinition
    {
        public string upgradeName;
        public int[] costs; // 각 단계별 비용
        public int[] values; // 각 단계별 값
        public UpgradeType upgradeType;
    }

    /// <summary>
    /// 업그레이드 정의 집합 (ScriptableObject)
    /// </summary>
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
                        costs = new int[] { 200, 450, 900 },
                        values = new int[] { 15, 20, 25 },
                        upgradeType = UpgradeType.InventorySlots
                    },
                    new UpgradeDefinition
                    {
                        upgradeName = "비둘기 스폰 증가",
                        costs = new int[] { 160, 380, 780 },
                        values = new int[] { 10, 15, 20 },
                        upgradeType = UpgradeType.PigeonsPerMap
                    },
                    new UpgradeDefinition
                    {
                        upgradeName = "덫 설치 개수 증가",
                        costs = new int[] { 260, 620, 1300 },
                        values = new int[] { 5, 7, 10 },
                        upgradeType = UpgradeType.MaxTrapCount
                    }
                };
            }
        }

        /// <summary>
        /// 업그레이드 타입으로 정의 가져오기
        /// </summary>
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
