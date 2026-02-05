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
        [SerializeField] private FoodTrap trap;
        [SerializeField] private string foodFormat = "남은 {0}: {1}/{2}";
        [SerializeField] private Color fullColor = Color.green;
        [SerializeField] private Color emptyColor = Color.red;
        private Camera mainCamera;
        private string trapName = "먹이";
        private Transform canvasTransform;
        private bool trapNameUpdated = false;

        private void Start()
        {
            mainCamera = Camera.main ?? FindFirstObjectByType<Camera>();

            UpdateTrapName();
            trapNameUpdated = true;

            foodBar.type = Image.Type.Filled;
            foodBar.fillMethod = Image.FillMethod.Horizontal;
            foodBar.fillOrigin = (int)Image.OriginHorizontal.Left;

            if (trap.MaxFeedAmount > 0)
                foodBar.fillAmount = Mathf.Clamp01((float)trap.CurrentFeedAmount / trap.MaxFeedAmount);
            else
                foodBar.fillAmount = 1f;

            canvasTransform = foodText.canvas?.transform ?? GetComponentInChildren<Canvas>()?.transform ?? transform;
        }

        private void UpdateTrapName()
        {
            var trapData = GameDataRegistry.Instance?.Traps?.GetTrapById(trap.TrapId);
            if (trapData != null)
                trapName = trapData.name;
        }

        private void Update()
        {
            if (mainCamera != null && canvasTransform != null)
            {
                canvasTransform.LookAt(canvasTransform.position + mainCamera.transform.rotation * Vector3.forward,
                    mainCamera.transform.rotation * Vector3.up);
            }

            if (!trapNameUpdated)
            {
                UpdateTrapName();
                trapNameUpdated = true;
            }

            int current = trap.CurrentFeedAmount;
            int max = trap.MaxFeedAmount;

            if (max <= 0)
                return;

            UpdateFoodDisplay(current, max);
        }

        private void UpdateFoodDisplay(int current, int max)
        {
            foodText.text = string.Format(foodFormat, trapName, current, max);

            float fillAmount = Mathf.Clamp01((float)current / max);
            foodBar.fillAmount = fillAmount;
            foodBar.color = Color.Lerp(emptyColor, fullColor, fillAmount);

            if (current <= 0)
                foodText.canvas?.gameObject.SetActive(false);
        }
    }
}

