using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using PigeonGame.UI;

namespace PigeonGame.Editor
{
    public static class EncyclopediaUIHelper
    {
        [MenuItem("Tools/Create Encyclopedia UI", false, 1)]
        public static void CreateEncyclopediaUI()
        {
            // Canvas 찾기 또는 생성
            Canvas canvas = FindOrCreateCanvas();

            // EncyclopediaUI 오브젝트 생성
            GameObject uiObj = new GameObject("EncyclopediaUI");
            uiObj.transform.SetParent(canvas.transform, false);
            
            EncyclopediaUI encyclopediaUI = uiObj.AddComponent<EncyclopediaUI>();

            // 메인 패널 생성
            GameObject mainPanel = new GameObject("EncyclopediaPanel");
            mainPanel.transform.SetParent(uiObj.transform, false);
            mainPanel.SetActive(false);

            RectTransform mainPanelRect = mainPanel.AddComponent<RectTransform>();
            mainPanelRect.anchorMin = Vector2.zero;
            mainPanelRect.anchorMax = Vector2.one;
            mainPanelRect.sizeDelta = Vector2.zero;
            mainPanelRect.anchoredPosition = Vector2.zero;

            Image mainPanelImage = mainPanel.AddComponent<Image>();
            mainPanelImage.color = new Color(0, 0, 0, 0.8f);

            // 닫기 버튼
            GameObject closeButtonObj = CreateButton("CloseButton", "닫기", mainPanel.transform);
            RectTransform closeButtonRect = closeButtonObj.GetComponent<RectTransform>();
            closeButtonRect.anchorMin = new Vector2(1, 1);
            closeButtonRect.anchorMax = new Vector2(1, 1);
            closeButtonRect.pivot = new Vector2(1, 1);
            closeButtonRect.anchoredPosition = new Vector2(-20, -20);
            closeButtonRect.sizeDelta = new Vector2(100, 40);

            Button closeButton = closeButtonObj.GetComponent<Button>();

            // Species 목록 패널
            GameObject speciesListPanel = new GameObject("SpeciesListPanel");
            speciesListPanel.transform.SetParent(mainPanel.transform, false);

            RectTransform speciesListRect = speciesListPanel.AddComponent<RectTransform>();
            speciesListRect.anchorMin = Vector2.zero;
            speciesListRect.anchorMax = Vector2.one;
            speciesListRect.sizeDelta = Vector2.zero;
            speciesListRect.anchoredPosition = Vector2.zero;
            speciesListRect.offsetMin = new Vector2(20, 60);
            speciesListRect.offsetMax = new Vector2(-20, -20);

            // Species 그리드
            GameObject speciesGrid = new GameObject("SpeciesGrid");
            speciesGrid.transform.SetParent(speciesListPanel.transform, false);
            RectTransform speciesGridRect = speciesGrid.AddComponent<RectTransform>();
            speciesGridRect.anchorMin = Vector2.zero;
            speciesGridRect.anchorMax = Vector2.one;
            speciesGridRect.sizeDelta = Vector2.zero;
            speciesGridRect.anchoredPosition = Vector2.zero;
            speciesGridRect.offsetMin = new Vector2(0, 0);
            speciesGridRect.offsetMax = new Vector2(0, 0);

            GridLayoutGroup speciesGridLayout = speciesGrid.AddComponent<GridLayoutGroup>();
            speciesGridLayout.cellSize = new Vector2(100, 120);
            speciesGridLayout.spacing = new Vector2(10, 10);
            speciesGridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            speciesGridLayout.constraintCount = 3;

            // Species 상세 패널
            GameObject speciesDetailPanel = new GameObject("SpeciesDetailPanel");
            speciesDetailPanel.transform.SetParent(mainPanel.transform, false);
            speciesDetailPanel.SetActive(false);

            RectTransform detailPanelRect = speciesDetailPanel.AddComponent<RectTransform>();
            detailPanelRect.anchorMin = Vector2.zero;
            detailPanelRect.anchorMax = Vector2.one;
            detailPanelRect.sizeDelta = Vector2.zero;
            detailPanelRect.anchoredPosition = Vector2.zero;
            detailPanelRect.offsetMin = new Vector2(20, 60);
            detailPanelRect.offsetMax = new Vector2(-20, -20);

            // 뒤로가기 버튼
            GameObject backButtonObj = CreateButton("BackButton", "뒤로", speciesDetailPanel.transform);
            RectTransform backButtonRect = backButtonObj.GetComponent<RectTransform>();
            backButtonRect.anchorMin = new Vector2(0, 1);
            backButtonRect.anchorMax = new Vector2(0, 1);
            backButtonRect.pivot = new Vector2(0, 1);
            backButtonRect.anchoredPosition = new Vector2(10, -10);
            backButtonRect.sizeDelta = new Vector2(100, 40);
            Button backButton = backButtonObj.GetComponent<Button>();

            // Species 정보 영역 (상단)
            GameObject speciesInfo = new GameObject("SpeciesInfo");
            speciesInfo.transform.SetParent(speciesDetailPanel.transform, false);
            RectTransform speciesInfoRect = speciesInfo.AddComponent<RectTransform>();
            speciesInfoRect.anchorMin = new Vector2(0, 0.75f);
            speciesInfoRect.anchorMax = new Vector2(1, 1);
            speciesInfoRect.sizeDelta = Vector2.zero;
            speciesInfoRect.anchoredPosition = Vector2.zero;
            speciesInfoRect.offsetMin = new Vector2(10, 0);
            speciesInfoRect.offsetMax = new Vector2(-10, -50);

            Image speciesInfoBg = speciesInfo.AddComponent<Image>();
            speciesInfoBg.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);

