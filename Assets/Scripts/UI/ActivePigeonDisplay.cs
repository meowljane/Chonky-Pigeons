using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PigeonGame.Gameplay;
using PigeonGame.Data;

namespace PigeonGame.UI
{
    /// <summary>
    /// 현재 먹고 있는 비둘기 표시 UI
    /// </summary>
    public class ActivePigeonDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI pigeonText;
        [SerializeField] private Image alertIndicator;
        [SerializeField] private string format = "먹는 중: {0}";
        [SerializeField] private Color[] alertColors = new Color[]
        {
            Color.green,    // Normal
            Color.yellow,    // Cautious
            Color.magenta,   // BackOff
            Color.red        // Flee
        };

        private FoodTrap currentTrap;
        private float updateInterval = 0.5f;
        private float updateTimer = 0f;

        private void Update()
        {
            updateTimer += Time.deltaTime;
            if (updateTimer >= updateInterval)
            {
                updateTimer = 0f;
                FindNearestTrap();
                UpdateDisplay();
            }
        }

        private void FindNearestTrap()
        {
            FoodTrap nearestTrap = null;
            float nearestDistance = float.MaxValue;

            FoodTrap[] allTraps = FindObjectsOfType<FoodTrap>();
            Vector3 playerPos = Vector3.zero;

            if (PlayerController.Instance != null)
            {
                playerPos = PlayerController.Instance.Position;
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
        }

        private void UpdateDisplay()
        {
            if (currentTrap == null || currentTrap.IsDepleted)
            {
                if (pigeonText != null)
                {
                    pigeonText.text = "";
                }
                if (alertIndicator != null)
                {
                    alertIndicator.gameObject.SetActive(false);
                }
                return;
            }

            // 덫 주변에서 먹고 있는 비둘기 찾기
            PigeonAI activePigeon = null;
            float highestAlert = -1f;

            Collider2D[] colliders = Physics2D.OverlapCircleAll(
                currentTrap.transform.position, 
                currentTrap.DetectionRadius
            );

            foreach (var col in colliders)
            {
                PigeonAI pigeon = col.GetComponent<PigeonAI>();
                if (pigeon != null && pigeon.CanEat())
                {
                    float alert = pigeon.Alert;
                    if (alert > highestAlert)
                    {
                        highestAlert = alert;
                        activePigeon = pigeon;
                    }
                }
            }

            if (activePigeon != null)
            {
                var controller = activePigeon.GetComponent<PigeonController>();
                if (controller != null && controller.Stats != null)
                {
                    // 종 이름 표시
                    if (pigeonText != null)
                    {
                        var registry = GameDataRegistry.Instance;
                        if (registry != null && registry.SpeciesSet != null)
                        {
                            var species = registry.SpeciesSet.GetSpeciesById(controller.Stats.speciesId);
                            string speciesName = species != null ? species.name : controller.Stats.speciesId;
                            pigeonText.text = string.Format(format, speciesName);
                        }
                        else
                        {
                            pigeonText.text = string.Format(format, controller.Stats.speciesId);
                        }
                    }

                    // Alert 상태 표시
                    if (alertIndicator != null)
                    {
                        alertIndicator.gameObject.SetActive(true);
                        PigeonState state = activePigeon.CurrentState;
                        int stateIndex = (int)state;
                        if (stateIndex >= 0 && stateIndex < alertColors.Length)
                        {
                            alertIndicator.color = alertColors[stateIndex];
                        }
                    }
                }
            }
            else
            {
                if (pigeonText != null)
                {
                    pigeonText.text = "";
                }
                if (alertIndicator != null)
                {
                    alertIndicator.gameObject.SetActive(false);
                }
            }
        }
    }
}

