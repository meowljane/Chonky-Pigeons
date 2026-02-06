using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PigeonGame.Data;

namespace PigeonGame.UI
{
    public class PigeonDetailPanelUI : MonoBehaviour
    {
        [Header("Detail Panel")]
        [SerializeField] private GameObject detailPanel;
        [SerializeField] private Image detailIconImage; 
        [SerializeField] private Image detailFaceIconImage; 
        [SerializeField] private TextMeshProUGUI detailNameText;
        [SerializeField] private TextMeshProUGUI detailWeightText;
        [SerializeField] private TextMeshProUGUI detailPriceText;
        [SerializeField] private TextMeshProUGUI detailRarityText;
        [SerializeField] private Button detailCloseButton;

        [Header("Move Buttons (Optional)")]
        [SerializeField] private Button moveButton; 
        [SerializeField] private TextMeshProUGUI moveButtonTextComponent; 

        private PigeonInstanceStats currentStats;
        private System.Action<PigeonInstanceStats> onClosed;
        private System.Action<PigeonInstanceStats> onMoveClicked;

        private void Start()
        {
            PanelUIHelper.InitializePanel(detailPanel, detailCloseButton, ClosePanel);
            UIHelper.SafeAddListener(moveButton, OnMoveButtonClicked);
        }

        public void ShowDetail(PigeonInstanceStats stats, System.Action<PigeonInstanceStats> onClosedCallback = null, 
            System.Action<PigeonInstanceStats> onMoveCallback = null, bool showMoveButton = false, string moveButtonText = "")
        {
            if (stats == null || detailPanel == null)
                return;

            var registry = GameDataRegistry.Instance;
            if (registry == null || registry.SpeciesSet == null)
                return;

            var species = registry.SpeciesSet.GetSpeciesById(stats.speciesId);
            var face = registry.Faces != null ? registry.Faces.GetFaceById(stats.faceId) : null;

            var defaultSpecies = UIHelper.GetDefaultSpecies();
            var defaultFace = UIHelper.GetDefaultFace();

            currentStats = stats;
            onClosed = onClosedCallback;
            onMoveClicked = onMoveCallback;

            PanelUIHelper.OpenPanel(detailPanel);

            UIHelper.SetSpeciesIcon(detailIconImage, species);
            UIHelper.SetFaceIcon(detailFaceIconImage, face);

            if (detailNameText != null)
            {
                string speciesName = species?.name ?? defaultSpecies?.name ?? stats.speciesId.ToString();
                string faceName = face?.name ?? defaultFace?.name ?? stats.faceId.ToString();
                detailNameText.text = $"{speciesName}({faceName})";
            }

            if (detailWeightText != null)
            {
                detailWeightText.text = $"무게: {stats.weight:F1}kg";
            }

            if (detailPriceText != null)
            {
                detailPriceText.text = $"가격: {stats.price}";
            }

            if (detailRarityText != null)
            {
                detailRarityText.text = $"희귀도: {species.rarityTier}";
            }

            if (moveButton != null)
            {
                moveButton.gameObject.SetActive(showMoveButton);
                if (showMoveButton && !string.IsNullOrEmpty(moveButtonText) && moveButtonTextComponent != null)
                    moveButtonTextComponent.text = moveButtonText;
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
            PanelUIHelper.ClosePanel(detailPanel);

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
            UIHelper.SafeRemoveListener(detailCloseButton);
            UIHelper.SafeRemoveListener(moveButton);
        }
    }
}
