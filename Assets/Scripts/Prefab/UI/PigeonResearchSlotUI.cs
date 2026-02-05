using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PigeonGame.UI
{
    public class PigeonResearchSlotUI : MonoBehaviour
    {
        [Header("Slot Components")]
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private Image iconImage; 
        [SerializeField] private Image faceIconImage; 
        [SerializeField] private Button buyButton;
        [SerializeField] private TextMeshProUGUI buttonText;

        public TextMeshProUGUI NameText => nameText;
        public Image IconImage => iconImage;
        public Image FaceIconImage => faceIconImage;
        public Button BuyButton => buyButton;
        public TextMeshProUGUI ButtonText => buttonText;
    }
}
