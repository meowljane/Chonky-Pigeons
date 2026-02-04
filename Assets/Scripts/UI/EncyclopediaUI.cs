using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using PigeonGame.Data;
using PigeonGame.Gameplay;

namespace PigeonGame.UI
{
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
        [SerializeField] private EncyclopediaSpeciesDetailUI speciesDetailUI;

        [Header("Settings")]
        [SerializeField] private Color unlockedColor = Color.white;
        [SerializeField] private Color lockedColor = Color.gray;
        private List<GameObject> speciesSlotObjects = new List<GameObject>();

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

            UIHelper.SafeAddListener(encyclopediaButton, OpenEncyclopedia);
            UIHelper.SafeAddListener(closeButton, CloseEncyclopedia);

            UpdateSpeciesList();
        }

        public void OpenEncyclopedia()
        {
            if (encyclopediaPanel != null)
            {
                encyclopediaPanel.SetActive(true);
                UpdateSpeciesList();
                ScrollRectHelper.ScrollToTop(encyclopediaPanel);
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

            ClearSpeciesSlots();

            var allSpecies = registry.SpeciesSet.species;

            foreach (var species in allSpecies)
            {
                GameObject slotObj = Instantiate(speciesSlot, speciesGridContainer, false);
                SetupSpeciesSlot(slotObj, species);
                speciesSlotObjects.Add(slotObj);
            }
        }

        private void SetupSpeciesSlot(GameObject slotObj, SpeciesDefinition species)
        {
            EncyclopediaSpeciesSlotUI slotUI = slotObj.GetComponent<EncyclopediaSpeciesSlotUI>();
            if (slotUI == null)
                return;

            var encyclopediaData = EncyclopediaManager.Instance != null 
                ? EncyclopediaManager.Instance.GetSpeciesData(species.speciesType) 
                : null;

            bool isUnlocked = encyclopediaData != null && encyclopediaData.isUnlocked;

            if (slotUI.Button != null)
            {
                slotUI.Button.onClick.RemoveAllListeners();
                slotUI.Button.onClick.AddListener(() => ShowSpeciesDetail(species));
            }

            if (slotUI.BackgroundImage != null)
            {
                slotUI.BackgroundImage.color = isUnlocked ? unlockedColor : lockedColor;
            }

            var registry = GameDataRegistry.Instance;
            var defaultSpecies = (registry != null && registry.SpeciesSet != null)
                ? registry.SpeciesSet.GetSpeciesById(PigeonSpecies.SP01)
                : null;
            var defaultFace = (registry != null && registry.Faces != null)
                ? registry.Faces.GetFaceById(FaceType.F00)
                : null;

            if (slotUI.IconImage != null)
            {
                var iconToUse = species?.icon ?? defaultSpecies?.icon;
                if (iconToUse != null)
                {
                    slotUI.IconImage.sprite = iconToUse;
                    slotUI.IconImage.enabled = true;
                }
            }

            if (slotUI.FaceIconImage != null && defaultFace?.icon != null)
            {
                slotUI.FaceIconImage.sprite = defaultFace.icon;
                slotUI.FaceIconImage.enabled = true;
            }

            if (slotUI.NameText != null)
            {
                slotUI.NameText.text = species.name;
            }

            if (slotUI.LockOverlay != null)
            {
                slotUI.LockOverlay.SetActive(!isUnlocked);
            }
        }

        private void ShowSpeciesDetail(SpeciesDefinition species)
        {
            if (speciesDetailUI != null)
            {
                speciesDetailUI.ShowSpeciesDetail(species);
            }
        }

        private void ClearSpeciesSlots()
        {
            UIHelper.ClearSlotList(speciesSlotObjects);
        }

        private void OnDestroy()
        {
            UIHelper.SafeRemoveListener(encyclopediaButton);
            UIHelper.SafeRemoveListener(closeButton);
        }
    }
}

