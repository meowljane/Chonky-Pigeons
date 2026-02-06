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
            PanelUIHelper.InitializePanel(encyclopediaPanel, closeButton, CloseEncyclopedia);

            if (speciesListPanel != null)
            {
                speciesListPanel.SetActive(true);
            }

            UIHelper.SafeAddListener(encyclopediaButton, OpenEncyclopedia);

            UpdateSpeciesList();
        }

        public void OpenEncyclopedia()
        {
            PanelUIHelper.OpenPanel(encyclopediaPanel, UpdateSpeciesList);
        }

        public void CloseEncyclopedia()
        {
            PanelUIHelper.ClosePanel(encyclopediaPanel);
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

            UIHelper.SetSpeciesIcon(slotUI.IconImage, species);
            UIHelper.SetFaceIcon(slotUI.FaceIconImage, null);

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

