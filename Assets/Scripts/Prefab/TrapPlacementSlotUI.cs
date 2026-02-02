using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PigeonGame.UI
{
    /// <summary>
    /// 덫 설치 슬롯 UI 컴포넌트
    /// 프리팹 내부의 UI 요소들을 미리 참조하여 저장
    /// </summary>
    public class TrapPlacementSlotUI : MonoBehaviour
    {
        [Header("Slot Components")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private GameObject checkmark;
        [SerializeField] private GameObject lockOverlay;
        [SerializeField] private Button button;

        public Image BackgroundImage => backgroundImage;
        public Image IconImage => iconImage;
        public TextMeshProUGUI NameText => nameText;
        public GameObject Checkmark => checkmark;
        public GameObject LockOverlay => lockOverlay;
        public Button Button => button;

        public void SetUnlocked(bool isUnlocked)
        {
            if (lockOverlay != null)
            {
                lockOverlay.SetActive(!isUnlocked);
            }

            if (button != null)
            {
                button.interactable = isUnlocked;
            }
        }

        public void SetSelected(bool isSelected)
        {
            if (checkmark != null)
            {
                checkmark.SetActive(isSelected);
            }
        }
    }
}
