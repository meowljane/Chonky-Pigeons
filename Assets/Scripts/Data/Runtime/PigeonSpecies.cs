using UnityEngine;
using System.Collections.Generic;

namespace PigeonGame.Data
{
    [System.Serializable]
    public class SpeciesDefinition
    {
        public string speciesId;
        public string name;
        public int rarityTier;
        public Vector2Int defaultObesityRange;
        public float baseSpawnWeight = 1.0f; // 종별 초기 스폰 확률 가중치
        
        // 단순화된 선호도: 각 종마다 좋아하는 덫과 terrain을 하나씩만 지정
        public string favoriteTrap; // "BREAD", "SEEDS", "CORN", "PELLET", "SHINY" 중 하나
        public string favoriteTerrain; // "sand", "grass", "water", "flower", "wetland" 중 하나
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
                        favoriteTrap = "BREAD",
                        favoriteTerrain = "sand"
                    },
                    new SpeciesDefinition
                    {
                        speciesId = "SP07",
                        name = "빵중독",
                        rarityTier = 1,
                        defaultObesityRange = new Vector2Int(5, 5),
                        baseSpawnWeight = 5.0f,
                        favoriteTrap = "BREAD",
                        favoriteTerrain = "sand"
                    },
                    new SpeciesDefinition
                    {
                        speciesId = "SP14",
                        name = "무지개기름광",
                        rarityTier = 3,
                        defaultObesityRange = new Vector2Int(1, 2),
                        baseSpawnWeight = 1.5f,
                        favoriteTrap = "SEEDS",
                        favoriteTerrain = "flower"
                    },
                    new SpeciesDefinition
                    {
                        speciesId = "SP20",
                        name = "왕관비둘기",
                        rarityTier = 5,
                        defaultObesityRange = new Vector2Int(1, 1),
                        baseSpawnWeight = 0.3f,
                        favoriteTrap = "PELLET",
                        favoriteTerrain = "wetland"
                    },
                    new SpeciesDefinition
                    {
                        speciesId = "SP02",
                        name = "회색도시",
                        rarityTier = 1,
                        defaultObesityRange = new Vector2Int(3, 4),
                        baseSpawnWeight = 5.0f,
                        favoriteTrap = "BREAD",
                        favoriteTerrain = "sand"
                    },
                    new SpeciesDefinition
                    {
                        speciesId = "SP03",
                        name = "검은비둘기",
                        rarityTier = 2,
                        defaultObesityRange = new Vector2Int(3, 5),
                        baseSpawnWeight = 3.0f,
                        favoriteTrap = "CORN",
                        favoriteTerrain = "flower"
                    },
                    new SpeciesDefinition
                    {
                        speciesId = "SP08",
                        name = "흰비둘기",
                        rarityTier = 2,
                        defaultObesityRange = new Vector2Int(2, 3),
                        baseSpawnWeight = 3.0f,
                        favoriteTrap = "BREAD",
                        favoriteTerrain = "grass"
                    },
                    new SpeciesDefinition
                    {
                        speciesId = "SP15",
                        name = "무지개비둘기",
                        rarityTier = 4,
                        defaultObesityRange = new Vector2Int(1, 3),
                        baseSpawnWeight = 0.8f,
                        favoriteTrap = "SEEDS",
                        favoriteTerrain = "grass"
                    },
                    new SpeciesDefinition
                    {
                        speciesId = "SP21",
                        name = "황금비둘기",
                        rarityTier = 5,
                        defaultObesityRange = new Vector2Int(1, 2),
                        baseSpawnWeight = 0.3f,
                        favoriteTrap = "PELLET",
                        favoriteTerrain = "wetland"
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


