using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.IO;

namespace PigeonGame.Editor
{
    /// <summary>
    /// UI 프리팹을 자동으로 생성하는 헬퍼 에디터
    /// </summary>
    public class UIHelperEditor : EditorWindow
    {
        private string prefabPath = "Assets/Prefabs/UI";

        [MenuItem("Tools/UI Helper/Create Inventory Item Prefab")]
        public static void CreateInventoryItemPrefab()
        {
            GetWindow<UIHelperEditor>("UI Helper").CreateInventoryItem();
        }

        [MenuItem("Tools/UI Helper/Create Trap Shop Item Prefab")]
        public static void CreateTrapShopItemPrefab()
        {
            GetWindow<UIHelperEditor>("UI Helper").CreateTrapShopItem();
        }

        [MenuItem("Tools/UI Helper/Create Inventory UI (In Scene)")]
        public static void CreateInventoryUI()
        {
            GetWindow<UIHelperEditor>("UI Helper").CreateInventoryUIScene();
        }

        [MenuItem("Tools/UI Helper/Create Trap Shop UI (In Scene)")]
        public static void CreateTrapShopUI()
        {
            GetWindow<UIHelperEditor>("UI Helper").CreateTrapShopUIScene();
        }

        private void CreateInventoryItem()
        {
            // 경로 확인 및 생성
            if (!Directory.Exists(prefabPath))
            {
                Directory.CreateDirectory(prefabPath);
            }

            // 루트 오브젝트 생성
            GameObject itemObj = new GameObject("InventoryItem");
            RectTransform itemRect = itemObj.AddComponent<RectTransform>();
            itemRect.sizeDelta = new Vector2(400, 80);

            // 배경 이미지
            Image bgImage = itemObj.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            // 종 이름 텍스트
            GameObject nameObj = new GameObject("NameText");
            nameObj.transform.SetParent(itemObj.transform, false);
            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = "비둘기 이름";
            nameText.fontSize = 24;
            nameText.color = Color.white;
            RectTransform nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 0.5f);
            nameRect.anchorMax = new Vector2(0.5f, 1);
            nameRect.sizeDelta = Vector2.zero;
            nameRect.anchoredPosition = Vector2.zero;

            // 가격 텍스트
            GameObject priceObj = new GameObject("PriceText");
            priceObj.transform.SetParent(itemObj.transform, false);
            TextMeshProUGUI priceText = priceObj.AddComponent<TextMeshProUGUI>();
            priceText.text = "가격: 100";
            priceText.fontSize = 20;
            priceText.color = Color.yellow;
            RectTransform priceRect = priceObj.GetComponent<RectTransform>();
            priceRect.anchorMin = new Vector2(0, 0);
            priceRect.anchorMax = new Vector2(0.5f, 0.5f);
            priceRect.sizeDelta = Vector2.zero;
            priceRect.anchoredPosition = Vector2.zero;

            // 비만도 텍스트
            GameObject obesityObj = new GameObject("ObesityText");
            obesityObj.transform.SetParent(itemObj.transform, false);
            TextMeshProUGUI obesityText = obesityObj.AddComponent<TextMeshProUGUI>();
            obesityText.text = "비만도: 3";
            obesityText.fontSize = 18;
            obesityText.color = Color.cyan;
            RectTransform obesityRect = obesityObj.GetComponent<RectTransform>();
            obesityRect.anchorMin = new Vector2(0.5f, 0);
            obesityRect.anchorMax = new Vector2(0.8f, 0.5f);
            obesityRect.sizeDelta = Vector2.zero;
            obesityRect.anchoredPosition = Vector2.zero;

