using UnityEngine;

namespace PigeonGame.Data
{
    public enum TerrainType
    {
        SAND,
        ROAD,
        WETLAND,
        SNOWY,
        GRASS
    }

    [System.Serializable]
    public class TerrainDefinition
    {
        public TerrainType terrainType;
        public string koreanName;
    }

    [CreateAssetMenu(fileName = "TerrainTypes", menuName = "PigeonGame/Terrain Types")]
    public class TerrainTypeSet : ScriptableObject
    {
        public int version;
        public TerrainDefinition[] terrains;

        private void OnEnable()
        {
            InitializeData();
        }

        private void InitializeData()
        {
            if (terrains == null || terrains.Length == 0)
            {
                version = 1;
                terrains = new TerrainDefinition[]
                {
                    new TerrainDefinition { terrainType = TerrainType.SAND, koreanName = "모래" },
                    new TerrainDefinition { terrainType = TerrainType.ROAD, koreanName = "도로" },
                    new TerrainDefinition { terrainType = TerrainType.WETLAND, koreanName = "습지" },
                    new TerrainDefinition { terrainType = TerrainType.SNOWY, koreanName = "눈밭" },
                    new TerrainDefinition { terrainType = TerrainType.GRASS, koreanName = "잔디" }
                };
            }
        }

        public TerrainDefinition GetTerrainById(TerrainType terrainType)
        {
            foreach (var terrain in terrains)
            {
                if (terrain.terrainType == terrainType)
                    return terrain;
            }
            return null;
        }
    }
}
