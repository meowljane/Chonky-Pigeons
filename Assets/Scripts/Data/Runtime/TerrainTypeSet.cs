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

    [CreateAssetMenu(fileName = "TerrainTypes", menuName = "PigeonGame/Terrain Types")]
    public class TerrainTypeSet : ScriptableObject
    {
        public int version;
        public TerrainType[] terrainTypes;

        private void OnEnable()
        {
            InitializeData();
        }

        private void InitializeData()
        {
            if (terrainTypes == null || terrainTypes.Length == 0)
            {
                version = 1;
                terrainTypes = new TerrainType[] { TerrainType.SAND, TerrainType.GRASS, TerrainType.WATER, TerrainType.FLOWER, TerrainType.WETLAND };
            }
        }
    }
}
