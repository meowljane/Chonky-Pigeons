using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using PigeonGame.Data;
using PigeonGame.Gameplay;

namespace PigeonGame.UI
{
    public class ExhibitionUI : MonoBehaviour
    {
        [Header("Main Panel")]
        [SerializeField] private GameObject exhibitionPanel;
        [SerializeField] private Transform inventoryGridContainer; 
        [SerializeField] private Transform exhibitionGridContainer; 
        [SerializeField] private GameObject inventorySlot; 
        [SerializeField] private Button closeButton;
        [SerializeField] private TextMeshProUGUI inventoryCountText;
        [SerializeField] private TextMeshProUGUI exhibitionCountText;

        [Header("Detail Panel")]
        [SerializeField] private PigeonDetailPanelUI detailPanelUI; 

        [Header("Exhibition Area")]

        [Header("Pigeon Spawning")]
        [SerializeField] private GameObject pigeonPrefab; 

        private List<GameObject> inventorySlotInstances = new List<GameObject>();
        private List<GameObject> exhibitionSlotInstances = new List<GameObject>();
        private List<PigeonController> exhibitionPigeons = new List<PigeonController>(); 
        private const int MAX_EXHIBITION_SLOTS = 20; 

        private PigeonInstanceStats currentDetailPigeonStats;
        private bool isDetailFromInventory = true; 
        private int currentDetailIndex = -1; 

        private void Start()
        {
            if (exhibitionPanel != null)
            {
                exhibitionPanel.SetActive(false);
            }

            UIHelper.SafeAddListener(closeButton, OnCloseButtonClicked);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPigeonAddedToInventory += OnPigeonAdded;
                GameManager.Instance.OnPigeonAddedToExhibition += OnPigeonAddedToExhibition;
                GameManager.Instance.OnPigeonRemovedFromExhibition += OnPigeonRemovedFromExhibition;
            }

            RefreshExhibitionPigeons();
        }

        public void OpenExhibitionPanel()
        {
            if (exhibitionPanel != null)
            {
                exhibitionPanel.SetActive(true);
                UpdateDisplay();
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

            for (int i = 0; i < slotCount; i++)
            {
                var pigeon = inventory[i];
                GameObject slotObj = Instantiate(inventorySlot, inventoryGridContainer, false);
                inventorySlotInstances.Add(slotObj);
                SetupSlotUI(slotObj, pigeon, true, i);
            }

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

            for (int i = 0; i < slotCount; i++)
            {
                var pigeon = exhibition[i];
                GameObject slotObj = Instantiate(inventorySlot, exhibitionGridContainer, false);
                exhibitionSlotInstances.Add(slotObj);
                SetupSlotUI(slotObj, pigeon, false, i);
            }

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

            var defaultSpecies = (registry?.SpeciesSet != null) ? registry.SpeciesSet.GetSpeciesById(PigeonSpecies.SP01) : null;
            var defaultFace = (registry?.Faces != null) ? registry.Faces.GetFaceById(FaceType.F00) : null;

            if (slotUI.IconImage != null)
            {
                var iconToUse = species?.icon ?? defaultSpecies?.icon;
                if (iconToUse != null)
                {
                    slotUI.IconImage.sprite = iconToUse;
                    slotUI.IconImage.enabled = true;
                }
            }

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

            string buttonText = isInventory ? "전시관으로" : "인벤토리로";

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

        private void RefreshExhibitionPigeons()
        {
            if (GameManager.Instance == null)
                return;

            ClearExhibitionPigeons();

            var exhibition = GameManager.Instance.Exhibition;
            foreach (var stats in exhibition)
            {
                SpawnExhibitionPigeon(stats);
            }
        }

        private void SpawnExhibitionPigeon(PigeonInstanceStats stats)
        {
            if (pigeonPrefab == null || stats == null)
                return;

            Vector3 spawnPos = Vector3.zero;
            if (TilemapRangeManager.Instance != null)
            {
                spawnPos = TilemapRangeManager.Instance.GetRandomPositionInExhibitionArea();
            }

            if (spawnPos == Vector3.zero)
            {
                return;
            }

            spawnPos.z = 0f; 

            GameObject pigeonObj = Instantiate(pigeonPrefab, spawnPos, Quaternion.identity);
            if (!pigeonObj.activeSelf)
            {
                pigeonObj.SetActive(true);
            }

            PigeonController controller = pigeonObj.GetComponent<PigeonController>();
            if (controller != null)
            {
                controller.Initialize(stats);

                controller.SetAsExhibitionPigeon();

                exhibitionPigeons.Add(controller);
            }
        }

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

            ClearExhibitionPigeons();
        }
    }
}
