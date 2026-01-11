using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace PigeonGame.Editor
{
    public static class InventorySlotPrefabHelper
    {
        [MenuItem("Tools/Create Inventory Slot Prefab", false, 3)]
        public static void CreateInventorySlotPrefab()
        {
            // 프리팹 저장 경로
            string prefabPath = "Assets/Prefabs/UI/InventorySlot.prefab";
            
            // 이미 프리팹이 있는지 확인
            GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existingPrefab != null)
            {
                if (EditorUtility.DisplayDialog("프리팹이 이미 존재합니다", 
                    "InventorySlot.prefab이 이미 존재합니다. 덮어쓰시겠습니까?", 
                    "덮어쓰기", "취소"))
                {
                    // 기존 프리팹 삭제
                    AssetDatabase.DeleteAsset(prefabPath);
                }
                else
                {
                    return;
                }
            }

            // 임시 GameObject 생성 (씬에 생성)
            GameObject slotObj = CreateSlotGameObject();
            
            // 프리팹으로 저장
            string folderPath = "Assets/Prefabs/UI";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                // 폴더가 없으면 생성
                string prefabsPath = "Assets/Prefabs";
                if (!AssetDatabase.IsValidFolder(prefabsPath))
                {
                    AssetDatabase.CreateFolder("Assets", "Prefabs");
                }
                AssetDatabase.CreateFolder("Assets/Prefabs", "UI");
            }

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(slotObj, prefabPath);
            
            // 임시 GameObject 삭제
            Object.DestroyImmediate(slotObj);
            
            // 프리팹 선택
            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);
            
            Debug.Log($"InventorySlot 프리팹이 생성되었습니다: {prefabPath}");
        }

        private static GameObject CreateSlotGameObject()
        {
            // 메인 슬롯 GameObject
            GameObject slotObj = new GameObject("InventorySlot");
            
            RectTransform slotRect = slotObj.AddComponent<RectTransform>();
            slotRect.sizeDelta = new Vector2(100, 100);
            
            // 배경 Image (선택사항)
            Image slotImage = slotObj.AddComponent<Image>();
            slotImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);
            
            // Button 컴포넌트 (클릭 가능하게)
            Button slotButton = slotObj.AddComponent<Button>();
            ColorBlock colors = slotButton.colors;
            colors.normalColor = new Color(0.3f, 0.3f, 0.3f, 1f);
            colors.highlightedColor = new Color(0.5f, 0.5f, 0.5f, 1f);
            colors.pressedColor = new Color(0.2f, 0.2f, 0.2f, 1f);
            colors.selectedColor = new Color(0.4f, 0.4f, 0.4f, 1f);
            colors.disabledColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
            slotButton.colors = colors;

            // Icon Image
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(slotObj.transform, false);
            
            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.1f, 0.3f);
            iconRect.anchorMax = new Vector2(0.9f, 0.9f);
            iconRect.sizeDelta = Vector2.zero;
            iconRect.anchoredPosition = Vector2.zero;

            Image iconImage = iconObj.AddComponent<Image>();
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = false; // 버튼 클릭을 방해하지 않도록

            // NameText
            GameObject nameTextObj = new GameObject("NameText");
            nameTextObj.transform.SetParent(slotObj.transform, false);
            
            RectTransform nameRect = nameTextObj.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 0);
            nameRect.anchorMax = new Vector2(1, 0.25f);
            nameRect.sizeDelta = Vector2.zero;
            nameRect.anchoredPosition = Vector2.zero;

            TextMeshProUGUI nameText = nameTextObj.AddComponent<TextMeshProUGUI>();
            nameText.text = "이름";
            nameText.fontSize = 12;
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.color = Color.white;
            nameText.raycastTarget = false; // 버튼 클릭을 방해하지 않도록
            nameText.enableWordWrapping = false;
            nameText.overflowMode = TextOverflowModes.Ellipsis;

            return slotObj;
        }

        [MenuItem("Tools/Create Inventory Slot Prefab (Simple)", false, 4)]
        public static void CreateInventorySlotPrefabSimple()
        {
            // 프리팹 저장 경로
            string prefabPath = "Assets/Prefabs/UI/InventorySlotSimple.prefab";
            
            // 이미 프리팹이 있는지 확인
            GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existingPrefab != null)
            {
                if (EditorUtility.DisplayDialog("프리팹이 이미 존재합니다", 
                    "InventorySlotSimple.prefab이 이미 존재합니다. 덮어쓰시겠습니까?", 
                    "덮어쓰기", "취소"))
                {
                    AssetDatabase.DeleteAsset(prefabPath);
                }
                else
                {
                    return;
                }
            }

            // 임시 GameObject 생성
            GameObject slotObj = CreateSimpleSlotGameObject();
            
            // 프리팹으로 저장
            string folderPath = "Assets/Prefabs/UI";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                string prefabsPath = "Assets/Prefabs";
                if (!AssetDatabase.IsValidFolder(prefabsPath))
                {
                    AssetDatabase.CreateFolder("Assets", "Prefabs");
                }
                AssetDatabase.CreateFolder("Assets/Prefabs", "UI");
            }

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(slotObj, prefabPath);
            
            // 임시 GameObject 삭제
            Object.DestroyImmediate(slotObj);
            
            // 프리팹 선택
            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);
            
            Debug.Log($"InventorySlotSimple 프리팹이 생성되었습니다: {prefabPath}");
        }

        private static GameObject CreateSimpleSlotGameObject()
        {
            // 메인 슬롯 GameObject
            GameObject slotObj = new GameObject("InventorySlot");
            
            RectTransform slotRect = slotObj.AddComponent<RectTransform>();
            slotRect.sizeDelta = new Vector2(100, 100);
            
            // 배경 Image
            Image slotImage = slotObj.AddComponent<Image>();
            slotImage.color = new Color(0.25f, 0.25f, 0.25f, 1f);

            // Icon Image (슬롯 자체가 아이콘 역할)
            Image iconImage = slotObj.AddComponent<Image>();
            iconImage.preserveAspect = true;
            iconImage.color = Color.white;
            
            // Button 컴포넌트
            Button slotButton = slotObj.AddComponent<Button>();
            ColorBlock colors = slotButton.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            slotButton.colors = colors;
            slotButton.targetGraphic = iconImage;

            // NameText (하위 오브젝트)
            GameObject nameTextObj = new GameObject("NameText");
            nameTextObj.transform.SetParent(slotObj.transform, false);
            
            RectTransform nameRect = nameTextObj.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 0);
            nameRect.anchorMax = new Vector2(1, 0.3f);
            nameRect.sizeDelta = Vector2.zero;
            nameRect.anchoredPosition = Vector2.zero;

            TextMeshProUGUI nameText = nameTextObj.AddComponent<TextMeshProUGUI>();
            nameText.text = "";
            nameText.fontSize = 10;
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.color = Color.white;
            nameText.raycastTarget = false;
            nameText.enableWordWrapping = false;
            nameText.overflowMode = TextOverflowModes.Ellipsis;

            return slotObj;
        }
    }
}
