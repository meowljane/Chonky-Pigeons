using UnityEngine;
using PigeonGame.Data;
using UnityEngine.Tilemaps;

namespace PigeonGame.Gameplay
{
    [RequireComponent(typeof(Tilemap))]
    public class DoorTilemapArea : MonoBehaviour
    {
        [SerializeField] private DoorType doorType = DoorType.DOOR1;

        public DoorType DoorType => doorType;
    }
}
