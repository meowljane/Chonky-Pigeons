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
                enabled = false;
                return;
            }

            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = FindFirstObjectByType<Camera>();
            }

            // UI 요소가 프리팹에서 할당되지 않은 경우 경고
            if (foodText == null || foodBar == null)
            {
                Debug.LogWarning($"TrapFoodDisplay: UI 요소가 할당되지 않았습니다. 프리팹에서 foodText와 foodBar를 할당해주세요.", this);
                enabled = false;
                return;
            }

            // 진행 바 초기 설정
            if (foodBar != null)
            {
                foodBar.type = Image.Type.Filled;
                foodBar.fillMethod = Image.FillMethod.Horizontal;
                foodBar.fillOrigin = (int)Image.OriginHorizontal.Left;
                foodBar.fillAmount = 1f; // 초기값 100%
            }
        }

        private void Update()
        {
            if (trap == null)
                return;

            // 카메라를 향하도록 회전
            if (mainCamera != null && mainCamera.transform != null)
            {
                Transform canvasTransform = null;
                
                // Canvas 찾기
                if (foodText != null && foodText.canvas != null)
                {
                    canvasTransform = foodText.canvas.transform;
                }
                else
                {
                    // Canvas를 직접 찾기
                    Canvas canvas = GetComponentInChildren<Canvas>();
                    if (canvas != null)
                    {
                        canvasTransform = canvas.transform;
                    }
                    else
                    {
                        canvasTransform = transform;
                    }
                }

                if (canvasTransform != null)
                {
                    canvasTransform.LookAt(canvasTransform.position + mainCamera.transform.rotation * Vector3.forward,
                        mainCamera.transform.rotation * Vector3.up);
                }
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
                // current가 max일 때 1.0 (100%), current가 0일 때 0.0 (0%)
                float fillAmount = max > 0 ? Mathf.Clamp01((float)current / max) : 0f;
                foodBar.fillAmount = fillAmount;

                // 색상 보간 (100%일 때 fullColor, 0%일 때 emptyColor)
                foodBar.color = Color.Lerp(emptyColor, fullColor, fillAmount);
            }

            // 먹이가 없으면 UI 숨기기
            if (current <= 0 && foodText != null && foodText.canvas != null)
            {
                foodText.canvas.gameObject.SetActive(false);
            }
        }
    }
}



