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
}
 

