using UnityEngine;
using PigeonGame.Data;
using UnityEngine.Tilemaps;

namespace PigeonGame.Gameplay
{
    /// <summary>
    /// 맵 영역을 정의하는 컴포넌트
    /// 각 맵 영역 Tilemap GameObject에 이 컴포넌트를 추가하고 맵 타입을 설정
    /// </summary>
    [RequireComponent(typeof(Tilemap))]
    public class MapArea : MonoBehaviour
    {
        [SerializeField] private MapType mapType = MapType.MAP1;

        public MapType MapType => mapType;
    }
}
