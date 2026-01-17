namespace PigeonGame.Data
{
    /// <summary>
    /// 비둘기 얼굴 타입 Enum
    /// </summary>
    public enum FaceType
    {
        F00, // 기본
        F01, // 찡긋
        F02, // 상처
        F03, // 하트눈
        F04, // 왕눈
        F05  // 마스크
    }

    [System.Serializable]
    public class FaceDefinition
    {
        public FaceType faceType;
        public string name;
        public float priceMultiplier;
    }
}


