using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using PigeonGame.Data;
using PigeonGame.Gameplay;

namespace PigeonGame.UI
{
    /// <summary>
    /// 전시관 UI
    /// 인벤토리에서 비둘기를 전시관에 넣거나, 전시관에서 인벤토리로 꺼낼 수 있음
    /// </summary>
    public class ExhibitionUI : MonoBehaviour
    {
        [Header("Main Panel")]
        [SerializeField] private GameObject exhibitionPanel;
        [SerializeField] private Transform inventoryGridContainer; // 인벤토리 그리드
        [SerializeField] private Transform exhibitionGridContainer; // 전시관 그리드
        [SerializeField] private GameObject inventorySlot; // 슬롯 프리팹
        [SerializeField] private Button closeButton;
        [SerializeField] private TextMeshProUGUI inventoryCountText;
        [SerializeField] private TextMeshProUGUI exhibitionCountText;

        [Header("Detail Panel")]
        [SerializeField] private PigeonDetailPanelUI detailPanelUI; // 상세 정보 패널 UI

        [Header("Exhibition Area")]
        // 전시 영역은 타일맵 기반으로 자동 감지됩니다 (ExhibitionArea 컴포넌트가 있는 타일맵)

        [Header("Pigeon Spawning")]
        [SerializeField] private GameObject pigeonPrefab; // 비둘기 프리팹

        private List<GameObject> inventorySlotInstances = new List<GameObject>();
        private List<GameObject> exhibitionSlotInstances = new List<GameObject>();
        private List<PigeonController> exhibitionPigeons = new List<PigeonController>(); // 전시된 비둘기들
        private const int MAX_EXHIBITION_SLOTS = 50; // 전시관 최대 슬롯 수

        private PigeonInstanceStats currentDetailPigeonStats;
        private bool isDetailFromInventory = true; // 상세 정보가 인벤토리에서 온 것인지
        private int currentDetailIndex = -1; // 현재 상세 정보의 인덱스

        private void Start()
        {
            if (exhibitionPanel != null)
            {
                exhibitionPanel.SetActive(false);
            }

            UIHelper.SafeAddListener(closeButton, OnCloseButtonClicked);

            // GameManager 이벤트 구독
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPigeonAddedToInventory += OnPigeonAdded;
                GameManager.Instance.OnPigeonAddedToExhibition += OnPigeonAddedToExhibition;
                GameManager.Instance.OnPigeonRemovedFromExhibition += OnPigeonRemovedFromExhibition;
            }

            // 기존 전시 비둘기들 스폰
            RefreshExhibitionPigeons();
        }

        /// <summary>
        /// 전시관 패널 열기
        /// </summary>
        public void OpenExhibitionPanel()
        {
            if (exhibitionPanel != null)
            {
                exhibitionPanel.SetActive(true);
                UpdateDisplay();
                // 스크롤을 맨 위로 초기화
                ScrollRectHelper.ScrollToTop(exhibitionPanel);
            }
        }

        private void OnPigeonAdded(PigeonInstanceStats stats)
        {
            UpdateDisplay();
        }

        private void OnPigeonAddedToExhibition(PigeonInstanceStats stats)
        {
            UpdateDisplay();
            RefreshExhibitionPigeons();
        }

        private void OnPigeonRemovedFromExhibition(PigeonInstanceStats stats)
        {
            UpdateDisplay();
            RefreshExhibitionPigeons();
        }

        private void UpdateDisplay()
        {
            UpdateInventoryDisplay();
            UpdateExhibitionDisplay();
        }

        private void UpdateInventoryDisplay()
        {
            if (GameManager.Instance == null || inventoryGridContainer == null || inventorySlot == null)
                return;

            ClearSlots(inventorySlotInstances);

            var inventory = GameManager.Instance.Inventory;
            int maxSlots = GameManager.Instance.MaxInventorySlots;
            int slotCount = Mathf.Min(inventory.Count, maxSlots);

            // 인벤토리 아이템으로 슬롯 채우기
            for (int i = 0; i < slotCount; i++)
            {
                var pigeon = inventory[i];
                GameObject slotObj = Instantiate(inventorySlot, inventoryGridContainer, false);
                inventorySlotInstances.Add(slotObj);
                SetupSlotUI(slotObj, pigeon, true, i);
            }

            // 빈 슬롯 채우기
            for (int i = slotCount; i < maxSlots; i++)
            {
                GameObject slotObj = Instantiate(inventorySlot, inventoryGridContainer, false);
                inventorySlotInstances.Add(slotObj);
                SetupEmptySlot(slotObj);
            }

            if (inventoryCountText != null)
            {
                inventoryCountText.text = $"인벤토리: {inventory.Count}";
            }
        }

