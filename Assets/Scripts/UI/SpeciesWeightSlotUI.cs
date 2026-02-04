using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PigeonGame.UI
{
    public class SpeciesWeightSlotUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI slotTypeText; 
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI multiplierText;
        [SerializeField] private Button leftButton; 
        [SerializeField] private Button rightButton; 

        public TextMeshProUGUI SlotTypeText => slotTypeText;
        public TextMeshProUGUI NameText => nameText;
        public TextMeshProUGUI MultiplierText => multiplierText;
        public Button LeftButton => leftButton;
        public Button RightButton => rightButton;
    }
}
