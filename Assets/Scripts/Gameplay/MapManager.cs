using UnityEngine;
using System.Collections.Generic;

namespace PigeonGame.Gameplay
{
    /// <summary>
    /// 맵 정보를 담는 구조체
    /// </summary>
    [System.Serializable]
    public class MapInfo
    {
        public string mapName;
        public Collider2D mapCollider;

        public MapInfo(string name, Collider2D collider)
        {
            mapName = name;
            mapCollider = collider;
        }
    }

    /// <summary>
    /// 맵 관리 시스템
    /// 맵 이름과 콜라이더, 다리를 관리
    /// </summary>
    public class MapManager : MonoBehaviour
    {
        public static MapManager Instance { get; private set; }

        [SerializeField] private MapInfo[] maps; // 맵 정보 배열
        [SerializeField] private Collider2D[] bridges; // 다리 콜라이더 배열

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
        /// 모든 맵 콜라이더 가져오기
        /// </summary>
        public Collider2D[] GetAllMapColliders()
        {
            if (maps == null || maps.Length == 0)
                return null;

            List<Collider2D> colliders = new List<Collider2D>(maps.Length);
            foreach (var map in maps)
            {
                if (map?.mapCollider != null)
                {
                    colliders.Add(map.mapCollider);
                }
            }
            return colliders.Count > 0 ? colliders.ToArray() : null;
        }

        /// <summary>
        /// 특정 콜라이더에 해당하는 맵 이름 가져오기
        /// </summary>
        public string GetMapName(Collider2D collider)
        {
            if (maps == null || collider == null)
                return "Unknown";

            foreach (var map in maps)
            {
                if (map != null && map.mapCollider == collider)
                {
                    return map.mapName;
                }
            }
            return "Unknown";
        }

        /// <summary>
        /// 특정 위치가 속한 맵 정보 가져오기
        /// </summary>
        public MapInfo GetMapAtPosition(Vector2 position)
        {
            if (maps == null)
                return null;

            foreach (var map in maps)
            {
                if (map?.mapCollider != null && 
                    ColliderUtility.IsPositionInsideCollider(position, map.mapCollider))
                {
                    return map;
                }
            }
            return null;
        }

        /// <summary>
        /// 특정 위치가 다리 위에 있는지 확인
        /// </summary>
        public bool IsPositionOnBridge(Vector2 position)
        {
            if (bridges == null)
                return false;
            
            foreach (var bridge in bridges)
            {
                if (bridge != null && ColliderUtility.IsPositionInsideCollider(position, bridge))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
