using UnityEngine;
using PigeonGame.UI;

namespace PigeonGame.Gameplay
{
    /// <summary>
    /// 다리 게이트 오브젝트
    /// 상호작용으로 구매 패널을 열고, 구매 후 게이트가 사라져 다리를 건널 수 있게 함
    /// </summary>
    public class BridgeGate : InteractableBase
    {
        [Header("Gate Settings")]
        [SerializeField] private int areaNumber = 2; // 지역 번호 (2, 3, 4, 5)
        [SerializeField] private int unlockCost = 200; // 해금 비용 (Region 2: 200, Region 3: 550, Region 4: 1200, Region 5: 2500)
        [SerializeField] private GameObject gateVisual; // 게이트 시각적 오브젝트 (문 등)
        [SerializeField] private Collider2D gateCollider; // 게이트 콜라이더 (다리를 막는 콜라이더)

        protected override void Start()
        {
            base.Start();

            // 게이트 시각적 오브젝트가 없으면 자동으로 찾기
            if (gateVisual == null)
            {
                gateVisual = gameObject;
            }

            // 게이트 콜라이더가 없으면 자동으로 찾기
            if (gateCollider == null)
            {
                gateCollider = GetComponent<Collider2D>();
            }

            // 이미 해금된 지역이면 게이트 제거
            if (MapManager.Instance?.IsAreaUnlocked(areaNumber) == true)
            {
                UnlockGate();
            }
        }

        public override void OnInteract()
        {
            if (!CanInteract())
                return;

            // 이미 해금된 지역이면 상호작용 불가
            if (MapManager.Instance?.IsAreaUnlocked(areaNumber) == true)
            {
                return;
            }

            // MapManager에서 지역 이름 가져오기
            string areaName = MapManager.Instance?.GetAreaName(areaNumber);

            // InteractionSystem을 통해 구매 패널 열기
            InteractionSystem.Instance?.OpenBridgeGatePurchase(this, areaNumber, unlockCost, areaName);
        }

        public override bool CanInteract()
        {
            // 이미 해금된 지역이면 상호작용 불가
            if (MapManager.Instance?.IsAreaUnlocked(areaNumber) == true)
            {
                return false;
            }

            return base.CanInteract();
        }

        /// <summary>
        /// 게이트 해제 (구매 후 호출)
        /// </summary>
        public void UnlockGate()
        {
            // 게이트 오브젝트 비활성화
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 지역 번호 가져오기
        /// </summary>
        public int AreaNumber => areaNumber;

        /// <summary>
        /// 해금 비용 가져오기
        /// </summary>
        public int UnlockCost => unlockCost;

        /// <summary>
        /// 게이트 콜라이더 가져오기
        /// </summary>
        public Collider2D GateCollider => gateCollider;
    }
}
