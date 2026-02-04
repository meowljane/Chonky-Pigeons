using UnityEngine;

namespace PigeonGame.Data
{
    public enum FaceType
    {
        F00, 
        F01, 
        F02, 
        F03, 
        F04, 
        F05  
    }

    [System.Serializable]
    public class FaceDefinition
    {
        public FaceType faceType;
        public string name;
        public float priceMultiplier;
        public Sprite icon; 
        [Tooltip("Face 애니메이션 컨트롤러 (Idle, Walking, Flying 애니메이션 포함, 비둘기 바디와 동일한 구조)")]
        public RuntimeAnimatorController animatorController; 
    }

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
                    new FaceDefinition { faceType = FaceType.F01, name = "콩눈이", priceMultiplier = 1.05f },
                    new FaceDefinition { faceType = FaceType.F02, name = "찡긋", priceMultiplier = 1.08f },
                    new FaceDefinition { faceType = FaceType.F03, name = "번쩍", priceMultiplier = 1.15f },
                    new FaceDefinition { faceType = FaceType.F04, name = "눈물", priceMultiplier = 1.2f },
                    new FaceDefinition { faceType = FaceType.F05, name = "하트", priceMultiplier = 1.25f }
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
