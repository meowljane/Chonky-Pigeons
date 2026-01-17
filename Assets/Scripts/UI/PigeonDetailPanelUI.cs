using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PigeonGame.Data;

namespace PigeonGame.UI
{
    /// <summary>
    /// 비둘기 상세 정보 패널 UI
    /// 여러 UI에서 공통으로 사용되는 디테일 패널
    /// </summary>
    public class PigeonDetailPanelUI : MonoBehaviour
    {
        [Header("Detail Panel")]
        [SerializeField] private GameObject detailPanel;
        [SerializeField] private Image detailIconImage;
        [SerializeField] private TextMeshProUGUI detailNameText;
        [SerializeField] private TextMeshProUGUI detailWeightText;
        [SerializeField] private TextMeshProUGUI detailPriceText;
        [SerializeField] private TextMeshProUGUI detailRarityText;
        [SerializeField] private Button detailCloseButton;

        [Header("Move Buttons (Optional)")]
        [SerializeField] private Button moveButton; // 전시관/인벤토리 이동 버튼 (ExhibitionUI에서만 사용)

        private PigeonInstanceStats currentStats;
        private System.Action<PigeonInstanceStats> onClosed;
        private System.Action<PigeonInstanceStats> onMoveClicked;

        private void Start()
        {
            if (detailCloseButton != null)
            {
                detailCloseButton.onClick.RemoveAllListeners();
                detailCloseButton.onClick.AddListener(ClosePanel);
            }

            if (moveButton != null)
            {
                moveButton.onClick.RemoveAllListeners();
                moveButton.onClick.AddListener(OnMoveButtonClicked);
            }

            if (detailPanel != null)
            {
                detailPanel.SetActive(false);
            }
        }

        /// <summary>
        /// 상세 정보 표시
        /// </summary>
        public void ShowDetail(PigeonInstanceStats stats, System.Action<PigeonInstanceStats> onClosedCallback = null, 
            System.Action<PigeonInstanceStats> onMoveCallback = null, bool showMoveButton = false, string moveButtonText = "")
        {
            if (stats == null || detailPanel == null)
                return;

            var registry = GameDataRegistry.Instance;
            if (registry == null || registry.SpeciesSet == null)
                return;

            var species = registry.SpeciesSet.GetSpeciesById(stats.speciesId);
            if (species == null)
                return;

            var face = registry.Faces != null ? registry.Faces.GetFaceById(stats.faceId) : null;

            currentStats = stats;
            onClosed = onClosedCallback;
            onMoveClicked = onMoveCallback;

            detailPanel.SetActive(true);

            // 아이콘
            if (detailIconImage != null)
            {
                if (species.icon != null)
                {
                    detailIconImage.sprite = species.icon;
                    detailIconImage.enabled = true;
                }
                else
                {
                    detailIconImage.enabled = false;
                }
            }

            // 종 이름 (얼굴 포함)
            if (detailNameText != null)
            {
                string faceName = face != null ? face.name : stats.faceId.ToString();
                detailNameText.text = $"{species.name}({faceName})";
            }

            // 무게
            if (detailWeightText != null)
            {
                detailWeightText.text = $"무게: {stats.weight:F1}kg";
            }

            // 가격
            if (detailPriceText != null)
            {
                detailPriceText.text = $"가격: {stats.price}";
            }

            // 희귀도
            if (detailRarityText != null)
            {
                detailRarityText.text = $"희귀도: {species.rarityTier}";
            }

            // 이동 버튼 표시/숨김 및 텍스트 설정
            if (moveButton != null)
            {
                moveButton.gameObject.SetActive(showMoveButton);
                if (showMoveButton && !string.IsNullOrEmpty(moveButtonText))
                {
                    TextMeshProUGUI buttonText = moveButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null)
                    {
                        buttonText.text = moveButtonText;
                    }
                }
            }
        }

        private void OnMoveButtonClicked()
        {
            if (currentStats != null && onMoveClicked != null)
            {
                onMoveClicked(currentStats);
            }
        }

        public void ClosePanel()
        {
            if (detailPanel != null)
            {
                detailPanel.SetActive(false);
            }

            if (onClosed != null && currentStats != null)
            {
                var stats = currentStats;
                onClosed.Invoke(stats);
                onClosed = null;
            }

            currentStats = null;
            onMoveClicked = null;
        }

        private void OnDestroy()
        {
            if (detailCloseButton != null)
            {
                detailCloseButton.onClick.RemoveAllListeners();
            }

            if (moveButton != null)
            {
                moveButton.onClick.RemoveAllListeners();
            }
        }
    }
}
