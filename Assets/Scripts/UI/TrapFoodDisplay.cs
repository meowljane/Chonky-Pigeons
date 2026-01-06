using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PigeonGame.Gameplay;

namespace PigeonGame.UI
{
    /// <summary>
    /// 덫 오브젝트에 직접 붙는 먹이 양 표시 UI
    /// World Space Canvas를 사용하여 덫 위에 표시됨
    /// </summary>
    public class TrapFoodDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI foodText;
        [SerializeField] private Image foodBar;
        [SerializeField] private string foodFormat = "남은 먹이: {0}/{1}";
        [SerializeField] private Color fullColor = Color.green;
        [SerializeField] private Color emptyColor = Color.red;
        [SerializeField] private Vector3 offset = new Vector3(0, 1f, 0); // 덫 위에 표시할 오프셋

        private FoodTrap trap;
        private Camera mainCamera;

        private void Start()
        {
            trap = GetComponentInParent<FoodTrap>();
            if (trap == null)
            {
                trap = GetComponent<FoodTrap>();
            }

            if (trap == null)
            {
                Debug.LogError("TrapFoodDisplay: FoodTrap을 찾을 수 없습니다!");
                enabled = false;
                return;
            }

            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = FindObjectOfType<Camera>();
            }

            // UI가 없으면 자동 생성
            if (foodText == null || foodBar == null)
            {
                CreateUI();
            }
        }

        private void CreateUI()
        {
            // World Space Canvas 생성
            GameObject canvasObj = new GameObject("TrapFoodCanvas");
            canvasObj.transform.SetParent(transform);
            canvasObj.transform.localPosition = offset;
            canvasObj.transform.localRotation = Quaternion.identity;
            canvasObj.transform.localScale = Vector3.one * 0.01f; // World Space에서 적절한 크기

            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;

            GraphicRaycaster raycaster = canvasObj.AddComponent<GraphicRaycaster>();

            // 배경 패널
            GameObject panelObj = new GameObject("Panel");
            panelObj.transform.SetParent(canvasObj.transform, false);
            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.sizeDelta = new Vector2(200, 60);
            panelRect.anchoredPosition = Vector2.zero;

            Image panelImage = panelObj.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.7f);

            // 텍스트 생성
            if (foodText == null)
            {
                GameObject textObj = new GameObject("FoodText");
                textObj.transform.SetParent(panelObj.transform, false);
                foodText = textObj.AddComponent<TextMeshProUGUI>();
                foodText.text = "먹이: 20/20";
                foodText.fontSize = 20;
                foodText.color = Color.white;
                foodText.alignment = TextAlignmentOptions.Center;

                RectTransform textRect = foodText.GetComponent<RectTransform>();
                textRect.anchorMin = new Vector2(0, 0.5f);
                textRect.anchorMax = new Vector2(1, 1);
                textRect.sizeDelta = Vector2.zero;
                textRect.anchoredPosition = Vector2.zero;
            }

            // 진행 바 생성
            if (foodBar == null)
            {
                GameObject barBgObj = new GameObject("BarBackground");
                barBgObj.transform.SetParent(panelObj.transform, false);
                Image barBg = barBgObj.AddComponent<Image>();
                barBg.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);

                RectTransform barBgRect = barBg.GetComponent<RectTransform>();
                barBgRect.anchorMin = new Vector2(0, 0);
                barBgRect.anchorMax = new Vector2(1, 0.5f);
                barBgRect.sizeDelta = Vector2.zero;
                barBgRect.anchoredPosition = Vector2.zero;

                GameObject barObj = new GameObject("FoodBar");
                barObj.transform.SetParent(barBgObj.transform, false);
                foodBar = barObj.AddComponent<Image>();
                foodBar.color = fullColor;
                foodBar.type = Image.Type.Filled;
                foodBar.fillMethod = Image.FillMethod.Horizontal;

                RectTransform barRect = foodBar.GetComponent<RectTransform>();
                barRect.anchorMin = Vector2.zero;
                barRect.anchorMax = Vector2.one;
                barRect.sizeDelta = Vector2.zero;
                barRect.anchoredPosition = Vector2.zero;
            }
        }

        private void Update()
        {
            if (trap == null)
                return;

            // 카메라를 향하도록 회전
            if (mainCamera != null)
            {
                Transform canvasTransform = foodText != null ? foodText.canvas.transform : transform;
                canvasTransform.LookAt(canvasTransform.position + mainCamera.transform.rotation * Vector3.forward,
                    mainCamera.transform.rotation * Vector3.up);
            }

            // 먹이 양 업데이트
            UpdateFoodDisplay(trap.CurrentFeedAmount, trap.MaxFeedAmount);
        }

        private void UpdateFoodDisplay(int current, int max)
        {
            if (foodText != null)
            {
                foodText.text = string.Format(foodFormat, current, max);
            }

            if (foodBar != null)
            {
                float fillAmount = max > 0 ? (float)current / max : 0f;
                foodBar.fillAmount = fillAmount;

                // 색상 보간
                foodBar.color = Color.Lerp(emptyColor, fullColor, fillAmount);
            }

            // 먹이가 없으면 UI 숨기기
            if (current <= 0 && foodText != null)
            {
                foodText.canvas.gameObject.SetActive(false);
            }
        }
    }
}



