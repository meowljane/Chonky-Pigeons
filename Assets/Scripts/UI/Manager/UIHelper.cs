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

            SetSpeciesIcon(slotUI.IconImage, species);
            SetFaceIcon(slotUI.FaceIconImage, face);

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

        public static void UpdateGoldText(TextMeshProUGUI goldText, string format = "현재 골드: {0}G")
        {
            if (goldText != null && GameManager.Instance != null)
            {
                goldText.text = string.Format(format, GameManager.Instance.CurrentMoney);
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

        public static SpeciesDefinition GetDefaultSpecies()
        {
            var registry = GameDataRegistry.Instance;
            return (registry?.SpeciesSet != null) ? registry.SpeciesSet.GetSpeciesById(PigeonSpecies.SP01) : null;
        }

        public static FaceDefinition GetDefaultFace()
        {
            var registry = GameDataRegistry.Instance;
            return (registry?.Faces != null) ? registry.Faces.GetFaceById(FaceType.F00) : null;
        }

        public static void SetSpeciesIcon(UnityEngine.UI.Image iconImage, SpeciesDefinition species)
        {
            if (iconImage == null) return;

            var defaultSpecies = GetDefaultSpecies();
            var iconToUse = species?.icon ?? defaultSpecies?.icon;
            if (iconToUse != null)
            {
                iconImage.sprite = iconToUse;
                iconImage.enabled = true;
            }
        }

        public static void SetFaceIcon(UnityEngine.UI.Image faceIconImage, FaceDefinition face)
        {
            if (faceIconImage == null) return;

            var defaultFace = GetDefaultFace();
            var faceIconToUse = face?.icon ?? defaultFace?.icon;
            if (faceIconToUse != null)
            {
                faceIconImage.sprite = faceIconToUse;
                faceIconImage.enabled = true;
            }
        }

        public static void SetTrapIcon(UnityEngine.UI.Image iconImage, Sprite trapIcon)
        {
            if (iconImage == null) return;

            if (trapIcon != null)
            {
                iconImage.sprite = trapIcon;
                iconImage.enabled = true;
            }
            else
            {
                iconImage.enabled = false;
            }
        }

        public static void SetupPigeonSlotWithCustomCallback(GameObject slotObj, PigeonInstanceStats stats, System.Action<PigeonInstanceStats, bool, int> onClick, bool isInventory, int index)
        {
            InventorySlotUI slotUI = slotObj.GetComponent<InventorySlotUI>();
            if (slotUI == null) return;

            var registry = GameDataRegistry.Instance;
            var species = (registry?.SpeciesSet != null) ? registry.SpeciesSet.GetSpeciesById(stats.speciesId) : null;
            var face = (registry?.Faces != null) ? registry.Faces.GetFaceById(stats.faceId) : null;

            SetSpeciesIcon(slotUI.IconImage, species);
            SetFaceIcon(slotUI.FaceIconImage, face);

            if (slotUI.NameText != null)
            {
                slotUI.NameText.text = species?.name ?? stats.speciesId.ToString();
            }

            if (slotUI.Button != null)
            {
                slotUI.Button.onClick.RemoveAllListeners();
                slotUI.Button.onClick.AddListener(() => onClick?.Invoke(stats, isInventory, index));
            }
        }

        public static string GetFavoriteSpeciesText(System.Func<SpeciesDefinition, bool> predicate, string prefix = "선호 비둘기")
        {
            var registry = GameDataRegistry.Instance;
            if (registry?.SpeciesSet == null)
                return $"{prefix}: 없음";

            List<string> favoriteSpeciesNames = new List<string>();
            foreach (var species in registry.SpeciesSet.species)
            {
                if (predicate(species))
                {
                    favoriteSpeciesNames.Add(species.name);
                }
            }

            return favoriteSpeciesNames.Count > 0
                ? $"{prefix}: {string.Join(", ", favoriteSpeciesNames)}"
                : $"{prefix}: 없음";
        }

        public static string GetTerrainName(TerrainType terrainType)
        {
            var registry = GameDataRegistry.Instance;
            return registry?.TerrainTypes?.GetTerrainById(terrainType)?.koreanName ?? terrainType.ToString();
        }

        public static string GetMapName(MapType mapType)
        {
            var registry = GameDataRegistry.Instance;
            return registry?.MapTypes?.GetMapById(mapType)?.displayName ?? mapType.ToString();
        }

        public static string GetTrapName(TrapType trapType)
        {
            var registry = GameDataRegistry.Instance;
            return registry?.Traps?.GetTrapById(trapType)?.name ?? trapType.ToString();
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

    public static class ShopUIHelper
    {
        public static void SetupUnlockButton(Button button, TextMeshProUGUI buttonText, bool isUnlocked, bool canAfford, int cost, string unlockedText = "해금됨", string affordText = "해금", string noMoneyText = "돈부족")
        {
            if (button == null) return;

            button.interactable = !isUnlocked && canAfford;

            if (buttonText != null)
            {
                if (isUnlocked)
                    buttonText.text = unlockedText;
                else if (canAfford)
                    buttonText.text = $"{affordText}\n{cost}G";
                else
                    buttonText.text = $"{noMoneyText}\n{cost}G";
            }
        }

        public static void OpenShopPanel(GameObject shopPanel, TextMeshProUGUI goldText, System.Action updateDisplay)
        {
            if (shopPanel == null) return;
            shopPanel.SetActive(true);
            UIHelper.UpdateGoldText(goldText);
            updateDisplay?.Invoke();
            ScrollRectHelper.ScrollToTop(shopPanel);
        }

        public static void CloseShopPanel(GameObject shopPanel)
        {
            shopPanel?.SetActive(false);
        }

        public static void HandleMoneyChanged(TextMeshProUGUI goldText, System.Action updateDisplay)
        {
            UIHelper.UpdateGoldText(goldText);
            updateDisplay?.Invoke();
        }

        public static void InitializeShopPanel(GameObject shopPanel, Button closeButton, TextMeshProUGUI goldText, UnityEngine.Events.UnityAction onCloseClicked)
        {
            if (shopPanel != null)
                shopPanel.SetActive(false);

            UIHelper.SafeAddListener(closeButton, onCloseClicked);
            UIHelper.UpdateGoldText(goldText);
        }
    }

    public static class PanelUIHelper
    {
        public static void OpenPanel(GameObject panel, System.Action onUpdate = null)
        {
            if (panel == null) return;
            panel.SetActive(true);
            onUpdate?.Invoke();
            ScrollRectHelper.ScrollToTop(panel);
        }

        public static void ClosePanel(GameObject panel)
        {
            panel?.SetActive(false);
        }

        public static void InitializePanel(GameObject panel, Button closeButton, UnityEngine.Events.UnityAction onCloseClicked)
        {
            if (panel != null)
                panel.SetActive(false);

            UIHelper.SafeAddListener(closeButton, onCloseClicked);
        }
    }
}
