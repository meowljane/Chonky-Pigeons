using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

namespace PigeonGame.UI
{
    /// <summary>
    /// 토스트 알림 타입
    /// </summary>
    public enum ToastType
    {
        Warning,    // 경고 (노란색)
        Success,    // 성공 (초록색)
        GoldChange  // 골드 변경 (+nG, -nG)
    }

    /// <summary>
    /// 토스트 알림 위치
    /// </summary>
    public enum ToastPosition
    {
        BelowGold,  // 골드 표시 아래
        Message     // 문구 표시 위치
    }

    /// <summary>
    /// 토스트 알림 시스템 매니저
    /// 싱글톤으로 전역에서 사용 가능
    /// </summary>
    public class ToastNotificationManager : MonoBehaviour
    {
        public static ToastNotificationManager Instance { get; private set; }

        [Header("Toast Settings")]
        [SerializeField] private float defaultDuration = 2f; // 기본 표시 시간
        [SerializeField] private float goldToastDuration = 1.5f; // 골드 토스트 표시 시간
        [SerializeField] private int maxToasts = 3; // 최대 동시 표시 개수

        [Header("Animation Settings")]
        [SerializeField] private float slideInDuration = 0.3f;
        [SerializeField] private float slideOutDuration = 0.2f;

        [Header("Text Colors")]
        [SerializeField] private Color warningTextColor = new Color(1f, 0.8f, 0.2f, 1f);
        [SerializeField] private Color successTextColor = new Color(0.2f, 0.8f, 0.3f, 1f);
        [SerializeField] private Color goldGainTextColor = new Color(0.2f, 0.8f, 0.3f, 1f);
        [SerializeField] private Color goldLossTextColor = new Color(0.9f, 0.3f, 0.2f, 1f);

        [Header("References")]
        [SerializeField] private Canvas toastCanvas; // 토스트 Canvas (필수)
        [SerializeField] private Transform belowGoldParent; // 골드 표시 아래 토스트 부모 Transform (필수)
        [SerializeField] private Transform messageParent; // 문구 표시 위치 토스트 부모 Transform (필수)
        [SerializeField] private GameObject toastPrefab; // 토스트 프리팹 (필수)

        private Queue<ToastData> toastQueue = new Queue<ToastData>();
        private List<GameObject> activeToasts = new List<GameObject>();
        private bool isProcessingQueue = false;
        private int lastMoney = 0; // 골드 변경 추적용

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
                DontDestroyOnLoad(gameObject);
                
                // 필수 참조 확인
                if (toastCanvas == null || belowGoldParent == null || messageParent == null || toastPrefab == null)
                {
                    Debug.LogError("ToastNotificationManager: toastCanvas, belowGoldParent, messageParent, toastPrefab을 모두 지정해주세요!");
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

        /// <summary>
        /// 골드 변경 이벤트 구독
        /// </summary>
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

        /// <summary>
        /// 골드 변경 시 호출
        /// </summary>
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

        /// <summary>
        /// 경고 토스트 표시
        /// </summary>
        public static void ShowWarning(string message)
        {
            if (Instance == null)
            {
                Debug.LogWarning("ToastNotificationManager가 없습니다. 씬에 추가해주세요.");
                return;
            }
            Instance.ShowToast(message, ToastType.Warning, ToastPosition.Message, Instance.defaultDuration);
        }

        /// <summary>
        /// 성공 토스트 표시
        /// </summary>
        public static void ShowSuccess(string message)
        {
            if (Instance == null)
            {
                Debug.LogWarning("ToastNotificationManager가 없습니다. 씬에 추가해주세요.");
                return;
            }
            Instance.ShowToast(message, ToastType.Success, ToastPosition.Message, Instance.defaultDuration);
        }

        /// <summary>
        /// 골드 변경 토스트 표시 (내부)
        /// </summary>
        private void ShowGoldChange(int amount)
        {
            string message = amount > 0 ? $"+{amount}G" : $"{amount}G";
            ShowToast(message, ToastType.GoldChange, ToastPosition.BelowGold, goldToastDuration);
        }

        /// <summary>
        /// 토스트 표시 내부 메서드
        /// </summary>
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

        /// <summary>
        /// 토스트 큐 처리
        /// </summary>
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

        /// <summary>
        /// 토스트 생성
        /// </summary>
        private GameObject CreateToast(ToastData data)
        {
            if (toastPrefab == null)
            {
                Debug.LogError("ToastNotificationManager: toastPrefab이 지정되지 않았습니다!");
                return null;
            }

            // 위치에 따라 적절한 부모 선택
            Transform parent = GetParentForPosition(data.position);
            if (parent == null)
            {
                Debug.LogError($"ToastNotificationManager: {data.position}에 해당하는 parent가 지정되지 않았습니다!");
                return null;
            }

            // 프리팹 인스턴스 생성 (worldPositionStays = false로 부모 기준으로 생성)
            GameObject toastObj = Instantiate(toastPrefab, parent, false);

            RectTransform rect = toastObj.GetComponent<RectTransform>();
            if (rect == null)
            {
                Debug.LogError("ToastNotificationManager: toastPrefab에 RectTransform이 없습니다!");
                Destroy(toastObj);
                return null;
            }

            // 위치 설정 (부모의 중앙에서 시작하도록)
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero; // 부모 중앙에서 시작

            // 레이아웃 강제 업데이트 (크기 계산을 위해)
            Canvas.ForceUpdateCanvases();

            // 텍스트 설정
            TextMeshProUGUI text = toastObj.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = data.message;
                
                // 타입에 따른 텍스트 색상 설정
                Color textColor = GetTextColorForType(data.type);
                if (data.type == ToastType.GoldChange)
                {
                    int goldAmount = ParseGoldAmountFromMessage(data.message);
                    textColor = goldAmount > 0 ? goldGainTextColor : goldLossTextColor;
                }
                text.color = textColor;
            }

            // 숨김 위치 계산 (부모 기준 상단 밖)
            float hiddenOffset = rect.rect.height * 0.5f + 20f;
            rect.anchoredPosition = new Vector2(0, hiddenOffset);

            // 애니메이션 시작
            StartCoroutine(ShowToastAnimation(toastObj, data.duration));

            return toastObj;
        }

