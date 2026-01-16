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
            if (speciesGridContainer == null || speciesItemPrefab == null)
                return;

            var registry = GameDataRegistry.Instance;
            if (registry == null || registry.SpeciesSet == null)
                return;

            ClearItemList(speciesItemObjects);

            // Species 목록 가져오기 (최대 9개)
            var allSpecies = registry.SpeciesSet.species;
            int maxSpecies = Mathf.Min(9, allSpecies.Length);

            // Species 아이템 생성 (3x3 그리드)
            for (int i = 0; i < maxSpecies; i++)
            {
                var species = allSpecies[i];
                GameObject itemObj = Instantiate(speciesItemPrefab, speciesGridContainer, false);
                SetupSpeciesItem(itemObj, species);
                speciesItemObjects.Add(itemObj);
            }
        }

        private void SetupSpeciesItem(GameObject itemObj, SpeciesDefinition species)
        {
            // 버튼 이벤트 연결
            Button button = itemObj.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => ShowSpeciesDetail(species));
            }

            // 도감 데이터 확인
            var encyclopediaData = EncyclopediaManager.Instance != null 
                ? EncyclopediaManager.Instance.GetSpeciesData(species.speciesType) 
                : null;

            bool isUnlocked = encyclopediaData != null && encyclopediaData.isUnlocked;

            // 배경 색상 설정
            Image bg = itemObj.GetComponent<Image>();
            if (bg != null)
            {
                bg.color = isUnlocked ? unlockedColor : lockedColor;
            }

            // 아이콘 설정
            Transform iconTransform = itemObj.transform.Find("Icon");
            if (iconTransform != null)
            {
                Image icon = iconTransform.GetComponent<Image>();
                if (icon != null && species.icon != null)
                {
                    icon.sprite = species.icon;
                }
            }

            // 이름 설정
            TextMeshProUGUI nameText = itemObj.GetComponentInChildren<TextMeshProUGUI>();
            if (nameText != null)
            {
                nameText.text = species.name;
            }

            // 잠금 오버레이 표시/숨김
            Transform lockOverlay = itemObj.transform.Find("LockOverlay");
            if (lockOverlay != null)
            {
                lockOverlay.gameObject.SetActive(!isUnlocked);
            }
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
                ? EncyclopediaManager.Instance.GetSpeciesData(species.speciesType)
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
            if (faceGridContainer == null || faceItemPrefab == null)
                return;

            ClearItemList(faceItemObjects);

            var registry = GameDataRegistry.Instance;
            if (registry == null || registry.Faces == null)
                return;

            var allFaces = registry.Faces.faces;
            var speciesData = EncyclopediaManager.Instance != null
                ? EncyclopediaManager.Instance.GetSpeciesData(species.speciesType)
                : null;

            foreach (var face in allFaces)
            {
                GameObject itemObj = Instantiate(faceItemPrefab, faceGridContainer, false);
                SetupFaceItem(itemObj, face, speciesData);
                faceItemObjects.Add(itemObj);
            }
        }

        private void SetupFaceItem(GameObject itemObj, FaceDefinition face, EncyclopediaManager.SpeciesEncyclopediaData speciesData)
        {
            // Face 데이터 확인
            var faceData = speciesData != null && speciesData.faces.ContainsKey(face.faceType)
                ? speciesData.faces[face.faceType]
                : null;

            bool isUnlocked = faceData != null && faceData.isUnlocked;

            // 배경 색상 설정
            Image bg = itemObj.GetComponent<Image>();
            if (bg != null)
            {
                bg.color = isUnlocked ? unlockedColor : lockedColor;
            }

            // 이름 표시
            TextMeshProUGUI nameText = itemObj.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
            if (nameText == null)
                nameText = itemObj.GetComponentInChildren<TextMeshProUGUI>();
            
            if (nameText != null)
            {
                nameText.text = face.name;
                nameText.color = isUnlocked ? Color.white : Color.gray;
            }

            // 무게 정보 표시
            TextMeshProUGUI weightText = itemObj.transform.Find("WeightText")?.GetComponent<TextMeshProUGUI>();
            if (weightText != null)
            {
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
            }

            // 잠금 오버레이 표시/숨김
            Transform lockOverlay = itemObj.transform.Find("LockOverlay");
            if (lockOverlay != null)
            {
                lockOverlay.gameObject.SetActive(!isUnlocked);
            }
        }

        private void BackToSpeciesList()
        {
            if (speciesListPanel != null)
                speciesListPanel.SetActive(true);
            if (speciesDetailPanel != null)
                speciesDetailPanel.SetActive(false);
        }

        private void ClearItemList(List<GameObject> list)
        {
            foreach (var item in list)
            {
                if (item != null)
                    Destroy(item);
            }
            list.Clear();
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

