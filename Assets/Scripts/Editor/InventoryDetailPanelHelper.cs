using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using PigeonGame.UI;

namespace PigeonGame.Editor
{
    /// <summary>
    /// 인벤토리 상세 정보 패널을 자동으로 생성하는 헬퍼 툴
    /// </summary>
    public class InventoryDetailPanelHelper : EditorWindow
    {
        [MenuItem("Tools/PigeonGame/Create Inventory Detail Panel")]
        public static void ShowWindow()
        {
            GetWindow<InventoryDetailPanelHelper>("Inventory Detail Panel Helper");
        }

        private void OnGUI()
        {
            GUILayout.Label("인벤토리 상세 정보 패널 생성", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "이 도구는 InventoryUI 컴포넌트가 있는 GameObject를 선택한 상태에서 실행하세요.\n" +
                "상세 정보 패널과 필요한 모든 UI 요소를 자동으로 생성합니다.",
                MessageType.Info);

            GUILayout.Space(10);

            if (GUILayout.Button("상세 정보 패널 생성", GUILayout.Height(30)))
            {
                CreateDetailPanel();
            }

            GUILayout.Space(10);

            if (Selection.activeGameObject != null)
            {
                var inventoryUI = Selection.activeGameObject.GetComponent<InventoryUI>();
                if (inventoryUI != null)
                {
                    EditorGUILayout.HelpBox(
                        $"선택된 오브젝트: {Selection.activeGameObject.name}\n" +
                        "InventoryUI 컴포넌트가 있습니다.",
                        MessageType.Info);
                }
                else
                {
                    EditorGUILayout.HelpBox(
                        $"선택된 오브젝트: {Selection.activeGameObject.name}\n" +
                        "InventoryUI 컴포넌트가 없습니다. InventoryUI가 있는 오브젝트를 선택하세요.",
                        MessageType.Warning);
                }
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "InventoryUI 컴포넌트가 있는 GameObject를 선택하세요.",
                    MessageType.Warning);
            }
        }

        private void CreateDetailPanel()
        {
            // 선택된 오브젝트 확인
            if (Selection.activeGameObject == null)
            {
                EditorUtility.DisplayDialog("오류", "InventoryUI 컴포넌트가 있는 GameObject를 선택하세요.", "확인");
                return;
            }

            var inventoryUI = Selection.activeGameObject.GetComponent<InventoryUI>();
            if (inventoryUI == null)
            {
                EditorUtility.DisplayDialog("오류", "선택된 오브젝트에 InventoryUI 컴포넌트가 없습니다.", "확인");
                return;
            }

            // Canvas 찾기
            Canvas canvas = Selection.activeGameObject.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                canvas = FindObjectOfType<Canvas>();
                if (canvas == null)
                {
                    EditorUtility.DisplayDialog("오류", "Canvas를 찾을 수 없습니다. Canvas가 있는 씬에서 실행하세요.", "확인");
                    return;
                }
            }

            // 기존 패널이 있는지 확인
            Transform existingPanel = canvas.transform.Find("InventoryDetailPanel");
            if (existingPanel != null)
            {
                if (!EditorUtility.DisplayDialog("확인", 
                    "이미 InventoryDetailPanel이 존재합니다. 삭제하고 새로 만들까요?",
                    "예", "아니오"))
                {
                    return;
                }
                DestroyImmediate(existingPanel.gameObject);
            }

            // 상세 정보 패널 생성
            GameObject detailPanel = new GameObject("InventoryDetailPanel");
            detailPanel.transform.SetParent(canvas.transform, false);
            
            RectTransform detailPanelRect = detailPanel.AddComponent<RectTransform>();
            detailPanelRect.anchorMin = new Vector2(0.5f, 0.5f);
            detailPanelRect.anchorMax = new Vector2(0.5f, 0.5f);
            detailPanelRect.sizeDelta = new Vector2(400, 500);
            detailPanelRect.anchoredPosition = Vector2.zero;

            Image detailPanelBg = detailPanel.AddComponent<Image>();
            detailPanelBg.color = new Color(0.2f, 0.2f, 0.2f, 0.95f);

            // Vertical Layout Group 추가
            VerticalLayoutGroup detailLayout = detailPanel.AddComponent<VerticalLayoutGroup>();
            detailLayout.spacing = 10;
            detailLayout.padding = new RectOffset(20, 20, 20, 20);
            detailLayout.childControlHeight = false;
            detailLayout.childControlWidth = true;
            detailLayout.childForceExpandHeight = false;
            detailLayout.childForceExpandWidth = true;

            // Content Size Fitter 추가
            ContentSizeFitter detailFitter = detailPanel.AddComponent<ContentSizeFitter>();
            detailFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // 제목 생성
            GameObject titleObj = CreateText("TitleText", detailPanel.transform, "비둘기 상세 정보", 24, TextAlignmentOptions.Center);
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.sizeDelta = new Vector2(0, 40);

            // 아이콘 영역 생성
            GameObject iconArea = new GameObject("IconArea");
            iconArea.transform.SetParent(detailPanel.transform, false);
            RectTransform iconAreaRect = iconArea.AddComponent<RectTransform>();
            iconAreaRect.sizeDelta = new Vector2(0, 120);

