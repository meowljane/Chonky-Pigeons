using UnityEngine;

namespace PigeonGame.Gameplay
{
    /// <summary>
    /// Terrain 영역을 정의하는 컴포넌트
    /// 각 terrain 영역 GameObject에 이 컴포넌트를 추가하고 terrain 타입을 설정
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class TerrainArea : MonoBehaviour
    {
        [SerializeField] private string terrainType = "grass"; // "grass", "water", "sand", "wetland" (기본값: "grass")

        public string TerrainType => terrainType;
        public Collider2D AreaCollider { get; private set; }

        private void Awake()
        {
            AreaCollider = GetComponent<Collider2D>();
        }

        /// <summary>
        /// 위치가 이 terrain 영역 내부에 있는지 확인
        /// </summary>
        public bool ContainsPosition(Vector3 position)
        {
            if (AreaCollider == null)
                return false;

            return ColliderUtility.IsPositionInsideCollider(position, AreaCollider);
        }
    }
}
