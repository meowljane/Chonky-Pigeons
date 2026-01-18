using UnityEngine;
using UnityEngine.UI;

namespace PigeonGame.UI
{
    /// <summary>
    /// ScrollRect 헬퍼 유틸리티
    /// </summary>
    public static class ScrollRectHelper
    {
        /// <summary>
        /// ScrollRect를 맨 위로 스크롤
        /// </summary>
        /// <param name="scrollRect">ScrollRect 컴포넌트</param>
        public static void ScrollToTop(ScrollRect scrollRect)
        {
            if (scrollRect != null)
            {
                scrollRect.verticalNormalizedPosition = 1f; // 1f = 맨 위, 0f = 맨 아래
            }
        }

        /// <summary>
        /// GameObject에서 ScrollRect를 찾아서 맨 위로 스크롤
        /// </summary>
        /// <param name="gameObject">ScrollRect를 포함하는 GameObject</param>
        public static void ScrollToTop(GameObject gameObject)
        {
            if (gameObject != null)
            {
                ScrollRect scrollRect = gameObject.GetComponentInChildren<ScrollRect>();
                ScrollToTop(scrollRect);
            }
        }

        /// <summary>
        /// Transform에서 ScrollRect를 찾아서 맨 위로 스크롤
        /// </summary>
        /// <param name="transform">ScrollRect를 포함하는 Transform</param>
        public static void ScrollToTop(Transform transform)
        {
            if (transform != null)
            {
                ScrollRect scrollRect = transform.GetComponentInChildren<ScrollRect>();
                ScrollToTop(scrollRect);
            }
        }
    }
}
