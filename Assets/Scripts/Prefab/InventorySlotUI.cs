using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PigeonGame.UI
{
    public class InventorySlotUI : MonoBehaviour
    {
        [Header("Slot Components")]
        [SerializeField] private Image iconImage; 
        [SerializeField] private Image faceIconImage; 
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private Button button;

        public Image IconImage => iconImage;
        public Image FaceIconImage => faceIconImage;
        public TextMeshProUGUI NameText => nameText;
        public Button Button => button;
    }
}
