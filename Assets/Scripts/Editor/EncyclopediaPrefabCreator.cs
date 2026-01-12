using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

namespace PigeonGame.Editor
{
    /// <summary>
    /// 도감 UI용 Prefab 생성 에디터 스크립트
    /// </summary>
    public class EncyclopediaPrefabCreator : EditorWindow
    {
        [MenuItem("Tools/Pigeon Game/Create Encyclopedia Prefabs")]
        public static void ShowWindow()
        {
            GetWindow<EncyclopediaPrefabCreator>("Encyclopedia Prefab Creator");
        }

        private void OnGUI()
        {
            GUILayout.Label("도감 UI Prefab 생성", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            if (GUILayout.Button("Species Item Prefab 생성", GUILayout.Height(30)))
            {
                CreateSpeciesItemPrefab();
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Face Item Prefab 생성", GUILayout.Height(30)))
            {
                CreateFaceItemPrefab();
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Trap Item Prefab 생성", GUILayout.Height(30)))
            {
                CreateTrapItemPrefab();
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("생성된 Prefab은 Assets/Prefabs/UI/ 폴더에 저장됩니다.", MessageType.Info);
        }

        private void CreateSpeciesItemPrefab()
        {
            // Prefabs/UI 폴더 확인 및 생성
            string folderPath = "Assets/Prefabs/UI";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                string parentFolder = "Assets/Prefabs";
                if (!AssetDatabase.IsValidFolder(parentFolder))
                {
                    AssetDatabase.CreateFolder("Assets", "Prefabs");
                }
                AssetDatabase.CreateFolder("Assets/Prefabs", "UI");
            }

            // GameObject 생성
            GameObject prefabObj = new GameObject("SpeciesItemPrefab");
            RectTransform rect = prefabObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(100, 100);

            // 배경 Image
            Image bg = prefabObj.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            // Button
            Button button = prefabObj.AddComponent<Button>();

            // Icon 자식 오브젝트
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(prefabObj.transform, false);
            Image icon = iconObj.AddComponent<Image>();
            RectTransform iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.1f, 0.3f);
            iconRect.anchorMax = new Vector2(0.9f, 0.9f);
            iconRect.sizeDelta = Vector2.zero;
            iconRect.anchoredPosition = Vector2.zero;

            // Name 자식 오브젝트
            GameObject nameObj = new GameObject("Name");
            nameObj.transform.SetParent(prefabObj.transform, false);
            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = "Species Name";
            nameText.fontSize = 12;
            nameText.color = Color.white;
            nameText.alignment = TextAlignmentOptions.Center;
            RectTransform nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 0);
            nameRect.anchorMax = new Vector2(1, 0.3f);
            nameRect.sizeDelta = Vector2.zero;
            nameRect.anchoredPosition = Vector2.zero;

            // LockOverlay 자식 오브젝트
            GameObject lockObj = new GameObject("LockOverlay");
            lockObj.transform.SetParent(prefabObj.transform, false);
            Image lockImage = lockObj.AddComponent<Image>();
            lockImage.color = new Color(0, 0, 0, 0.7f);
            RectTransform lockRect = lockObj.GetComponent<RectTransform>();
            lockRect.anchorMin = Vector2.zero;
            lockRect.anchorMax = Vector2.one;
            lockRect.sizeDelta = Vector2.zero;
            lockRect.anchoredPosition = Vector2.zero;
            lockObj.SetActive(false); // 기본적으로 비활성화

            // Prefab으로 저장
            string prefabPath = $"{folderPath}/SpeciesItemPrefab.prefab";
            PrefabUtility.SaveAsPrefabAsset(prefabObj, prefabPath);
            DestroyImmediate(prefabObj);

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("완료", $"SpeciesItemPrefab이 생성되었습니다!\n경로: {prefabPath}", "확인");
            
            // 생성된 Prefab 선택
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        }

        private void CreateFaceItemPrefab()
        {
            // Prefabs/UI 폴더 확인 및 생성
            string folderPath = "Assets/Prefabs/UI";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                string parentFolder = "Assets/Prefabs";
                if (!AssetDatabase.IsValidFolder(parentFolder))
                {
                    AssetDatabase.CreateFolder("Assets", "Prefabs");
                }
                AssetDatabase.CreateFolder("Assets/Prefabs", "UI");
            }

            // GameObject 생성
            GameObject prefabObj = new GameObject("FaceItemPrefab");
            RectTransform rect = prefabObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(80, 100);

            // 배경 Image
            Image bg = prefabObj.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            // Name 자식 오브젝트
            GameObject nameObj = new GameObject("Name");
            nameObj.transform.SetParent(prefabObj.transform, false);
            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = "Face Name";
            nameText.fontSize = 12;
            nameText.color = Color.white;
            nameText.alignment = TextAlignmentOptions.Center;
            RectTransform nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 0.6f);
            nameRect.anchorMax = new Vector2(1, 1);
            nameRect.sizeDelta = Vector2.zero;
            nameRect.anchoredPosition = Vector2.zero;

