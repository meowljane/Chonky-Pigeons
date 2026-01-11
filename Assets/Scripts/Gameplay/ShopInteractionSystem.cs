using UnityEngine;
using PigeonGame.UI;

namespace PigeonGame.Gameplay
{
    /// <summary>
    /// 상점 상호작용 시스템
    /// 플레이어가 상점 근처에 있을 때 상호작용 버튼으로 패널 열기
    /// </summary>
    public class ShopInteractionSystem : MonoBehaviour
    {
        [SerializeField] private float detectionRadius = 5f;
        [SerializeField] private bool showDebugGizmos = true;

        private WorldShop nearestShop;
        private PigeonShopUI pigeonShopUI;
        private TrapShopUI trapShopUI;

        private void Start()
        {
            pigeonShopUI = FindObjectOfType<PigeonShopUI>();
            trapShopUI = FindObjectOfType<TrapShopUI>();
        }

        private void Update()
        {
            // 가장 가까운 상점 찾기
            FindNearestShop();
        }

        private void FindNearestShop()
        {
            WorldShop[] shops = FindObjectsOfType<WorldShop>();
            nearestShop = null;
            float nearestDistance = float.MaxValue;

            foreach (var shop in shops)
            {
                float distance = shop.GetDistanceToPlayer();
                if (distance <= detectionRadius && distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestShop = shop;
                }
            }
        }

        /// <summary>
        /// 상호작용 버튼이 눌렸을 때 호출
        /// </summary>
        public void OnInteract()
        {
            if (nearestShop == null)
            {
                Debug.Log("근처에 상점이 없습니다.");
                return;
            }

            if (!nearestShop.IsPlayerNearby())
            {
                Debug.Log("상점이 너무 멀리 있습니다.");
                return;
            }

            // 상점 타입에 따라 다른 패널 열기
            switch (nearestShop.Type)
            {
                case WorldShop.ShopType.PigeonShop:
                    OpenPigeonShop();
                    break;
                case WorldShop.ShopType.TrapShop:
                    OpenTrapShop();
                    break;
            }
        }

        private void OpenPigeonShop()
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

        private void OpenTrapShop()
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
        /// 현재 상호작용 가능한 상점이 있는지 확인
        /// </summary>
        public bool CanInteract()
        {
            return nearestShop != null && nearestShop.IsPlayerNearby();
        }

        /// <summary>
        /// 가장 가까운 상점 반환
        /// </summary>
        public WorldShop GetNearestShop()
        {
            return nearestShop;
        }

        private void OnDrawGizmosSelected()
        {
            if (!showDebugGizmos)
                return;

            Gizmos.color = Color.yellow;
            if (PlayerController.Instance != null)
            {
                Gizmos.DrawWireSphere(PlayerController.Instance.Position, detectionRadius);
            }

            if (nearestShop != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(
                    PlayerController.Instance != null ? PlayerController.Instance.Position : transform.position,
                    nearestShop.transform.position
                );
            }
        }
    }
}

