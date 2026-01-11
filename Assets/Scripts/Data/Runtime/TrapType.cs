using UnityEngine;

namespace PigeonGame.Data
{
    [System.Serializable]
    public class TrapDefinition
    {
        public string id;
        public string name;
        public int cost;
        public int feedAmount;
        public float attractionRadius = 5f; // 비둘기를 이끄는 반경
        public float eatingRadius = 2f; // 먹을 수 있는 범위 반경
    }

    [CreateAssetMenu(fileName = "Traps", menuName = "PigeonGame/Traps")]
    public class TrapTypeSet : ScriptableObject
    {
        public int version;
        public TrapDefinition[] traps;

        public TrapDefinition GetTrapById(string trapId)
        {
            foreach (var trap in traps)
            {
                if (trap.id == trapId)
                    return trap;
            }
            return null;
        }
    }
}
 

