using UnityEngine;
using PigeonGame.Data;

namespace PigeonGame.Gameplay
{
    /// <summary>
    /// 맵 관리 시스템 (타일맵 기반)
    /// </summary>
    public class MapManager : MonoBehaviour
    {
        public static MapManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// 특정 위치의 맵 이름 가져오기
        /// </summary>
        public string GetMapNameAtPosition(Vector2 position)
        {
            if (TilemapRangeManager.Instance != null)
            {
                return TilemapRangeManager.Instance.GetMapNameAtPosition(position);
            }
            return "Unknown";
        }

        /// <summary>
        /// 특정 위치의 지역 타입 반환 (TerrainType)
        /// </summary>
        public TerrainType GetTerrainTypeAtPosition(Vector3 position)
        {
            if (TilemapRangeManager.Instance != null)
            {
                return TilemapRangeManager.Instance.GetTerrainTypeAtPosition(position);
            }
            return TerrainType.SAND;
        }

        /// <summary>
        /// 타일맵 기반으로 맵 범위 확인
        /// </summary>
        public bool IsPositionInMapRange(Vector2 position)
        {
            if (TilemapRangeManager.Instance != null)
            {
                return TilemapRangeManager.Instance.IsInMapRange(position);
            }
            return false;
        }
    }
}
