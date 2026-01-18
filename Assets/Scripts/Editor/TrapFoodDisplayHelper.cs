using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using PigeonGame.UI;

namespace PigeonGame.Editor
{
    /// <summary>
    /// 트랩 상태바 UI 프리팹을 만드는 헬퍼 스크립트
    /// </summary>
    public class TrapFoodDisplayHelper : EditorWindow
    {
        [MenuItem("Tools/Trap/Create Trap Food Display UI")]
        public static void CreateTrapFoodDisplayUI()
        {
            // 선택된 오브젝트 확인
            GameObject selectedObject = Selection.activeGameObject;
            
            if (selectedObject == null)
            {
                EditorUtility.DisplayDialog("오류", "먼저 트랩 오브젝트를 선택해주세요.", "확인");
                return;
            }

            // 이미 TrapFoodDisplay가 있는지 확인
            TrapFoodDisplay existingDisplay = selectedObject.GetComponent<TrapFoodDisplay>();
            if (existingDisplay != null)
            {
                bool overwrite = EditorUtility.DisplayDialog(
                    "이미 존재함",
                    "이 오브젝트에 이미 TrapFoodDisplay 컴포넌트가 있습니다. UI를 다시 생성하시겠습니까?",
                    "예",
                    "아니오"
                );
                
                if (!overwrite)
                    return;
            }

            // UI 구조 생성
            CreateUIStructure(selectedObject);

            EditorUtility.DisplayDialog("완료", "트랩 상태바 UI가 생성되었습니다!", "확인");
        }

        private static void CreateUIStructure(GameObject parentObject)
        {
            // TrapFoodDisplay 컴포넌트 추가 또는 가져오기
            TrapFoodDisplay display = parentObject.GetComponent<TrapFoodDisplay>();
            if (display == null)
            {
                display = parentObject.AddComponent<TrapFoodDisplay>();
            }

            // World Space Canvas 생성
            GameObject canvasObj = new GameObject("TrapFoodCanvas");
            canvasObj.transform.SetParent(parentObject.transform);
            canvasObj.transform.localPosition = Vector3.zero;
            canvasObj.transform.localRotation = Quaternion.identity;
            canvasObj.transform.localScale = Vector3.one * 0.01f; // World Space에서 적절한 크기

            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;

            GraphicRaycaster raycaster = canvasObj.AddComponent<GraphicRaycaster>();

            // 배경 패널
            GameObject panelObj = new GameObject("Panel");
            panelObj.transform.SetParent(canvasObj.transform, false);
            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.sizeDelta = new Vector2(200, 60);
            panelRect.anchoredPosition = Vector2.zero;

            Image panelImage = panelObj.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.7f);

            // 텍스트 생성
            GameObject textObj = new GameObject("FoodText");
            textObj.transform.SetParent(panelObj.transform, false);
            TextMeshProUGUI foodText = textObj.AddComponent<TextMeshProUGUI>();
            foodText.text = "먹이: 20/20";
            foodText.fontSize = 20;
            foodText.color = Color.white;
            foodText.alignment = TextAlignmentOptions.Center;

            RectTransform textRect = foodText.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 0.5f);
            textRect.anchorMax = new Vector2(1, 1);
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;

            // 진행 바 배경
            GameObject barBgObj = new GameObject("BarBackground");
            barBgObj.transform.SetParent(panelObj.transform, false);
            Image barBg = barBgObj.AddComponent<Image>();
            barBg.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);

            RectTransform barBgRect = barBg.GetComponent<RectTransform>();
            barBgRect.anchorMin = new Vector2(0, 0);
            barBgRect.anchorMax = new Vector2(1, 0.5f);
            barBgRect.sizeDelta = Vector2.zero;
            barBgRect.anchoredPosition = Vector2.zero;

            // 진행 바
            GameObject barObj = new GameObject("FoodBar");
            barObj.transform.SetParent(barBgObj.transform, false);
            Image foodBar = barObj.AddComponent<Image>();
            foodBar.color = Color.green;
            foodBar.type = Image.Type.Filled;
            foodBar.fillMethod = Image.FillMethod.Horizontal;
            foodBar.fillOrigin = (int)Image.OriginHorizontal.Left;
            foodBar.fillAmount = 1f; // 초기값 100%

            RectTransform barRect = foodBar.GetComponent<RectTransform>();
            barRect.anchorMin = Vector2.zero;
            barRect.anchorMax = Vector2.one;
            barRect.sizeDelta = Vector2.zero;
            barRect.anchoredPosition = Vector2.zero;

            // TrapFoodDisplay 컴포넌트에 참조 할당
            SerializedObject serializedDisplay = new SerializedObject(display);
            SerializedProperty foodTextProp = serializedDisplay.FindProperty("foodText");
            SerializedProperty foodBarProp = serializedDisplay.FindProperty("foodBar");

            if (foodTextProp != null)
            {
                foodTextProp.objectReferenceValue = foodText;
            }

            if (foodBarProp != null)
            {
                foodBarProp.objectReferenceValue = foodBar;
            }

            serializedDisplay.ApplyModifiedProperties();

            // 변경사항 저장
            EditorUtility.SetDirty(parentObject);
            if (PrefabUtility.IsPartOfPrefabInstance(parentObject))
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(parentObject);
            }

            Debug.Log($"트랩 상태바 UI가 생성되었습니다: {parentObject.name}");
        }

        [MenuItem("Tools/Trap/Create Trap Food Display UI", true)]
        public static bool ValidateCreateTrapFoodDisplayUI()
        {
            return Selection.activeGameObject != null;
        }
    }
}
