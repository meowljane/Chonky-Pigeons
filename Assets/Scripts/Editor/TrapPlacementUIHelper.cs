using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using PigeonGame.UI;

namespace PigeonGame.Editor
{
    public static class TrapPlacementUIHelper
    {
        [MenuItem("Tools/Create Trap Placement UI", false, 1)]
        public static void CreateTrapPlacementUI()
        {
            // Canvas 찾기 또는 생성
            Canvas canvas = FindOrCreateCanvas();

            // TrapPlacementUI 오브젝트 생성
            GameObject uiObj = new GameObject("TrapPlacementUI");
            uiObj.transform.SetParent(canvas.transform, false);
            
            TrapPlacementUI trapPlacementUI = uiObj.AddComponent<TrapPlacementUI>();

            // 우측 하단 버튼 컨테이너 생성
            GameObject buttonContainer = new GameObject("ButtonContainer");
            buttonContainer.transform.SetParent(uiObj.transform, false);
            RectTransform buttonContainerRect = buttonContainer.AddComponent<RectTransform>();
            buttonContainerRect.anchorMin = new Vector2(1, 0);
            buttonContainerRect.anchorMax = new Vector2(1, 0);
            buttonContainerRect.pivot = new Vector2(1, 0);
            buttonContainerRect.anchoredPosition = new Vector2(-20, 20);
            buttonContainerRect.sizeDelta = new Vector2(200, 120);

            // Vertical Layout Group 추가
            VerticalLayoutGroup buttonLayout = buttonContainer.AddComponent<VerticalLayoutGroup>();
            buttonLayout.spacing = 10;
            buttonLayout.childControlHeight = false;
            buttonLayout.childControlWidth = false;
            buttonLayout.childForceExpandHeight = false;
            buttonLayout.childForceExpandWidth = false;

            // 덫 설치 버튼 생성
            GameObject trapButtonObj = CreateButton("TrapPlacementButton", "덫 설치", buttonContainer.transform);
            Button trapButton = trapButtonObj.GetComponent<Button>();
            RectTransform trapButtonRect = trapButtonObj.GetComponent<RectTransform>();
            trapButtonRect.sizeDelta = new Vector2(150, 50);

            // 상호작용 버튼 생성
            GameObject interactionButtonObj = CreateButton("InteractionButton", "상호작용", buttonContainer.transform);
            Button interactionButton = interactionButtonObj.GetComponent<Button>();
            RectTransform interactionButtonRect = interactionButtonObj.GetComponent<RectTransform>();
            interactionButtonRect.sizeDelta = new Vector2(150, 50);

            // 덫 선택 패널 생성
            GameObject panelObj = new GameObject("TrapSelectionPanel");
            panelObj.transform.SetParent(uiObj.transform, false);
            panelObj.SetActive(false); // 기본 비활성화

            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(1, 0);
            panelRect.anchorMax = new Vector2(1, 0);
            panelRect.pivot = new Vector2(1, 0);
            panelRect.anchoredPosition = new Vector2(-20, 150);
            panelRect.sizeDelta = new Vector2(350, 250);

            Image panelImage = panelObj.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.8f);

            // 패널 제목
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(panelObj.transform, false);
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "덫 선택";
            titleText.fontSize = 24;
            titleText.color = Color.white;
            titleText.alignment = TextAlignmentOptions.Center;

            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 1);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.pivot = new Vector2(0.5f, 1);
            titleRect.anchoredPosition = new Vector2(0, -10);
            titleRect.sizeDelta = new Vector2(0, 40);

            // 그리드 컨테이너 생성
            GameObject gridObj = new GameObject("TrapGridContainer");
            gridObj.transform.SetParent(panelObj.transform, false);
            RectTransform gridRect = gridObj.AddComponent<RectTransform>();
            gridRect.anchorMin = new Vector2(0, 0);
            gridRect.anchorMax = new Vector2(1, 1);
            gridRect.sizeDelta = Vector2.zero;
            gridRect.anchoredPosition = Vector2.zero;
            gridRect.offsetMin = new Vector2(10, 50);
            gridRect.offsetMax = new Vector2(-10, -10);

            GridLayoutGroup gridLayout = gridObj.AddComponent<GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(100, 100);
            gridLayout.spacing = new Vector2(10, 10);
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 3;
            gridLayout.childAlignment = TextAnchor.MiddleCenter;

            // 닫기 버튼
            GameObject closeButtonObj = CreateButton("CloseButton", "닫기", panelObj.transform);
            RectTransform closeButtonRect = closeButtonObj.GetComponent<RectTransform>();
            closeButtonRect.anchorMin = new Vector2(0.5f, 0);
            closeButtonRect.anchorMax = new Vector2(0.5f, 0);
            closeButtonRect.pivot = new Vector2(0.5f, 0);
            closeButtonRect.anchoredPosition = new Vector2(0, 10);
            closeButtonRect.sizeDelta = new Vector2(100, 30);

            Button closeButton = closeButtonObj.GetComponent<Button>();
            closeButton.onClick.AddListener(() => panelObj.SetActive(false));

            // TrapPlacementUI 컴포넌트에 참조 할당
            SerializedObject serializedUI = new SerializedObject(trapPlacementUI);
            serializedUI.FindProperty("trapPlacementButton").objectReferenceValue = trapButton;
            serializedUI.FindProperty("interactionButton").objectReferenceValue = interactionButton;
            serializedUI.FindProperty("trapSelectionPanel").objectReferenceValue = panelObj;
            serializedUI.FindProperty("trapGridContainer").objectReferenceValue = gridRect;
            serializedUI.FindProperty("closeButton").objectReferenceValue = closeButton;
            serializedUI.ApplyModifiedProperties();

            // 선택
            Selection.activeGameObject = uiObj;
            
            Debug.Log("Trap Placement UI가 생성되었습니다! 우측 하단에 배치되어 있습니다.");
        }

        private static GameObject CreateButton(string name, string text, Transform parent)
        {
            GameObject buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent, false);

            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.6f, 0.8f, 1f);

            Button button = buttonObj.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.2f, 0.6f, 0.8f, 1f);
            colors.highlightedColor = new Color(0.3f, 0.7f, 0.9f, 1f);
            colors.pressedColor = new Color(0.1f, 0.5f, 0.7f, 1f);
            colors.selectedColor = new Color(0.2f, 0.6f, 0.8f, 1f);
            button.colors = colors;

            RectTransform rect = buttonObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(100, 30);

            // 텍스트 추가
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            TextMeshProUGUI textComponent = textObj.AddComponent<TextMeshProUGUI>();
            textComponent.text = text;
            textComponent.fontSize = 18;
            textComponent.color = Color.white;
            textComponent.alignment = TextAlignmentOptions.Center;

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;

            return buttonObj;
        }

        private static Canvas FindOrCreateCanvas()
        {
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                // Canvas 생성
                GameObject canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();

                // EventSystem 생성
                if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
                {
                    GameObject eventSystemObj = new GameObject("EventSystem");
                    eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                    eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                }
            }
            return canvas;
        }
    }
}

