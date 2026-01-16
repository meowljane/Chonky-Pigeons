using UnityEngine;
using System.Collections.Generic;
using PigeonGame.Data;

namespace PigeonGame.Gameplay
{
    /// <summary>
    /// 전시관 건물
    /// 플레이어가 근처에 있을 때 상호작용 가능
    /// 전시된 비둘기들을 건물 내에서 wander만 하도록 관리
    /// </summary>
    public class ExhibitionBuilding : MonoBehaviour, IInteractable
    {
        [Header("Interaction")]
        [SerializeField] private float interactionRadius = 2f;

        [Header("Exhibition Area")]
        [SerializeField] private Collider2D exhibitionArea; // 전시 영역 (사용자가 설정)

        [Header("Pigeon Spawning")]
        [SerializeField] private GameObject pigeonPrefab; // 비둘기 프리팹

        private Collider2D interactionTrigger;
        private bool isPlayerInRange = false;
        private List<PigeonController> exhibitionPigeons = new List<PigeonController>(); // 전시된 비둘기들

        public float InteractionRadius => interactionRadius;
        public bool IsPlayerInRange => isPlayerInRange;
        public Collider2D ExhibitionArea => exhibitionArea;

        private void Start()
        {
            SetupInteractionTrigger();
            
            // 전시 영역이 없으면 자동으로 찾기 (같은 오브젝트의 Collider2D)
            if (exhibitionArea == null)
            {
                exhibitionArea = GetComponent<Collider2D>();
            }

            // GameManager 이벤트 구독
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPigeonAddedToExhibition += OnPigeonAddedToExhibition;
                GameManager.Instance.OnPigeonRemovedFromExhibition += OnPigeonRemovedFromExhibition;
            }

            // 기존 전시 비둘기들 스폰
            RefreshExhibitionPigeons();
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPigeonAddedToExhibition -= OnPigeonAddedToExhibition;
                GameManager.Instance.OnPigeonRemovedFromExhibition -= OnPigeonRemovedFromExhibition;
            }

            // 전시 비둘기들 제거
            ClearExhibitionPigeons();
        }

        private void SetupInteractionTrigger()
        {
            interactionTrigger = GetComponent<Collider2D>();
            
            if (interactionTrigger == null)
            {
                CircleCollider2D circleCollider = gameObject.AddComponent<CircleCollider2D>();
                circleCollider.radius = interactionRadius;
                circleCollider.isTrigger = true;
                interactionTrigger = circleCollider;
            }
            else
            {
                interactionTrigger.isTrigger = true;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<PlayerController>() != null)
            {
                isPlayerInRange = true;
                if (InteractionSystem.Instance != null)
                {
                    InteractionSystem.Instance.RegisterInteractable(this);
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.GetComponent<PlayerController>() != null)
            {
                isPlayerInRange = false;
                if (InteractionSystem.Instance != null)
                {
                    InteractionSystem.Instance.UnregisterInteractable(this);
                }
            }
        }

        // IInteractable 구현
        public bool CanInteract()
        {
            return isPlayerInRange;
        }

        public void OnInteract()
        {
            if (!CanInteract())
                return;

            var interactionSystem = InteractionSystem.Instance;
            if (interactionSystem != null)
            {
                interactionSystem.OpenExhibition();
            }
        }

        /// <summary>
        /// 전시관에 비둘기가 추가되었을 때 호출
        /// </summary>
        private void OnPigeonAddedToExhibition(PigeonInstanceStats stats)
        {
            RefreshExhibitionPigeons();
        }

        /// <summary>
        /// 전시관에서 비둘기가 제거되었을 때 호출
        /// </summary>
        private void OnPigeonRemovedFromExhibition(PigeonInstanceStats stats)
        {
            RefreshExhibitionPigeons();
        }

        /// <summary>
        /// 전시 비둘기 목록 새로고침 (GameManager의 전시관 리스트와 동기화)
        /// </summary>
        private void RefreshExhibitionPigeons()
        {
            if (GameManager.Instance == null)
                return;

            // 기존 전시 비둘기들 제거
            ClearExhibitionPigeons();

            // GameManager의 전시관 리스트와 동기화하여 스폰
            var exhibition = GameManager.Instance.Exhibition;
            foreach (var stats in exhibition)
            {
                SpawnExhibitionPigeon(stats);
            }
        }

        /// <summary>
        /// 전시 비둘기 스폰
        /// </summary>
        private void SpawnExhibitionPigeon(PigeonInstanceStats stats)
        {
            if (pigeonPrefab == null || exhibitionArea == null || stats == null)
                return;

            // 전시 영역 내 랜덤 위치 생성
            Vector3 spawnPos = GetRandomPositionInCollider(exhibitionArea);
            spawnPos.z = 0f; // 2D 게임용

            GameObject pigeonObj = Instantiate(pigeonPrefab, spawnPos, Quaternion.identity);
            if (!pigeonObj.activeSelf)
            {
                pigeonObj.SetActive(true);
            }

            PigeonController controller = pigeonObj.GetComponent<PigeonController>();
            if (controller != null)
            {
                // 비둘기 초기화
                controller.Initialize(stats);
                
                // 전시 비둘기로 설정
                controller.SetAsExhibitionPigeon(exhibitionArea);

                exhibitionPigeons.Add(controller);
            }
        }

        /// <summary>
        /// 콜라이더 내 랜덤 위치 생성
        /// </summary>
        private Vector3 GetRandomPositionInCollider(Collider2D collider)
        {
            if (collider == null)
                return transform.position;

            Bounds bounds = collider.bounds;
            int attempts = 0;
            Vector2 position;
            
            do
            {
                position = new Vector2(
                    Random.Range(bounds.min.x, bounds.max.x),
                    Random.Range(bounds.min.y, bounds.max.y)
                );
                attempts++;
            } while (attempts < 20 && !collider.OverlapPoint(position));

            // 20번 시도해도 실패하면 bounds 중심 사용
            if (attempts >= 20)
            {
                position = bounds.center;
            }

            return position;
        }

        /// <summary>
        /// 모든 전시 비둘기 제거
        /// </summary>
        private void ClearExhibitionPigeons()
        {
            foreach (var pigeon in exhibitionPigeons)
            {
                if (pigeon != null && pigeon.gameObject != null)
                {
                    Destroy(pigeon.gameObject);
                }
            }
            exhibitionPigeons.Clear();
        }
    }
}
