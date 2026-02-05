using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PigeonGame.Data;

namespace PigeonGame.UI
{
    public class PigeonShopSlotUI : MonoBehaviour
    {
        [Header("Slot Components")]
        [SerializeField] private Image iconImage; 
        [SerializeField] private Image faceIconImage; 
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

        public void Setup(PigeonInstanceStats stats, int index, 
            System.Action<PigeonInstanceStats> detailCallback, 
            System.Action<int> sellCallback)
        {
            currentStats = stats;
            itemIndex = index;
            onDetailClick = detailCallback;
            onSellClick = sellCallback;

            var registry = GameDataRegistry.Instance;
            var species = registry?.SpeciesSet?.GetSpeciesById(stats.speciesId);
            var face = registry?.Faces?.GetFaceById(stats.faceId);
            var defaultSpecies = registry?.SpeciesSet?.GetSpeciesById(PigeonSpecies.SP01);
            var defaultFace = registry?.Faces?.GetFaceById(FaceType.F00);

            var iconToUse = species?.icon ?? defaultSpecies?.icon;
            if (iconToUse != null)
            {
                iconImage.sprite = iconToUse;
                iconImage.enabled = true;
            }

            var faceIconToUse = face?.icon ?? defaultFace?.icon;
            if (faceIconToUse != null)
            {
                faceIconImage.sprite = faceIconToUse;
                faceIconImage.enabled = true;
            }

            nameText.text = species?.name ?? stats.speciesId.ToString();

            detailButton.onClick.RemoveAllListeners();
            detailButton.onClick.AddListener(OnDetailButtonClicked);

            sellButton.onClick.RemoveAllListeners();
            sellButton.onClick.AddListener(OnSellButtonClicked);
            sellButtonText.text = $"판매\n{stats.price}G";
        }

        private void OnDetailButtonClicked()
        {
            onDetailClick?.Invoke(currentStats);
        }

        private void OnSellButtonClicked()
        {
            onSellClick?.Invoke(itemIndex);
        }
    }
}
