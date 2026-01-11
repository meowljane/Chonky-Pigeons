using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PigeonGame.Data;

namespace PigeonGame.UI
{
    /// <summary>
    /// 상점 아이템 UI 컴포넌트 (직접 참조용)
    /// </summary>
    public class ShopItemUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private TextMeshProUGUI obesityText;
        [SerializeField] private Button detailButton;
        [SerializeField] private Button sellButton;

        private PigeonInstanceStats currentStats;
        private System.Action<PigeonInstanceStats> onDetailClick;
        private System.Action<int> onSellClick;
        private int itemIndex;

        /// <summary>
        /// 아이템 정보 설정
        /// </summary>
        public void Setup(PigeonInstanceStats stats, int index, 
            System.Action<PigeonInstanceStats> detailCallback, 
            System.Action<int> sellCallback)
        {
            currentStats = stats;
            itemIndex = index;
            onDetailClick = detailCallback;
            onSellClick = sellCallback;

            // 종 이름 표시
            if (nameText != null)
            {
                var registry = GameDataRegistry.Instance;
                if (registry != null && registry.SpeciesSet != null)
                {
                    var species = registry.SpeciesSet.GetSpeciesById(stats.speciesId);
                    nameText.text = species != null ? species.name : stats.speciesId;
                }
                else
                {
                    nameText.text = stats.speciesId;
                }
            }

            // 가격 표시
            if (priceText != null)
            {
                priceText.text = $"가격: {stats.price}";
            }

            // 비만도 표시
            if (obesityText != null)
            {
                obesityText.text = $"비만도: {stats.obesity}";
            }

            // 상세정보 보기 버튼
            if (detailButton != null)
            {
                detailButton.onClick.RemoveAllListeners();
                detailButton.onClick.AddListener(OnDetailButtonClicked);
            }

            // 판매 버튼
            if (sellButton != null)
            {
                sellButton.onClick.RemoveAllListeners();
                sellButton.onClick.AddListener(OnSellButtonClicked);
            }
        }

        private void OnDetailButtonClicked()
        {
            if (currentStats != null && onDetailClick != null)
            {
                onDetailClick(currentStats);
            }
        }

        private void OnSellButtonClicked()
        {
            if (onSellClick != null)
            {
                onSellClick(itemIndex);
            }
        }
    }
}
