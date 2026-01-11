using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PigeonGame.Gameplay;
using PigeonGame.Data;

namespace PigeonGame.UI
{
    /// <summary>
    /// 비둘기 머리 위에 표시되는 UI (긴장도 바, 먹는 중 표시)
    /// </summary>
    public class PigeonStatusUI : MonoBehaviour
    {
        [SerializeField] private Image alertBar;
        [SerializeField] private Image alertBarBackground;
        [SerializeField] private TextMeshProUGUI eatingText;
        [SerializeField] private Vector3 offset = new Vector3(0, 1f, 0);
        [SerializeField] private Color[] alertColors = new Color[]
        {
            Color.green,
            Color.yellow,
            Color.magenta,
            Color.red
        };

        private PigeonAI pigeonAI;
        private PigeonController pigeonController;
        private Camera mainCamera;
        private bool isEating = false;

        private void Awake()
        {
            if (alertBar == null || alertBarBackground == null)
            {
                CreateUI();
            }
        }

        private void Start()
        {
            pigeonAI = GetComponent<PigeonAI>();
            pigeonController = GetComponent<PigeonController>();

            if (pigeonAI == null)
            {
                Debug.LogWarning("PigeonStatusUI: PigeonAI를 찾을 수 없습니다!");
            }

            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = FindObjectOfType<Camera>();
            }

            if (alertBar == null || alertBarBackground == null)
            {
                CreateUI();
            }
        }

        private void CreateUI()
        {
            Canvas existingCanvas = GetComponentInChildren<Canvas>();
            if (existingCanvas != null && existingCanvas.name == "PigeonStatusCanvas")
            {
                Image[] images = existingCanvas.GetComponentsInChildren<Image>();
                foreach (var img in images)
                {
                    if (img.name == "AlertBarBackground")
                        alertBarBackground = img;
                    else if (img.name == "AlertBar")
                        alertBar = img;
                }
                eatingText = existingCanvas.GetComponentInChildren<TextMeshProUGUI>();
                if (alertBar != null && alertBarBackground != null && eatingText != null)
                {
                    return;
                }
            }

            GameObject canvasObj = new GameObject("PigeonStatusCanvas");
            canvasObj.transform.SetParent(transform);
            canvasObj.transform.localPosition = offset;
            canvasObj.transform.localRotation = Quaternion.identity;
            canvasObj.transform.localScale = Vector3.one * 0.02f;

            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;

            GraphicRaycaster raycaster = canvasObj.AddComponent<GraphicRaycaster>();

            GameObject panelObj = new GameObject("Panel");
            panelObj.transform.SetParent(canvasObj.transform, false);
            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.sizeDelta = new Vector2(200, 40);
            panelRect.anchoredPosition = Vector2.zero;

            Image panelImage = panelObj.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.7f);

            if (alertBarBackground == null)
            {
                GameObject barBgObj = new GameObject("AlertBarBackground");
                barBgObj.transform.SetParent(panelObj.transform, false);
                alertBarBackground = barBgObj.AddComponent<Image>();
                alertBarBackground.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);

                RectTransform barBgRect = alertBarBackground.GetComponent<RectTransform>();
                barBgRect.anchorMin = new Vector2(0.1f, 0.2f);
                barBgRect.anchorMax = new Vector2(0.9f, 0.8f);
                barBgRect.sizeDelta = Vector2.zero;
                barBgRect.anchoredPosition = Vector2.zero;
            }

            if (alertBar == null)
            {
                GameObject barObj = new GameObject("AlertBar");
                barObj.transform.SetParent(alertBarBackground.transform, false);
                alertBar = barObj.AddComponent<Image>();
                alertBar.color = alertColors[0];
                alertBar.type = Image.Type.Filled;
                alertBar.fillMethod = Image.FillMethod.Horizontal;

                RectTransform barRect = alertBar.GetComponent<RectTransform>();
                barRect.anchorMin = Vector2.zero;
                barRect.anchorMax = Vector2.one;
                barRect.sizeDelta = Vector2.zero;
                barRect.anchoredPosition = Vector2.zero;
            }

            if (eatingText == null)
            {
                GameObject eatingObj = new GameObject("EatingText");
                eatingObj.transform.SetParent(panelObj.transform, false);
                eatingText = eatingObj.AddComponent<TextMeshProUGUI>();
                eatingText.text = "먹는 중";
                eatingText.fontSize = 14;
                eatingText.color = Color.yellow;
                eatingText.alignment = TextAlignmentOptions.Center;
                eatingText.gameObject.SetActive(false);

                RectTransform eatingRect = eatingText.GetComponent<RectTransform>();
                eatingRect.anchorMin = new Vector2(0, 0.8f);
                eatingRect.anchorMax = new Vector2(1, 1);
                eatingRect.sizeDelta = Vector2.zero;
                eatingRect.anchoredPosition = Vector2.zero;
            }
        }

        private void Update()
        {
            if (pigeonAI == null)
                pigeonAI = GetComponent<PigeonAI>();
            if (pigeonController == null)
                pigeonController = GetComponent<PigeonController>();

            if (pigeonAI == null || pigeonController == null || pigeonController.Stats == null)
            {
                return;
            }

            if (mainCamera != null && alertBar != null && alertBar.canvas != null)
            {
                Transform canvasTransform = alertBar.canvas.transform;
                canvasTransform.LookAt(canvasTransform.position + mainCamera.transform.rotation * Vector3.forward,
                    mainCamera.transform.rotation * Vector3.up);
            }

            UpdateAlertBar();
            CheckEatingStatus();
        }

        private void UpdateAlertBar()
        {
            if (alertBar == null || pigeonAI == null || pigeonController == null || pigeonController.Stats == null)
                return;

            float alert = pigeonAI.Alert;
            float maxAlert = pigeonController.Stats.fleeThreshold;
            float fillAmount = maxAlert > 0 ? Mathf.Clamp01(alert / maxAlert) : 0f;

            alertBar.fillAmount = fillAmount;

            PigeonState state = pigeonAI.CurrentState;
            int stateIndex = (int)state;
            if (stateIndex >= 0 && stateIndex < alertColors.Length)
            {
                alertBar.color = alertColors[stateIndex];
            }
        }

        private void CheckEatingStatus()
        {
            if (pigeonAI == null || !pigeonAI.CanEat())
            {
                isEating = false;
                if (eatingText != null)
                {
                    eatingText.gameObject.SetActive(false);
                }
                return;
            }

            // 실제로 먹고 있는지 확인
            FoodTrap[] allTraps = FindObjectsOfType<FoodTrap>();
            isEating = false;

            foreach (var trap in allTraps)
            {
                if (trap == null || trap.IsDepleted)
                    continue;

                // 덫에서 실제로 먹고 있는지 확인
                if (trap.IsPigeonEating(pigeonAI))
                {
                    isEating = true;
                    break;
                }
            }

            if (eatingText != null)
            {
                eatingText.gameObject.SetActive(isEating);
            }
        }
    }
}


