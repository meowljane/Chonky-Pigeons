using UnityEngine;
using PigeonGame.Data;

namespace PigeonGame.Gameplay
{
    /// <summary>
    /// 덫 구매/해금을 관리하는 시스템
    /// </summary>
    public class TrapShop : MonoBehaviour
    {
        /// <summary>
        /// 덫 구매/해금 시도
        /// </summary>
        public bool TryPurchaseTrap(string trapId)
        {
            if (GameManager.Instance == null)
            {
                Debug.LogError("GameManager를 찾을 수 없습니다!");
                return false;
            }

            return GameManager.Instance.UnlockTrap(trapId);
        }

        /// <summary>
        /// 덫이 구매 가능한지 확인 (돈이 충분한지)
        /// </summary>
        public bool CanAffordTrap(string trapId)
        {
            if (GameManager.Instance == null)
                return false;

            var registry = GameDataRegistry.Instance;
            if (registry == null || registry.Traps == null)
                return false;

            var trapData = registry.Traps.GetTrapById(trapId);
            if (trapData == null)
                return false;

            return GameManager.Instance.CurrentMoney >= trapData.cost;
        }

        /// <summary>
        /// 덫이 이미 해금되어 있는지 확인
        /// </summary>
        public bool IsTrapUnlocked(string trapId)
        {
            if (GameManager.Instance == null)
                return false;

            return GameManager.Instance.IsTrapUnlocked(trapId);
        }

        /// <summary>
        /// 모든 덫 정보 가져오기
        /// </summary>
        public TrapDefinition[] GetAllTraps()
        {
            var registry = GameDataRegistry.Instance;
            if (registry == null || registry.Traps == null)
                return new TrapDefinition[0];

            return registry.Traps.traps;
        }
    }
}



