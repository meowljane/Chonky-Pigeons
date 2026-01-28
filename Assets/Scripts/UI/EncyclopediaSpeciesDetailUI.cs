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
        [SerializeField] private Image speciesIconImage; // Species 아이콘 또는 기본 표정이 적용된 몸+표정 이미지
        [SerializeField] private Image speciesFaceIconImage; // Face 아이콘 (몸+표정 합쳐진 이미지, 선택적)
        [SerializeField] private TextMeshProUGUI speciesWeightText;
        [SerializeField] private TextMeshProUGUI preferenceText;
        [SerializeField] private Transform faceGridContainer;
        [SerializeField] private GameObject faceSlot;
        [SerializeField] private Button backButton;

        [Header("Settings")]
        [SerializeField] private Color unlockedColor = Color.white;
        [SerializeField] private Color lockedColor = Color.gray;

        private List<GameObject> faceSlotObjects = new List<GameObject>();
        private SpeciesDefinition currentSpecies; // 현재 표시 중인 종

        private void Start()
        {
            if (speciesDetailPanel != null)
            {
                speciesDetailPanel.SetActive(false);
            }

            UIHelper.SafeAddListener(backButton, CloseDetail);
        }

        /// <summary>
        /// 종 상세 정보 표시
        /// </summary>
        public void ShowSpeciesDetail(SpeciesDefinition species)
        {
            if (species == null || speciesDetailPanel == null)
                return;

            currentSpecies = species;
            speciesDetailPanel.SetActive(true);

            // Species 정보 표시
            if (speciesNameText != null)
                speciesNameText.text = species.name;

            // 기본 표정(F00)으로 표시
            UpdateSpeciesIcon(FaceType.F00);

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

            // 선호 지형 및 덫 표시
            if (preferenceText != null)
            {
                var registry = GameDataRegistry.Instance;
                string terrainName = species.favoriteTerrain.ToString();
                string trapName = species.favoriteTrapType.ToString();

                if (registry?.TerrainTypes != null)
                {
                    var terrainDef = registry.TerrainTypes.GetTerrainById(species.favoriteTerrain);
                    if (terrainDef != null)
                    {
                        terrainName = terrainDef.koreanName;
                    }
                }

                if (registry?.Traps != null)
                {
                    var trapDef = registry.Traps.GetTrapById(species.favoriteTrapType);
                    if (trapDef != null)
                    {
                        trapName = trapDef.name;
                    }
                }

                preferenceText.text = $"선호 지형: {terrainName} / 선호 덫: {trapName}";
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

            // 버튼 클릭 이벤트 연결 (표정 변경)
            if (slotUI.Button != null)
            {
                slotUI.Button.onClick.RemoveAllListeners();
                FaceType faceType = face.faceType; // 클로저를 위한 로컬 변수
                slotUI.Button.onClick.AddListener(() => OnFaceSlotClicked(faceType));
            }
        }

        /// <summary>
        /// 표정 슬롯 클릭 시 해당 표정으로 변경
        /// </summary>
        private void OnFaceSlotClicked(FaceType faceType)
        {
            if (currentSpecies == null)
                return;

            UpdateSpeciesIcon(faceType);
        }

        /// <summary>
        /// 종 아이콘을 지정된 표정으로 업데이트
        /// </summary>
        private void UpdateSpeciesIcon(FaceType faceType)
        {
            if (currentSpecies == null)
                return;

            var registry = GameDataRegistry.Instance;
            if (registry == null || registry.Faces == null)
                return;

            var face = registry.Faces.GetFaceById(faceType);
            
            // 기본값 설정
            var defaultSpecies = registry.SpeciesSet.GetSpeciesById(PigeonSpecies.SP01);
            var defaultFace = registry.Faces.GetFaceById(FaceType.F00);
            
            // speciesIconImage: Species icon 표시 (없으면 기본값 SP01 사용)
            if (speciesIconImage != null)
            {
                var iconToUse = currentSpecies?.icon ?? defaultSpecies?.icon;
                if (iconToUse != null)
                {
                    speciesIconImage.sprite = iconToUse;
                    speciesIconImage.enabled = true;
                }
            }

            // speciesFaceIconImage: 선택된 Face icon 표시 (없으면 기본값 F00 사용)
            if (speciesFaceIconImage != null)
            {
                var faceIconToUse = face?.icon ?? defaultFace?.icon;
                if (faceIconToUse != null)
                {
                    speciesFaceIconImage.sprite = faceIconToUse;
                    speciesFaceIconImage.enabled = true;
                }
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
            UIHelper.ClearSlotList(faceSlotObjects);
        }

        private void OnDestroy()
        {
            UIHelper.SafeRemoveListener(backButton);
        }
    }
}
