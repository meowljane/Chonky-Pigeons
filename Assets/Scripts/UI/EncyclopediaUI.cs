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
                // 스크롤을 맨 위로 초기화
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

            // Species 목록 가져오기
            var allSpecies = registry.SpeciesSet.species;

            // Species 슬롯 생성
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

