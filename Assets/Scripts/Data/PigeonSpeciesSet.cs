using UnityEngine;
using System.Collections.Generic;

namespace PigeonGame.Data
{
    public enum PigeonSpecies
    {
        SP01, 
        SP02, 
        SP03, 
        SP04, 
        SP05, 
        SP06, 
        SP07, 
        SP08, 
        SP09  
    }

    [System.Serializable]
    public class SpeciesDefinition
    {
        public PigeonSpecies speciesType;
        public string name;
        public int rarityTier;
        public float baseSpawnWeight = 1.0f; 
        public int basePrice = 0; 
        public int unlockCost = 0; 

        public TrapType favoriteTrapType; 
        public TerrainType favoriteTerrain; 
        public Sprite icon; 

        [Tooltip("종별 Animator Controller (Idle, Walking, Flying 애니메이션 포함)")]
        public RuntimeAnimatorController animatorController; 
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
                    new SpeciesDefinition
                    {
                        speciesType = PigeonSpecies.SP01,
                        name = "집둘기",
                        rarityTier = 1,
                        baseSpawnWeight = 5.0f,
                        basePrice = 7,
                        unlockCost = 0, 
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
                        unlockCost = 0, 
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
