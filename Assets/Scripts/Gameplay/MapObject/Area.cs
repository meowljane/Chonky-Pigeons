using UnityEngine;
using UnityEngine.Tilemaps;
using PigeonGame.Data;

namespace PigeonGame.Gameplay
{
    public enum AreaType
    {
        Map,
        Terrain,
        Door,
        PlayerMovement,
        Exhibition
    }

    public class Area : MonoBehaviour
    {
        [Header("Area Settings")]
        [SerializeField] private AreaType areaType = AreaType.Map;

        [Header("Type-specific Data")]
        [SerializeField] private MapType mapType = MapType.MAP1;
        [SerializeField] private TerrainType terrainType = TerrainType.GRASS;
        [SerializeField] private DoorType doorType = DoorType.DOOR1;

        public AreaType AreaType => areaType;
        public MapType MapType => mapType;
        public TerrainType TerrainType => terrainType;
        public DoorType DoorType => doorType;

        private void OnValidate()
        {
        }
    }
}
