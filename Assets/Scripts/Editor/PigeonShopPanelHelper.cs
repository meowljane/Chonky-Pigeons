using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using PigeonGame.UI;

namespace PigeonGame.Editor
{
    public static class PigeonShopPanelHelper
    {
        [MenuItem("Tools/Create Pigeon Shop Panel", false, 2)]
        public static void CreatePigeonShopPanel()
        {
            // Canvas 찾기 또는 생성
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
                
                Debug.Log("Canvas가 없어서 새로 생성했습니다.");
            }

            // 이미 PigeonShopPanel이 있는지 확인
            Transform existingPanel = canvas.transform.Find("PigeonShopPanel");
            if (existingPanel != null)
            {
                Debug.LogWarning("PigeonShopPanel이 이미 존재합니다!");
                Selection.activeGameObject = existingPanel.gameObject;
                return;
            }

            // PigeonShopPanel 생성
            GameObject panelObj = CreatePanel(canvas.transform);
            
            // PigeonShopUI 컴포넌트 추가 및 설정
            PigeonShopUI shopUI = panelObj.AddComponent<PigeonShopUI>();
            SetupPigeonShopUI(shopUI, panelObj);

            Selection.activeGameObject = panelObj;
            Undo.RegisterCreatedObjectUndo(panelObj, "Create Pigeon Shop Panel");
            
            Debug.Log("PigeonShopPanel이 생성되었습니다! Item Prefab을 할당해주세요.");
        }

        private static GameObject CreatePanel(Transform parent)
        {
            // 메인 패널
            GameObject panelObj = new GameObject("PigeonShopPanel");
            panelObj.transform.SetParent(parent, false);
            
            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.05f, 0.15f);
            panelRect.anchorMax = new Vector2(0.95f, 0.85f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = Vector2.zero;

            // 배경 Image
            Image panelImage = panelObj.AddComponent<Image>();
            panelImage.color = new Color(0.2f, 0.2f, 0.2f, 0.95f);

            // 헤더 영역
            GameObject headerObj = CreateHeader(panelObj.transform);
            
            // ScrollRect 영역
            GameObject scrollRectObj = CreateScrollRect(panelObj.transform);
            
            // 닫기 버튼
            GameObject closeButtonObj = CreateCloseButton(panelObj.transform);
            
            // 인벤토리 개수 텍스트
            GameObject countTextObj = CreateCountText(panelObj.transform);

            return panelObj;
        }

        private static GameObject CreateHeader(Transform parent)
        {
            GameObject headerObj = new GameObject("Header");
            headerObj.transform.SetParent(parent, false);
            
            RectTransform headerRect = headerObj.AddComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0, 1);
            headerRect.anchorMax = new Vector2(1, 1);
            headerRect.pivot = new Vector2(0.5f, 1);
            headerRect.anchoredPosition = Vector2.zero;
            headerRect.sizeDelta = new Vector2(0, 60);

