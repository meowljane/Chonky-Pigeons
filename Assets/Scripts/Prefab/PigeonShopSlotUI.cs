using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PigeonGame.Data;

namespace PigeonGame.UI
{
    /// <summary>
    /// 비둘기 상점 슬롯 UI 컴포넌트
    /// 프리팹 내부의 UI 요소들을 미리 참조하여 저장
    /// </summary>
    public class PigeonShopSlotUI : MonoBehaviour
    {
        [Header("Slot Components")]
        [SerializeField] private Image iconImage; // Species 아이콘 또는 기본 표정이 적용된 몸+표정 이미지
        [SerializeField] private Image faceIconImage; // Face 아이콘 (몸+표정 합쳐진 이미지, 선택적)
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private Button detailButton;
        [SerializeField] private Button sellButton;
        [SerializeField] private TextMeshProUGUI sellButtonText;

        private PigeonInstanceStats currentStats;
        private System.Action<PigeonInstanceStats> onDetailClick;
        private System.Action<int> onSellClick;
        private int itemIndex;

        public Image IconImage => iconImage;
        public Image FaceIconImage => faceIconImage;
        public TextMeshProUGUI NameText => nameText;
        public Button DetailButton => detailButton;
        public Button SellButton => sellButton;
        public TextMeshProUGUI SellButtonText => sellButtonText;

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

            var registry = GameDataRegistry.Instance;
            var species = (registry != null && registry.SpeciesSet != null) 
                ? registry.SpeciesSet.GetSpeciesById(stats.speciesId) 
                : null;
            var face = (registry != null && registry.Faces != null)
                ? registry.Faces.GetFaceById(stats.faceId)
                : null;

            // 기본값 설정
            var defaultSpecies = (registry != null && registry.SpeciesSet != null)
                ? registry.SpeciesSet.GetSpeciesById(PigeonSpecies.SP01)
                : null;
            var defaultFace = (registry != null && registry.Faces != null)
                ? registry.Faces.GetFaceById(FaceType.F00)
                : null;

            // IconImage: Species icon 표시 (없으면 기본값 SP01 사용)
            if (iconImage != null)
            {
                var iconToUse = species?.icon ?? defaultSpecies?.icon;
                if (iconToUse != null)
                {
                    iconImage.sprite = iconToUse;
                    iconImage.enabled = true;
                }
            }

            // FaceIconImage: Face icon 표시 (없으면 기본값 F00 사용)
            if (faceIconImage != null)
            {
                var faceIconToUse = face?.icon ?? defaultFace?.icon;
                if (faceIconToUse != null)
                {
                    faceIconImage.sprite = faceIconToUse;
                    faceIconImage.enabled = true;
                }
            }

            // 종 이름 표시
            if (nameText != null)
            {
                nameText.text = species != null ? species.name : stats.speciesId.ToString();
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

                // 판매 버튼 텍스트
                if (sellButtonText != null)
                {
                    sellButtonText.text = $"판매\n{stats.price}G";
                }
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
