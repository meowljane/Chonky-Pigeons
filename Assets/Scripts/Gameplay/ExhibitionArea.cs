using UnityEngine;
using UnityEngine.Tilemaps;

namespace PigeonGame.Gameplay
{
    /// <summary>
    /// 전시 영역을 정의하는 컴포넌트
    /// 각 전시 영역 Tilemap GameObject에 이 컴포넌트를 추가
    /// </summary>
    [RequireComponent(typeof(Tilemap))]
    public class ExhibitionArea : MonoBehaviour
    {
        // 마커 컴포넌트로, 별도의 설정 값은 없습니다.
    }
}
