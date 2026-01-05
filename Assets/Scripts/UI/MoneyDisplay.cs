using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PigeonGame.UI
{
    /// <summary>
    /// 돈 표시 UI
    /// </summary>
    public class MoneyDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI moneyText;
        [SerializeField] private string moneyFormat = "돈: {0}";

        private void Start()
        {
            if (moneyText == null)
            {
                moneyText = GetComponent<TextMeshProUGUI>();
                if (moneyText == null)
                {
                    Debug.LogError("MoneyDisplay: TextMeshProUGUI를 찾을 수 없습니다!");
                    return;
                }
            }

            // GameManager 이벤트 구독
            if (Gameplay.GameManager.Instance != null)
            {
                Gameplay.GameManager.Instance.OnMoneyChanged += UpdateMoneyDisplay;
                UpdateMoneyDisplay(Gameplay.GameManager.Instance.CurrentMoney);
            }
            else
            {
                Debug.LogWarning("MoneyDisplay: GameManager를 찾을 수 없습니다!");
            }
        }

        private void OnDestroy()
        {
            // 이벤트 구독 해제
            if (Gameplay.GameManager.Instance != null)
            {
                Gameplay.GameManager.Instance.OnMoneyChanged -= UpdateMoneyDisplay;
            }
        }

        private void UpdateMoneyDisplay(int money)
        {
            if (moneyText != null)
            {
                moneyText.text = string.Format(moneyFormat, money);
            }
        }
    }
}

