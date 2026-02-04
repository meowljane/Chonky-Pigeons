using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PigeonGame.UI
{
    public class TrapShopSlotUI : MonoBehaviour
    {
        [Header("Slot Components")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI preferenceText;
        [SerializeField] private Button buyButton;
        [SerializeField] private TextMeshProUGUI buttonText;

        public Image IconImage => iconImage;
        public TextMeshProUGUI NameText => nameText;
        public TextMeshProUGUI PreferenceText => preferenceText;
        public Button BuyButton => buyButton;
        public TextMeshProUGUI ButtonText => buttonText;
    }
}
