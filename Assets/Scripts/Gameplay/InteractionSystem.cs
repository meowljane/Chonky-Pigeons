using UnityEngine;
using PigeonGame.UI;

namespace PigeonGame.Gameplay
{
    /// <summary>
    /// 통합 상호작용 시스템
    /// 플레이어가 상호작용 가능한 오브젝트 근처에 있을 때 상호작용 버튼으로 처리
    /// </summary>
    public class InteractionSystem : MonoBehaviour
    {
        public static InteractionSystem Instance { get; private set; }

        private IInteractable currentInteractable; // 현재 상호작용 가능한 오브젝트
        private InventoryUI inventoryUI;
        private PigeonShopUI pigeonShopUI;
        private TrapShopUI trapShopUI;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            inventoryUI = FindFirstObjectByType<InventoryUI>();
            pigeonShopUI = FindFirstObjectByType<PigeonShopUI>();
            trapShopUI = FindFirstObjectByType<TrapShopUI>();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// 플레이어가 범위 안에 들어온 상호작용 가능한 오브젝트 등록
        /// </summary>
        public void RegisterInteractable(IInteractable interactable)
        {
            if (interactable != null && interactable.CanInteract())
            {
                // 이미 다른 오브젝트가 있으면 무시 (가장 먼저 등록된 것 사용)
                if (currentInteractable == null)
                {
                    currentInteractable = interactable;
                }
            }
        }

        /// <summary>
        /// 플레이어가 범위 밖으로 나간 상호작용 가능한 오브젝트 제거
        /// </summary>
        public void UnregisterInteractable(IInteractable interactable)
        {
            if (currentInteractable == interactable)
            {
                currentInteractable = null;
            }
        }

        /// <summary>
        /// 상호작용 버튼이 눌렸을 때 호출
        /// </summary>
        public void OnInteract()
        {
            if (currentInteractable == null)
            {
                return;
            }

            if (!currentInteractable.CanInteract())
            {
                return;
            }

            currentInteractable.OnInteract();
        }

        /// <summary>
        /// 비둘기 상점 열기 (WorldShop에서 호출)
        /// </summary>
        public void OpenPigeonShop()
        {
            if (pigeonShopUI != null)
            {
                pigeonShopUI.OpenShopPanel();
            }
            else
            {
                Debug.LogWarning("PigeonShopUI를 찾을 수 없습니다!");
            }
        }

        /// <summary>
        /// 덫 상점 열기 (WorldShop에서 호출)
        /// </summary>
        public void OpenTrapShop()
        {
            if (trapShopUI != null)
            {
                trapShopUI.OpenShopPanel();
            }
            else
            {
                Debug.LogWarning("TrapShopUI를 찾을 수 없습니다!");
            }
        }

        /// <summary>
        /// 현재 상호작용 가능한 오브젝트가 있는지 확인
        /// </summary>
        public bool CanInteract()
        {
            return currentInteractable != null && currentInteractable.CanInteract();
        }
    }
}