            // 판매 버튼
            GameObject buttonObj = new GameObject("SellButton");
            buttonObj.transform.SetParent(itemObj.transform, false);
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.8f, 0.2f, 0.2f, 1f);
            Button sellButton = buttonObj.AddComponent<Button>();
            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.8f, 0.1f);
            buttonRect.anchorMax = new Vector2(1f, 0.9f);
            buttonRect.sizeDelta = Vector2.zero;
            buttonRect.anchoredPosition = Vector2.zero;

            // 버튼 텍스트
            GameObject buttonTextObj = new GameObject("Text");
            buttonTextObj.transform.SetParent(buttonObj.transform, false);
            TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = "판매";
            buttonText.fontSize = 20;
            buttonText.color = Color.white;
            buttonText.alignment = TextAlignmentOptions.Center;
            RectTransform buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
            buttonTextRect.anchorMin = Vector2.zero;
            buttonTextRect.anchorMax = Vector2.one;
            buttonTextRect.sizeDelta = Vector2.zero;
            buttonTextRect.anchoredPosition = Vector2.zero;

            // 프리팹으로 저장
            string path = $"{prefabPath}/InventoryItem.prefab";
            PrefabUtility.SaveAsPrefabAsset(itemObj, path);
            DestroyImmediate(itemObj);

            Debug.Log($"인벤토리 아이템 프리팹 생성 완료: {path}");
            EditorUtility.DisplayDialog("완료", $"인벤토리 아이템 프리팹이 생성되었습니다!\n{path}", "OK");
        }

        private void CreateTrapShopItem()
        {
            // 경로 확인 및 생성
            if (!Directory.Exists(prefabPath))
            {
                Directory.CreateDirectory(prefabPath);
            }

            // 루트 오브젝트 생성
            GameObject itemObj = new GameObject("TrapShopItem");
            RectTransform itemRect = itemObj.AddComponent<RectTransform>();
            itemRect.sizeDelta = new Vector2(400, 100);

            // 배경 이미지
            Image bgImage = itemObj.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            // 덫 이름 텍스트
            GameObject nameObj = new GameObject("NameText");
            nameObj.transform.SetParent(itemObj.transform, false);
            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = "덫 이름";
            nameText.fontSize = 24;
            nameText.color = Color.white;
            RectTransform nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 0.6f);
            nameRect.anchorMax = new Vector2(0.6f, 1);
            nameRect.sizeDelta = Vector2.zero;
            nameRect.anchoredPosition = Vector2.zero;

            // 가격 텍스트
            GameObject priceObj = new GameObject("PriceText");
            priceObj.transform.SetParent(itemObj.transform, false);
            TextMeshProUGUI priceText = priceObj.AddComponent<TextMeshProUGUI>();
            priceText.text = "가격: 100";
            priceText.fontSize = 20;
            priceText.color = Color.yellow;
            RectTransform priceRect = priceObj.GetComponent<RectTransform>();
            priceRect.anchorMin = new Vector2(0, 0.3f);
            priceRect.anchorMax = new Vector2(0.6f, 0.6f);
            priceRect.sizeDelta = Vector2.zero;
            priceRect.anchoredPosition = Vector2.zero;

            // 해금 상태 텍스트
            GameObject statusObj = new GameObject("StatusText");
            statusObj.transform.SetParent(itemObj.transform, false);
            TextMeshProUGUI statusText = statusObj.AddComponent<TextMeshProUGUI>();
            statusText.text = "미해금";
            statusText.fontSize = 18;
            statusText.color = Color.red;
            RectTransform statusRect = statusObj.GetComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0, 0);
            statusRect.anchorMax = new Vector2(0.6f, 0.3f);
            statusRect.sizeDelta = Vector2.zero;
            statusRect.anchoredPosition = Vector2.zero;

            // 구매 버튼
            GameObject buttonObj = new GameObject("BuyButton");
            buttonObj.transform.SetParent(itemObj.transform, false);
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.8f, 0.2f, 1f);
            Button buyButton = buttonObj.AddComponent<Button>();
            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.6f, 0.1f);
            buttonRect.anchorMax = new Vector2(1f, 0.9f);
            buttonRect.sizeDelta = Vector2.zero;
            buttonRect.anchoredPosition = Vector2.zero;

            // 버튼 텍스트
            GameObject buttonTextObj = new GameObject("Text");
            buttonTextObj.transform.SetParent(buttonObj.transform, false);
            TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = "구매";
            buttonText.fontSize = 20;
            buttonText.color = Color.white;
            buttonText.alignment = TextAlignmentOptions.Center;
            RectTransform buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
            buttonTextRect.anchorMin = Vector2.zero;
            buttonTextRect.anchorMax = Vector2.one;
            buttonTextRect.sizeDelta = Vector2.zero;
            buttonTextRect.anchoredPosition = Vector2.zero;

            // 프리팹으로 저장
            string path = $"{prefabPath}/TrapShopItem.prefab";
            PrefabUtility.SaveAsPrefabAsset(itemObj, path);
            DestroyImmediate(itemObj);

            Debug.Log($"덫 상점 아이템 프리팹 생성 완료: {path}");
            EditorUtility.DisplayDialog("완료", $"덫 상점 아이템 프리팹이 생성되었습니다!\n{path}", "OK");
        }

        private void CreateInventoryUIScene()
        {
            // Canvas 찾기 또는 생성
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
                canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }

            // InventoryUI GameObject 생성
            GameObject inventoryUIObj = new GameObject("InventoryUI");
            inventoryUIObj.transform.SetParent(canvas.transform, false);
            PigeonGame.UI.InventoryUI inventoryUI = inventoryUIObj.AddComponent<PigeonGame.UI.InventoryUI>();

            // 인벤토리 패널 생성
            GameObject panelObj = new GameObject("InventoryPanel");
            panelObj.transform.SetParent(inventoryUIObj.transform, false);
            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(600, 500);
            panelRect.anchoredPosition = Vector2.zero;

            Image panelImage = panelObj.AddComponent<Image>();
            panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);

            // 제목 텍스트
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(panelObj.transform, false);
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "인벤토리";
            titleText.fontSize = 28;
            titleText.color = Color.white;
            titleText.alignment = TextAlignmentOptions.Center;
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.9f);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.sizeDelta = Vector2.zero;
            titleRect.anchoredPosition = Vector2.zero;

            // 인벤토리 개수 텍스트
            GameObject countObj = new GameObject("InventoryCountText");
            countObj.transform.SetParent(panelObj.transform, false);
            TextMeshProUGUI countText = countObj.AddComponent<TextMeshProUGUI>();
            countText.text = "인벤토리: 0";
            countText.fontSize = 20;
            countText.color = Color.cyan;
            RectTransform countRect = countObj.GetComponent<RectTransform>();
            countRect.anchorMin = new Vector2(0, 0.85f);
            countRect.anchorMax = new Vector2(1, 0.9f);
            countRect.sizeDelta = Vector2.zero;
            countRect.anchoredPosition = Vector2.zero;

            // ScrollView 생성
            GameObject scrollViewObj = new GameObject("ScrollView");
            scrollViewObj.transform.SetParent(panelObj.transform, false);
            RectTransform scrollRect = scrollViewObj.AddComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0.05f, 0.15f);
            scrollRect.anchorMax = new Vector2(0.95f, 0.8f);
            scrollRect.sizeDelta = Vector2.zero;
            scrollRect.anchoredPosition = Vector2.zero;

            Image scrollImage = scrollViewObj.AddComponent<Image>();
            scrollImage.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
            ScrollRect scrollRectComponent = scrollViewObj.AddComponent<ScrollRect>();
            scrollRectComponent.horizontal = false;
            scrollRectComponent.vertical = true;

            // Viewport 생성
            GameObject viewportObj = new GameObject("Viewport");
            viewportObj.transform.SetParent(scrollViewObj.transform, false);
            RectTransform viewportRect = viewportObj.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.sizeDelta = Vector2.zero;
            viewportRect.anchoredPosition = Vector2.zero;

            Image viewportImage = viewportObj.AddComponent<Image>();
            viewportImage.color = new Color(1, 1, 1, 0);
            Mask mask = viewportObj.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            // Content 생성 (itemContainer)
            GameObject contentObj = new GameObject("Content");
            contentObj.transform.SetParent(viewportObj.transform, false);
            RectTransform contentRect = contentObj.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(0, 0);
            contentRect.anchoredPosition = Vector2.zero;

            VerticalLayoutGroup layoutGroup = contentObj.AddComponent<VerticalLayoutGroup>();
            layoutGroup.spacing = 10;
            layoutGroup.padding = new RectOffset(10, 10, 10, 10);
            layoutGroup.childControlHeight = false;
            layoutGroup.childControlWidth = true;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = false;

            ContentSizeFitter sizeFitter = contentObj.AddComponent<ContentSizeFitter>();
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRectComponent.content = contentRect;
            scrollRectComponent.viewport = viewportRect;

            // 전체 판매 버튼
            GameObject sellAllObj = new GameObject("SellAllButton");
            sellAllObj.transform.SetParent(panelObj.transform, false);
            Image sellAllImage = sellAllObj.AddComponent<Image>();
            sellAllImage.color = new Color(0.8f, 0.2f, 0.2f, 1f);
            Button sellAllButton = sellAllObj.AddComponent<Button>();
            RectTransform sellAllRect = sellAllObj.GetComponent<RectTransform>();
            sellAllRect.anchorMin = new Vector2(0.3f, 0.05f);
            sellAllRect.anchorMax = new Vector2(0.7f, 0.12f);
            sellAllRect.sizeDelta = Vector2.zero;
            sellAllRect.anchoredPosition = Vector2.zero;

            GameObject sellAllTextObj = new GameObject("Text");
            sellAllTextObj.transform.SetParent(sellAllObj.transform, false);
            TextMeshProUGUI sellAllText = sellAllTextObj.AddComponent<TextMeshProUGUI>();
            sellAllText.text = "전체 판매";
            sellAllText.fontSize = 24;
            sellAllText.color = Color.white;
            sellAllText.alignment = TextAlignmentOptions.Center;
            RectTransform sellAllTextRect = sellAllTextObj.GetComponent<RectTransform>();
            sellAllTextRect.anchorMin = Vector2.zero;
            sellAllTextRect.anchorMax = Vector2.one;
            sellAllTextRect.sizeDelta = Vector2.zero;
            sellAllTextRect.anchoredPosition = Vector2.zero;

            // 우측 상단 토글 버튼 생성
            GameObject toggleButtonObj = new GameObject("InventoryToggleButton");
            toggleButtonObj.transform.SetParent(canvas.transform, false);
            Image toggleButtonImage = toggleButtonObj.AddComponent<Image>();
            toggleButtonImage.color = new Color(0.3f, 0.5f, 0.8f, 0.9f);
            Button toggleButton = toggleButtonObj.AddComponent<Button>();
            RectTransform toggleButtonRect = toggleButtonObj.GetComponent<RectTransform>();
            toggleButtonRect.anchorMin = new Vector2(0.85f, 0.85f);
            toggleButtonRect.anchorMax = new Vector2(0.98f, 0.98f);
            toggleButtonRect.sizeDelta = Vector2.zero;
            toggleButtonRect.anchoredPosition = Vector2.zero;

            GameObject toggleButtonTextObj = new GameObject("Text");
            toggleButtonTextObj.transform.SetParent(toggleButtonObj.transform, false);
            TextMeshProUGUI toggleButtonText = toggleButtonTextObj.AddComponent<TextMeshProUGUI>();
            toggleButtonText.text = "인벤토리";
            toggleButtonText.fontSize = 20;
            toggleButtonText.color = Color.white;
            toggleButtonText.alignment = TextAlignmentOptions.Center;
            RectTransform toggleButtonTextRect = toggleButtonTextObj.GetComponent<RectTransform>();
            toggleButtonTextRect.anchorMin = Vector2.zero;
            toggleButtonTextRect.anchorMax = Vector2.one;
            toggleButtonTextRect.sizeDelta = Vector2.zero;
            toggleButtonTextRect.anchoredPosition = Vector2.zero;

            // InventoryUI 컴포넌트에 참조 할당
            var serializedObject = new SerializedObject(inventoryUI);
            serializedObject.FindProperty("inventoryPanel").objectReferenceValue = panelObj;
            serializedObject.FindProperty("itemContainer").objectReferenceValue = contentObj.transform;
            serializedObject.FindProperty("sellAllButton").objectReferenceValue = sellAllButton;
            serializedObject.FindProperty("inventoryCountText").objectReferenceValue = countText;
            serializedObject.FindProperty("toggleButton").objectReferenceValue = toggleButton;
            serializedObject.ApplyModifiedProperties();

            // 아이템 프리팹이 없으면 생성
            string itemPrefabPath = $"{prefabPath}/InventoryItem.prefab";
            if (!File.Exists(itemPrefabPath))
            {
                CreateInventoryItem();
            }
            GameObject itemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(itemPrefabPath);
            if (itemPrefab != null)
            {
                serializedObject.FindProperty("itemPrefab").objectReferenceValue = itemPrefab;
                serializedObject.ApplyModifiedProperties();
            }

            panelObj.SetActive(false);

            Selection.activeGameObject = inventoryUIObj;
            EditorUtility.DisplayDialog("완료", "인벤토리 UI가 생성되었습니다!\n\n설정:\n- I키로 열기/닫기\n- 아이템 프리팹은 자동으로 할당됩니다", "OK");
        }

        private void CreateTrapShopUIScene()
        {
            // Canvas 찾기 또는 생성
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
                canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }

            // TrapShopUI GameObject 생성
            GameObject shopUIObj = new GameObject("TrapShopUI");
            shopUIObj.transform.SetParent(canvas.transform, false);
            PigeonGame.UI.TrapShopUI shopUI = shopUIObj.AddComponent<PigeonGame.UI.TrapShopUI>();

            // 상점 패널 생성
            GameObject panelObj = new GameObject("ShopPanel");
            panelObj.transform.SetParent(shopUIObj.transform, false);
            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(600, 500);
            panelRect.anchoredPosition = Vector2.zero;

            Image panelImage = panelObj.AddComponent<Image>();
            panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);

            // 제목 텍스트
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(panelObj.transform, false);
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "덫 상점";
            titleText.fontSize = 28;
            titleText.color = Color.white;
            titleText.alignment = TextAlignmentOptions.Center;
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.9f);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.sizeDelta = Vector2.zero;
            titleRect.anchoredPosition = Vector2.zero;

            // ScrollView 생성
            GameObject scrollViewObj = new GameObject("ScrollView");
            scrollViewObj.transform.SetParent(panelObj.transform, false);
            RectTransform scrollRect = scrollViewObj.AddComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0.05f, 0.05f);
            scrollRect.anchorMax = new Vector2(0.95f, 0.85f);
            scrollRect.sizeDelta = Vector2.zero;
            scrollRect.anchoredPosition = Vector2.zero;

            Image scrollImage = scrollViewObj.AddComponent<Image>();
            scrollImage.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
            ScrollRect scrollRectComponent = scrollViewObj.AddComponent<ScrollRect>();
            scrollRectComponent.horizontal = false;
            scrollRectComponent.vertical = true;

            // Viewport 생성
            GameObject viewportObj = new GameObject("Viewport");
            viewportObj.transform.SetParent(scrollViewObj.transform, false);
            RectTransform viewportRect = viewportObj.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.sizeDelta = Vector2.zero;
            viewportRect.anchoredPosition = Vector2.zero;

            Image viewportImage = viewportObj.AddComponent<Image>();
            viewportImage.color = new Color(1, 1, 1, 0);
            Mask mask = viewportObj.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            // Content 생성 (trapContainer)
            GameObject contentObj = new GameObject("Content");
            contentObj.transform.SetParent(viewportObj.transform, false);
            RectTransform contentRect = contentObj.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(0, 0);
            contentRect.anchoredPosition = Vector2.zero;

            VerticalLayoutGroup layoutGroup = contentObj.AddComponent<VerticalLayoutGroup>();
            layoutGroup.spacing = 10;
            layoutGroup.padding = new RectOffset(10, 10, 10, 10);
            layoutGroup.childControlHeight = false;
            layoutGroup.childControlWidth = true;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = false;

            ContentSizeFitter sizeFitter = contentObj.AddComponent<ContentSizeFitter>();
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRectComponent.content = contentRect;
            scrollRectComponent.viewport = viewportRect;

            // 우측 상단 토글 버튼 생성 (인벤토리 버튼 아래에)
            GameObject toggleButtonObj = new GameObject("TrapShopToggleButton");
            toggleButtonObj.transform.SetParent(canvas.transform, false);
            Image toggleButtonImage = toggleButtonObj.AddComponent<Image>();
            toggleButtonImage.color = new Color(0.8f, 0.5f, 0.3f, 0.9f);
            Button toggleButton = toggleButtonObj.AddComponent<Button>();
            RectTransform toggleButtonRect = toggleButtonObj.GetComponent<RectTransform>();
            toggleButtonRect.anchorMin = new Vector2(0.85f, 0.72f);
            toggleButtonRect.anchorMax = new Vector2(0.98f, 0.85f);
            toggleButtonRect.sizeDelta = Vector2.zero;
            toggleButtonRect.anchoredPosition = Vector2.zero;

            GameObject toggleButtonTextObj = new GameObject("Text");
            toggleButtonTextObj.transform.SetParent(toggleButtonObj.transform, false);
            TextMeshProUGUI toggleButtonText = toggleButtonTextObj.AddComponent<TextMeshProUGUI>();
            toggleButtonText.text = "덫 상점";
            toggleButtonText.fontSize = 20;
            toggleButtonText.color = Color.white;
            toggleButtonText.alignment = TextAlignmentOptions.Center;
            RectTransform toggleButtonTextRect = toggleButtonTextObj.GetComponent<RectTransform>();
            toggleButtonTextRect.anchorMin = Vector2.zero;
            toggleButtonTextRect.anchorMax = Vector2.one;
            toggleButtonTextRect.sizeDelta = Vector2.zero;
            toggleButtonTextRect.anchoredPosition = Vector2.zero;

            // TrapShopUI 컴포넌트에 참조 할당
            var serializedObject = new SerializedObject(shopUI);
            serializedObject.FindProperty("shopPanel").objectReferenceValue = panelObj;
            serializedObject.FindProperty("trapContainer").objectReferenceValue = contentObj.transform;
            serializedObject.FindProperty("toggleButton").objectReferenceValue = toggleButton;
            serializedObject.ApplyModifiedProperties();

            // 덫 아이템 프리팹이 없으면 생성
            string itemPrefabPath = $"{prefabPath}/TrapShopItem.prefab";
            if (!File.Exists(itemPrefabPath))
            {
                CreateTrapShopItem();
            }
            GameObject itemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(itemPrefabPath);
            if (itemPrefab != null)
            {
                serializedObject.FindProperty("trapItemPrefab").objectReferenceValue = itemPrefab;
                serializedObject.ApplyModifiedProperties();
            }

            panelObj.SetActive(false);

            Selection.activeGameObject = shopUIObj;
            EditorUtility.DisplayDialog("완료", "덫 상점 UI가 생성되었습니다!\n\n설정:\n- T키로 열기/닫기\n- 덫 아이템 프리팹은 자동으로 할당됩니다", "OK");
        }
    }
}

