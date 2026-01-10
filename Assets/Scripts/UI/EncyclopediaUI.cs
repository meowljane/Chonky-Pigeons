using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using PigeonGame.Data;
using PigeonGame.Gameplay;

namespace PigeonGame.UI
{
    /// <summary>
    /// 도감 UI
    /// </summary>
    public class EncyclopediaUI : MonoBehaviour
    {
        [Header("Main Panel")]
        [SerializeField] private GameObject encyclopediaPanel;
        [SerializeField] private Button encyclopediaButton;
        [SerializeField] private Button closeButton;

        [Header("Species List")]
        [SerializeField] private GameObject speciesListPanel;
        [SerializeField] private Transform speciesGridContainer;
        [SerializeField] private GameObject speciesItemPrefab;

        [Header("Species Detail")]
        [SerializeField] private GameObject speciesDetailPanel;
        [SerializeField] private TextMeshProUGUI speciesNameText;
        [SerializeField] private Image speciesIconImage;
        [SerializeField] private TextMeshProUGUI speciesWeightText;
        [SerializeField] private Transform faceGridContainer;
        [SerializeField] private GameObject faceItemPrefab;
        [SerializeField] private Button backButton;

        [Header("Settings")]
        [SerializeField] private Color unlockedColor = Color.white;
        [SerializeField] private Color lockedColor = Color.gray;
        private List<GameObject> speciesItemObjects = new List<GameObject>();
        private List<GameObject> faceItemObjects = new List<GameObject>();
        private SpeciesDefinition currentSpecies;

        private void Start()
        {
            if (encyclopediaPanel != null)
            {
                encyclopediaPanel.SetActive(false);
            }

            if (speciesListPanel != null)
            {
                speciesListPanel.SetActive(true);
            }

            if (speciesDetailPanel != null)
            {
                speciesDetailPanel.SetActive(false);
            }

            // 버튼 연결
            if (encyclopediaButton != null)
            {
                encyclopediaButton.onClick.AddListener(OpenEncyclopedia);
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(CloseEncyclopedia);
            }

            if (backButton != null)
            {
                backButton.onClick.AddListener(BackToSpeciesList);
            }

            // 도감 Manager가 없으면 생성
            if (EncyclopediaManager.Instance == null)
            {
                GameObject managerObj = new GameObject("EncyclopediaManager");
                managerObj.AddComponent<EncyclopediaManager>();
            }

            UpdateSpeciesList();
        }

        public void OpenEncyclopedia()
        {
            if (encyclopediaPanel != null)
            {
                encyclopediaPanel.SetActive(true);
                speciesListPanel.SetActive(true);
                speciesDetailPanel.SetActive(false);
                UpdateSpeciesList();
            }
        }

        public void CloseEncyclopedia()
        {
            if (encyclopediaPanel != null)
            {
                encyclopediaPanel.SetActive(false);
            }
        }

        private void UpdateSpeciesList()
        {
            if (speciesGridContainer == null)
                return;

            var registry = GameDataRegistry.Instance;
            if (registry == null || registry.SpeciesSet == null)
                return;

            // 기존 아이템 제거
            foreach (var item in speciesItemObjects)
            {
                if (item != null)
                    Destroy(item);
            }
            speciesItemObjects.Clear();

            // Species 목록 가져오기 (최대 9개)
            var allSpecies = registry.SpeciesSet.species;
            int maxSpecies = Mathf.Min(9, allSpecies.Length);

            // Species 아이템 생성 (3x3 그리드)
            for (int i = 0; i < maxSpecies; i++)
            {
                var species = allSpecies[i];
                GameObject itemObj = CreateSpeciesItem(species);
                if (itemObj != null)
                {
                    itemObj.transform.SetParent(speciesGridContainer, false);
                    speciesItemObjects.Add(itemObj);
                }
            }
        }

