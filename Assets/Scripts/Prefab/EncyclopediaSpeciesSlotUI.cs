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
        [SerializeField] private Image iconImage; // Species 아이콘 (또는 기본 표정이 적용된 몸+표정 이미지)
        [SerializeField] private Image faceIconImage; // Face 아이콘 (몸+표정 합쳐진 이미지, 선택적)
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
