using UnityEngine;

namespace PigeonGame.Data
{
    [CreateAssetMenu(fileName = "FaceSet", menuName = "PigeonGame/Face Set")]
    public class PigeonFaceSet : ScriptableObject
    {
        public int version;
        public FaceDefinition[] faces;

        private void OnEnable()
        {
            InitializeData();
        }

        private void InitializeData()
        {
            if (faces == null || faces.Length == 0)
            {
                version = 1;
                faces = new FaceDefinition[]
                {
                    new FaceDefinition { faceType = FaceType.F00, name = "기본", priceMultiplier = 1.0f },
                    new FaceDefinition { faceType = FaceType.F01, name = "찡긋", priceMultiplier = 1.05f },
                    new FaceDefinition { faceType = FaceType.F02, name = "상처", priceMultiplier = 1.08f },
                    new FaceDefinition { faceType = FaceType.F03, name = "하트눈", priceMultiplier = 1.15f },
                    new FaceDefinition { faceType = FaceType.F04, name = "왕눈", priceMultiplier = 1.2f },
                    new FaceDefinition { faceType = FaceType.F05, name = "마스크", priceMultiplier = 1.25f }
                };
            }
        }

        public FaceDefinition GetFaceById(FaceType faceType)
        {
            foreach (var face in faces)
            {
                if (face.faceType == faceType)
                    return face;
            }
            return null;
        }
    }
}