        private GameObject CreateSpeciesItem(SpeciesDefinition species)
        {
            GameObject itemObj = null;
            Image bg = null;
            Button button = null;

            if (speciesItemPrefab != null)
            {
                itemObj = Instantiate(speciesItemPrefab, speciesGridContainer);
            }
            else
            {
                itemObj = new GameObject($"SpeciesItem_{species.speciesId}");
                RectTransform rect = itemObj.AddComponent<RectTransform>();
                rect.sizeDelta = new Vector2(100, 100);

                bg = itemObj.AddComponent<Image>();
                bg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

                button = itemObj.AddComponent<Button>();

                // 아이콘
                GameObject iconObj = new GameObject("Icon");
                iconObj.transform.SetParent(itemObj.transform, false);
                Image icon = iconObj.AddComponent<Image>();
                if (species.icon != null)
                    icon.sprite = species.icon;

                RectTransform iconRect = icon.GetComponent<RectTransform>();
                iconRect.anchorMin = new Vector2(0.1f, 0.3f);
                iconRect.anchorMax = new Vector2(0.9f, 0.9f);
                iconRect.sizeDelta = Vector2.zero;
                iconRect.anchoredPosition = Vector2.zero;

                // 이름
                GameObject nameObj = new GameObject("Name");
                nameObj.transform.SetParent(itemObj.transform, false);
                TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
                nameText.text = species.name;
                nameText.fontSize = 12;
                nameText.color = Color.white;
                nameText.alignment = TextAlignmentOptions.Center;

                RectTransform nameRect = nameText.GetComponent<RectTransform>();
                nameRect.anchorMin = new Vector2(0, 0);
                nameRect.anchorMax = new Vector2(1, 0.3f);
                nameRect.sizeDelta = Vector2.zero;
                nameRect.anchoredPosition = Vector2.zero;
            }

            // 버튼 이벤트 연결
            if (button == null)
                button = itemObj.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => ShowSpeciesDetail(species));
            }

            // 도감 데이터 확인
            var encyclopediaData = EncyclopediaManager.Instance != null 
                ? EncyclopediaManager.Instance.GetSpeciesData(species.speciesId) 
                : null;

            bool isUnlocked = encyclopediaData != null && encyclopediaData.isUnlocked;

            // 색상 설정
            if (bg == null)
                bg = itemObj.GetComponent<Image>();
            if (bg != null)
            {
                bg.color = isUnlocked ? unlockedColor : lockedColor;
            }

            // 잠금 표시
            if (!isUnlocked)
            {
                // 잠금 오버레이 추가
                Transform lockOverlay = itemObj.transform.Find("LockOverlay");
                if (lockOverlay == null)
                {
                    GameObject lockObj = new GameObject("LockOverlay");
                    lockObj.transform.SetParent(itemObj.transform, false);
                    Image lockImage = lockObj.AddComponent<Image>();
                    lockImage.color = new Color(0, 0, 0, 0.7f);

                    RectTransform lockRect = lockImage.GetComponent<RectTransform>();
                    lockRect.anchorMin = Vector2.zero;
                    lockRect.anchorMax = Vector2.one;
                    lockRect.sizeDelta = Vector2.zero;
                    lockRect.anchoredPosition = Vector2.zero;
                }
            }

