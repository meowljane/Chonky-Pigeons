using UnityEngine;
using System.Collections.Generic;

namespace PigeonGame.Data
{
    /// <summary>
    /// 비둘기 종 Enum
    /// </summary>
    public enum PigeonSpecies
    {
        SP01, // 도시회색
        SP02, // 회색도시
        SP03, // 검은비둘기
        SP04, // 빵중독
        SP05, // 흰비둘기
        SP06, // 무지개기름광
        SP07, // 무지개비둘기
        SP08, // 왕관비둘기
        SP09  // 황금비둘기
    }
 
    [System.Serializable]
    public class SpeciesDefinition
    {
        public PigeonSpecies speciesType;
        public string name;
        public int rarityTier;
        public float baseSpawnWeight = 1.0f; // 종별 초기 스폰 확률 가중치
        public int basePrice = 0; // 비둘기 판매 가격 기본값 (필수, 0이면 안 됨)
        public int unlockCost = 0; // 해금 비용 (0이면 티어 * 50으로 계산)
        
        // 단순화된 선호도: 각 종마다 좋아하는 덫과 terrain을 하나씩만 지정
        public TrapType favoriteTrapType; // enum 타입 (Inspector에서 선택)
        public TerrainType favoriteTerrain; // enum 타입 (Inspector에서 선택)
        public Sprite icon; // 에디터에서 직접 할당
    }
}


