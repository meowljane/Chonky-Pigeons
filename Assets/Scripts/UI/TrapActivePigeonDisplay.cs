using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PigeonGame.Gameplay;
using PigeonGame.Data;

namespace PigeonGame.UI
{
    /// <summary>
    /// 덫 오브젝트에 직접 붙는 현재 먹고 있는 비둘기 표시 UI
    /// World Space Canvas를 사용하여 덫 위에 표시됨
    /// </summary>
    public class TrapActivePigeonDisplay : MonoBehaviour
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
        [SerializeField] private Vector3 offset = new Vector3(0, 1.5f, 0); // 덫 위에 표시할 오프셋

        private FoodTrap trap;
        private Camera mainCamera;
        private float updateInterval = 0.5f;
        private float updateTimer = 0f;

        private void Start()
        {
            trap = GetComponentInParent<FoodTrap>();
            if (trap == null)
            {
                trap = GetComponent<FoodTrap>();
            }

            if (trap == null)
            {
                Debug.LogError("TrapActivePigeonDisplay: FoodTrap을 찾을 수 없습니다!");
                enabled = false;
                return;
            }

            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = FindObjectOfType<Camera>();
            }

            // UI가 없으면 자동 생성
            if (pigeonText == null || alertIndicator == null)
            {
                CreateUI();
            }
        }

        private void CreateUI()
        {
            // World Space Canvas 생성
            GameObject canvasObj = new GameObject("ActivePigeonCanvas");
            canvasObj.transform.SetParent(transform);
            canvasObj.transform.localPosition = offset;
            canvasObj.transform.localRotation = Quaternion.identity;
            canvasObj.transform.localScale = Vector3.one * 0.01f;

            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 101;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;

            GraphicRaycaster raycaster = canvasObj.AddComponent<GraphicRaycaster>();

            // 배경 패널
            GameObject panelObj = new GameObject("Panel");
            panelObj.transform.SetParent(canvasObj.transform, false);
            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.sizeDelta = new Vector2(200, 40);
            panelRect.anchoredPosition = Vector2.zero;

            Image panelImage = panelObj.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.7f);

            // 텍스트 생성
            if (pigeonText == null)
            {
                GameObject textObj = new GameObject("PigeonText");
                textObj.transform.SetParent(panelObj.transform, false);
                pigeonText = textObj.AddComponent<TextMeshProUGUI>();
                pigeonText.text = "먹는 중: -";
                pigeonText.fontSize = 18;
                pigeonText.color = Color.white;
                pigeonText.alignment = TextAlignmentOptions.Center;

                RectTransform textRect = pigeonText.GetComponent<RectTransform>();
                textRect.anchorMin = new Vector2(0, 0);
                textRect.anchorMax = new Vector2(0.8f, 1);
                textRect.sizeDelta = Vector2.zero;
                textRect.anchoredPosition = Vector2.zero;
            }

            // Alert 인디케이터 생성
            if (alertIndicator == null)
            {
                GameObject indicatorObj = new GameObject("AlertIndicator");
                indicatorObj.transform.SetParent(panelObj.transform, false);
                alertIndicator = indicatorObj.AddComponent<Image>();
                alertIndicator.color = alertColors[0];

                RectTransform indicatorRect = alertIndicator.GetComponent<RectTransform>();
                indicatorRect.anchorMin = new Vector2(0.8f, 0.1f);
                indicatorRect.anchorMax = new Vector2(1f, 0.9f);
                indicatorRect.sizeDelta = Vector2.zero;
                indicatorRect.anchoredPosition = Vector2.zero;
            }
        }

        private void Update()
        {
            if (trap == null || trap.IsDepleted)
            {
                if (pigeonText != null)
                {
                    pigeonText.canvas.gameObject.SetActive(false);
                }
                return;
            }

            // 카메라를 향하도록 회전
            if (mainCamera != null && pigeonText != null)
            {
                Transform canvasTransform = pigeonText.canvas.transform;
                canvasTransform.LookAt(canvasTransform.position + mainCamera.transform.rotation * Vector3.forward,
                    mainCamera.transform.rotation * Vector3.up);
            }

            updateTimer += Time.deltaTime;
            if (updateTimer >= updateInterval)
            {
                updateTimer = 0f;
                UpdateDisplay();
            }
        }

        private void UpdateDisplay()
        {
            if (trap == null || trap.IsDepleted)
            {
                if (pigeonText != null)
                {
                    pigeonText.text = "";
                    pigeonText.canvas.gameObject.SetActive(false);
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
                trap.transform.position, 
                trap.DetectionRadius
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
                    if (pigeonText != null)
                    {
                        pigeonText.canvas.gameObject.SetActive(true);
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
                    pigeonText.canvas.gameObject.SetActive(false);
                }
                if (alertIndicator != null)
                {
                    alertIndicator.gameObject.SetActive(false);
                }
            }
        }
    }
}