            Image headerImage = headerObj.AddComponent<Image>();
            headerImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);

            // 제목 텍스트
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(headerObj.transform, false);
            
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = Vector2.zero;
            titleRect.anchorMax = Vector2.one;
            titleRect.sizeDelta = Vector2.zero;
            titleRect.anchoredPosition = Vector2.zero;

            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "비둘기 상점";
            titleText.fontSize = 24;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = Color.white;

            return headerObj;
        }

        private static GameObject CreateScrollRect(Transform parent)
        {
            // ScrollRect GameObject
            GameObject scrollRectObj = new GameObject("ScrollRect");
            scrollRectObj.transform.SetParent(parent, false);
            
            RectTransform scrollRect = scrollRectObj.AddComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0, 0);
            scrollRect.anchorMax = new Vector2(1, 1);
            scrollRect.anchoredPosition = Vector2.zero;
            scrollRect.sizeDelta = new Vector2(-20, -80); // 여백
            scrollRect.anchoredPosition = new Vector2(0, -30);

            ScrollRect scrollRectComponent = scrollRectObj.AddComponent<ScrollRect>();
            scrollRectComponent.horizontal = false;
            scrollRectComponent.vertical = true;
            scrollRectComponent.movementType = ScrollRect.MovementType.Elastic;
            scrollRectComponent.elasticity = 0.1f;
            scrollRectComponent.inertia = true;
            scrollRectComponent.decelerationRate = 0.135f;
            scrollRectComponent.scrollSensitivity = 1f;

            // Viewport
            GameObject viewportObj = new GameObject("Viewport");
            viewportObj.transform.SetParent(scrollRectObj.transform, false);
            
            RectTransform viewportRect = viewportObj.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.sizeDelta = Vector2.zero;
            viewportRect.anchoredPosition = Vector2.zero;

            Image viewportImage = viewportObj.AddComponent<Image>();
            viewportImage.color = new Color(0, 0, 0, 0); // 투명

            Mask mask = viewportObj.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            scrollRectComponent.viewport = viewportRect;

            // Content
            GameObject contentObj = new GameObject("Content");
            contentObj.transform.SetParent(viewportObj.transform, false);
            
            RectTransform contentRect = contentObj.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(0, 0);

            VerticalLayoutGroup layoutGroup = contentObj.AddComponent<VerticalLayoutGroup>();
            layoutGroup.spacing = 10f;
            layoutGroup.padding = new RectOffset(10, 10, 10, 10);
            layoutGroup.childAlignment = TextAnchor.UpperCenter;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = false;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = false;

            ContentSizeFitter sizeFitter = contentObj.AddComponent<ContentSizeFitter>();
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRectComponent.content = contentRect;

            return scrollRectObj;
        }

        private static GameObject CreateCloseButton(Transform parent)
        {
            GameObject closeButtonObj = new GameObject("CloseButton");
            closeButtonObj.transform.SetParent(parent, false);
            
            RectTransform closeRect = closeButtonObj.AddComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(1, 1);
            closeRect.anchorMax = new Vector2(1, 1);
            closeRect.pivot = new Vector2(1, 1);
            closeRect.anchoredPosition = new Vector2(-10, -10);
            closeRect.sizeDelta = new Vector2(40, 40);

            Image closeImage = closeButtonObj.AddComponent<Image>();
            closeImage.color = new Color(0.8f, 0.2f, 0.2f, 1f);

            Button closeButton = closeButtonObj.AddComponent<Button>();

            // X 텍스트
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(closeButtonObj.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;

            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = "X";
            text.fontSize = 20;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;

            return closeButtonObj;
        }

        private static GameObject CreateCountText(Transform parent)
        {
            GameObject countTextObj = new GameObject("InventoryCountText");
            countTextObj.transform.SetParent(parent, false);
            
            RectTransform countRect = countTextObj.AddComponent<RectTransform>();
            countRect.anchorMin = new Vector2(0, 0);
            countRect.anchorMax = new Vector2(1, 0);
            countRect.pivot = new Vector2(0.5f, 0);
            countRect.anchoredPosition = new Vector2(0, 10);
            countRect.sizeDelta = new Vector2(0, 30);

            TextMeshProUGUI countText = countTextObj.AddComponent<TextMeshProUGUI>();
            countText.text = "인벤토리: 0";
            countText.fontSize = 16;
            countText.alignment = TextAlignmentOptions.Center;
            countText.color = Color.white;

            return countTextObj;
        }

        private static void SetupPigeonShopUI(PigeonShopUI shopUI, GameObject panelObj)
        {
            SerializedObject serializedUI = new SerializedObject(shopUI);
            
            // Shop Panel 할당
            serializedUI.FindProperty("shopPanel").objectReferenceValue = panelObj;
            
            // Item Container 할당 (Content)
            Transform contentTransform = panelObj.transform.Find("ScrollRect/Viewport/Content");
            if (contentTransform != null)
            {
                serializedUI.FindProperty("itemContainer").objectReferenceValue = contentTransform;
            }
            
            // Close Button 할당
            Transform closeButtonTransform = panelObj.transform.Find("CloseButton");
            if (closeButtonTransform != null)
            {
                serializedUI.FindProperty("closeButton").objectReferenceValue = 
                    closeButtonTransform.GetComponent<Button>();
            }
            
            // Inventory Count Text 할당
            Transform countTextTransform = panelObj.transform.Find("InventoryCountText");
            if (countTextTransform != null)
            {
                serializedUI.FindProperty("inventoryCountText").objectReferenceValue = 
                    countTextTransform.GetComponent<TextMeshProUGUI>();
            }
            
            serializedUI.ApplyModifiedProperties();
        }
    }
}
