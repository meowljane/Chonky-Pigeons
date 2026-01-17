using UnityEngine;
using System.Collections.Generic;

namespace PigeonGame.Data
{
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
                        name = "도시회색",
                        rarityTier = 1,
                        baseSpawnWeight = 5.0f,
                        basePrice = 8,
                        unlockCost = 0, // 티어 1은 초기 해금
                        favoriteTrapType = TrapType.BREAD,
                        favoriteTerrain = TerrainType.SAND
                    },
                    new SpeciesDefinition
                    {
                        speciesType = PigeonSpecies.SP02,
                        name = "회색도시",
                        rarityTier = 1,
                        baseSpawnWeight = 5.0f,
                        basePrice = 8,
                        unlockCost = 0, // 티어 1은 초기 해금
                        favoriteTrapType = TrapType.BREAD,
                        favoriteTerrain = TerrainType.SAND
                    },
                    new SpeciesDefinition
                    {
                        speciesType = PigeonSpecies.SP03,
                        name = "빵중독",
                        rarityTier = 1,
                        baseSpawnWeight = 5.0f,
                        basePrice = 8,
                        unlockCost = 0, // 티어 1은 초기 해금
                        favoriteTrapType = TrapType.BREAD,
                        favoriteTerrain = TerrainType.SAND
                    },
                    new SpeciesDefinition
                    {
                        speciesType = PigeonSpecies.SP04,
                        name = "검은비둘기",
                        rarityTier = 2,
                        baseSpawnWeight = 3.0f,
                        basePrice = 18,
                        unlockCost = 100, // 티어 2 * 50
                        favoriteTrapType = TrapType.CORN,
                        favoriteTerrain = TerrainType.FLOWER
                    },
                    new SpeciesDefinition
                    {
                        speciesType = PigeonSpecies.SP05,
                        name = "흰비둘기",
                        rarityTier = 2,
                        baseSpawnWeight = 3.0f,
                        basePrice = 18,
                        unlockCost = 100, // 티어 2 * 50
                        favoriteTrapType = TrapType.BREAD,
                        favoriteTerrain = TerrainType.GRASS
                    },
                    new SpeciesDefinition
                    {
                        speciesType = PigeonSpecies.SP06,
                        name = "무지개기름광",
                        rarityTier = 3,
                        baseSpawnWeight = 1.5f,
                        basePrice = 40,
                        unlockCost = 150, // 티어 3 * 50
                        favoriteTrapType = TrapType.SEEDS,
                        favoriteTerrain = TerrainType.FLOWER
                    },
                    new SpeciesDefinition
                    {
                        speciesType = PigeonSpecies.SP07,
                        name = "무지개비둘기",
                        rarityTier = 4,
                        baseSpawnWeight = 0.8f,
                        basePrice = 90,
                        unlockCost = 200, // 티어 4 * 50
                        favoriteTrapType = TrapType.SEEDS,
                        favoriteTerrain = TerrainType.GRASS
                    },
                    new SpeciesDefinition
                    {
                        speciesType = PigeonSpecies.SP08,
                        name = "왕관비둘기",
                        rarityTier = 5,
                        baseSpawnWeight = 0.3f,
                        basePrice = 180,
                        unlockCost = 250, // 티어 5 * 50
                        favoriteTrapType = TrapType.PELLET,
                        favoriteTerrain = TerrainType.WETLAND
                    },
                    new SpeciesDefinition
                    {
                        speciesType = PigeonSpecies.SP09,
                        name = "황금비둘기",
                        rarityTier = 5,
                        baseSpawnWeight = 0.3f,
                        basePrice = 180,
                        unlockCost = 250, // 티어 5 * 50
                        favoriteTrapType = TrapType.PELLET,
                        favoriteTerrain = TerrainType.WETLAND
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
