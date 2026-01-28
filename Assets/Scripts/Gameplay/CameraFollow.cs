using UnityEngine;

namespace PigeonGame.Gameplay
{
    public class CameraFollow : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;
        
        [Header("Camera Reference")]
        [SerializeField] private Camera targetCamera;
        
        private Transform cameraTransform;
        
        private void Awake()
        {
            if (targetCamera == null)
            {
                enabled = false;
                return;
            }
            
            cameraTransform = targetCamera.transform;
        }
        
        private void Start()
        {
            if (target != null)
            {
                Vector3 targetPos = target.position;
                cameraTransform.position = new Vector3(targetPos.x, targetPos.y, cameraTransform.position.z);
            }
        }
        
        private void LateUpdate()
        {
            if (target == null || cameraTransform == null) return;
            
            Vector3 targetPos = target.position;
            cameraTransform.position = new Vector3(targetPos.x, targetPos.y, cameraTransform.position.z);
        }
    }
}