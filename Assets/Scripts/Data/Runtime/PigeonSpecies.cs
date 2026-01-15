using UnityEngine;
using System.Collections.Generic;

namespace PigeonGame.Data
{
    [System.Serializable]
    public class TrapPreference
    {
        public int BREAD;
        public int SEEDS;
        public int CORN;
        public int PELLET;
        public int SHINY;
    }

    [System.Serializable]
    public class TerrainPreference
    {
        public int sand; // 기본값 (기존 default 값이 여기로 이동)
        public int grass;
        public int water;
        public int flower; // 기존 sand 값이 여기로 이동
        public int wetland;
    }

    [System.Serializable]
    public class SpeciesDefinition
    {
        public string speciesId;
        public string name;
        public int rarityTier;
        public Vector2Int defaultObesityRange;
        public float baseSpawnWeight = 1.0f; // 종별 초기 스폰 확률 가중치
        public TrapPreference trapPreference;
        public TerrainPreference terrainPreference; // terrain 타입 선호도
        public Sprite icon; // 에디터에서 직접 할당
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
                        speciesId = "SP01",
                        name = "도시회색",
                        rarityTier = 1,
                        defaultObesityRange = new Vector2Int(4, 5),
                        baseSpawnWeight = 5.0f,
                        terrainPreference = new TerrainPreference { sand = 90, grass = 80, water = 30, flower = 70, wetland = 20 },
                        trapPreference = new TrapPreference { BREAD = 90, SEEDS = 60, CORN = 70, PELLET = 20, SHINY = 10 }
                    },
                    new SpeciesDefinition
                    {
                        speciesId = "SP07",
                        name = "빵중독",
                        rarityTier = 1,
                        defaultObesityRange = new Vector2Int(5, 5),
                        baseSpawnWeight = 5.0f,
                        terrainPreference = new TerrainPreference { sand = 90, grass = 90, water = 20, flower = 80, wetland = 10 },
                        trapPreference = new TrapPreference { BREAD = 100, SEEDS = 40, CORN = 60, PELLET = 10, SHINY = 10 }
                    },
                    new SpeciesDefinition
                    {
                        speciesId = "SP14",
                        name = "무지개기름광",
                        rarityTier = 3,
                        defaultObesityRange = new Vector2Int(1, 2),
                        baseSpawnWeight = 1.5f,
                        terrainPreference = new TerrainPreference { sand = 30, grass = 60, water = 55, flower = 65, wetland = 50 },
                        trapPreference = new TrapPreference { BREAD = 55, SEEDS = 60, CORN = 60, PELLET = 55, SHINY = 45 }
                    },
                    new SpeciesDefinition
                    {
                        speciesId = "SP20",
                        name = "왕관비둘기",
                        rarityTier = 5,
                        defaultObesityRange = new Vector2Int(1, 1),
                        baseSpawnWeight = 0.3f,
                        terrainPreference = new TerrainPreference { sand = 10, grass = 40, water = 60, flower = 30, wetland = 90 },
                        trapPreference = new TrapPreference { BREAD = 10, SEEDS = 25, CORN = 35, PELLET = 85, SHINY = 80 }
                    },
                    new SpeciesDefinition
                    {
                        speciesId = "SP02",
                        name = "회색도시",
                        rarityTier = 1,
                        defaultObesityRange = new Vector2Int(3, 4),
                        baseSpawnWeight = 5.0f,
                        terrainPreference = new TerrainPreference { sand = 85, grass = 75, water = 35, flower = 65, wetland = 25 },
                        trapPreference = new TrapPreference { BREAD = 85, SEEDS = 55, CORN = 65, PELLET = 15, SHINY = 5 }
                    },
                    new SpeciesDefinition
                    {
                        speciesId = "SP03",
                        name = "검은비둘기",
                        rarityTier = 2,
                        defaultObesityRange = new Vector2Int(3, 5),
                        baseSpawnWeight = 3.0f,
                        terrainPreference = new TerrainPreference { sand = 60, grass = 50, water = 50, flower = 70, wetland = 50 },
                        trapPreference = new TrapPreference { BREAD = 70, SEEDS = 70, CORN = 80, PELLET = 30, SHINY = 20 }
                    },
                    new SpeciesDefinition
                    {
                        speciesId = "SP08",
                        name = "흰비둘기",
                        rarityTier = 2,
                        defaultObesityRange = new Vector2Int(2, 3),
                        baseSpawnWeight = 3.0f,
                        terrainPreference = new TerrainPreference { sand = 60, grass = 85, water = 70, flower = 40, wetland = 35 },
                        trapPreference = new TrapPreference { BREAD = 75, SEEDS = 65, CORN = 70, PELLET = 40, SHINY = 30 }
                    },
                    new SpeciesDefinition
                    {
                        speciesId = "SP15",
                        name = "무지개비둘기",
                        rarityTier = 4,
                        defaultObesityRange = new Vector2Int(1, 3),
                        baseSpawnWeight = 0.8f,
                        terrainPreference = new TerrainPreference { sand = 20, grass = 70, water = 60, flower = 55, wetland = 60 },
                        trapPreference = new TrapPreference { BREAD = 60, SEEDS = 65, CORN = 65, PELLET = 60, SHINY = 50 }
                    },
                    new SpeciesDefinition
                    {
                        speciesId = "SP21",
                        name = "황금비둘기",
                        rarityTier = 5,
                        defaultObesityRange = new Vector2Int(1, 2),
                        baseSpawnWeight = 0.3f,
                        terrainPreference = new TerrainPreference { sand = 10, grass = 30, water = 70, flower = 40, wetland = 90 },
                        trapPreference = new TrapPreference { BREAD = 15, SEEDS = 30, CORN = 40, PELLET = 90, SHINY = 90 }
                    }
                };
            }
        }

        public SpeciesDefinition GetSpeciesById(string speciesId)
        {
            foreach (var s in species)
            {
                if (s.speciesId == speciesId)
                    return s;
            }
            return null;
        }
    }
}


