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
        /// ScrollRect를 맨 위로 스크롤 (모든 ScrollRect에 적용)
        /// </summary>
        public static void ScrollToTop(GameObject gameObject)
        {
            if (gameObject != null)
            {
                ScrollRect[] scrollRects = gameObject.GetComponentsInChildren<ScrollRect>();
                foreach (var scrollRect in scrollRects)
                {
                    if (scrollRect != null)
                    {
                        scrollRect.verticalNormalizedPosition = 1f;
                    }
                }
            }
        }
    }
}
