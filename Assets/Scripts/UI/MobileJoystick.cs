using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PigeonGame.UI
{
    /// <summary>
    /// 좌측 하단 모바일 조이스틱
    /// 드래그로 플레이어 이동 제어
    /// </summary>
    public class MobileJoystick : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform background;
        [SerializeField] private RectTransform handle;
        [SerializeField] private float handleRange = 1f; // 핸들이 움직일 수 있는 범위 (0~1)
        
        private Vector2 inputVector = Vector2.zero;
        private bool isDragging = false;
        
        /// <summary>
        /// 조이스틱 입력 값 (정규화된 벡터, -1 ~ 1)
        /// </summary>
        public Vector2 InputVector => inputVector;
        
        /// <summary>
        /// 조이스틱이 활성화되어 있는지 (드래그 중인지)
        /// </summary>
        public bool IsActive => isDragging;

        private void Start()
        {
            // 자동으로 UI 요소 찾기
            if (background == null)
            {
                background = transform.Find("Background")?.GetComponent<RectTransform>();
            }
            if (handle == null)
            {
                handle = transform.Find("Handle")?.GetComponent<RectTransform>();
            }
            
            // 초기 위치 설정
            if (handle != null && background != null)
            {
                handle.anchoredPosition = Vector2.zero;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            isDragging = true;
            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (background == null || handle == null)
                return;

            Vector2 localPoint;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                background, eventData.position, eventData.pressEventCamera, out localPoint))
            {
                // 배경의 크기에 맞춰 정규화
                Vector2 sizeDelta = background.sizeDelta;
                localPoint.x /= sizeDelta.x * 0.5f;
                localPoint.y /= sizeDelta.y * 0.5f;

                // 입력 벡터 계산
                inputVector = Vector2.ClampMagnitude(localPoint, handleRange);
                
                // 핸들 위치 업데이트
                handle.anchoredPosition = inputVector * (sizeDelta * 0.5f * handleRange);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isDragging = false;
            inputVector = Vector2.zero;
            
            if (handle != null)
            {
                handle.anchoredPosition = Vector2.zero;
            }
        }

        /// <summary>
        /// 조이스틱 리셋 (외부에서 호출 가능)
        /// </summary>
        public void ResetJoystick()
        {
            isDragging = false;
            inputVector = Vector2.zero;
            if (handle != null)
            {
                handle.anchoredPosition = Vector2.zero;
            }
        }
    }
}

