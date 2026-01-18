using UnityEngine;

namespace PigeonGame.Data
{
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
    /// 업그레이드 타입
    /// </summary>
    public enum UpgradeType
    {
        InventorySlots,      // 인벤토리 슬롯
        PigeonsPerMap,       // 맵당 비둘기 수
        MaxTrapCount,        // 동시 덫 설치 개수
        SpeciesWeightAdjust  // 비둘기 확률 조정 (해금만)
    }
}
