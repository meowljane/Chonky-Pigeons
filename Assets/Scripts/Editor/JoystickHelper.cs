using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using PigeonGame.UI;

namespace PigeonGame.Editor
{
    public static class JoystickHelper
    {
        [MenuItem("GameObject/UI/Mobile Joystick", false, 10)]
        public static void CreateMobileJoystick()
        {
            // Canvas 찾기 또는 생성
            Canvas canvas = FindOrCreateCanvas();

            // 조이스틱 부모 오브젝트 생성
            GameObject joystickObj = new GameObject("MobileJoystick");
            joystickObj.transform.SetParent(canvas.transform, false);
            
            RectTransform joystickRect = joystickObj.AddComponent<RectTransform>();
            joystickRect.anchorMin = new Vector2(0, 0);
            joystickRect.anchorMax = new Vector2(0, 0);
            joystickRect.pivot = new Vector2(0, 0);
            joystickRect.anchoredPosition = new Vector2(100, 100); // 좌측 하단
            joystickRect.sizeDelta = new Vector2(200, 200);

            // MobileJoystick 컴포넌트 추가
            MobileJoystick joystick = joystickObj.AddComponent<MobileJoystick>();

            // Background 생성
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(joystickObj.transform, false);
            RectTransform bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.zero;
            bgRect.pivot = new Vector2(0.5f, 0.5f);
            bgRect.anchoredPosition = Vector2.zero;
            bgRect.sizeDelta = new Vector2(150, 150);

            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.5f); // 반투명 회색

            // Handle 생성
            GameObject handleObj = new GameObject("Handle");
            handleObj.transform.SetParent(bgObj.transform, false);
            RectTransform handleRect = handleObj.AddComponent<RectTransform>();
            handleRect.anchorMin = new Vector2(0.5f, 0.5f);
            handleRect.anchorMax = new Vector2(0.5f, 0.5f);
            handleRect.pivot = new Vector2(0.5f, 0.5f);
            handleRect.anchoredPosition = Vector2.zero;
            handleRect.sizeDelta = new Vector2(60, 60);

            Image handleImage = handleObj.AddComponent<Image>();
            handleImage.color = new Color(1f, 1f, 1f, 0.8f); // 반투명 흰색

            // MobileJoystick 컴포넌트에 참조 할당
            SerializedObject serializedJoystick = new SerializedObject(joystick);
            serializedJoystick.FindProperty("background").objectReferenceValue = bgRect;
            serializedJoystick.FindProperty("handle").objectReferenceValue = handleRect;
            serializedJoystick.ApplyModifiedProperties();

            // 선택
            Selection.activeGameObject = joystickObj;
            
            Debug.Log("모바일 조이스틱이 생성되었습니다! 좌측 하단에 배치되어 있습니다.");
        }

        private static Canvas FindOrCreateCanvas()
        {
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                // Canvas 생성
                GameObject canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();

                // EventSystem 생성
                if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
                {
                    GameObject eventSystemObj = new GameObject("EventSystem");
                    eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                    eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                }
            }
            return canvas;
        }
    }
}

