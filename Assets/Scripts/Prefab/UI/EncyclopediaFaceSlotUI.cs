using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PigeonGame.UI
{
    public class EncyclopediaFaceSlotUI : MonoBehaviour
    {
        [Header("Face Slot Components")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private GameObject lockOverlay;
        [SerializeField] private Button button; 

        public Image BackgroundImage => backgroundImage;
        public TextMeshProUGUI NameText => nameText;
        public TextMeshProUGUI StatusText => statusText;
        public GameObject LockOverlay => lockOverlay;
        public Button Button => button;
    }
}
