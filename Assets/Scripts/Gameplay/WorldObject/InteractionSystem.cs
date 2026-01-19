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

        [Header("Highlight Settings")]
        [SerializeField] private Color highlightColor = new Color(1f, 1f, 0f, 0.3f); // 노란색 반투명

        private IInteractable currentInteractable; // 현재 상호작용 가능한 오브젝트
        private GameObject currentOutlineObject; // 현재 테두리 오브젝트
        private InventoryUI inventoryUI;
        private PigeonShopUI pigeonShopUI;
        private TrapShopUI trapShopUI;
        private UI.ExhibitionUI exhibitionUI;
        private UI.PigeonResearchUI pigeonResearchUI;
        private UpgradeShopUI upgradeShopUI;
        private BridgeGatePurchaseUI bridgeGatePurchaseUI;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            InitializeUIComponents();
        }

        /// <summary>
        /// UI 컴포넌트 초기화 (동적 생성 시 즉시 호출 가능)
        /// </summary>
        public void InitializeUIComponents()
        {
            inventoryUI = FindFirstObjectByType<InventoryUI>();
            pigeonShopUI = FindFirstObjectByType<PigeonShopUI>();
            trapShopUI = FindFirstObjectByType<TrapShopUI>();
            exhibitionUI = FindFirstObjectByType<UI.ExhibitionUI>();
            pigeonResearchUI = FindFirstObjectByType<UI.PigeonResearchUI>();
            upgradeShopUI = FindFirstObjectByType<UpgradeShopUI>();
            bridgeGatePurchaseUI = FindFirstObjectByType<BridgeGatePurchaseUI>();
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
                    ShowOutline(interactable);
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
                HideOutline();
                currentInteractable = null;
            }
        }

        /// <summary>
        /// 상호작용 버튼이 눌렸을 때 호출
        /// </summary>
        public void OnInteract()
        {
            if (currentInteractable?.CanInteract() == true)
            {
                currentInteractable.OnInteract();
            }
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
        }

        /// <summary>
        /// 전시관 열기 (WorldShop에서 호출)
        /// </summary>
        public void OpenExhibition()
        {
            if (exhibitionUI != null)
            {
                exhibitionUI.OpenExhibitionPanel();
            }
        }

        /// <summary>
        /// 비둘기 연구소 열기 (WorldShop에서 호출)
        /// </summary>
        public void OpenPigeonResearch()
        {
            if (pigeonResearchUI != null)
            {
                pigeonResearchUI.OpenShopPanel();
            }
        }

        /// <summary>
        /// 업그레이드 상점 열기 (WorldShop에서 호출)
        /// </summary>
        public void OpenUpgradeShop()
        {
            if (upgradeShopUI != null)
            {
                upgradeShopUI.OpenShopPanel();
            }
        }

        /// <summary>
        /// 다리 게이트 구매 패널 열기 (BridgeGate에서 호출)
        /// </summary>
        public void OpenBridgeGatePurchase(BridgeGate gate, int areaNumber, int cost, string areaName = null)
        {
            if (bridgeGatePurchaseUI != null)
            {
                bridgeGatePurchaseUI.OpenPurchasePanel(gate, areaNumber, cost, areaName);
            }
        }

        /// <summary>
        /// 현재 상호작용 가능한 오브젝트가 있는지 확인
        /// </summary>
        public bool CanInteract()
        {
            return currentInteractable != null && currentInteractable.CanInteract();
        }

        /// <summary>
        /// 상호작용 가능한 오브젝트에 반투명 하이라이트 표시
        /// </summary>
        private void ShowOutline(IInteractable interactable)
        {
            // 기존 하이라이트 제거
            HideOutline();

            // IInteractable이 MonoBehaviour인지 확인
            if (interactable is MonoBehaviour monoBehaviour)
            {
                GameObject targetObject = monoBehaviour.gameObject;
                
                // SpriteRenderer 찾기
                SpriteRenderer spriteRenderer = targetObject.GetComponent<SpriteRenderer>();
                if (spriteRenderer == null)
                {
                    spriteRenderer = targetObject.GetComponentInChildren<SpriteRenderer>();
                }

                if (spriteRenderer != null && spriteRenderer.sprite != null)
                {
                    // 반투명 오버레이용 SpriteRenderer 생성
                    GameObject highlightObj = new GameObject("InteractionHighlight");
                    highlightObj.transform.SetParent(targetObject.transform, false);
                    highlightObj.transform.localPosition = Vector3.zero;
                    highlightObj.transform.localScale = Vector3.one;
                    highlightObj.transform.localRotation = Quaternion.identity;

                    SpriteRenderer highlightRenderer = highlightObj.AddComponent<SpriteRenderer>();
                    highlightRenderer.sprite = spriteRenderer.sprite;
                    highlightRenderer.color = highlightColor; // 반투명 노란색
                    highlightRenderer.sortingOrder = spriteRenderer.sortingOrder + 1; // 앞에 배치
                    highlightRenderer.sortingLayerName = spriteRenderer.sortingLayerName;

                    currentOutlineObject = highlightObj;
                }
            }
        }

        /// <summary>
        /// 하이라이트 제거
        /// </summary>
        private void HideOutline()
        {
            if (currentOutlineObject != null)
            {
                Destroy(currentOutlineObject);
                currentOutlineObject = null;
            }
        }
    }
}