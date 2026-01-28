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
        
        // 애니메이션 관련
        [Tooltip("종별 Animator Controller (Idle, Walking, Flying 애니메이션 포함)")]
        public RuntimeAnimatorController animatorController; // 종별 애니메이션 컨트롤러
    }

    [CreateAssetMenu(fileName = "SpeciesSet", menuName = "PigeonGame/Species Set")]
    public class PigeonSpeciesSet : ScriptableObject
    {
        public int version;
        public SpeciesDefinition[] species;

        private void OnEnable()
        {
            InitializeData();
        }

        private void InitializeData()
        {
            if (species == null || species.Length == 0)
            {
                version = 1;
                species = new SpeciesDefinition[]
                {
                    // 가격순 정렬 (SP01~SP09 오름차순)
                    new SpeciesDefinition
                    {
                        speciesType = PigeonSpecies.SP01,
                        name = "집둘기",
                        rarityTier = 1,
                        baseSpawnWeight = 5.0f,
                        basePrice = 7,
                        unlockCost = 0, // 티어 1은 초기 해금
                        favoriteTrapType = TrapType.SEED,
                        favoriteTerrain = TerrainType.SAND
                    },
                    new SpeciesDefinition
                    {
                        speciesType = PigeonSpecies.SP02,
                        name = "돌둘기",
                        rarityTier = 1,
                        baseSpawnWeight = 4.5f,
                        basePrice = 8,
                        unlockCost = 0, // 티어 1은 초기 해금
                        favoriteTrapType = TrapType.CORN,
                        favoriteTerrain = TerrainType.WETLAND
                    },
                    new SpeciesDefinition
                    {
                        speciesType = PigeonSpecies.SP03,
                        name = "흑둘기",
                        rarityTier = 2,
                        baseSpawnWeight = 3.0f,
                        basePrice = 18,
                        unlockCost = 120, 
                        favoriteTrapType = TrapType.CORN,
                        favoriteTerrain = TerrainType.WETLAND
                    },
                    new SpeciesDefinition
                    {
                        speciesType = PigeonSpecies.SP04,
                        name = "백둘기",
                        rarityTier = 2,
                        baseSpawnWeight = 2.5f,
                        basePrice = 22,
                        unlockCost = 180,
                        favoriteTrapType = TrapType.BREAD,
                        favoriteTerrain = TerrainType.SNOWY
                    },
                    new SpeciesDefinition
                    {
                        speciesType = PigeonSpecies.SP05,
                        name = "꽃둘기",
                        rarityTier = 3,
                        baseSpawnWeight = 1.5f,
                        basePrice = 45,
                        unlockCost = 380,
                        favoriteTrapType = TrapType.BREAD,
                        favoriteTerrain = TerrainType.GRASS
                    },
                    new SpeciesDefinition
                    {
                        speciesType = PigeonSpecies.SP06,
                        name = "금둘기",
                        rarityTier = 3,
                        baseSpawnWeight = 1.0f,
                        basePrice = 55,
                        unlockCost = 520,
                        favoriteTrapType = TrapType.LUXURY,
                        favoriteTerrain = TerrainType.ROAD
                    },
                    new SpeciesDefinition
                    {
                        speciesType = PigeonSpecies.SP07,
                        name = "산타둘기",
                        rarityTier = 4,
                        baseSpawnWeight = 0.8f,
                        basePrice = 110,
                        unlockCost = 900,
                        favoriteTrapType = TrapType.LUXURY,
                        favoriteTerrain = TerrainType.SNOWY
                    },
                    new SpeciesDefinition
                    {
                        speciesType = PigeonSpecies.SP08,
                        name = "아이돌기",
                        rarityTier = 4,
                        baseSpawnWeight = 0.5f,
                        basePrice = 135,
                        unlockCost = 1150,
                        favoriteTrapType = TrapType.SHINY,
                        favoriteTerrain = TerrainType.ROAD
                    },
                    new SpeciesDefinition
                    {
                        speciesType = PigeonSpecies.SP09,
                        name = "냥둘기",
                        rarityTier = 5,
                        baseSpawnWeight = 0.3f,
                        basePrice = 280,
                        unlockCost = 1900,
                        favoriteTrapType = TrapType.SHINY,
                        favoriteTerrain = TerrainType.GRASS
                    }
                };
            }
        }

        public SpeciesDefinition GetSpeciesById(PigeonSpecies speciesType)
        {
            foreach (var s in species)
            {
                if (s.speciesType == speciesType)
                    return s;
            }
            return null;
        }
    }
}
