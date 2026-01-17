using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PigeonGame.UI
{
    /// <summary>
    /// 도감 얼굴 슬롯 UI 컴포넌트
    /// 프리팹 내부의 UI 요소들을 미리 참조하여 저장
    /// </summary>
    public class EncyclopediaFaceSlotUI : MonoBehaviour
    {
        [Header("Face Slot Components")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private GameObject lockOverlay;

        public Image BackgroundImage => backgroundImage;
        public TextMeshProUGUI NameText => nameText;
        public TextMeshProUGUI StatusText => statusText;
        public GameObject LockOverlay => lockOverlay;
    }
}
