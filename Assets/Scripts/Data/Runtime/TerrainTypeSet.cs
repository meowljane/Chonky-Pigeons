using UnityEngine;

namespace PigeonGame.Data
{
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
