using UnityEngine;

namespace PigeonGame.Gameplay
{
    public class CameraFollow : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;
        
        [Header("Camera Reference")]
        [SerializeField] private Camera targetCamera;
        
        [Header("Camera Settings")]
        [SerializeField] private Vector2 offset = Vector2.zero;
        [SerializeField] private float baseOrthographicSize = 5f;
        
        private Transform cameraTransform;
        
        private void Awake()
        {
            if (targetCamera == null)
            {
                enabled = false;
                return;
            }
            
            cameraTransform = targetCamera.transform;
            SetupCameraSize();
        }
        
        private void Start()
        {
            if (target != null)
            {
                Vector3 targetPos = target.position;
                cameraTransform.position = new Vector3(targetPos.x + offset.x, targetPos.y + offset.y, cameraTransform.position.z);
            }
        }
        
        private void SetupCameraSize()
        {
            if (targetCamera == null) return;
            
            float screenAspect = (float)Screen.width / (float)Screen.height;
            float baseAspect = 16f / 9f;
            
            if (screenAspect > baseAspect)
            {
                targetCamera.orthographicSize = baseOrthographicSize;
            }
            else
            {
                targetCamera.orthographicSize = baseOrthographicSize * (baseAspect / screenAspect);
            }
        }
        
        private void LateUpdate()
        {
            if (target == null || cameraTransform == null) return;
            
            Vector3 targetPos = target.position;
            cameraTransform.position = new Vector3(targetPos.x + offset.x, targetPos.y + offset.y, cameraTransform.position.z);
        }
    }
}