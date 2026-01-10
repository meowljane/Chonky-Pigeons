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
 

