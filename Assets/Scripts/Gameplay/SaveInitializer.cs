using UnityEngine;

namespace PigeonGame.Gameplay
{
    /// <summary>
    /// 씬 시작 시 세이브 데이터 로드 및 적용 담당
    /// (씬에 한 개 배치해서 사용)
    /// </summary>
    public class SaveInitializer : MonoBehaviour
    {
        private void Start()
        {
            // 모든 매니저들의 Awake가 끝난 뒤에 호출됨
            SaveManager.LoadOrCreateAndApply();
        }
    }
}