            return itemObj;
        }

        private void ShowSpeciesDetail(SpeciesDefinition species)
        {
            currentSpecies = species;

            if (speciesListPanel != null)
                speciesListPanel.SetActive(false);
            if (speciesDetailPanel != null)
                speciesDetailPanel.SetActive(true);

            // Species 정보 표시
            if (speciesNameText != null)
                speciesNameText.text = species.name;

            if (speciesIconImage != null && species.icon != null)
                speciesIconImage.sprite = species.icon;

            // Species 무게 정보
            var speciesData = EncyclopediaManager.Instance != null
                ? EncyclopediaManager.Instance.GetSpeciesData(species.speciesId)
                : null;

            if (speciesWeightText != null)
            {
                if (speciesData != null && speciesData.isUnlocked && 
                    speciesData.minWeight != int.MaxValue && speciesData.maxWeight != int.MinValue)
                {
                    speciesWeightText.text = $"무게 범위: {speciesData.minWeight} ~ {speciesData.maxWeight}";
                    speciesWeightText.color = Color.white;
                }
                else
                {
                    speciesWeightText.text = "미발견";
                    speciesWeightText.color = Color.gray;
                }
            }

            // Faces 목록 표시
            UpdateFaceList(species);
        }

        private void UpdateFaceList(SpeciesDefinition species)
        {
            if (faceGridContainer == null)
                return;

            // 기존 아이템 제거
            foreach (var item in faceItemObjects)
            {
                if (item != null)
                    Destroy(item);
            }
            faceItemObjects.Clear();

            var registry = GameDataRegistry.Instance;
            if (registry == null || registry.Faces == null)
                return;

            var allFaces = registry.Faces.faces;
            var speciesData = EncyclopediaManager.Instance != null
                ? EncyclopediaManager.Instance.GetSpeciesData(species.speciesId)
                : null;

            foreach (var face in allFaces)
            {
                GameObject itemObj = CreateFaceItem(face, species.speciesId, speciesData);
                if (itemObj != null)
                {
                    itemObj.transform.SetParent(faceGridContainer, false);
                    faceItemObjects.Add(itemObj);
                }
            }
        }

        private GameObject CreateFaceItem(FaceDefinition face, string speciesId, EncyclopediaManager.SpeciesEncyclopediaData speciesData)
        {
            GameObject itemObj = null;
            Image bg = null;

            if (faceItemPrefab != null)
            {
                itemObj = Instantiate(faceItemPrefab, faceGridContainer);
            }
            else
            {
                itemObj = new GameObject($"FaceItem_{face.id}");
                RectTransform rect = itemObj.AddComponent<RectTransform>();
                rect.sizeDelta = new Vector2(80, 100);

                bg = itemObj.AddComponent<Image>();
                bg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            }

            // Face 데이터 확인
            var faceData = speciesData != null && speciesData.faces.ContainsKey(face.id)
                ? speciesData.faces[face.id]
                : null;

            bool isUnlocked = faceData != null && faceData.isUnlocked;

            // 색상 설정
            if (bg == null)
                bg = itemObj.GetComponent<Image>();
            if (bg != null)
            {
                bg.color = isUnlocked ? unlockedColor : lockedColor;
            }

            // 이름 표시
            TextMeshProUGUI nameText = itemObj.GetComponentInChildren<TextMeshProUGUI>();
            if (nameText == null)
            {
                GameObject nameObj = new GameObject("Name");
                nameObj.transform.SetParent(itemObj.transform, false);
                nameText = nameObj.AddComponent<TextMeshProUGUI>();
                nameText.fontSize = 12;
                nameText.alignment = TextAlignmentOptions.Center;

                RectTransform nameRect = nameText.GetComponent<RectTransform>();
                nameRect.anchorMin = new Vector2(0, 0);
                nameRect.anchorMax = new Vector2(1, 0.3f);
                nameRect.sizeDelta = Vector2.zero;
                nameRect.anchoredPosition = Vector2.zero;
            }

            nameText.text = face.name;
            nameText.color = isUnlocked ? Color.white : Color.gray;

            // 무게 정보 표시
            TextMeshProUGUI weightText = itemObj.transform.Find("WeightText")?.GetComponent<TextMeshProUGUI>();
            if (weightText == null)
            {
                GameObject weightObj = new GameObject("WeightText");
                weightObj.transform.SetParent(itemObj.transform, false);
                weightText = weightObj.AddComponent<TextMeshProUGUI>();
                weightText.fontSize = 10;
                weightText.alignment = TextAlignmentOptions.Center;

                RectTransform weightRect = weightText.GetComponent<RectTransform>();
                weightRect.anchorMin = new Vector2(0, 0.3f);
                weightRect.anchorMax = new Vector2(1, 0.6f);
                weightRect.sizeDelta = Vector2.zero;
                weightRect.anchoredPosition = Vector2.zero;
            }

            if (isUnlocked && faceData != null && 
                faceData.minWeight != int.MaxValue && faceData.maxWeight != int.MinValue)
            {
                weightText.text = $"무게: {faceData.minWeight}~{faceData.maxWeight}";
                weightText.color = Color.yellow;
            }
            else
            {
                weightText.text = "미발견";
                weightText.color = Color.gray;
            }

            // 잠금 오버레이
            if (!isUnlocked)
            {
                Transform lockOverlay = itemObj.transform.Find("LockOverlay");
                if (lockOverlay == null)
                {
                    GameObject lockObj = new GameObject("LockOverlay");
                    lockObj.transform.SetParent(itemObj.transform, false);
                    Image lockImage = lockObj.AddComponent<Image>();
                    lockImage.color = new Color(0, 0, 0, 0.7f);

                    RectTransform lockRect = lockImage.GetComponent<RectTransform>();
                    lockRect.anchorMin = Vector2.zero;
                    lockRect.anchorMax = Vector2.one;
                    lockRect.sizeDelta = Vector2.zero;
                    lockRect.anchoredPosition = Vector2.zero;
                }
            }

            return itemObj;
        }

        private void BackToSpeciesList()
        {
            if (speciesListPanel != null)
                speciesListPanel.SetActive(true);
            if (speciesDetailPanel != null)
                speciesDetailPanel.SetActive(false);
        }

        private void OnDestroy()
        {
            if (encyclopediaButton != null)
                encyclopediaButton.onClick.RemoveAllListeners();
            if (closeButton != null)
                closeButton.onClick.RemoveAllListeners();
            if (backButton != null)
                backButton.onClick.RemoveAllListeners();
        }
    }
}

