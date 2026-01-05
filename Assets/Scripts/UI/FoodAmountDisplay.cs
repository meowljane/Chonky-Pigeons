using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PigeonGame.Gameplay;

namespace PigeonGame.UI
{
    /// <summary>
    /// 덫의 남은 먹이 양 표시 UI
    /// 덫에 20의 먹이가 있고, 비둘기들이 먹으면 감소하며 0이 되면 포획됨
    /// </summary>
    public class FoodAmountDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI foodText;
        [SerializeField] private Image foodBar;
        [SerializeField] private string foodFormat = "남은 먹이: {0}/{1}";
        [SerializeField] private Color fullColor = Color.green;
        [SerializeField] private Color emptyColor = Color.red;

        private FoodTrap currentTrap;
        private int maxFoodAmount = 20;

        private void Update()
        {
            // 가장 가까운 덫 찾기
            FindNearestTrap();

            if (currentTrap != null)
            {
                UpdateFoodDisplay(currentTrap.CurrentFeedAmount, maxFoodAmount);
            }
            else
            {
                // 덫이 없으면 숨기거나 기본값 표시
                if (foodText != null)
                {
                    foodText.text = "먹이: -";
                }
                if (foodBar != null)
                {
                    foodBar.fillAmount = 0f;
                }
            }
        }

        private void FindNearestTrap()
        {
            FoodTrap nearestTrap = null;
            float nearestDistance = float.MaxValue;

            // 모든 덫 찾기
            FoodTrap[] allTraps = FindObjectsOfType<FoodTrap>();
            Vector3 playerPos = Vector3.zero;

            if (Gameplay.PlayerController.Instance != null)
            {
                playerPos = Gameplay.PlayerController.Instance.Position;
            }
            else
            {
                Camera mainCam = Camera.main;
                if (mainCam != null)
                {
                    playerPos = mainCam.transform.position;
                }
            }

            foreach (var trap in allTraps)
            {
                if (trap.IsDepleted)
                    continue;

                float distance = Vector3.Distance(playerPos, trap.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestTrap = trap;
                }
            }

            currentTrap = nearestTrap;
            if (nearestTrap != null)
            {
                maxFoodAmount = nearestTrap.MaxFeedAmount;
            }
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
        }
    }
}

