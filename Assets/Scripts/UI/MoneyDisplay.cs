using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PigeonGame.UI
{
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

            if (Gameplay.GameManager.Instance != null)
            {
                Gameplay.GameManager.Instance.OnMoneyChanged += UpdateMoneyDisplay;
                UpdateMoneyDisplay(Gameplay.GameManager.Instance.CurrentMoney);
            }
            else
            {
                StartCoroutine(WaitForGameManager());
            }
        }

        private System.Collections.IEnumerator WaitForGameManager()
        {
            while (Gameplay.GameManager.Instance == null)
            {
                yield return null;
            }

            Gameplay.GameManager.Instance.OnMoneyChanged += UpdateMoneyDisplay;
            UpdateMoneyDisplay(Gameplay.GameManager.Instance.CurrentMoney);
        }

        private void OnDestroy()
        {
            if (Gameplay.GameManager.Instance != null)
            {
                Gameplay.GameManager.Instance.OnMoneyChanged -= UpdateMoneyDisplay;
            }
        }

        private void UpdateMoneyDisplay(int money)
        {
            if (moneyText != null)
            {
                if (moneyFormat.Contains("{0}"))
                {
                    moneyText.text = string.Format(moneyFormat, money);
                }
                else
                {
                    moneyText.text = money.ToString();
                }
            }
        }
    }
}

