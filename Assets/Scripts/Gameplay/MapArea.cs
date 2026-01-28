using UnityEngine;
using PigeonGame.Data;

namespace PigeonGame.Gameplay
{
    /// <summary>
    /// 맵 영역을 정의하는 컴포넌트
    /// 타일맵 셀 위치에 GameObject를 만들고 이 컴포넌트를 추가하여 맵 타입을 설정
    /// </summary>
    public class MapArea : MonoBehaviour
    {
        [SerializeField] private MapType mapType = MapType.MAP1; // 기본값: MAP1

        public MapType MapType => mapType;
        
        /// <summary>
        /// 맵 타입의 표시 이름 가져오기
        /// </summary>
        public string GetDisplayName()
        {
            var registry = GameDataRegistry.Instance;
            if (registry?.MapTypes != null)
            {
                var mapDef = registry.MapTypes.GetMapById(mapType);
                if (mapDef != null)
                {
                    return mapDef.displayName;
                }
            }
            return mapType.ToString();
        }
    }
}
