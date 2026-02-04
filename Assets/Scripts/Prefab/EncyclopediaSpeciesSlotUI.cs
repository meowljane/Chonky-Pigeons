using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PigeonGame.UI
{
    public class EncyclopediaSpeciesSlotUI : MonoBehaviour
    {
        [Header("Species Slot Components")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image iconImage; 
        [SerializeField] private Image faceIconImage; 
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private GameObject lockOverlay;
        [SerializeField] private Button button;

        public Image BackgroundImage => backgroundImage;
        public Image IconImage => iconImage;
        public Image FaceIconImage => faceIconImage;
        public TextMeshProUGUI NameText => nameText;
        public GameObject LockOverlay => lockOverlay;
        public Button Button => button;
    }
}
