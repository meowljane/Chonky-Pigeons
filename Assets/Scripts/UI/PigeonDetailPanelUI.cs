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
            UIHelper.SafeAddListener(detailCloseButton, ClosePanel);
            UIHelper.SafeAddListener(moveButton, OnMoveButtonClicked);

            if (detailPanel != null)
            {
                detailPanel.SetActive(false);
            }
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

            var defaultSpecies = registry.SpeciesSet.GetSpeciesById(PigeonSpecies.SP01);
            var defaultFace = registry.Faces != null ? registry.Faces.GetFaceById(FaceType.F00) : null;

            currentStats = stats;
            onClosed = onClosedCallback;
            onMoveClicked = onMoveCallback;

            detailPanel.SetActive(true);

            if (detailIconImage != null)
            {
                var iconToUse = species?.icon ?? defaultSpecies?.icon;
                if (iconToUse != null)
                {
                    detailIconImage.sprite = iconToUse;
                    detailIconImage.enabled = true;
                }
            }

            if (detailFaceIconImage != null)
            {
                var faceIconToUse = face?.icon ?? defaultFace?.icon;
                if (faceIconToUse != null)
                {
                    detailFaceIconImage.sprite = faceIconToUse;
                    detailFaceIconImage.enabled = true;
                }
            }

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
                if (showMoveButton && !string.IsNullOrEmpty(moveButtonText))
                {
                    if (moveButtonTextComponent != null)
                    {
                        moveButtonTextComponent.text = moveButtonText;
                    }
                    else
                    {
                        Debug.LogWarning("MoveButtonTextComponent가 할당되지 않았습니다. 버튼 텍스트가 업데이트되지 않습니다.", this);
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
            UIHelper.SafeRemoveListener(detailCloseButton);
            UIHelper.SafeRemoveListener(moveButton);
        }
    }
}
