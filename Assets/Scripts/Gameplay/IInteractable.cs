namespace PigeonGame.Gameplay
{
    /// <summary>
    /// 상호작용 가능한 오브젝트 인터페이스
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// 상호작용 가능한지 확인
        /// </summary>
        bool CanInteract();

        /// <summary>
        /// 상호작용 실행
        /// </summary>
        void OnInteract();
    }
}