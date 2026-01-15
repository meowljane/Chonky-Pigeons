using UnityEngine;

namespace PigeonGame.Data
{
    [System.Serializable]
    public class FaceDefinition
    {
        public string id;
        public string name;
        public float priceMultiplier;
    }

    [CreateAssetMenu(fileName = "Faces", menuName = "PigeonGame/Faces")]
    public class PigeonFace : ScriptableObject
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
                    new FaceDefinition { id = "F00", name = "기본", priceMultiplier = 1.0f },
                    new FaceDefinition { id = "F01", name = "찡긋", priceMultiplier = 1.05f },
                    new FaceDefinition { id = "F02", name = "상처", priceMultiplier = 1.08f },
                    new FaceDefinition { id = "F03", name = "하트눈", priceMultiplier = 1.15f },
                    new FaceDefinition { id = "F04", name = "왕눈", priceMultiplier = 1.2f },
                    new FaceDefinition { id = "F05", name = "마스크", priceMultiplier = 1.25f }
                };
            }
        }

        public FaceDefinition GetFaceById(string faceId)
        {
            foreach (var face in faces)
            {
                if (face.id == faceId)
                    return face;
            }
            return null;
        }
    }
}


