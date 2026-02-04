using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PigeonGame.Gameplay;
using PigeonGame.Data;

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
        [SerializeField] private string foodFormat = "남은 {0}: {1}/{2}";
        [SerializeField] private Color fullColor = Color.green;
        [SerializeField] private Color emptyColor = Color.red;
        [SerializeField] private Vector3 offset = new Vector3(0, 1f, 0); // 덫 위에 표시할 오프셋

        private FoodTrap trap;
        private Camera mainCamera;
        private string trapName = "먹이"; // 기본값

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

            // UI 요소가 프리팹에서 할당되지 않은 경우 비활성화
            if (foodText == null || foodBar == null)
            {
                enabled = false;
                return;
            }

            // 덫 이름 가져오기
            UpdateTrapName();

            // 진행 바 초기 설정 (비둘기 Alert 바처럼)
            if (foodBar != null)
            {
                foodBar.type = Image.Type.Filled;
                foodBar.fillMethod = Image.FillMethod.Horizontal;
                foodBar.fillOrigin = (int)Image.OriginHorizontal.Left;
                
                // 초기값: 덫이 설치되면 먹이량이 max이므로 100% (꽉 참)
                if (trap != null && trap.MaxFeedAmount > 0)
                {
                    foodBar.fillAmount = Mathf.Clamp01((float)trap.CurrentFeedAmount / trap.MaxFeedAmount);
                }
                else
                {
                    foodBar.fillAmount = 1f; // 기본값 100%
                }
            }
        }

        private void UpdateTrapName()
        {
            if (trap == null)
                return;

            var registry = GameDataRegistry.Instance;
            if (registry?.Traps != null)
            {
                var trapData = registry.Traps.GetTrapById(trap.TrapId);
                if (trapData != null)
                {
                    trapName = trapData.name;
                }
            }
        }

        private void Update()
        {
            if (trap == null)
            {
                // trap이 없으면 다시 찾기 시도
                trap = GetComponentInParent<FoodTrap>();
                if (trap == null)
                {
                    trap = GetComponent<FoodTrap>();
                }
                if (trap == null)
                    return;
            }

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

            // 덫 이름 업데이트 (덫 타입이 변경될 수 있으므로)
            UpdateTrapName();

            // 먹이 양 업데이트 (비둘기 Alert 바처럼: 꽉 차다가 줄어들게)
            int current = trap.CurrentFeedAmount;
            int max = trap.MaxFeedAmount;
            
            // MaxFeedAmount가 0이면 초기화 대기
            if (max <= 0)
                return;
            
            UpdateFoodDisplay(current, max);
        }

        private void UpdateFoodDisplay(int current, int max)
        {
            if (foodText != null)
            {
                // 덫 이름을 포함한 형식: "남은 고급먹이: 0/20"
                foodText.text = string.Format(foodFormat, trapName, current, max);
            }

            if (foodBar != null)
            {
                // 비둘기 Alert 바처럼: current가 max일 때 1.0 (100%), current가 0일 때 0.0 (0%)
                // 먹이량이 줄어들면 바가 꽉 차다가 줄어들도록
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



