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