        private void UpdateExhibitionDisplay()
        {
            if (GameManager.Instance == null || exhibitionGridContainer == null || inventorySlot == null)
                return;

            ClearSlots(exhibitionSlotInstances);

            var exhibition = GameManager.Instance.Exhibition;
            int slotCount = Mathf.Min(exhibition.Count, MAX_EXHIBITION_SLOTS);

            // 전시관 아이템으로 슬롯 채우기
            for (int i = 0; i < slotCount; i++)
            {
                var pigeon = exhibition[i];
                GameObject slotObj = Instantiate(inventorySlot, exhibitionGridContainer, false);
                exhibitionSlotInstances.Add(slotObj);
                SetupSlotUI(slotObj, pigeon, false, i);
            }

            // 빈 슬롯 채우기
            for (int i = slotCount; i < MAX_EXHIBITION_SLOTS; i++)
            {
                GameObject slotObj = Instantiate(inventorySlot, exhibitionGridContainer, false);
                exhibitionSlotInstances.Add(slotObj);
                SetupEmptySlot(slotObj);
            }

            if (exhibitionCountText != null)
            {
                exhibitionCountText.text = $"전시관: {exhibition.Count}/{MAX_EXHIBITION_SLOTS}";
            }
        }

        private void SetupSlotUI(GameObject slotObj, PigeonInstanceStats stats, bool isInventory, int index)
        {
            InventorySlotUI slotUI = slotObj.GetComponent<InventorySlotUI>();
            if (slotUI == null) return;

            var registry = GameDataRegistry.Instance;
            var species = (registry?.SpeciesSet != null) ? registry.SpeciesSet.GetSpeciesById(stats.speciesId) : null;
            var face = (registry?.Faces != null) ? registry.Faces.GetFaceById(stats.faceId) : null;

            // 기본값 설정
            var defaultSpecies = (registry?.SpeciesSet != null) ? registry.SpeciesSet.GetSpeciesById(PigeonSpecies.SP01) : null;
            var defaultFace = (registry?.Faces != null) ? registry.Faces.GetFaceById(FaceType.F00) : null;

            // IconImage: Species icon 표시 (없으면 기본값 SP01 사용)
            if (slotUI.IconImage != null)
            {
                var iconToUse = species?.icon ?? defaultSpecies?.icon;
                if (iconToUse != null)
                {
                    slotUI.IconImage.sprite = iconToUse;
                    slotUI.IconImage.enabled = true;
                }
            }

            // FaceIconImage: Face icon 표시 (없으면 기본값 F00 사용)
            if (slotUI.FaceIconImage != null)
            {
                var faceIconToUse = face?.icon ?? defaultFace?.icon;
                if (faceIconToUse != null)
                {
                    slotUI.FaceIconImage.sprite = faceIconToUse;
                    slotUI.FaceIconImage.enabled = true;
                }
            }

            if (slotUI.NameText != null)
            {
                slotUI.NameText.text = species?.name ?? stats.speciesId.ToString();
            }

            if (slotUI.Button != null)
            {
                slotUI.Button.onClick.RemoveAllListeners();
                slotUI.Button.onClick.AddListener(() => OnSlotClicked(stats, isInventory, index));
            }
        }

        private void SetupEmptySlot(GameObject slotObj)
        {
            UIHelper.SetupEmptySlot(slotObj);
        }

        private void OnSlotClicked(PigeonInstanceStats stats, bool isInventory, int index)
        {
            currentDetailPigeonStats = stats;
            isDetailFromInventory = isInventory;
            currentDetailIndex = index;
            ShowPigeonDetail(stats, isInventory);
        }

        private void ShowPigeonDetail(PigeonInstanceStats stats, bool isInventory)
        {
            if (stats == null || detailPanelUI == null)
                return;

            // 이동 버튼 텍스트 설정
            string buttonText = isInventory ? "전시관으로" : "인벤토리로";

            // 디테일 패널 UI에 위임
            detailPanelUI.ShowDetail(stats, 
                onClosedCallback: null, 
                onMoveCallback: (stats) => OnMoveButtonClicked(), 
                showMoveButton: true, 
                moveButtonText: buttonText);
        }

