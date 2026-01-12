using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

namespace PigeonGame.Editor
{
    /// <summary>
    /// EncyclopediaUI의 디테일 패널 구조를 자동으로 생성하는 에디터 스크립트
    /// </summary>
    public class EncyclopediaDetailPanelCreator : EditorWindow
    {
        private GameObject targetPanel;
        private Vector2 scrollPosition;

        [MenuItem("Tools/Pigeon Game/Create Encyclopedia Detail Panel")]
        public static void ShowWindow()
        {
            GetWindow<EncyclopediaDetailPanelCreator>("Encyclopedia Detail Panel Creator");
        }

        private void OnGUI()
        {
            GUILayout.Label("도감 디테일 패널 생성", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox("디테일 패널로 사용할 GameObject를 선택하거나 드래그하세요.", MessageType.Info);
            EditorGUILayout.Space();

            targetPanel = (GameObject)EditorGUILayout.ObjectField(
                "Target Panel (선택사항)", 
                targetPanel, 
                typeof(GameObject), 
                true
            );

            EditorGUILayout.Space();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            if (GUILayout.Button("디테일 패널 구조 생성", GUILayout.Height(30)))
            {
                CreateDetailPanelStructure();
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("기존 패널에 컴포넌트만 추가", GUILayout.Height(30)))
            {
                AddComponentsToExistingPanel();
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "디테일 패널 구조:\n" +
                "- Species Name Text\n" +
                "- Species Icon Image\n" +
                "- Species Weight Text\n" +
                "- Face Grid Container\n" +
                "- Back Button",
                MessageType.Info
            );
        }

        private void CreateDetailPanelStructure()
        {
            GameObject panel = targetPanel;
            
            // 패널이 없으면 새로 생성
            if (panel == null)
            {
                panel = new GameObject("SpeciesDetailPanel");
                RectTransform rect = panel.AddComponent<RectTransform>();
                rect.sizeDelta = new Vector2(800, 600);
                
                Image bg = panel.AddComponent<Image>();
                bg.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
            }

            // 기존 자식 제거 확인
            if (panel.transform.childCount > 0)
            {
                if (!EditorUtility.DisplayDialog(
                    "경고", 
                    "패널에 이미 자식 오브젝트가 있습니다. 모두 제거하고 새로 생성하시겠습니까?",
                    "예", 
                    "아니오"))
                {
                    return;
                }

                for (int i = panel.transform.childCount - 1; i >= 0; i--)
                {
                    DestroyImmediate(panel.transform.GetChild(i).gameObject);
                }
            }

            Undo.RegisterCreatedObjectUndo(panel, "Create Encyclopedia Detail Panel");

            // Species Name Text
            GameObject nameObj = CreateTextElement(panel.transform, "SpeciesNameText", "Species Name");
            RectTransform nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.1f, 0.85f);
            nameRect.anchorMax = new Vector2(0.9f, 0.95f);
            TextMeshProUGUI nameText = nameObj.GetComponent<TextMeshProUGUI>();
            nameText.fontSize = 32;
            nameText.fontStyle = FontStyles.Bold;
            nameText.alignment = TextAlignmentOptions.Center;

            // Species Icon Image
            GameObject iconObj = new GameObject("SpeciesIconImage");
            iconObj.transform.SetParent(panel.transform, false);
            Image iconImage = iconObj.AddComponent<Image>();
            iconImage.preserveAspect = true;
            RectTransform iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.1f, 0.6f);
            iconRect.anchorMax = new Vector2(0.4f, 0.85f);
            iconRect.sizeDelta = Vector2.zero;
            iconRect.anchoredPosition = Vector2.zero;

            // Species Weight Text
            GameObject weightObj = CreateTextElement(panel.transform, "SpeciesWeightText", "무게 범위: 미발견");
            RectTransform weightRect = weightObj.GetComponent<RectTransform>();
            weightRect.anchorMin = new Vector2(0.1f, 0.5f);
            weightRect.anchorMax = new Vector2(0.9f, 0.6f);
            TextMeshProUGUI weightText = weightObj.GetComponent<TextMeshProUGUI>();
            weightText.fontSize = 20;
            weightText.alignment = TextAlignmentOptions.Center;

