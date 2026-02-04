using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PigeonGame.Gameplay;
using PigeonGame.Data;

namespace PigeonGame.UI
{
    public class TrapFoodDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI foodText;
        [SerializeField] private Image foodBar;
        [SerializeField] private string foodFormat = "남은 {0}: {1}/{2}";
        [SerializeField] private Color fullColor = Color.green;
        [SerializeField] private Color emptyColor = Color.red;
        [SerializeField] private Vector3 offset = new Vector3(0, 1f, 0); 

        private FoodTrap trap;
        private Camera mainCamera;
        private string trapName = "먹이"; 

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

            if (foodText == null || foodBar == null)
            {
                enabled = false;
                return;
            }

            UpdateTrapName();

            if (foodBar != null)
            {
                foodBar.type = Image.Type.Filled;
                foodBar.fillMethod = Image.FillMethod.Horizontal;
                foodBar.fillOrigin = (int)Image.OriginHorizontal.Left;

                if (trap != null && trap.MaxFeedAmount > 0)
                {
                    foodBar.fillAmount = Mathf.Clamp01((float)trap.CurrentFeedAmount / trap.MaxFeedAmount);
                }
                else
                {
                    foodBar.fillAmount = 1f; 
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
                trap = GetComponentInParent<FoodTrap>();
                if (trap == null)
                {
                    trap = GetComponent<FoodTrap>();
                }
                if (trap == null)
                    return;
            }

            if (mainCamera != null && mainCamera.transform != null)
            {
                Transform canvasTransform = null;

                if (foodText != null && foodText.canvas != null)
                {
                    canvasTransform = foodText.canvas.transform;
                }
                else
                {
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

            UpdateTrapName();

            int current = trap.CurrentFeedAmount;
            int max = trap.MaxFeedAmount;

            if (max <= 0)
                return;

            UpdateFoodDisplay(current, max);
        }

        private void UpdateFoodDisplay(int current, int max)
        {
            if (foodText != null)
            {
                foodText.text = string.Format(foodFormat, trapName, current, max);
            }

            if (foodBar != null)
            {
                float fillAmount = max > 0 ? Mathf.Clamp01((float)current / max) : 0f;
                foodBar.fillAmount = fillAmount;

                foodBar.color = Color.Lerp(emptyColor, fullColor, fillAmount);
            }

            if (current <= 0 && foodText != null && foodText.canvas != null)
            {
                foodText.canvas.gameObject.SetActive(false);
            }
        }
    }
}