        private void OnMoveButtonClicked()
        {
            if (currentDetailPigeonStats == null || currentDetailIndex < 0)
                return;

            if (GameManager.Instance == null)
                return;

            if (isDetailFromInventory)
            {
                // 전시관으로 이동
                if (GameManager.Instance.ExhibitionCount >= MAX_EXHIBITION_SLOTS)
                {
                    ToastNotificationManager.ShowWarning("전시관이 가득 찼습니다!");
                    return;
                }

                var inventory = GameManager.Instance.Inventory;
                if (currentDetailIndex >= 0 && currentDetailIndex < inventory.Count)
                {
                    GameManager.Instance.AddPigeonToExhibition(currentDetailIndex);
                    if (detailPanelUI != null)
                    {
                        detailPanelUI.ClosePanel();
                    }
                }
            }
            else
            {
                // 인벤토리로 이동
                if (GameManager.Instance != null && GameManager.Instance.InventoryCount >= GameManager.Instance.MaxInventorySlots)
                {
                    ToastNotificationManager.ShowWarning("인벤토리가 가득 찼습니다!");
                    return;
                }

                var exhibition = GameManager.Instance.Exhibition;
                if (currentDetailIndex >= 0 && currentDetailIndex < exhibition.Count)
                {
                    GameManager.Instance.RemovePigeonFromExhibition(currentDetailIndex);
                    if (detailPanelUI != null)
                    {
                        detailPanelUI.ClosePanel();
                    }
                }
            }
        }

        private void OnCloseButtonClicked()
        {
            if (exhibitionPanel != null)
            {
                exhibitionPanel.SetActive(false);
            }
            
            if (detailPanelUI != null)
            {
                detailPanelUI.ClosePanel();
            }
        }

        private void ClearSlots(List<GameObject> list)
        {
            UIHelper.ClearSlotList(list);
        }

        /// <summary>
        /// 전시 비둘기 목록 새로고침 (GameManager의 전시관 리스트와 동기화)
        /// </summary>
        private void RefreshExhibitionPigeons()
        {
            if (GameManager.Instance == null)
                return;

            // 기존 전시 비둘기들 제거
            ClearExhibitionPigeons();

            // GameManager의 전시관 리스트와 동기화하여 스폰
            var exhibition = GameManager.Instance.Exhibition;
            foreach (var stats in exhibition)
            {
                SpawnExhibitionPigeon(stats);
            }
        }

        /// <summary>
        /// 전시 비둘기 스폰
        /// </summary>
        private void SpawnExhibitionPigeon(PigeonInstanceStats stats)
        {
            if (pigeonPrefab == null || stats == null)
                return;

            // 타일맵 기반 전시 영역에서 랜덤 위치 생성
            Vector3 spawnPos = Vector3.zero;
            if (TilemapRangeManager.Instance != null)
            {
                spawnPos = TilemapRangeManager.Instance.GetRandomPositionInExhibitionArea();
            }

            if (spawnPos == Vector3.zero)
            {
                Debug.LogWarning("ExhibitionUI: 전시 영역 타일맵을 찾을 수 없습니다. ExhibitionArea 컴포넌트가 있는 타일맵을 확인하세요.");
                return;
            }

            spawnPos.z = 0f; // 2D 게임용

            GameObject pigeonObj = Instantiate(pigeonPrefab, spawnPos, Quaternion.identity);
            if (!pigeonObj.activeSelf)
            {
                pigeonObj.SetActive(true);
            }

            PigeonController controller = pigeonObj.GetComponent<PigeonController>();
            if (controller != null)
            {
                // 비둘기 초기화
                controller.Initialize(stats);
                
                // 전시 비둘기로 설정 (타일맵 기반)
                controller.SetAsExhibitionPigeon();

                exhibitionPigeons.Add(controller);
            }
        }


        /// <summary>
        /// 모든 전시 비둘기 제거
        /// </summary>
        private void ClearExhibitionPigeons()
        {
            foreach (var pigeon in exhibitionPigeons)
            {
                if (pigeon != null && pigeon.gameObject != null)
                {
                    Destroy(pigeon.gameObject);
                }
            }
            exhibitionPigeons.Clear();
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPigeonAddedToInventory -= OnPigeonAdded;
                GameManager.Instance.OnPigeonAddedToExhibition -= OnPigeonAddedToExhibition;
                GameManager.Instance.OnPigeonRemovedFromExhibition -= OnPigeonRemovedFromExhibition;
            }
            UIHelper.SafeRemoveListener(closeButton);

            // 전시 비둘기들 제거
            ClearExhibitionPigeons();
        }
    }
}
