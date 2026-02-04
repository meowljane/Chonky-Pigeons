using UnityEngine;
using PigeonGame.Data;

namespace PigeonGame.Gameplay
{
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

        public string GetMapNameAtPosition(Vector2 position)
        {
            if (TilemapRangeManager.Instance != null)
            {
                return TilemapRangeManager.Instance.GetMapNameAtPosition(position);
            }
            return "Unknown";
        }

        public TerrainType GetTerrainTypeAtPosition(Vector3 position)
        {
            if (TilemapRangeManager.Instance != null)
            {
                return TilemapRangeManager.Instance.GetTerrainTypeAtPosition(position);
            }
            return TerrainType.SAND;
        }

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