            // Face Grid Container
            GameObject gridObj = new GameObject("FaceGridContainer");
            gridObj.transform.SetParent(panel.transform, false);
            RectTransform gridRect = gridObj.AddComponent<RectTransform>();
            gridRect.anchorMin = new Vector2(0.1f, 0.1f);
            gridRect.anchorMax = new Vector2(0.9f, 0.45f);
            gridRect.sizeDelta = Vector2.zero;
            gridRect.anchoredPosition = Vector2.zero;

            // Grid Layout Group 추가
            GridLayoutGroup gridLayout = gridObj.AddComponent<GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(80, 100);
            gridLayout.spacing = new Vector2(10, 10);
            gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
            gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
            gridLayout.childAlignment = TextAnchor.UpperLeft;
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 4;

            // Content Size Fitter 추가 (선택사항)
            ContentSizeFitter fitter = gridObj.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // ScrollRect 추가 (선택사항)
            ScrollRect scrollRect = gridObj.AddComponent<ScrollRect>();
            scrollRect.horizontal = true;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;

            // ScrollRect용 Viewport 생성
            GameObject viewportObj = new GameObject("Viewport");
            viewportObj.transform.SetParent(gridObj.transform, false);
            RectTransform viewportRect = viewportObj.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.sizeDelta = Vector2.zero;
            viewportRect.anchoredPosition = Vector2.zero;
            Image viewportImage = viewportObj.AddComponent<Image>();
            viewportImage.color = new Color(0, 0, 0, 0);
            Mask mask = viewportObj.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            // Content를 Viewport의 자식으로 이동
            GameObject contentObj = new GameObject("Content");
            contentObj.transform.SetParent(viewportObj.transform, false);
            RectTransform contentRect = contentObj.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(0, 1);
            contentRect.pivot = new Vector2(0, 1);
            contentRect.sizeDelta = new Vector2(0, 0);
            contentRect.anchoredPosition = Vector2.zero;

            // GridLayoutGroup을 Content로 이동
            GridLayoutGroup contentGrid = contentObj.AddComponent<GridLayoutGroup>();
            contentGrid.cellSize = gridLayout.cellSize;
            contentGrid.spacing = gridLayout.spacing;
            contentGrid.startCorner = gridLayout.startCorner;
            contentGrid.startAxis = gridLayout.startAxis;
            contentGrid.childAlignment = gridLayout.childAlignment;
            contentGrid.constraint = gridLayout.constraint;
            contentGrid.constraintCount = gridLayout.constraintCount;

            DestroyImmediate(gridLayout);
            DestroyImmediate(fitter);

            scrollRect.content = contentRect;
            scrollRect.viewport = viewportRect;

            // Back Button
            GameObject backBtnObj = new GameObject("BackButton");
            backBtnObj.transform.SetParent(panel.transform, false);
            Image btnBg = backBtnObj.AddComponent<Image>();
            btnBg.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            Button backButton = backBtnObj.AddComponent<Button>();
            
            RectTransform btnRect = backBtnObj.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.05f, 0.05f);
            btnRect.anchorMax = new Vector2(0.2f, 0.15f);
            btnRect.sizeDelta = Vector2.zero;
            btnRect.anchoredPosition = Vector2.zero;

            // Back Button Text
            GameObject btnTextObj = CreateTextElement(backBtnObj.transform, "Text", "뒤로");
            RectTransform btnTextRect = btnTextObj.GetComponent<RectTransform>();
            btnTextRect.anchorMin = Vector2.zero;
            btnTextRect.anchorMax = Vector2.one;
            btnTextRect.sizeDelta = Vector2.zero;
            btnTextRect.anchoredPosition = Vector2.zero;
            TextMeshProUGUI btnText = btnTextObj.GetComponent<TextMeshProUGUI>();
            btnText.fontSize = 18;
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.color = Color.white;

            // Hierarchy에서 선택
            Selection.activeGameObject = panel;
            EditorGUIUtility.PingObject(panel);

