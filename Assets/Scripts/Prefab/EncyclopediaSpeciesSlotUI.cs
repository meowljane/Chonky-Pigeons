using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PigeonGame.UI
{
    /// <summary>
    /// 도감 종 슬롯 UI 컴포넌트
    /// 프리팹 내부의 UI 요소들을 미리 참조하여 저장
    /// </summary>
    public class EncyclopediaSpeciesSlotUI : MonoBehaviour
    {
        [Header("Species Slot Components")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private GameObject lockOverlay;
        [SerializeField] private Button button;

        public Image BackgroundImage => backgroundImage;
        public Image IconImage => iconImage;
        public TextMeshProUGUI NameText => nameText;
        public GameObject LockOverlay => lockOverlay;
        public Button Button => button;
    }
}
