using UnityEngine;
using PigeonGame.Data;

namespace PigeonGame.Gameplay
{
    public class TerrainArea : MonoBehaviour
    {
        [SerializeField] private TerrainType terrainType = TerrainType.GRASS; 

        public TerrainType TerrainType => terrainType;
    }
}
