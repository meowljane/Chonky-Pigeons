using UnityEngine;
using PigeonGame.Data;

namespace PigeonGame.Gameplay
{
    /// <summary>
    /// Terrain 영역을 정의하는 컴포넌트
    /// 타일맵 셀 위치에 GameObject를 만들고 이 컴포넌트를 추가하여 지형 타입을 설정
    /// </summary>
    public class TerrainArea : MonoBehaviour
    {
        [SerializeField] private TerrainType terrainType = TerrainType.GRASS; // 기본값: GRASS

        public TerrainType TerrainType => terrainType;
    }
}