            // WeightText 자식 오브젝트
            GameObject weightObj = new GameObject("WeightText");
            weightObj.transform.SetParent(prefabObj.transform, false);
            TextMeshProUGUI weightText = weightObj.AddComponent<TextMeshProUGUI>();
            weightText.text = "미발견";
            weightText.fontSize = 10;
            weightText.color = Color.gray;
            weightText.alignment = TextAlignmentOptions.Center;
            RectTransform weightRect = weightObj.GetComponent<RectTransform>();
            weightRect.anchorMin = new Vector2(0, 0.3f);
            weightRect.anchorMax = new Vector2(1, 0.6f);
            weightRect.sizeDelta = Vector2.zero;
            weightRect.anchoredPosition = Vector2.zero;

            // LockOverlay 자식 오브젝트
            GameObject lockObj = new GameObject("LockOverlay");
            lockObj.transform.SetParent(prefabObj.transform, false);
            Image lockImage = lockObj.AddComponent<Image>();
            lockImage.color = new Color(0, 0, 0, 0.7f);
            RectTransform lockRect = lockObj.GetComponent<RectTransform>();
            lockRect.anchorMin = Vector2.zero;
            lockRect.anchorMax = Vector2.one;
            lockRect.sizeDelta = Vector2.zero;
            lockRect.anchoredPosition = Vector2.zero;
            lockObj.SetActive(false); // 기본적으로 비활성화

            // Prefab으로 저장
            string prefabPath = $"{folderPath}/FaceItemPrefab.prefab";
            PrefabUtility.SaveAsPrefabAsset(prefabObj, prefabPath);
            DestroyImmediate(prefabObj);

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("완료", $"FaceItemPrefab이 생성되었습니다!\n경로: {prefabPath}", "확인");
            
            // 생성된 Prefab 선택
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        }

        private void CreateTrapItemPrefab()
        {
            // Prefabs/UI 폴더 확인 및 생성
            string folderPath = "Assets/Prefabs/UI";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                string parentFolder = "Assets/Prefabs";
                if (!AssetDatabase.IsValidFolder(parentFolder))
                {
                    AssetDatabase.CreateFolder("Assets", "Prefabs");
                }
                AssetDatabase.CreateFolder("Assets/Prefabs", "UI");
            }

            // GameObject 생성
            GameObject prefabObj = new GameObject("TrapItemPrefab");
            RectTransform rect = prefabObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(100, 100);

            // 배경 Image
            Image bg = prefabObj.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            // Button
            Button button = prefabObj.AddComponent<Button>();

            // Text 자식 오브젝트
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(prefabObj.transform, false);
            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = "Trap Name";
            text.fontSize = 14;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;

            // Prefab으로 저장
            string prefabPath = $"{folderPath}/TrapItemPrefab.prefab";
            PrefabUtility.SaveAsPrefabAsset(prefabObj, prefabPath);
            DestroyImmediate(prefabObj);

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("완료", $"TrapItemPrefab이 생성되었습니다!\n경로: {prefabPath}", "확인");
            
            // 생성된 Prefab 선택
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        }
    }
}
