using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PigeonGame.UI
{
    /// <summary>
    /// 비둘기 확률 조정 슬롯 UI 컴포넌트
    /// 좌우 버튼으로 종 선택 시 자동으로 확률 조정
    /// </summary>
    public class SpeciesWeightSlotUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI slotTypeText; // "증가" 또는 "감소"
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI multiplierText;
        [SerializeField] private Button leftButton; // 이전 종
        [SerializeField] private Button rightButton; // 다음 종

        public TextMeshProUGUI SlotTypeText => slotTypeText;
        public TextMeshProUGUI NameText => nameText;
        public TextMeshProUGUI MultiplierText => multiplierText;
        public Button LeftButton => leftButton;
        public Button RightButton => rightButton;
    }
}
