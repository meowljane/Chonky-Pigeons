using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

namespace PigeonGame.UI
{
    public enum ToastType
    {
        Warning,    
        Success,    
        GoldChange  
    }

    public enum ToastPosition
    {
        BelowGold,  
        Message     
    }

    public class ToastNotificationManager : MonoBehaviour
    {
        public static ToastNotificationManager Instance { get; private set; }

        [Header("Toast Settings")]
        [SerializeField] private float defaultDuration = 2f; 
        [SerializeField] private float goldToastDuration = 1.5f; 
        [SerializeField] private int maxToasts = 3; 

        [Header("Animation Settings")]
        [SerializeField] private float slideInDuration = 0.3f;
        [SerializeField] private float slideOutDuration = 0.2f;

        [Header("Text Colors")]
        [SerializeField] private Color warningTextColor = new Color(1f, 0.8f, 0.2f, 1f);
        [SerializeField] private Color successTextColor = new Color(0.2f, 0.8f, 0.3f, 1f);
        [SerializeField] private Color goldGainTextColor = new Color(0.2f, 0.8f, 0.3f, 1f);
        [SerializeField] private Color goldLossTextColor = new Color(0.9f, 0.3f, 0.2f, 1f);

        [Header("References")]
        [SerializeField] private Canvas toastCanvas; 
        [SerializeField] private Transform belowGoldParent; 
        [SerializeField] private Transform messageParent; 
        [SerializeField] private GameObject toastPrefab; 

        private Queue<ToastData> toastQueue = new Queue<ToastData>();
        private List<GameObject> activeToasts = new List<GameObject>();
        private bool isProcessingQueue = false;
        private int lastMoney = 0; 

        private struct ToastData
        {
            public string message;
            public ToastType type;
            public ToastPosition position;
            public float duration;
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;

                if (toastCanvas == null || belowGoldParent == null || messageParent == null || toastPrefab == null)
                {
                    enabled = false;
                    return;
                }

                SubscribeToMoneyChanges();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void SubscribeToMoneyChanges()
        {
            if (Gameplay.GameManager.Instance != null)
            {
                lastMoney = Gameplay.GameManager.Instance.CurrentMoney;
                Gameplay.GameManager.Instance.OnMoneyChanged += OnMoneyChanged;
            }
            else
            {
                StartCoroutine(WaitForGameManager());
            }
        }

        private IEnumerator WaitForGameManager()
        {
            while (Gameplay.GameManager.Instance == null)
            {
                yield return null;
            }
            lastMoney = Gameplay.GameManager.Instance.CurrentMoney;
            Gameplay.GameManager.Instance.OnMoneyChanged += OnMoneyChanged;
        }

        private void OnMoneyChanged(int newMoney)
        {
            int change = newMoney - lastMoney;
            lastMoney = newMoney;

            if (change != 0)
            {
                ShowGoldChange(change);
            }
        }

        private void OnDestroy()
        {
            if (Gameplay.GameManager.Instance != null)
            {
                Gameplay.GameManager.Instance.OnMoneyChanged -= OnMoneyChanged;
            }
        }

        public static void ShowWarning(string message)
        {
            if (Instance == null)
            {
                return;
            }
            Instance.ShowToast(message, ToastType.Warning, ToastPosition.Message, Instance.defaultDuration);
        }

        public static void ShowSuccess(string message)
        {
            if (Instance == null)
            {
                return;
            }
            Instance.ShowToast(message, ToastType.Success, ToastPosition.Message, Instance.defaultDuration);
        }

        private void ShowGoldChange(int amount)
        {
            string message = amount > 0 ? $"+{amount}G" : $"{amount}G";
            ShowToast(message, ToastType.GoldChange, ToastPosition.BelowGold, goldToastDuration);
        }

        private void ShowToast(string message, ToastType type, ToastPosition position, float duration)
        {
            if (string.IsNullOrEmpty(message)) return;

            ToastData data = new ToastData
            {
                message = message,
                type = type,
                position = position,
                duration = duration
            };

            toastQueue.Enqueue(data);

            if (!isProcessingQueue)
            {
                StartCoroutine(ProcessToastQueue());
            }
        }

        private IEnumerator ProcessToastQueue()
        {
            isProcessingQueue = true;

            while (toastQueue.Count > 0)
            {
                if (activeToasts.Count >= maxToasts)
                {
                    if (activeToasts.Count > 0)
                    {
                        var oldestToast = activeToasts[0];
                        activeToasts.RemoveAt(0);
                        StartCoroutine(DismissToast(oldestToast));
                    }
                }

                ToastData data = toastQueue.Dequeue();
                GameObject toastObj = CreateToast(data);
                if (toastObj != null)
                {
                    activeToasts.Add(toastObj);
                }

                yield return new WaitForSeconds(0.1f);
            }

            isProcessingQueue = false;
        }