        /// <summary>
        /// 위치에 따른 부모 Transform 반환
        /// </summary>
        private Transform GetParentForPosition(ToastPosition position)
        {
            return position switch
            {
                ToastPosition.BelowGold => belowGoldParent,
                ToastPosition.Message => messageParent,
                _ => messageParent
            };
        }

        /// <summary>
        /// 토스트 표시 애니메이션
        /// </summary>
        private IEnumerator ShowToastAnimation(GameObject toastObj, float duration)
        {
            RectTransform rect = toastObj.GetComponent<RectTransform>();
            if (rect == null) yield break;

            TextMeshProUGUI text = toastObj.GetComponentInChildren<TextMeshProUGUI>();

            Vector2 hiddenPos = rect.anchoredPosition;
            Vector2 targetPos = Vector2.zero; // 부모의 중앙 위치 (0, 0)

            // 슬라이드 인
            float elapsed = 0f;
            while (elapsed < slideInDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / slideInDuration;
                t = Mathf.SmoothStep(0f, 1f, t);
                rect.anchoredPosition = Vector2.Lerp(hiddenPos, targetPos, t);
                yield return null;
            }
            rect.anchoredPosition = targetPos;

            // 대기
            yield return new WaitForSeconds(duration);

            // 슬라이드 아웃
            elapsed = 0f;
            Vector2 startPos = rect.anchoredPosition;
            Vector2 endPos = hiddenPos;

            while (elapsed < slideOutDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / slideOutDuration;
                t = Mathf.SmoothStep(0f, 1f, t);

                rect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);

                // 텍스트 페이드 아웃
                if (text != null)
                {
                    Color c = text.color;
                    c.a = Mathf.Lerp(1f, 0f, t);
                    text.color = c;
                }

                yield return null;
            }

            // 제거
            if (activeToasts.Contains(toastObj))
            {
                activeToasts.Remove(toastObj);
            }
            Destroy(toastObj);
        }

        /// <summary>
        /// 토스트 제거 애니메이션
        /// </summary>
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
                elapsed += Time.deltaTime;
                float t = elapsed / slideOutDuration;
                t = Mathf.SmoothStep(0f, 1f, t);

                rect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);

                // 텍스트 페이드 아웃
                if (text != null)
                {
                    Color c = text.color;
                    c.a = Mathf.Lerp(1f, 0f, t);
                    text.color = c;
                }

                yield return null;
            }

            if (activeToasts.Contains(toastObj))
            {
                activeToasts.Remove(toastObj);
            }
            Destroy(toastObj);
        }



        /// <summary>
        /// 모든 토스트 제거
        /// </summary>
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

        /// <summary>
        /// 타입에 따른 텍스트 색상 반환
        /// </summary>
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

        /// <summary>
        /// 골드 메시지에서 숫자 파싱 (내부용)
        /// </summary>
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
