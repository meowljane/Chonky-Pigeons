using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using PigeonGame.UI;

namespace PigeonGame.Editor
{
    /// <summary>
    /// UI와 프리팹을 자동으로 생성하는 헬퍼 툴
    /// </summary>
    public class UIHelperTool : EditorWindow
    {
        [MenuItem("PigeonGame/UI Helper/Create MapInfoUI")]
        public static void CreateMapInfoUI()
        {
            // Canvas 찾기 또는 생성
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
            }

            // MapInfoUI GameObject 생성
            GameObject mapInfoObj = new GameObject("MapInfoUI");
            mapInfoObj.transform.SetParent(canvas.transform, false);
            
            RectTransform mapInfoRect = mapInfoObj.AddComponent<RectTransform>();
            mapInfoRect.anchorMin = new Vector2(0f, 1f);
            mapInfoRect.anchorMax = new Vector2(0f, 1f);
            mapInfoRect.pivot = new Vector2(0f, 1f);
            mapInfoRect.anchoredPosition = new Vector2(10f, -10f);
            mapInfoRect.sizeDelta = new Vector2(300f, 200f);

            MapInfoUI mapInfoUI = mapInfoObj.AddComponent<MapInfoUI>();

            // Terrain 타입 표시 텍스트 생성
            GameObject terrainTextObj = new GameObject("TerrainTypeText");
            terrainTextObj.transform.SetParent(mapInfoObj.transform, false);
            
            RectTransform terrainRect = terrainTextObj.AddComponent<RectTransform>();
            terrainRect.anchorMin = new Vector2(0f, 1f);
            terrainRect.anchorMax = new Vector2(0f, 1f);
            terrainRect.pivot = new Vector2(0f, 1f);
            terrainRect.anchoredPosition = new Vector2(0f, 0f);
            terrainRect.sizeDelta = new Vector2(300f, 30f);

            TextMeshProUGUI terrainText = terrainTextObj.AddComponent<TextMeshProUGUI>();
            terrainText.text = "Terrain: default";
            terrainText.fontSize = 18f;
            terrainText.color = Color.white;

            // 종별 확률 컨테이너 생성
            GameObject containerObj = new GameObject("SpeciesProbabilityContainer");
            containerObj.transform.SetParent(mapInfoObj.transform, false);
            
            RectTransform containerRect = containerObj.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0f, 1f);
            containerRect.anchorMax = new Vector2(0f, 1f);
            containerRect.pivot = new Vector2(0f, 1f);
            containerRect.anchoredPosition = new Vector2(0f, -35f);
            containerRect.sizeDelta = new Vector2(300f, 165f);

            VerticalLayoutGroup layoutGroup = containerObj.AddComponent<VerticalLayoutGroup>();
            layoutGroup.spacing = 5f;
            layoutGroup.padding = new RectOffset(5, 5, 5, 5);
            layoutGroup.childControlHeight = false;
            layoutGroup.childControlWidth = true;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = false;

            ContentSizeFitter sizeFitter = containerObj.AddComponent<ContentSizeFitter>();
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // 종별 확률 아이템 프리팹 생성
            GameObject prefabObj = new GameObject("SpeciesProbabilityItem");
            RectTransform prefabRect = prefabObj.AddComponent<RectTransform>();
            prefabRect.sizeDelta = new Vector2(290f, 25f);

            GameObject prefabTextObj = new GameObject("Text");
            prefabTextObj.transform.SetParent(prefabObj.transform, false);
            
            RectTransform prefabTextRect = prefabTextObj.AddComponent<RectTransform>();
            prefabTextRect.anchorMin = Vector2.zero;
            prefabTextRect.anchorMax = Vector2.one;
            prefabTextRect.sizeDelta = Vector2.zero;
            prefabTextRect.anchoredPosition = Vector2.zero;

            TextMeshProUGUI prefabText = prefabTextObj.AddComponent<TextMeshProUGUI>();
            prefabText.text = "종 이름: 0.0%";
            prefabText.fontSize = 14f;
            prefabText.color = Color.white;

            // 프리팹으로 저장
            string prefabPath = "Assets/Prefabs/UI/SpeciesProbabilityItem.prefab";
            string prefabDirectory = System.IO.Path.GetDirectoryName(prefabPath);
            if (!System.IO.Directory.Exists(prefabDirectory))
            {
                System.IO.Directory.CreateDirectory(prefabDirectory);
            }

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(prefabObj, prefabPath);
            DestroyImmediate(prefabObj);

            // MapInfoUI 컴포넌트에 참조 할당
            SerializedObject serializedObject = new SerializedObject(mapInfoUI);
            serializedObject.FindProperty("terrainTypeTextMesh").objectReferenceValue = terrainText;
            serializedObject.FindProperty("speciesProbabilityContainer").objectReferenceValue = containerRect;
            serializedObject.FindProperty("speciesProbabilityItemPrefab").objectReferenceValue = prefab;
            serializedObject.ApplyModifiedProperties();

            Selection.activeGameObject = mapInfoObj;
            EditorUtility.DisplayDialog("완료", "MapInfoUI가 생성되었습니다!\n\n- Terrain 타입 텍스트: TerrainTypeText\n- 종별 확률 컨테이너: SpeciesProbabilityContainer\n- 프리팹: Assets/Prefabs/UI/SpeciesProbabilityItem.prefab", "확인");
        }

        [MenuItem("PigeonGame/UI Helper/Create MoneyDisplay UI")]
        public static void CreateMoneyDisplayUI()
        {
            // Canvas 찾기 또는 생성
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
            }

            // MoneyDisplay GameObject 생성
            GameObject moneyObj = new GameObject("MoneyDisplay");
            moneyObj.transform.SetParent(canvas.transform, false);
            
            RectTransform moneyRect = moneyObj.AddComponent<RectTransform>();
            moneyRect.anchorMin = new Vector2(1f, 1f);
            moneyRect.anchorMax = new Vector2(1f, 1f);
            moneyRect.pivot = new Vector2(1f, 1f);
            moneyRect.anchoredPosition = new Vector2(-10f, -10f);
            moneyRect.sizeDelta = new Vector2(200f, 30f);

            MoneyDisplay moneyDisplay = moneyObj.AddComponent<MoneyDisplay>();

            // 돈 표시 텍스트 생성
            GameObject textObj = new GameObject("MoneyText");
            textObj.transform.SetParent(moneyObj.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;

            TextMeshProUGUI moneyText = textObj.AddComponent<TextMeshProUGUI>();
            moneyText.text = "돈: 0";
            moneyText.fontSize = 20f;
            moneyText.color = Color.yellow;
            moneyText.alignment = TextAlignmentOptions.TopRight;

            // MoneyDisplay 컴포넌트에 참조 할당
            SerializedObject serializedObject = new SerializedObject(moneyDisplay);
            serializedObject.FindProperty("moneyText").objectReferenceValue = moneyText;
            serializedObject.ApplyModifiedProperties();

            Selection.activeGameObject = moneyObj;
            EditorUtility.DisplayDialog("완료", "MoneyDisplay UI가 생성되었습니다!", "확인");
        }

        [MenuItem("PigeonGame/UI Helper/Create All UI")]
        public static void CreateAllUI()
        {
            if (EditorUtility.DisplayDialog("UI 생성", "모든 UI를 생성하시겠습니까?\n\n- MapInfoUI\n- MoneyDisplay", "생성", "취소"))
            {
                CreateMapInfoUI();
                CreateMoneyDisplayUI();
                EditorUtility.DisplayDialog("완료", "모든 UI가 생성되었습니다!", "확인");
            }
        }
    }
}
