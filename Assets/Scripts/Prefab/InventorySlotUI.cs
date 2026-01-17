using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PigeonGame.UI
{
    /// <summary>
    /// 인벤토리 슬롯 UI 컴포넌트
    /// 프리팹 내부의 UI 요소들을 미리 참조하여 저장
    /// </summary>
    public class InventorySlotUI : MonoBehaviour
    {
        [Header("Slot Components")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private Button button;

        public Image IconImage => iconImage;
        public TextMeshProUGUI NameText => nameText;
        public Button Button => button;
    }
}
