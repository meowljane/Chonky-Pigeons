using UnityEngine;

namespace PigeonGame.Data
{
    /// <summary>
    /// 지형 타입 Enum
    /// </summary>
    public enum TerrainType
    {
        SAND,
        GRASS,
        WATER,
        FLOWER,
        WETLAND
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
                    new TerrainDefinition { terrainType = TerrainType.SAND, koreanName = "모래사장" },
                    new TerrainDefinition { terrainType = TerrainType.GRASS, koreanName = "초원" },
                    new TerrainDefinition { terrainType = TerrainType.WATER, koreanName = "호수" },
                    new TerrainDefinition { terrainType = TerrainType.FLOWER, koreanName = "꽃밭" },
                    new TerrainDefinition { terrainType = TerrainType.WETLAND, koreanName = "습지" }
                };
            }
        }

        /// <summary>
        /// 지형 타입으로 정의 가져오기
        /// </summary>
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
