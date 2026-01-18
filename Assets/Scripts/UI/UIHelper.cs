using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using PigeonGame.Data;
using PigeonGame.Gameplay;

namespace PigeonGame.UI
{
    /// <summary>
    /// UI 공통 유틸리티 클래스
    /// </summary>
    public static class UIHelper
    {
        /// <summary>
        /// 빈 슬롯 설정
        /// </summary>
        public static void SetupEmptySlot(GameObject slotObj)
        {
            InventorySlotUI slotUI = slotObj.GetComponent<InventorySlotUI>();
            if (slotUI == null) return;

            if (slotUI.IconImage != null) slotUI.IconImage.enabled = false;
            if (slotUI.NameText != null) slotUI.NameText.text = "";
            if (slotUI.Button != null) slotUI.Button.interactable = false;
        }

        /// <summary>
        /// 비둘기 슬롯 UI 설정
        /// </summary>
        public static void SetupPigeonSlot(GameObject slotObj, PigeonInstanceStats stats, int index, System.Action<int> onClick)
        {
            InventorySlotUI slotUI = slotObj.GetComponent<InventorySlotUI>();
            if (slotUI == null) return;

            var registry = GameDataRegistry.Instance;
            var species = (registry?.SpeciesSet != null) ? registry.SpeciesSet.GetSpeciesById(stats.speciesId) : null;

            if (slotUI.IconImage != null)
            {
                if (species?.icon != null)
                {
                    slotUI.IconImage.sprite = species.icon;
                    slotUI.IconImage.enabled = true;
                }
                else
                {
                    slotUI.IconImage.enabled = false;
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

        /// <summary>
        /// 골드 텍스트 업데이트
        /// </summary>
        public static void UpdateGoldText(TextMeshProUGUI goldText)
        {
            if (goldText != null && GameManager.Instance != null)
            {
                goldText.text = $"현재 골드: {GameManager.Instance.CurrentMoney}G";
            }
        }

        /// <summary>
        /// 슬롯 리스트 정리
        /// </summary>
        public static void ClearSlotList(List<GameObject> list)
        {
            foreach (var item in list)
            {
                if (item != null) Object.Destroy(item);
            }
            list.Clear();
        }

        /// <summary>
        /// 버튼 이벤트 안전하게 연결
        /// </summary>
        public static void SafeAddListener(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(action);
            }
        }

        /// <summary>
        /// 버튼 이벤트 안전하게 해제
        /// </summary>
        public static void SafeRemoveListener(Button button)
        {
            if (button != null) button.onClick.RemoveAllListeners();
        }
    }

    /// <summary>
    /// ScrollRect 헬퍼 유틸리티
    /// </summary>
    public static class ScrollRectHelper
    {
        /// <summary>
        /// ScrollRect를 맨 위로 스크롤 (모든 ScrollRect에 적용)
        /// </summary>
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
