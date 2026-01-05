using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

namespace PigeonGame.Editor
{
    /// <summary>
    /// 비둘기 프리팹에 PigeonStatusUI를 추가하는 헬퍼
    /// </summary>
    public class PigeonPrefabHelper
    {
        [MenuItem("Tools/Pigeon Helper/Create Status UI Bar and Text in Pigeon Prefab")]
        public static void CreateStatusUIBarAndText()
        {
            string prefabPath = "Assets/Prefabs/Pigeon.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            
            if (prefab == null)
            {
                EditorUtility.DisplayDialog("에러", $"프리팹을 찾을 수 없습니다: {prefabPath}", "OK");
                return;
            }

            // 프리팹 인스턴스 생성
            GameObject instance = PrefabUtility.LoadPrefabContents(prefabPath);
            
            // 이미 Canvas가 있는지 확인
            Canvas existingCanvas = instance.GetComponentInChildren<Canvas>();
            if (existingCanvas != null && existingCanvas.name == "PigeonStatusCanvas")
            {
                EditorUtility.DisplayDialog("알림", "이미 PigeonStatusCanvas가 존재합니다.", "OK");
                PrefabUtility.UnloadPrefabContents(instance);
                return;
            }

            // World Space Canvas 생성
            GameObject canvasObj = new GameObject("PigeonStatusCanvas");
            canvasObj.transform.SetParent(instance.transform);
            canvasObj.transform.localPosition = new Vector3(0, 1f, 0);
            canvasObj.transform.localRotation = Quaternion.identity;
            canvasObj.transform.localScale = Vector3.one * 0.02f;

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
            panelRect.sizeDelta = new Vector2(200, 40);
            panelRect.anchoredPosition = Vector2.zero;

            Image panelImage = panelObj.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.7f);

            // 긴장도 바 배경
            GameObject barBgObj = new GameObject("AlertBarBackground");
            barBgObj.transform.SetParent(panelObj.transform, false);
            Image alertBarBackground = barBgObj.AddComponent<Image>();
            alertBarBackground.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);

            RectTransform barBgRect = alertBarBackground.GetComponent<RectTransform>();
            barBgRect.anchorMin = new Vector2(0.1f, 0.2f);
            barBgRect.anchorMax = new Vector2(0.9f, 0.8f);
            barBgRect.sizeDelta = Vector2.zero;
            barBgRect.anchoredPosition = Vector2.zero;

            // 긴장도 바
            GameObject barObj = new GameObject("AlertBar");
            barObj.transform.SetParent(barBgObj.transform, false);
            Image alertBar = barObj.AddComponent<Image>();
            alertBar.color = Color.green;
            alertBar.type = Image.Type.Filled;
            alertBar.fillMethod = Image.FillMethod.Horizontal;

            RectTransform barRect = alertBar.GetComponent<RectTransform>();
            barRect.anchorMin = Vector2.zero;
            barRect.anchorMax = Vector2.one;
            barRect.sizeDelta = Vector2.zero;
            barRect.anchoredPosition = Vector2.zero;

            // 먹는 중 텍스트
            GameObject eatingObj = new GameObject("EatingText");
            eatingObj.transform.SetParent(panelObj.transform, false);
            TextMeshProUGUI eatingText = eatingObj.AddComponent<TextMeshProUGUI>();
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

            // 프리팹 저장
            PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            PrefabUtility.UnloadPrefabContents(instance);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("완료", "비둘기 프리팹에 UI 바와 텍스트가 생성되었습니다!", "OK");
        }
    }
}

