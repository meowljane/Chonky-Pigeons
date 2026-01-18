using UnityEngine;

namespace PigeonGame.UI
{
    /// <summary>
    /// 원형 범위를 시각적으로 표시하는 컴포넌트
    /// </summary>
    public class CircleRangeIndicator : MonoBehaviour
    {
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private int segments = 64; // 원의 세그먼트 수 (높을수록 부드러움)
        [SerializeField] private float lineWidth = 0.1f;
        [SerializeField] private Color lineColor = new Color(1f, 1f, 0f, 0.8f); // 노란색 반투명
        
        private float currentRadius = 1f;
        private bool isVisible = false;

        private void Awake()
        {
            // LineRenderer가 없으면 자동 생성
            if (lineRenderer == null)
            {
                lineRenderer = GetComponent<LineRenderer>();
                if (lineRenderer == null)
                {
                    GameObject lineObj = new GameObject("RangeIndicatorLine");
                    lineObj.transform.SetParent(transform);
                    lineObj.transform.localPosition = Vector3.zero;
                    lineRenderer = lineObj.AddComponent<LineRenderer>();
                }
            }

            SetupLineRenderer();
        }

        private void SetupLineRenderer()
        {
            if (lineRenderer == null)
                return;

            lineRenderer.useWorldSpace = false;
            
            // Material 설정 (기본 Material 사용)
            if (lineRenderer.material == null)
            {
                Shader shader = Shader.Find("Sprites/Default");
                if (shader != null)
                {
                    lineRenderer.material = new Material(shader);
                }
            }
            
            lineRenderer.startColor = lineColor;
            lineRenderer.endColor = lineColor;
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            lineRenderer.positionCount = segments + 1;
            lineRenderer.loop = true;
            lineRenderer.sortingOrder = 100; // 다른 오브젝트 위에 표시
        }

        /// <summary>
        /// 범위 표시 설정
        /// </summary>
        public void SetRadius(float radius)
        {
            currentRadius = radius;
            UpdateCircle();
        }

        /// <summary>
        /// 표시/숨김 설정
        /// </summary>
        public void SetVisible(bool visible)
        {
            isVisible = visible;
            if (lineRenderer != null)
            {
                lineRenderer.enabled = visible;
            }
        }

        /// <summary>
        /// 색상 설정
        /// </summary>
        public void SetColor(Color color)
        {
            lineColor = color;
            if (lineRenderer != null)
            {
                lineRenderer.startColor = color;
                lineRenderer.endColor = color;
            }
        }

        /// <summary>
        /// 원형 라인 업데이트
        /// </summary>
        private void UpdateCircle()
        {
            if (lineRenderer == null || !isVisible)
                return;

            float angle = 0f;
            for (int i = 0; i <= segments; i++)
            {
                float x = Mathf.Sin(Mathf.Deg2Rad * angle) * currentRadius;
                float y = Mathf.Cos(Mathf.Deg2Rad * angle) * currentRadius;
                lineRenderer.SetPosition(i, new Vector3(x, y, 0f));
                angle += 360f / segments;
            }
        }

        private void Update()
        {
            if (isVisible)
            {
                UpdateCircle();
            }
        }
    }
}
