using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using PigeonGame.Data;
using PigeonGame.Gameplay;

namespace PigeonGame.UI
{
    public static class UIHelper
    {
        public static void SetupEmptySlot(GameObject slotObj)
        {
            InventorySlotUI slotUI = slotObj.GetComponent<InventorySlotUI>();
            if (slotUI == null) return;

            if (slotUI.IconImage != null) slotUI.IconImage.enabled = false;
            if (slotUI.FaceIconImage != null) slotUI.FaceIconImage.enabled = false;
            if (slotUI.NameText != null) slotUI.NameText.text = "";
            if (slotUI.Button != null) slotUI.Button.interactable = false;
        }

        public static void SetupPigeonSlot(GameObject slotObj, PigeonInstanceStats stats, int index, System.Action<int> onClick)
        {
            InventorySlotUI slotUI = slotObj.GetComponent<InventorySlotUI>();
            if (slotUI == null) return;

            var registry = GameDataRegistry.Instance;
            var species = (registry?.SpeciesSet != null) ? registry.SpeciesSet.GetSpeciesById(stats.speciesId) : null;
            var face = (registry?.Faces != null) ? registry.Faces.GetFaceById(stats.faceId) : null;

            if (slotUI.IconImage != null)
            {
                var defaultSpecies = (registry?.SpeciesSet != null) ? registry.SpeciesSet.GetSpeciesById(PigeonSpecies.SP01) : null;
                var iconToUse = species?.icon ?? defaultSpecies?.icon;
                if (iconToUse != null)
                {
                    slotUI.IconImage.sprite = iconToUse;
                    slotUI.IconImage.enabled = true;
                }
            }

            if (slotUI.FaceIconImage != null)
            {
                var defaultFace = (registry?.Faces != null) ? registry.Faces.GetFaceById(FaceType.F00) : null;
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
                slotUI.Button.onClick.AddListener(() => onClick?.Invoke(index));
            }
        }

        public static void UpdateGoldText(TextMeshProUGUI goldText)
        {
            if (goldText != null && GameManager.Instance != null)
            {
                goldText.text = $"현재 골드: {GameManager.Instance.CurrentMoney}G";
            }
        }

        public static void ClearSlotList(List<GameObject> list)
        {
            foreach (var item in list)
            {
                if (item != null) Object.Destroy(item);
            }
            list.Clear();
        }

        public static void SafeAddListener(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(action);
            }
        }

        public static void SafeRemoveListener(Button button)
        {
            if (button != null) button.onClick.RemoveAllListeners();
        }
    }

    public static class ScrollRectHelper
    {
        public static void ScrollToTop(GameObject gameObject)
        {
            if (gameObject != null)
            {
                ScrollRect[] scrollRects = gameObject.GetComponentsInChildren<ScrollRect>();
                foreach (var scrollRect in scrollRects)
                {
                    if (scrollRect != null)
                    {
                        scrollRect.verticalNormalizedPosition = 1f;
                    }
                }
            }
        }
    }
}
