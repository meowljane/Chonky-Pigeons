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

        [MenuItem("PigeonGame/UI Helper/Create TrapPlacementUI")]
        public static void CreateTrapPlacementUI()
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

            // TrapPlacementUI GameObject 생성
            GameObject trapPlacementObj = new GameObject("TrapPlacementUI");
            trapPlacementObj.transform.SetParent(canvas.transform, false);
            
            RectTransform trapPlacementRect = trapPlacementObj.AddComponent<RectTransform>();
            trapPlacementRect.anchorMin = Vector2.zero;
            trapPlacementRect.anchorMax = Vector2.one;
            trapPlacementRect.sizeDelta = Vector2.zero;
            trapPlacementRect.anchoredPosition = Vector2.zero;

            TrapPlacementUI trapPlacementUI = trapPlacementObj.AddComponent<TrapPlacementUI>();

            // 우측 하단 버튼 영역 생성
            GameObject buttonAreaObj = new GameObject("ButtonArea");
            buttonAreaObj.transform.SetParent(trapPlacementObj.transform, false);
            
            RectTransform buttonAreaRect = buttonAreaObj.AddComponent<RectTransform>();
            buttonAreaRect.anchorMin = new Vector2(1f, 0f);
            buttonAreaRect.anchorMax = new Vector2(1f, 0f);
            buttonAreaRect.pivot = new Vector2(1f, 0f);
            buttonAreaRect.anchoredPosition = new Vector2(-10f, 10f);
            buttonAreaRect.sizeDelta = new Vector2(200f, 100f);

            HorizontalLayoutGroup buttonLayout = buttonAreaObj.AddComponent<HorizontalLayoutGroup>();
            buttonLayout.spacing = 10f;
            buttonLayout.padding = new RectOffset(0, 0, 0, 0);
            buttonLayout.childControlHeight = false;
            buttonLayout.childControlWidth = false;
            buttonLayout.childForceExpandWidth = false;
            buttonLayout.childForceExpandHeight = false;

            // 덫 설치 버튼 생성
            GameObject trapButtonObj = CreateButton("TrapPlacementButton", "덫 설치", buttonAreaObj.transform);
            Button trapButton = trapButtonObj.GetComponent<Button>();

            // 상호작용 버튼 생성
            GameObject interactionButtonObj = CreateButton("InteractionButton", "상호작용", buttonAreaObj.transform);
            Button interactionButton = interactionButtonObj.GetComponent<Button>();

            // 덫 선택 패널 생성
            GameObject selectionPanelObj = new GameObject("TrapSelectionPanel");
            selectionPanelObj.transform.SetParent(trapPlacementObj.transform, false);
            selectionPanelObj.SetActive(false);
            
            RectTransform selectionPanelRect = selectionPanelObj.AddComponent<RectTransform>();
            selectionPanelRect.anchorMin = new Vector2(0.5f, 0.5f);
            selectionPanelRect.anchorMax = new Vector2(0.5f, 0.5f);
            selectionPanelRect.pivot = new Vector2(0.5f, 0.5f);
            selectionPanelRect.anchoredPosition = Vector2.zero;
            selectionPanelRect.sizeDelta = new Vector2(800f, 600f);

            Image selectionPanelImage = selectionPanelObj.AddComponent<Image>();
            selectionPanelImage.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

            VerticalLayoutGroup panelLayout = selectionPanelObj.AddComponent<VerticalLayoutGroup>();
            panelLayout.spacing = 10f;
            panelLayout.padding = new RectOffset(10, 10, 10, 10);
            panelLayout.childControlHeight = false;
            panelLayout.childControlWidth = true;
            panelLayout.childForceExpandWidth = true;
            panelLayout.childForceExpandHeight = false;

            // 상단: 제목과 닫기 버튼
            GameObject headerObj = new GameObject("Header");
            headerObj.transform.SetParent(selectionPanelObj.transform, false);
            headerObj.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, 40f);

            HorizontalLayoutGroup headerLayout = headerObj.AddComponent<HorizontalLayoutGroup>();
            headerLayout.spacing = 10f;
            headerLayout.childControlHeight = false;
            headerLayout.childControlWidth = true;
            headerLayout.childForceExpandWidth = true;

            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(headerObj.transform, false);
            titleObj.AddComponent<RectTransform>().sizeDelta = new Vector2(200f, 30f);
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "덫 선택";
            titleText.fontSize = 24f;
            titleText.color = Color.white;

            GameObject closeButtonObj = CreateButton("CloseButton", "X", headerObj.transform);
            closeButtonObj.GetComponent<RectTransform>().sizeDelta = new Vector2(40f, 40f);
            Button closeButton = closeButtonObj.GetComponent<Button>();

            // 상단: 정보 표시 영역 (Terrain, 선택한 덫, 비둘기 정보)
            GameObject infoAreaObj = new GameObject("InfoArea");
            infoAreaObj.transform.SetParent(selectionPanelObj.transform, false);
            RectTransform infoAreaRect = infoAreaObj.AddComponent<RectTransform>();
            infoAreaRect.sizeDelta = new Vector2(0f, 120f);

            VerticalLayoutGroup infoLayout = infoAreaObj.AddComponent<VerticalLayoutGroup>();
            infoLayout.spacing = 5f;
            infoLayout.padding = new RectOffset(10, 10, 10, 10);
            infoLayout.childControlHeight = false;
            infoLayout.childControlWidth = true;
            infoLayout.childForceExpandWidth = true;

            Image infoBg = infoAreaObj.AddComponent<Image>();
            infoBg.color = new Color(0.15f, 0.15f, 0.15f, 1f);

            // Terrain 표시
            TextMeshProUGUI currentTerrainText = CreateText("CurrentTerrainText", "Terrain: sand", infoAreaObj.transform, 18f);
            currentTerrainText.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 25f);
            currentTerrainText.color = Color.cyan;

            // 선택한 덫 이름
            TextMeshProUGUI selectedTrapNameText = CreateText("SelectedTrapNameText", "선택한 덫: 없음", infoAreaObj.transform, 18f);
            selectedTrapNameText.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 25f);
            selectedTrapNameText.color = Color.yellow;

            // Terrain 비둘기 정보
            TextMeshProUGUI terrainPigeonsText = CreateText("TerrainPigeonsText", "이 Terrain을 좋아하는 비둘기: 없음", infoAreaObj.transform, 14f);
            terrainPigeonsText.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 30f);
            terrainPigeonsText.color = Color.cyan;

            // 덫 비둘기 정보
            TextMeshProUGUI trapPigeonsText = CreateText("TrapPigeonsText", "이 덫을 좋아하는 비둘기: 없음", infoAreaObj.transform, 14f);
            trapPigeonsText.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 30f);
            trapPigeonsText.color = Color.yellow;

            // 중간: 덫 그리드 컨테이너
            GameObject gridContainerObj = new GameObject("TrapGridContainer");
            gridContainerObj.transform.SetParent(selectionPanelObj.transform, false);
            RectTransform gridContainerRect = gridContainerObj.AddComponent<RectTransform>();
            gridContainerRect.sizeDelta = new Vector2(0f, 350f);

            GridLayoutGroup gridLayout = gridContainerObj.AddComponent<GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(120f, 80f);
            gridLayout.spacing = new Vector2(10f, 10f);
            gridLayout.padding = new RectOffset(10, 10, 10, 10);
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 5;

            // 덫 아이템 프리팹 생성 (간단한 버튼)
            GameObject trapItemPrefabObj = CreateButton("TrapItem", "덫", null);
            RectTransform trapItemRect = trapItemPrefabObj.GetComponent<RectTransform>();
            trapItemRect.sizeDelta = new Vector2(120f, 80f);
            
            TextMeshProUGUI trapItemText = trapItemPrefabObj.GetComponentInChildren<TextMeshProUGUI>();
            if (trapItemText != null)
            {
                trapItemText.text = "덫 이름";
                trapItemText.fontSize = 14f;
            }

            // 프리팹으로 저장
            string prefabPath = "Assets/Prefabs/UI/TrapItem.prefab";
            string prefabDirectory = System.IO.Path.GetDirectoryName(prefabPath);
            if (!System.IO.Directory.Exists(prefabDirectory))
            {
                System.IO.Directory.CreateDirectory(prefabDirectory);
            }
            GameObject trapItemPrefab = PrefabUtility.SaveAsPrefabAsset(trapItemPrefabObj, prefabPath);
            DestroyImmediate(trapItemPrefabObj);

            // 하단: 모이 수량 입력과 가격 표시
            GameObject bottomAreaObj = new GameObject("BottomArea");
            bottomAreaObj.transform.SetParent(selectionPanelObj.transform, false);
            RectTransform bottomAreaRect = bottomAreaObj.AddComponent<RectTransform>();
            bottomAreaRect.sizeDelta = new Vector2(0f, 80f);

            HorizontalLayoutGroup bottomLayout = bottomAreaObj.AddComponent<HorizontalLayoutGroup>();
            bottomLayout.spacing = 20f;
            bottomLayout.padding = new RectOffset(10, 10, 10, 10);
            bottomLayout.childControlHeight = false;
            bottomLayout.childControlWidth = false;
            bottomLayout.childForceExpandWidth = true;
            bottomLayout.childForceExpandHeight = false;

            // 모이 수량 입력 영역 (버튼 + 입력 필드)
            GameObject feedAreaObj = new GameObject("FeedAmountArea");
            feedAreaObj.transform.SetParent(bottomAreaObj.transform, false);
            RectTransform feedAreaRect = feedAreaObj.AddComponent<RectTransform>();
            feedAreaRect.sizeDelta = new Vector2(250f, 40f);

            HorizontalLayoutGroup feedAreaLayout = feedAreaObj.AddComponent<HorizontalLayoutGroup>();
            feedAreaLayout.spacing = 5f;
            feedAreaLayout.padding = new RectOffset(0, 0, 0, 0);
            feedAreaLayout.childControlHeight = false;
            feedAreaLayout.childControlWidth = false;
            feedAreaLayout.childForceExpandWidth = false;
            feedAreaLayout.childForceExpandHeight = false;

            // 감소 버튼 (←)
            GameObject feedDecreaseBtnObj = CreateButton("FeedDecreaseButton", "←", feedAreaObj.transform);
            feedDecreaseBtnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(40f, 40f);
            Button feedDecreaseButton = feedDecreaseBtnObj.GetComponent<Button>();

            // 모이 수량 입력 필드
            GameObject feedInputObj = new GameObject("FeedAmountInput");
            feedInputObj.transform.SetParent(feedAreaObj.transform, false);
            RectTransform feedInputRect = feedInputObj.AddComponent<RectTransform>();
            feedInputRect.sizeDelta = new Vector2(150f, 40f);

            Image feedInputBg = feedInputObj.AddComponent<Image>();
            feedInputBg.color = new Color(0.3f, 0.3f, 0.3f, 1f);

            GameObject feedLabelObj = new GameObject("Text");
            feedLabelObj.transform.SetParent(feedInputObj.transform, false);
            RectTransform feedLabelRect = feedLabelObj.AddComponent<RectTransform>();
            feedLabelRect.anchorMin = Vector2.zero;
            feedLabelRect.anchorMax = Vector2.one;
            feedLabelRect.sizeDelta = Vector2.zero;

            TextMeshProUGUI feedLabel = feedLabelObj.AddComponent<TextMeshProUGUI>();
            feedLabel.text = "20";
            feedLabel.fontSize = 16f;
            feedLabel.color = Color.white;
            feedLabel.alignment = TextAlignmentOptions.Center;

            TMPro.TMP_InputField feedAmountInput = feedInputObj.AddComponent<TMPro.TMP_InputField>();
            feedAmountInput.textComponent = feedLabel;
            feedAmountInput.text = "20";
            feedAmountInput.contentType = TMPro.TMP_InputField.ContentType.IntegerNumber;
            feedAmountInput.characterLimit = 4; // 최대 1000까지 입력 가능

            // 증가 버튼 (→)
            GameObject feedIncreaseBtnObj = CreateButton("FeedIncreaseButton", "→", feedAreaObj.transform);
            feedIncreaseBtnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(40f, 40f);
            Button feedIncreaseButton = feedIncreaseBtnObj.GetComponent<Button>();
            feedAmountInput.characterLimit = 4; // 최대 1000까지 입력 가능

            // 총 가격 표시
            GameObject priceObj = new GameObject("TotalPriceText");
            priceObj.transform.SetParent(bottomAreaObj.transform, false);
            RectTransform priceRect = priceObj.AddComponent<RectTransform>();
            priceRect.sizeDelta = new Vector2(250f, 60f);

            TextMeshProUGUI totalPriceText = priceObj.AddComponent<TextMeshProUGUI>();
            totalPriceText.text = "총 비용: 0 골드 / 보유 골드: 0 골드";
            totalPriceText.fontSize = 18f;
            totalPriceText.color = Color.white;
            totalPriceText.alignment = TextAlignmentOptions.Left;

            // 설치 버튼
            GameObject installButtonObj = CreateButton("InstallButton", "설치", bottomAreaObj.transform);
            installButtonObj.GetComponent<RectTransform>().sizeDelta = new Vector2(120f, 50f);
            Button installButton = installButtonObj.GetComponent<Button>();

            // TrapPlacementUI 컴포넌트에 참조 할당
            SerializedObject serializedObject = new SerializedObject(trapPlacementUI);
            
            // Buttons
            serializedObject.FindProperty("trapPlacementButton").objectReferenceValue = trapButton;
            serializedObject.FindProperty("interactionButton").objectReferenceValue = interactionButton;
            
            // Trap Selection Panel
            serializedObject.FindProperty("trapSelectionPanel").objectReferenceValue = selectionPanelObj;
            serializedObject.FindProperty("trapGridContainer").objectReferenceValue = gridContainerRect;
            serializedObject.FindProperty("trapItemPrefab").objectReferenceValue = trapItemPrefab;
            serializedObject.FindProperty("closeButton").objectReferenceValue = closeButton;
            
            // Info Display
            serializedObject.FindProperty("currentTerrainText").objectReferenceValue = currentTerrainText;
            serializedObject.FindProperty("selectedTrapNameText").objectReferenceValue = selectedTrapNameText;
            serializedObject.FindProperty("terrainPigeonsText").objectReferenceValue = terrainPigeonsText;
            serializedObject.FindProperty("trapPigeonsText").objectReferenceValue = trapPigeonsText;
            
            // Bottom Controls
            serializedObject.FindProperty("feedAmountInput").objectReferenceValue = feedAmountInput;
            serializedObject.FindProperty("feedDecreaseButton").objectReferenceValue = feedDecreaseButton;
            serializedObject.FindProperty("feedIncreaseButton").objectReferenceValue = feedIncreaseButton;
            serializedObject.FindProperty("totalPriceText").objectReferenceValue = totalPriceText;
            serializedObject.FindProperty("installButton").objectReferenceValue = installButton;
            
            serializedObject.ApplyModifiedProperties();

            Selection.activeGameObject = trapPlacementObj;
            EditorUtility.DisplayDialog("완료", 
                "TrapPlacementUI가 생성되었습니다!\n\n" +
                "구조:\n" +
                "1. 상단: Terrain 이름 + 선택한 덫 이름 + 각각 좋아하는 비둘기 표시\n" +
                "2. 중간: 덫 선택 그리드 (5열)\n" +
                "3. 하단: 모이 수량 입력 + 총 가격 표시 + 설치 버튼\n\n" +
                "덫 아이템 프리팹: Assets/Prefabs/UI/TrapItem.prefab\n\n" +
                "참고: TrapPlacer 컴포넌트가 씬에 있어야 합니다.", 
                "확인");
        }

        private static GameObject CreateButton(string name, string text, Transform parent)
        {
            GameObject buttonObj = new GameObject(name);
            if (parent != null)
                buttonObj.transform.SetParent(parent, false);
            
            RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(100f, 40f);

            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.6f, 0.2f, 1f);

            Button button = buttonObj.AddComponent<Button>();

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;

            TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = text;
            buttonText.fontSize = 16f;
            buttonText.color = Color.white;
            buttonText.alignment = TextAlignmentOptions.Center;

            return buttonObj;
        }

        private static TextMeshProUGUI CreateText(string name, string text, Transform parent, float fontSize)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.sizeDelta = new Vector2(0f, 30f);

            TextMeshProUGUI textComponent = textObj.AddComponent<TextMeshProUGUI>();
            textComponent.text = text;
            textComponent.fontSize = fontSize;
            textComponent.color = Color.white;
            textComponent.alignment = TextAlignmentOptions.Left;

            return textComponent;
        }

        [MenuItem("PigeonGame/UI Helper/Create ExhibitionUI")]
        public static void CreateExhibitionUI()
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

            // ExhibitionUI GameObject 생성
            GameObject exhibitionObj = new GameObject("ExhibitionUI");
            exhibitionObj.transform.SetParent(canvas.transform, false);
            
            RectTransform exhibitionRect = exhibitionObj.AddComponent<RectTransform>();
            exhibitionRect.anchorMin = Vector2.zero;
            exhibitionRect.anchorMax = Vector2.one;
            exhibitionRect.sizeDelta = Vector2.zero;
            exhibitionRect.anchoredPosition = Vector2.zero;

            ExhibitionUI exhibitionUI = exhibitionObj.AddComponent<ExhibitionUI>();

            // 전시관 패널 생성
            GameObject panelObj = new GameObject("ExhibitionPanel");
            panelObj.transform.SetParent(exhibitionObj.transform, false);
            panelObj.SetActive(false);
            
            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = new Vector2(1000f, 700f);

            Image panelImage = panelObj.AddComponent<Image>();
            panelImage.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

            VerticalLayoutGroup panelLayout = panelObj.AddComponent<VerticalLayoutGroup>();
            panelLayout.spacing = 10f;
            panelLayout.padding = new RectOffset(20, 20, 20, 20);
            panelLayout.childControlHeight = false;
            panelLayout.childControlWidth = true;
            panelLayout.childForceExpandWidth = true;

            // 헤더 (제목, 닫기 버튼)
            GameObject headerObj = new GameObject("Header");
            headerObj.transform.SetParent(panelObj.transform, false);
            headerObj.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, 50f);

            HorizontalLayoutGroup headerLayout = headerObj.AddComponent<HorizontalLayoutGroup>();
            headerLayout.spacing = 10f;
            headerLayout.childControlHeight = false;
            headerLayout.childControlWidth = true;
            headerLayout.childForceExpandWidth = true;

            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(headerObj.transform, false);
            titleObj.AddComponent<RectTransform>().sizeDelta = new Vector2(200f, 40f);
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "전시관";
            titleText.fontSize = 24f;
            titleText.color = Color.white;

            GameObject closeBtnObj = CreateButton("CloseButton", "X", headerObj.transform);
            closeBtnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(40f, 40f);
            Button closeButton = closeBtnObj.GetComponent<Button>();

            // 메인 컨텐츠 영역 (인벤토리와 전시관 나란히)
            GameObject contentObj = new GameObject("Content");
            contentObj.transform.SetParent(panelObj.transform, false);
            RectTransform contentRect = contentObj.AddComponent<RectTransform>();
            contentRect.sizeDelta = new Vector2(0f, 600f);

            HorizontalLayoutGroup contentLayout = contentObj.AddComponent<HorizontalLayoutGroup>();
            contentLayout.spacing = 20f;
            contentLayout.padding = new RectOffset(10, 10, 10, 10);
            contentLayout.childControlHeight = false;
            contentLayout.childControlWidth = true;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;

            // 인벤토리 영역
            GameObject inventoryAreaObj = new GameObject("InventoryArea");
            inventoryAreaObj.transform.SetParent(contentObj.transform, false);
            RectTransform inventoryAreaRect = inventoryAreaObj.AddComponent<RectTransform>();
            inventoryAreaRect.sizeDelta = new Vector2(450f, 0f);

            VerticalLayoutGroup inventoryAreaLayout = inventoryAreaObj.AddComponent<VerticalLayoutGroup>();
            inventoryAreaLayout.spacing = 10f;
            inventoryAreaLayout.childControlHeight = false;
            inventoryAreaLayout.childControlWidth = true;
            inventoryAreaLayout.childForceExpandWidth = true;

            Image inventoryBg = inventoryAreaObj.AddComponent<Image>();
            inventoryBg.color = new Color(0.15f, 0.15f, 0.15f, 1f);

            GameObject inventoryTitleObj = new GameObject("InventoryTitle");
            inventoryTitleObj.transform.SetParent(inventoryAreaObj.transform, false);
            inventoryTitleObj.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, 30f);
            TextMeshProUGUI inventoryTitleText = inventoryTitleObj.AddComponent<TextMeshProUGUI>();
            inventoryTitleText.text = "인벤토리";
            inventoryTitleText.fontSize = 20f;
            inventoryTitleText.color = Color.cyan;

            GameObject inventoryCountObj = new GameObject("InventoryCountText");
            inventoryCountObj.transform.SetParent(inventoryAreaObj.transform, false);
            inventoryCountObj.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, 25f);
            TextMeshProUGUI inventoryCountText = inventoryCountObj.AddComponent<TextMeshProUGUI>();
            inventoryCountText.text = "인벤토리: 0";
            inventoryCountText.fontSize = 16f;
            inventoryCountText.color = Color.white;

            GameObject inventoryGridObj = new GameObject("InventoryGrid");
            inventoryGridObj.transform.SetParent(inventoryAreaObj.transform, false);
            RectTransform inventoryGridRect = inventoryGridObj.AddComponent<RectTransform>();
            inventoryGridRect.sizeDelta = new Vector2(0f, 545f);

            // 전시관 영역
            GameObject exhibitionAreaObj = new GameObject("ExhibitionArea");
            exhibitionAreaObj.transform.SetParent(contentObj.transform, false);
            RectTransform exhibitionAreaRect = exhibitionAreaObj.AddComponent<RectTransform>();
            exhibitionAreaRect.sizeDelta = new Vector2(450f, 0f);

            VerticalLayoutGroup exhibitionAreaLayout = exhibitionAreaObj.AddComponent<VerticalLayoutGroup>();
            exhibitionAreaLayout.spacing = 10f;
            exhibitionAreaLayout.childControlHeight = false;
            exhibitionAreaLayout.childControlWidth = true;
            exhibitionAreaLayout.childForceExpandWidth = true;

            Image exhibitionBg = exhibitionAreaObj.AddComponent<Image>();
            exhibitionBg.color = new Color(0.15f, 0.15f, 0.15f, 1f);

            GameObject exhibitionTitleObj = new GameObject("ExhibitionTitle");
            exhibitionTitleObj.transform.SetParent(exhibitionAreaObj.transform, false);
            exhibitionTitleObj.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, 30f);
            TextMeshProUGUI exhibitionTitleText = exhibitionTitleObj.AddComponent<TextMeshProUGUI>();
            exhibitionTitleText.text = "전시관";
            exhibitionTitleText.fontSize = 20f;
            exhibitionTitleText.color = Color.yellow;

            GameObject exhibitionCountObj = new GameObject("ExhibitionCountText");
            exhibitionCountObj.transform.SetParent(exhibitionAreaObj.transform, false);
            exhibitionCountObj.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, 25f);
            TextMeshProUGUI exhibitionCountText = exhibitionCountObj.AddComponent<TextMeshProUGUI>();
            exhibitionCountText.text = "전시관: 0/50";
            exhibitionCountText.fontSize = 16f;
            exhibitionCountText.color = Color.white;

            GameObject exhibitionGridObj = new GameObject("ExhibitionGrid");
            exhibitionGridObj.transform.SetParent(exhibitionAreaObj.transform, false);
            RectTransform exhibitionGridRect = exhibitionGridObj.AddComponent<RectTransform>();
            exhibitionGridRect.sizeDelta = new Vector2(0f, 545f);

            // 슬롯 프리팹 생성 (인벤토리와 동일)
            GameObject slotPrefabObj = new GameObject("SlotPrefab");
            RectTransform slotRect = slotPrefabObj.AddComponent<RectTransform>();
            slotRect.sizeDelta = new Vector2(100f, 100f);

            Image slotBg = slotPrefabObj.AddComponent<Image>();
            slotBg.color = new Color(0.3f, 0.3f, 0.3f, 1f);

            Button slotButton = slotPrefabObj.AddComponent<Button>();

            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(slotPrefabObj.transform, false);
            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.1f, 0.3f);
            iconRect.anchorMax = new Vector2(0.9f, 0.9f);
            iconRect.sizeDelta = Vector2.zero;
            Image iconImage = iconObj.AddComponent<Image>();

            GameObject nameObj = new GameObject("NameText");
            nameObj.transform.SetParent(slotPrefabObj.transform, false);
            RectTransform nameRect = nameObj.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0f, 0f);
            nameRect.anchorMax = new Vector2(1f, 0.3f);
            nameRect.sizeDelta = Vector2.zero;
            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = "비둘기";
            nameText.fontSize = 12f;
            nameText.color = Color.white;
            nameText.alignment = TextAlignmentOptions.Center;

            string prefabPath = "Assets/Prefabs/UI/ExhibitionSlot.prefab";
            string prefabDirectory = System.IO.Path.GetDirectoryName(prefabPath);
            if (!System.IO.Directory.Exists(prefabDirectory))
            {
                System.IO.Directory.CreateDirectory(prefabDirectory);
            }
            GameObject slotPrefab = PrefabUtility.SaveAsPrefabAsset(slotPrefabObj, prefabPath);
            DestroyImmediate(slotPrefabObj);

            // 상세 정보 패널
            GameObject detailPanelObj = new GameObject("DetailPanel");
            detailPanelObj.transform.SetParent(exhibitionObj.transform, false);
            detailPanelObj.SetActive(false);
            
            RectTransform detailPanelRect = detailPanelObj.AddComponent<RectTransform>();
            detailPanelRect.anchorMin = new Vector2(0.5f, 0.5f);
            detailPanelRect.anchorMax = new Vector2(0.5f, 0.5f);
            detailPanelRect.pivot = new Vector2(0.5f, 0.5f);
            detailPanelRect.anchoredPosition = Vector2.zero;
            detailPanelRect.sizeDelta = new Vector2(400f, 500f);

            Image detailPanelImage = detailPanelObj.AddComponent<Image>();
            detailPanelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);

            VerticalLayoutGroup detailLayout = detailPanelObj.AddComponent<VerticalLayoutGroup>();
            detailLayout.spacing = 10f;
            detailLayout.padding = new RectOffset(20, 20, 20, 20);
            detailLayout.childControlHeight = false;
            detailLayout.childControlWidth = true;
            detailLayout.childForceExpandWidth = true;

            GameObject detailCloseBtnObj = CreateButton("CloseButton", "X", detailPanelObj.transform);
            detailCloseBtnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(30f, 30f);
            RectTransform detailCloseBtnRect = detailCloseBtnObj.GetComponent<RectTransform>();
            detailCloseBtnRect.anchorMin = new Vector2(1f, 1f);
            detailCloseBtnRect.anchorMax = new Vector2(1f, 1f);
            detailCloseBtnRect.pivot = new Vector2(1f, 1f);
            detailCloseBtnRect.anchoredPosition = new Vector2(-5f, -5f);
            Button detailCloseButton = detailCloseBtnObj.GetComponent<Button>();

            GameObject detailIconObj = new GameObject("IconImage");
            detailIconObj.transform.SetParent(detailPanelObj.transform, false);
            detailIconObj.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, 150f);
            Image detailIconImage = detailIconObj.AddComponent<Image>();

            TextMeshProUGUI detailNameText = CreateText("NameText", "비둘기 이름", detailPanelObj.transform, 20f);
            detailNameText.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 30f);

            TextMeshProUGUI detailObesityText = CreateText("ObesityText", "비만도: 0", detailPanelObj.transform, 18f);
            detailObesityText.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 25f);

            TextMeshProUGUI detailPriceText = CreateText("PriceText", "가격: 0", detailPanelObj.transform, 18f);
            detailPriceText.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 25f);

            TextMeshProUGUI detailRarityText = CreateText("RarityText", "희귀도: 0", detailPanelObj.transform, 18f);
            detailRarityText.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 25f);

            GameObject buttonAreaObj = new GameObject("ButtonArea");
            buttonAreaObj.transform.SetParent(detailPanelObj.transform, false);
            buttonAreaObj.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, 50f);

            HorizontalLayoutGroup buttonAreaLayout = buttonAreaObj.AddComponent<HorizontalLayoutGroup>();
            buttonAreaLayout.spacing = 10f;
            buttonAreaLayout.childControlHeight = false;
            buttonAreaLayout.childControlWidth = true;
            buttonAreaLayout.childForceExpandWidth = true;

            GameObject moveToExhibitionBtnObj = CreateButton("MoveToExhibitionButton", "전시관으로", buttonAreaObj.transform);
            Button moveToExhibitionButton = moveToExhibitionBtnObj.GetComponent<Button>();

            GameObject moveToInventoryBtnObj = CreateButton("MoveToInventoryButton", "인벤토리로", buttonAreaObj.transform);
            Button moveToInventoryButton = moveToInventoryBtnObj.GetComponent<Button>();

            // ExhibitionUI 컴포넌트에 참조 할당
            SerializedObject serializedObject = new SerializedObject(exhibitionUI);
            
            serializedObject.FindProperty("exhibitionPanel").objectReferenceValue = panelObj;
            serializedObject.FindProperty("inventoryGridContainer").objectReferenceValue = inventoryGridRect;
            serializedObject.FindProperty("exhibitionGridContainer").objectReferenceValue = exhibitionGridRect;
            serializedObject.FindProperty("slotPrefab").objectReferenceValue = slotPrefab;
            serializedObject.FindProperty("closeButton").objectReferenceValue = closeButton;
            serializedObject.FindProperty("inventoryCountText").objectReferenceValue = inventoryCountText;
            serializedObject.FindProperty("exhibitionCountText").objectReferenceValue = exhibitionCountText;
            serializedObject.FindProperty("detailPanel").objectReferenceValue = detailPanelObj;
            serializedObject.FindProperty("detailIconImage").objectReferenceValue = detailIconImage;
            serializedObject.FindProperty("detailNameText").objectReferenceValue = detailNameText;
            serializedObject.FindProperty("detailObesityText").objectReferenceValue = detailObesityText;
            serializedObject.FindProperty("detailPriceText").objectReferenceValue = detailPriceText;
            serializedObject.FindProperty("detailRarityText").objectReferenceValue = detailRarityText;
            serializedObject.FindProperty("detailCloseButton").objectReferenceValue = detailCloseButton;
            serializedObject.FindProperty("moveToExhibitionButton").objectReferenceValue = moveToExhibitionButton;
            serializedObject.FindProperty("moveToInventoryButton").objectReferenceValue = moveToInventoryButton;
            
            serializedObject.ApplyModifiedProperties();

            Selection.activeGameObject = exhibitionObj;
            EditorUtility.DisplayDialog("완료", 
                "ExhibitionUI가 생성되었습니다!\n\n" +
                "구조:\n" +
                "- 전시관 패널 (인벤토리와 전시관 나란히 표시)\n" +
                "- 인벤토리 영역 (왼쪽)\n" +
                "- 전시관 영역 (오른쪽)\n" +
                "- 상세 정보 패널 (비둘기 정보 + 이동 버튼)\n" +
                "- 슬롯 프리팹: Assets/Prefabs/UI/ExhibitionSlot.prefab\n\n" +
                "참고: ExhibitionBuilding 오브젝트를 씬에 배치하고 상호작용할 수 있도록 설정하세요.", 
                "확인");
        }

        [MenuItem("PigeonGame/UI Helper/Create All UI")]
        public static void CreateAllUI()
        {
            if (EditorUtility.DisplayDialog("UI 생성", "모든 UI를 생성하시겠습니까?\n\n- MapInfoUI\n- MoneyDisplay\n- TrapPlacementUI\n- ExhibitionUI", "생성", "취소"))
            {
                CreateMapInfoUI();
                CreateMoneyDisplayUI();
                CreateTrapPlacementUI();
                CreateExhibitionUI();
                EditorUtility.DisplayDialog("완료", "모든 UI가 생성되었습니다!", "확인");
            }
        }
    }
}
