using UnityEngine;
using PigeonGame.Data;
using UnityEngine.Tilemaps;

namespace PigeonGame.Gameplay
{
    /// <summary>
    /// 문 타일맵 영역을 정의하는 컴포넌트
    /// 각 문 타일맵 GameObject에 이 컴포넌트를 추가하고 문 타입을 설정
    /// </summary>
    [RequireComponent(typeof(Tilemap))]
    public class DoorTilemapArea : MonoBehaviour
    {
        [SerializeField] private DoorType doorType = DoorType.DOOR1;

        public DoorType DoorType => doorType;
    }
}
