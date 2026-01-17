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
        [SerializeField] private GameObject speciesSlot;

        [Header("Species Detail")]
        [SerializeField] private GameObject speciesDetailPanel;
        [SerializeField] private TextMeshProUGUI speciesNameText;
        [SerializeField] private Image speciesIconImage;
        [SerializeField] private TextMeshProUGUI speciesWeightText;
        [SerializeField] private Transform faceGridContainer;
        [SerializeField] private GameObject faceSlot;
        [SerializeField] private Button backButton;

        [Header("Settings")]
        [SerializeField] private Color unlockedColor = Color.white;
        [SerializeField] private Color lockedColor = Color.gray;
        private List<GameObject> speciesItemObjects = new List<GameObject>();
        private List<GameObject> faceItemObjects = new List<GameObject>();

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
            if (speciesGridContainer == null || speciesSlot == null)
                return;

            var registry = GameDataRegistry.Instance;
            if (registry == null || registry.SpeciesSet == null)
                return;

            ClearItemList(speciesItemObjects);

            // Species 목록 가져오기
            var allSpecies = registry.SpeciesSet.species;

            // Species 슬롯 생성
            foreach (var species in allSpecies)
            {
                GameObject slotObj = Instantiate(speciesSlot, speciesGridContainer, false);
                SetupSpeciesSlot(slotObj, species);
                speciesItemObjects.Add(slotObj);
            }
        }

        private void SetupSpeciesSlot(GameObject slotObj, SpeciesDefinition species)
        {
            EncyclopediaSpeciesSlotUI slotUI = slotObj.GetComponent<EncyclopediaSpeciesSlotUI>();
            if (slotUI == null)
                return;

            // 도감 데이터 확인
            var encyclopediaData = EncyclopediaManager.Instance != null 
                ? EncyclopediaManager.Instance.GetSpeciesData(species.speciesType) 
                : null;

            bool isUnlocked = encyclopediaData != null && encyclopediaData.isUnlocked;

            // 버튼 이벤트 연결
            if (slotUI.Button != null)
            {
                slotUI.Button.onClick.RemoveAllListeners();
                slotUI.Button.onClick.AddListener(() => ShowSpeciesDetail(species));
            }

            // 배경 색상 설정
            if (slotUI.BackgroundImage != null)
            {
                slotUI.BackgroundImage.color = isUnlocked ? unlockedColor : lockedColor;
            }

            // 아이콘 설정
            if (slotUI.IconImage != null && species.icon != null)
            {
                slotUI.IconImage.sprite = species.icon;
            }

            // 이름 설정
            if (slotUI.NameText != null)
            {
                slotUI.NameText.text = species.name;
            }

            // 잠금 오버레이 표시/숨김
            if (slotUI.LockOverlay != null)
            {
                slotUI.LockOverlay.SetActive(!isUnlocked);
            }
        }

        private void ShowSpeciesDetail(SpeciesDefinition species)
        {
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
                    speciesData.minWeight != float.MaxValue && speciesData.maxWeight != float.MinValue)
                {
                    // 무게를 kg 단위로 표시 (소수점 1자리)
                    speciesWeightText.text = $"발견됨 ({speciesData.minWeight:F1}kg~{speciesData.maxWeight:F1}kg)";
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
            if (faceGridContainer == null || faceSlot == null)
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
                GameObject slotObj = Instantiate(faceSlot, faceGridContainer, false);
                SetupFaceSlot(slotObj, face, speciesData);
                faceItemObjects.Add(slotObj);
            }
        }

        private void SetupFaceSlot(GameObject slotObj, FaceDefinition face, EncyclopediaManager.SpeciesEncyclopediaData speciesData)
        {
            EncyclopediaFaceSlotUI slotUI = slotObj.GetComponent<EncyclopediaFaceSlotUI>();
            if (slotUI == null)
                return;

            // Face 데이터 확인
            var faceData = speciesData != null && speciesData.faces.ContainsKey(face.faceType)
                ? speciesData.faces[face.faceType]
                : null;

            bool isUnlocked = faceData != null && faceData.isUnlocked;

            // 배경 색상 설정
            if (slotUI.BackgroundImage != null)
            {
                slotUI.BackgroundImage.color = isUnlocked ? unlockedColor : lockedColor;
            }

            // 이름 표시
            if (slotUI.NameText != null)
            {
                slotUI.NameText.text = face.name;
                slotUI.NameText.color = isUnlocked ? Color.white : Color.gray;
            }

            // 발견/미발견 상태 표시
            if (slotUI.StatusText != null)
            {
                slotUI.StatusText.text = isUnlocked ? "발견" : "미발견";
                slotUI.StatusText.color = isUnlocked ? Color.white : Color.gray;
            }

            // 잠금 오버레이 표시/숨김
            if (slotUI.LockOverlay != null)
            {
                slotUI.LockOverlay.SetActive(!isUnlocked);
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

