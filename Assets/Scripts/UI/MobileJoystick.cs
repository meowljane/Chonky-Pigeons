using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PigeonGame.UI
{
    public class MobileJoystick : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Joystick Components")]
        [SerializeField] private RectTransform background;
        [SerializeField] private RectTransform handle;
        [SerializeField] private float handleRange = 1f; 

        private Vector2 inputVector = Vector2.zero;
        private bool isDragging = false;

        public Vector2 InputVector => inputVector;

        public bool IsActive => isDragging;

        private void Start()
        {
            if (background == null)
                Debug.LogError("Background RectTransform이 할당되지 않았습니다!", this);
            if (handle == null)
                Debug.LogError("Handle RectTransform이 할당되지 않았습니다!", this);

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
                Vector2 sizeDelta = background.sizeDelta;
                localPoint.x /= sizeDelta.x * 0.5f;
                localPoint.y /= sizeDelta.y * 0.5f;

                inputVector = Vector2.ClampMagnitude(localPoint, handleRange);

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

