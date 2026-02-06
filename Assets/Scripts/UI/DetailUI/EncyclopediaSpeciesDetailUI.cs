using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using PigeonGame.Data;
using PigeonGame.Gameplay;

namespace PigeonGame.UI
{
    public class EncyclopediaSpeciesDetailUI : MonoBehaviour
    {
        [Header("Detail Panel")]
        [SerializeField] private GameObject speciesDetailPanel;
        [SerializeField] private TextMeshProUGUI speciesNameText;
        [SerializeField] private Image speciesIconImage; 
        [SerializeField] private Image speciesFaceIconImage; 
        [SerializeField] private TextMeshProUGUI speciesWeightText;
        [SerializeField] private TextMeshProUGUI preferenceText;
        [SerializeField] private Transform faceGridContainer;
        [SerializeField] private GameObject faceSlot;
        [SerializeField] private Button backButton;

        [Header("Settings")]
        [SerializeField] private Color unlockedColor = Color.white;
        [SerializeField] private Color lockedColor = Color.gray;

        private List<GameObject> faceSlotObjects = new List<GameObject>();
        private SpeciesDefinition currentSpecies; 

        private void Start()
        {
            PanelUIHelper.InitializePanel(speciesDetailPanel, backButton, CloseDetail);
        }

        public void ShowSpeciesDetail(SpeciesDefinition species)
        {
            if (species == null || speciesDetailPanel == null)
                return;

            currentSpecies = species;
            PanelUIHelper.OpenPanel(speciesDetailPanel);

            if (speciesNameText != null)
                speciesNameText.text = species.name;

            UpdateSpeciesIcon(FaceType.F00);

            var speciesData = EncyclopediaManager.Instance != null
                ? EncyclopediaManager.Instance.GetSpeciesData(species.speciesType)
                : null;

            if (speciesWeightText != null)
            {
                if (speciesData != null && speciesData.isUnlocked && 
                    speciesData.minWeight != float.MaxValue && speciesData.maxWeight != float.MinValue)
                {
                    speciesWeightText.text = $"발견됨 ({speciesData.minWeight:F1}kg~{speciesData.maxWeight:F1}kg)";
                    speciesWeightText.color = Color.white;
                }
                else
                {
                    speciesWeightText.text = "미발견";
                    speciesWeightText.color = Color.gray;
                }
            }

            if (preferenceText != null)
            {
                string terrainName = UIHelper.GetTerrainName(species.favoriteTerrain);
                string trapName = UIHelper.GetTrapName(species.favoriteTrapType);
                preferenceText.text = $"선호 지형: {terrainName} / 선호 덫: {trapName}";
            }

            UpdateFaceList(species, speciesData);
        }

        private void UpdateFaceList(SpeciesDefinition species, EncyclopediaManager.SpeciesEncyclopediaData speciesData)
        {
            if (faceGridContainer == null || faceSlot == null)
                return;

            UIHelper.ClearSlotList(faceSlotObjects);

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

            var faceData = speciesData != null && speciesData.faces.ContainsKey(face.faceType)
                ? speciesData.faces[face.faceType]
                : null;

            bool isUnlocked = faceData != null && faceData.isUnlocked;

            if (slotUI.BackgroundImage != null)
            {
                slotUI.BackgroundImage.color = isUnlocked ? unlockedColor : lockedColor;
            }

            if (slotUI.NameText != null)
            {
                slotUI.NameText.text = face.name;
                slotUI.NameText.color = isUnlocked ? Color.white : Color.gray;
            }

            if (slotUI.StatusText != null)
            {
                slotUI.StatusText.text = isUnlocked ? "발견" : "미발견";
                slotUI.StatusText.color = isUnlocked ? Color.white : Color.gray;
            }

            if (slotUI.LockOverlay != null)
            {
                slotUI.LockOverlay.SetActive(!isUnlocked);
            }

            if (slotUI.Button != null)
            {
                slotUI.Button.onClick.RemoveAllListeners();
                FaceType faceType = face.faceType; 
                slotUI.Button.onClick.AddListener(() => OnFaceSlotClicked(faceType));
            }
        }

        private void OnFaceSlotClicked(FaceType faceType)
        {
            if (currentSpecies == null)
                return;

            UpdateSpeciesIcon(faceType);
        }

        private void UpdateSpeciesIcon(FaceType faceType)
        {
            if (currentSpecies == null)
                return;

            var registry = GameDataRegistry.Instance;
            if (registry == null || registry.Faces == null)
                return;

            var face = registry.Faces.GetFaceById(faceType);

            UIHelper.SetSpeciesIcon(speciesIconImage, currentSpecies);
            UIHelper.SetFaceIcon(speciesFaceIconImage, face);
        }

        public void CloseDetail()
        {
            PanelUIHelper.ClosePanel(speciesDetailPanel);
        }

        private void OnDestroy()
        {
            UIHelper.SafeRemoveListener(backButton);
        }
    }
}
