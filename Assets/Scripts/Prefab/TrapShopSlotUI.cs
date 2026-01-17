using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PigeonGame.UI
{
    /// <summary>
    /// 덫 상점 슬롯 UI 컴포넌트
    /// 프리팹 내부의 UI 요소들을 미리 참조하여 저장
    /// </summary>
    public class TrapShopSlotUI : MonoBehaviour
    {
        [Header("Slot Components")]
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI preferenceText;
        [SerializeField] private Button buyButton;
        [SerializeField] private TextMeshProUGUI buttonText;

        public TextMeshProUGUI NameText => nameText;
        public TextMeshProUGUI PreferenceText => preferenceText;
        public Button BuyButton => buyButton;
        public TextMeshProUGUI ButtonText => buttonText;
    }
}
