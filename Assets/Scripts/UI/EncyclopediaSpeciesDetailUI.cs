using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using PigeonGame.Data;
using PigeonGame.Gameplay;

namespace PigeonGame.UI
{
    /// <summary>
    /// 도감 종 상세 정보 UI
    /// </summary>
    public class EncyclopediaSpeciesDetailUI : MonoBehaviour
    {
        [Header("Detail Panel")]
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

        private List<GameObject> faceSlotObjects = new List<GameObject>();

        private void Start()
        {
            if (speciesDetailPanel != null)
            {
                speciesDetailPanel.SetActive(false);
            }

            if (backButton != null)
            {
                backButton.onClick.RemoveAllListeners();
                backButton.onClick.AddListener(CloseDetail);
            }
        }

        /// <summary>
        /// 종 상세 정보 표시
        /// </summary>
        public void ShowSpeciesDetail(SpeciesDefinition species)
        {
            if (species == null || speciesDetailPanel == null)
                return;

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
            UpdateFaceList(species, speciesData);
        }

        private void UpdateFaceList(SpeciesDefinition species, EncyclopediaManager.SpeciesEncyclopediaData speciesData)
        {
            if (faceGridContainer == null || faceSlot == null)
                return;

            ClearFaceSlots();

            var registry = GameDataRegistry.Instance;
            if (registry == null || registry.Faces == null)
                return;

            var allFaces = registry.Faces.faces;

            foreach (var face in allFaces)
            {
                GameObject slotObj = Instantiate(faceSlot, faceGridContainer, false);
                SetupFaceSlot(slotObj, face, speciesData);
                faceSlotObjects.Add(slotObj);
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

        public void CloseDetail()
        {
            if (speciesDetailPanel != null)
            {
                speciesDetailPanel.SetActive(false);
            }
        }

        private void ClearFaceSlots()
        {
            foreach (var item in faceSlotObjects)
            {
                if (item != null)
                    Destroy(item);
            }
            faceSlotObjects.Clear();
        }

        private void OnDestroy()
        {
            if (backButton != null)
            {
                backButton.onClick.RemoveAllListeners();
            }
        }
    }
}
