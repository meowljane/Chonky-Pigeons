using UnityEngine;
using TMPro;
using PigeonGame.Gameplay;

namespace PigeonGame.UI
{
    public class MoneyDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI moneyText;
        [SerializeField] private string moneyFormat = " {0}G";

        private void Start()
        {
            if (moneyText == null)
                moneyText = GetComponent<TextMeshProUGUI>();
            if (moneyText == null) return;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnMoneyChanged += UpdateMoneyDisplay;
                UpdateMoneyDisplay(GameManager.Instance.CurrentMoney);
            }
            else
            {
                StartCoroutine(WaitForGameManager());
            }
        }

        private System.Collections.IEnumerator WaitForGameManager()
        {
            while (GameManager.Instance == null)
                yield return null;

            GameManager.Instance.OnMoneyChanged += UpdateMoneyDisplay;
            UpdateMoneyDisplay(GameManager.Instance.CurrentMoney);
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnMoneyChanged -= UpdateMoneyDisplay;
        }

        private void UpdateMoneyDisplay(int money)
        {
            if (moneyText == null) return;
            UIHelper.UpdateGoldText(moneyText, moneyFormat);
        }
    }
}