            // Species 아이콘
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(speciesInfo.transform, false);
            Image iconImage = iconObj.AddComponent<Image>();
            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0, 0);
            iconRect.anchorMax = new Vector2(0, 1);
            iconRect.pivot = new Vector2(0, 0.5f);
            iconRect.anchoredPosition = new Vector2(10, 0);
            iconRect.sizeDelta = new Vector2(80, 80);

            // Species 이름
            GameObject nameObj = new GameObject("NameText");
            nameObj.transform.SetParent(speciesInfo.transform, false);
            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = "Species Name";
            nameText.fontSize = 28;
            nameText.color = Color.white;
            nameText.fontStyle = FontStyles.Bold;
            RectTransform nameRect = nameObj.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.15f, 0.5f);
            nameRect.anchorMax = new Vector2(1, 1);
            nameRect.sizeDelta = Vector2.zero;
            nameRect.anchoredPosition = Vector2.zero;
            nameRect.offsetMin = new Vector2(10, 0);
            nameRect.offsetMax = new Vector2(-10, -5);

            // Species 무게
            GameObject weightObj = new GameObject("WeightText");
            weightObj.transform.SetParent(speciesInfo.transform, false);
            TextMeshProUGUI weightText = weightObj.AddComponent<TextMeshProUGUI>();
            weightText.text = "무게 범위: -";
            weightText.fontSize = 20;
            weightText.color = Color.yellow;
            RectTransform weightRect = weightObj.AddComponent<RectTransform>();
            weightRect.anchorMin = new Vector2(0.15f, 0);
            weightRect.anchorMax = new Vector2(1, 0.5f);
            weightRect.sizeDelta = Vector2.zero;
            weightRect.anchoredPosition = Vector2.zero;
            weightRect.offsetMin = new Vector2(10, 5);
            weightRect.offsetMax = new Vector2(-10, 0);

            // Face 제목
            GameObject faceTitleObj = new GameObject("FaceTitle");
            faceTitleObj.transform.SetParent(speciesDetailPanel.transform, false);
            TextMeshProUGUI faceTitle = faceTitleObj.AddComponent<TextMeshProUGUI>();
            faceTitle.text = "얼굴 종류";
            faceTitle.fontSize = 22;
            faceTitle.color = Color.white;
            faceTitle.fontStyle = FontStyles.Bold;
            RectTransform faceTitleRect = faceTitleObj.AddComponent<RectTransform>();
            faceTitleRect.anchorMin = new Vector2(0, 0.65f);
            faceTitleRect.anchorMax = new Vector2(1, 0.75f);
            faceTitleRect.sizeDelta = Vector2.zero;
            faceTitleRect.anchoredPosition = Vector2.zero;
            faceTitleRect.offsetMin = new Vector2(10, 0);
            faceTitleRect.offsetMax = new Vector2(-10, 0);

            // Face 그리드
            GameObject faceGrid = new GameObject("FaceGrid");
            faceGrid.transform.SetParent(speciesDetailPanel.transform, false);
            RectTransform faceGridRect = faceGrid.AddComponent<RectTransform>();
            faceGridRect.anchorMin = Vector2.zero;
            faceGridRect.anchorMax = new Vector2(1, 0.65f);
            faceGridRect.sizeDelta = Vector2.zero;
            faceGridRect.anchoredPosition = Vector2.zero;
            faceGridRect.offsetMin = new Vector2(10, 10);
            faceGridRect.offsetMax = new Vector2(-10, -10);

            GridLayoutGroup faceGridLayout = faceGrid.AddComponent<GridLayoutGroup>();
            faceGridLayout.cellSize = new Vector2(80, 100);
            faceGridLayout.spacing = new Vector2(10, 10);
            faceGridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            faceGridLayout.constraintCount = 6;

            // 도감 버튼 (캔버스에 추가)
            GameObject encyclopediaButtonObj = CreateButton("EncyclopediaButton", "도감", canvas.transform);
            RectTransform encButtonRect = encyclopediaButtonObj.GetComponent<RectTransform>();
            encButtonRect.anchorMin = new Vector2(0, 1);
            encButtonRect.anchorMax = new Vector2(0, 1);
            encButtonRect.pivot = new Vector2(0, 1);
            encButtonRect.anchoredPosition = new Vector2(20, -20);
            encButtonRect.sizeDelta = new Vector2(100, 40);
            Button encyclopediaButton = encyclopediaButtonObj.GetComponent<Button>();

            // EncyclopediaUI 컴포넌트에 참조 할당
            SerializedObject serializedUI = new SerializedObject(encyclopediaUI);
            serializedUI.FindProperty("encyclopediaPanel").objectReferenceValue = mainPanel;
            serializedUI.FindProperty("encyclopediaButton").objectReferenceValue = encyclopediaButton;
            serializedUI.FindProperty("closeButton").objectReferenceValue = closeButton;
            serializedUI.FindProperty("speciesListPanel").objectReferenceValue = speciesListPanel;
            serializedUI.FindProperty("speciesGridContainer").objectReferenceValue = speciesGridRect;
            serializedUI.FindProperty("speciesDetailPanel").objectReferenceValue = speciesDetailPanel;
            serializedUI.FindProperty("speciesNameText").objectReferenceValue = nameText;
            serializedUI.FindProperty("speciesIconImage").objectReferenceValue = iconImage;
            serializedUI.FindProperty("speciesWeightText").objectReferenceValue = weightText;
            serializedUI.FindProperty("faceGridContainer").objectReferenceValue = faceGridRect;
            serializedUI.FindProperty("backButton").objectReferenceValue = backButton;
            serializedUI.ApplyModifiedProperties();

            Selection.activeGameObject = uiObj;
            
            Debug.Log("Encyclopedia UI가 생성되었습니다!");
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
                GameObject canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();

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