        private GameObject CreateToast(ToastData data)
        {
            if (toastPrefab == null)
            {
                return null;
            }

            Transform parent = GetParentForPosition(data.position);
            if (parent == null)
            {
                return null;
            }

            GameObject toastObj = Instantiate(toastPrefab, parent, false);

            RectTransform rect = toastObj.GetComponent<RectTransform>();
            if (rect == null)
            {
                Destroy(toastObj);
                return null;
            }

            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero; 

            Canvas.ForceUpdateCanvases();

            TextMeshProUGUI text = toastObj.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = data.message;

                Color textColor = GetTextColorForType(data.type);
                if (data.type == ToastType.GoldChange)
                {
                    int goldAmount = ParseGoldAmountFromMessage(data.message);
                    textColor = goldAmount > 0 ? goldGainTextColor : goldLossTextColor;
                }
                text.color = textColor;
            }

            float hiddenOffset = rect.rect.height * 0.5f + 20f;
            rect.anchoredPosition = new Vector2(0, hiddenOffset);

            StartCoroutine(ShowToastAnimation(toastObj, data.duration));

            return toastObj;
        }

        private Transform GetParentForPosition(ToastPosition position)
        {
            return position switch
            {
                ToastPosition.BelowGold => belowGoldParent,
                ToastPosition.Message => messageParent,
                _ => messageParent
            };
        }

        private IEnumerator ShowToastAnimation(GameObject toastObj, float duration)
        {
            if (toastObj == null) yield break;

            RectTransform rect = toastObj.GetComponent<RectTransform>();
            if (rect == null) yield break;

            TextMeshProUGUI text = toastObj.GetComponentInChildren<TextMeshProUGUI>();

            Vector2 hiddenPos = rect.anchoredPosition;
            Vector2 targetPos = Vector2.zero; 

            float elapsed = 0f;
            while (elapsed < slideInDuration)
            {
                if (toastObj == null || rect == null) yield break;

                elapsed += Time.deltaTime;
                float t = elapsed / slideInDuration;
                t = Mathf.SmoothStep(0f, 1f, t);
                rect.anchoredPosition = Vector2.Lerp(hiddenPos, targetPos, t);
                yield return null;
            }

            if (toastObj != null && rect != null)
            {
                rect.anchoredPosition = targetPos;
            }

            yield return new WaitForSeconds(duration);

            if (toastObj == null || rect == null) yield break;

            elapsed = 0f;
            Vector2 startPos = rect.anchoredPosition;
            Vector2 endPos = hiddenPos;

            while (elapsed < slideOutDuration)
            {
                if (toastObj == null || rect == null) yield break;

                elapsed += Time.deltaTime;
                float t = elapsed / slideOutDuration;
                t = Mathf.SmoothStep(0f, 1f, t);

                rect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);

                if (text != null)
                {
                    Color c = text.color;
                    c.a = Mathf.Lerp(1f, 0f, t);
                    text.color = c;
                }

                yield return null;
            }

            if (toastObj != null)
            {
                if (activeToasts.Contains(toastObj))
                {
                    activeToasts.Remove(toastObj);
                }
                Destroy(toastObj);
            }
        }

        private IEnumerator DismissToast(GameObject toastObj)
        {
            if (toastObj == null) yield break;

            RectTransform rect = toastObj.GetComponent<RectTransform>();
            if (rect == null) yield break;

            TextMeshProUGUI text = toastObj.GetComponentInChildren<TextMeshProUGUI>();

            Vector2 startPos = rect.anchoredPosition;
            float hiddenOffset = rect.rect.height * 0.5f + 20f;
            Vector2 endPos = new Vector2(0, startPos.y + hiddenOffset);

            float elapsed = 0f;
            while (elapsed < slideOutDuration)
            {
                if (toastObj == null || rect == null) yield break;

                elapsed += Time.deltaTime;
                float t = elapsed / slideOutDuration;
                t = Mathf.SmoothStep(0f, 1f, t);

                rect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);

                if (text != null)
                {
                    Color c = text.color;
                    c.a = Mathf.Lerp(1f, 0f, t);
                    text.color = c;
                }

                yield return null;
            }

            if (toastObj != null)
            {
                if (activeToasts.Contains(toastObj))
                {
                    activeToasts.Remove(toastObj);
                }
                Destroy(toastObj);
            }
        }

        public void ClearAll()
        {
            toastQueue.Clear();
            foreach (var toast in activeToasts)
            {
                if (toast != null)
                {
                    Destroy(toast);
                }
            }
            activeToasts.Clear();
        }

        private Color GetTextColorForType(ToastType type)
        {
            return type switch
            {
                ToastType.Warning => warningTextColor,
                ToastType.Success => successTextColor,
                ToastType.GoldChange => goldGainTextColor,
                _ => warningTextColor
            };
        }

        private int ParseGoldAmountFromMessage(string message)
        {
            string numberStr = message.Replace("G", "").Replace("+", "");
            if (int.TryParse(numberStr, out int result))
            {
                return message.Contains("-") ? -result : result;
            }
            return 0;
        }
    }
}
