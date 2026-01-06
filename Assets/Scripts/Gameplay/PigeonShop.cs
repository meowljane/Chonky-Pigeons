using UnityEngine;
using PigeonGame.Data;

namespace PigeonGame.Gameplay
{
    /// <summary>
    /// 비둘기 판매를 관리하는 시스템
    /// </summary>
    public class PigeonShop : MonoBehaviour
    {
        /// <summary>
        /// 특정 인덱스의 비둘기 판매
        /// </summary>
        public bool SellPigeon(int index)
        {
            if (GameManager.Instance == null)
            {
                Debug.LogError("GameManager를 찾을 수 없습니다!");
                return false;
            }

            return GameManager.Instance.SellPigeon(index);
        }

        /// <summary>
        /// 모든 비둘기 판매
        /// </summary>
        public int SellAllPigeons()
        {
            if (GameManager.Instance == null)
            {
                Debug.LogError("GameManager를 찾을 수 없습니다!");
                return 0;
            }

            return GameManager.Instance.SellAllPigeons();
        }

        /// <summary>
        /// 인벤토리 가져오기
        /// </summary>
        public System.Collections.Generic.IReadOnlyList<PigeonInstanceStats> GetInventory()
        {
            if (GameManager.Instance == null)
                return new System.Collections.Generic.List<PigeonInstanceStats>();

            return GameManager.Instance.Inventory;
        }

        /// <summary>
        /// 인벤토리 개수 가져오기
        /// </summary>
        public int GetInventoryCount()
        {
            if (GameManager.Instance == null)
                return 0;

            return GameManager.Instance.InventoryCount;
        }
    }
}



