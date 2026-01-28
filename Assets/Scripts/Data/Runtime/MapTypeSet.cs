using UnityEngine;

namespace PigeonGame.Data
{
    /// <summary>
    /// 맵 타입 Enum
    /// </summary>
    public enum MapType
    {
        MAP1,
        MAP2,
        MAP3,
        MAP4,
        MAP5
    }

    [System.Serializable]
    public class MapDefinition
    {
        public MapType mapType;
        public string displayName; // 표시용 이름 (예: "맵 1", "도시")
    }

    [CreateAssetMenu(fileName = "MapTypes", menuName = "PigeonGame/Map Types")]
    public class MapTypeSet : ScriptableObject
    {
        public int version;
        public MapDefinition[] maps;

        private void OnEnable()
        {
            InitializeData();
        }

        private void InitializeData()
        {
            if (maps == null || maps.Length == 0)
            {
                version = 1;
                maps = new MapDefinition[]
                {
                    new MapDefinition { mapType = MapType.MAP1, displayName = "맵 1" },
                    new MapDefinition { mapType = MapType.MAP2, displayName = "맵 2" },
                    new MapDefinition { mapType = MapType.MAP3, displayName = "맵 3" },
                    new MapDefinition { mapType = MapType.MAP4, displayName = "맵 4" },
                    new MapDefinition { mapType = MapType.MAP5, displayName = "맵 5" }
                };
            }
        }

        /// <summary>
        /// 맵 타입으로 정의 가져오기
        /// </summary>
        public MapDefinition GetMapById(MapType mapType)
        {
            foreach (var map in maps)
            {
                if (map.mapType == mapType)
                    return map;
            }
            return null;
        }
    }
}