            HorizontalLayoutGroup iconLayout = iconArea.AddComponent<HorizontalLayoutGroup>();
            iconLayout.childAlignment = TextAnchor.MiddleCenter;
            iconLayout.childControlHeight = false;
            iconLayout.childControlWidth = false;
            iconLayout.childForceExpandHeight = false;
            iconLayout.childForceExpandWidth = false;

            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(iconArea.transform, false);
            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.sizeDelta = new Vector2(100, 100);
            Image iconImage = iconObj.AddComponent<Image>();
            iconImage.color = Color.white;

            // 정보 텍스트들 생성
            GameObject nameText = CreateText("NameText", detailPanel.transform, "종 이름", 20, TextAlignmentOptions.Left);
            GameObject speciesText = CreateText("SpeciesText", detailPanel.transform, "종: SP01", 16, TextAlignmentOptions.Left);
            GameObject rarityText = CreateText("RarityText", detailPanel.transform, "희귀도: Tier 1", 16, TextAlignmentOptions.Left);
            GameObject obesityText = CreateText("ObesityText", detailPanel.transform, "비만도: 3", 16, TextAlignmentOptions.Left);
            GameObject bitePowerText = CreateText("BitePowerText", detailPanel.transform, "한입값: 3", 16, TextAlignmentOptions.Left);
            GameObject faceText = CreateText("FaceText", detailPanel.transform, "얼굴: 기본", 16, TextAlignmentOptions.Left);
            GameObject priceText = CreateText("PriceText", detailPanel.transform, "가격: 100", 18, TextAlignmentOptions.Left);

            // 가격 텍스트 강조
            TextMeshProUGUI priceTextComponent = priceText.GetComponent<TextMeshProUGUI>();
            if (priceTextComponent != null)
            {
                priceTextComponent.color = Color.yellow;
            }

            // 닫기 버튼 생성
            GameObject closeButtonObj = new GameObject("CloseButton");
            closeButtonObj.transform.SetParent(detailPanel.transform, false);
            RectTransform closeButtonRect = closeButtonObj.AddComponent<RectTransform>();
            closeButtonRect.sizeDelta = new Vector2(0, 40);

            Image closeButtonBg = closeButtonObj.AddComponent<Image>();
            closeButtonBg.color = new Color(0.4f, 0.2f, 0.2f, 1f);

            Button closeButton = closeButtonObj.AddComponent<Button>();
            
            // 버튼 텍스트
            GameObject closeButtonTextObj = CreateText("Text", closeButtonObj.transform, "닫기", 16, TextAlignmentOptions.Center);
            RectTransform closeButtonTextRect = closeButtonTextObj.GetComponent<RectTransform>();
            closeButtonTextRect.anchorMin = Vector2.zero;
            closeButtonTextRect.anchorMax = Vector2.one;
            closeButtonTextRect.sizeDelta = Vector2.zero;
            closeButtonTextRect.anchoredPosition = Vector2.zero;

            // InventoryUI에 할당
            SerializedObject serializedObject = new SerializedObject(inventoryUI);
            serializedObject.FindProperty("detailPanel").objectReferenceValue = detailPanel;
            serializedObject.FindProperty("detailIconImage").objectReferenceValue = iconImage;
            serializedObject.FindProperty("detailNameText").objectReferenceValue = nameText.GetComponent<TextMeshProUGUI>();
            serializedObject.FindProperty("detailSpeciesText").objectReferenceValue = speciesText.GetComponent<TextMeshProUGUI>();
            serializedObject.FindProperty("detailObesityText").objectReferenceValue = obesityText.GetComponent<TextMeshProUGUI>();
            serializedObject.FindProperty("detailPriceText").objectReferenceValue = priceText.GetComponent<TextMeshProUGUI>();
            serializedObject.FindProperty("detailFaceText").objectReferenceValue = faceText.GetComponent<TextMeshProUGUI>();
            serializedObject.FindProperty("detailRarityText").objectReferenceValue = rarityText.GetComponent<TextMeshProUGUI>();
            serializedObject.FindProperty("detailBitePowerText").objectReferenceValue = bitePowerText.GetComponent<TextMeshProUGUI>();
            serializedObject.FindProperty("detailCloseButton").objectReferenceValue = closeButton;
            serializedObject.ApplyModifiedProperties();

            // 초기에는 비활성화
            detailPanel.SetActive(false);

            EditorUtility.DisplayDialog("완료", 
                "인벤토리 상세 정보 패널이 생성되었습니다!\n\n" +
                "생성된 패널: InventoryDetailPanel\n" +
                "InventoryUI 컴포넌트에 자동으로 할당되었습니다.",
                "확인");

            // 생성된 패널 선택
            Selection.activeGameObject = detailPanel;
            EditorGUIUtility.PingObject(detailPanel);
        }

        private GameObject CreateText(string name, Transform parent, string text, int fontSize, TextAlignmentOptions alignment)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent, false);
            
            RectTransform rect = textObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 30);

            TextMeshProUGUI textComponent = textObj.AddComponent<TextMeshProUGUI>();
            textComponent.text = text;
            textComponent.fontSize = fontSize;
            textComponent.alignment = alignment;
            textComponent.color = Color.white;

            return textObj;
        }
    }
}
