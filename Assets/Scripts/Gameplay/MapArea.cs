using UnityEngine;
using PigeonGame.Data;
using UnityEngine.Tilemaps;

namespace PigeonGame.Gameplay
{
    [RequireComponent(typeof(Tilemap))]
    public class MapArea : MonoBehaviour
    {
        [SerializeField] private MapType mapType = MapType.MAP1;

        public MapType MapType => mapType;
    }
}