            EditorUtility.DisplayDialog("완료", "디테일 패널 구조가 생성되었습니다!\n\nEncyclopediaUI 컴포넌트에 다음을 연결하세요:\n- SpeciesNameText\n- SpeciesIconImage\n- SpeciesWeightText\n- FaceGridContainer (Content 오브젝트)\n- BackButton", "확인");
        }

        private void AddComponentsToExistingPanel()
        {
            if (targetPanel == null)
            {
                EditorUtility.DisplayDialog("오류", "대상 패널을 선택해주세요.", "확인");
                return;
            }

            // 필요한 컴포넌트가 있는지 확인하고 추가
            bool hasChanges = false;

            // TextMeshProUGUI 컴포넌트 찾기 및 추가
            TextMeshProUGUI[] texts = targetPanel.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length == 0)
            {
                GameObject nameObj = CreateTextElement(targetPanel.transform, "SpeciesNameText", "Species Name");
                hasChanges = true;
            }

            // Image 컴포넌트 찾기
            Image[] images = targetPanel.GetComponentsInChildren<Image>();
            bool hasIcon = false;
            foreach (var img in images)
            {
                if (img.name.Contains("Icon") || img.name.Contains("icon"))
                {
                    hasIcon = true;
                    break;
                }
            }

            if (!hasIcon)
            {
                GameObject iconObj = new GameObject("SpeciesIconImage");
                iconObj.transform.SetParent(targetPanel.transform, false);
                Image iconImage = iconObj.AddComponent<Image>();
                iconImage.preserveAspect = true;
                RectTransform iconRect = iconObj.GetComponent<RectTransform>();
                iconRect.anchorMin = new Vector2(0.1f, 0.6f);
                iconRect.anchorMax = new Vector2(0.4f, 0.85f);
                iconRect.sizeDelta = Vector2.zero;
                iconRect.anchoredPosition = Vector2.zero;
                hasChanges = true;
            }

            // Grid Container 찾기
            Transform gridContainer = targetPanel.transform.Find("FaceGridContainer");
            if (gridContainer == null)
            {
                GameObject gridObj = new GameObject("FaceGridContainer");
                gridObj.transform.SetParent(targetPanel.transform, false);
                RectTransform gridRect = gridObj.AddComponent<RectTransform>();
                gridRect.anchorMin = new Vector2(0.1f, 0.1f);
                gridRect.anchorMax = new Vector2(0.9f, 0.45f);
                gridRect.sizeDelta = Vector2.zero;
                gridRect.anchoredPosition = Vector2.zero;

                GridLayoutGroup gridLayout = gridObj.AddComponent<GridLayoutGroup>();
                gridLayout.cellSize = new Vector2(80, 100);
                gridLayout.spacing = new Vector2(10, 10);
                hasChanges = true;
            }

            // Back Button 찾기
            Button backButton = targetPanel.GetComponentInChildren<Button>();
            if (backButton == null)
            {
                GameObject backBtnObj = new GameObject("BackButton");
                backBtnObj.transform.SetParent(targetPanel.transform, false);
                Image btnBg = backBtnObj.AddComponent<Image>();
                btnBg.color = new Color(0.2f, 0.2f, 0.2f, 1f);
                backButton = backBtnObj.AddComponent<Button>();
                
                RectTransform btnRect = backBtnObj.GetComponent<RectTransform>();
                btnRect.anchorMin = new Vector2(0.05f, 0.05f);
                btnRect.anchorMax = new Vector2(0.2f, 0.15f);
                btnRect.sizeDelta = Vector2.zero;
                btnRect.anchoredPosition = Vector2.zero;

                GameObject btnTextObj = CreateTextElement(backBtnObj.transform, "Text", "뒤로");
                RectTransform btnTextRect = btnTextObj.GetComponent<RectTransform>();
                btnTextRect.anchorMin = Vector2.zero;
                btnTextRect.anchorMax = Vector2.one;
                btnTextRect.sizeDelta = Vector2.zero;
                btnTextRect.anchoredPosition = Vector2.zero;
                hasChanges = true;
            }

            if (hasChanges)
            {
                Selection.activeGameObject = targetPanel;
                EditorUtility.DisplayDialog("완료", "컴포넌트가 추가되었습니다!", "확인");
            }
            else
            {
                EditorUtility.DisplayDialog("알림", "추가할 컴포넌트가 없습니다.", "확인");
            }
        }

        private GameObject CreateTextElement(Transform parent, string name, string text)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent, false);
            TextMeshProUGUI textComponent = textObj.AddComponent<TextMeshProUGUI>();
            textComponent.text = text;
            textComponent.fontSize = 16;
            textComponent.color = Color.white;
            textComponent.alignment = TextAlignmentOptions.Center;

            RectTransform rect = textObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(200, 30);
            rect.anchoredPosition = Vector2.zero;

            return textObj;
        }
    }
}
