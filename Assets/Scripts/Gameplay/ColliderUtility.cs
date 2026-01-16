using UnityEngine;

namespace PigeonGame.Gameplay
{
    /// <summary>
    /// 콜라이더 관련 유틸리티 메서드
    /// </summary>
    public static class ColliderUtility
    {
        /// <summary>
        /// 위치가 콜라이더 내부에 있는지 확인
        /// </summary>
        public static bool IsPositionInsideCollider(Vector3 position, Collider2D collider)
        {
            if (collider == null)
                return false;

            // 대부분의 콜라이더는 bounds 체크로 충분
            if (collider is BoxCollider2D || collider is CircleCollider2D || collider is EdgeCollider2D)
            {
                return collider.bounds.Contains(position);
            }

            // PolygonCollider2D만 특별 처리
            if (collider is PolygonCollider2D polygonCollider)
            {
                Vector2 pos2D = new Vector2(position.x, position.y);
                return IsPointInPolygon(pos2D, polygonCollider.points, polygonCollider.transform);
            }

            return collider.bounds.Contains(position);
        }

        /// <summary>
        /// 점이 폴리곤 내부에 있는지 확인 (PolygonCollider2D용)
        /// </summary>
        public static bool IsPointInPolygon(Vector2 point, Vector2[] polygon, Transform transform)
        {
            // 월드 좌표로 변환
            Vector2[] worldPolygon = new Vector2[polygon.Length];
            for (int i = 0; i < polygon.Length; i++)
            {
                worldPolygon[i] = transform.TransformPoint(polygon[i]);
            }

            // Ray casting 알고리즘
            bool inside = false;
            int j = worldPolygon.Length - 1;

            for (int i = 0; i < worldPolygon.Length; i++)
            {
                Vector2 vi = worldPolygon[i];
                Vector2 vj = worldPolygon[j];

                if (((vi.y > point.y) != (vj.y > point.y)) &&
                    (point.x < (vj.x - vi.x) * (point.y - vi.y) / (vj.y - vi.y) + vi.x))
                {
                    inside = !inside;
                }
                j = i;
            }

            return inside;
        }
    }
}
