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
                    return;
            }

            // GameManager 이벤트 구독
            if (Gameplay.GameManager.Instance != null)
            {
                Gameplay.GameManager.Instance.OnMoneyChanged += UpdateMoneyDisplay;
                // 초기 값 표시
                UpdateMoneyDisplay(Gameplay.GameManager.Instance.CurrentMoney);
            }
            else
            {
                // GameManager가 아직 초기화되지 않았을 수 있으므로 코루틴으로 재시도
                StartCoroutine(WaitForGameManager());
            }
        }

        private System.Collections.IEnumerator WaitForGameManager()
        {
            // GameManager가 초기화될 때까지 대기
            while (Gameplay.GameManager.Instance == null)
            {
                yield return null;
            }

            // 이벤트 구독 및 초기 값 표시
            Gameplay.GameManager.Instance.OnMoneyChanged += UpdateMoneyDisplay;
            UpdateMoneyDisplay(Gameplay.GameManager.Instance.CurrentMoney);
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
                // moneyFormat이 "{0}" 형식이면 string.Format 사용, 아니면 직접 대체
                if (moneyFormat.Contains("{0}"))
                {
                    moneyText.text = string.Format(moneyFormat, money);
                }
                else
                {
                    // "0G" 같은 형식의 경우 숫자만 표시
                    moneyText.text = money.ToString();
                }
            }
        }
    }
}



