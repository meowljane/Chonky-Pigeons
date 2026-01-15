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
        public int pigeonSpawnCount; // 덫 설치 시 추가로 스폰되는 비둘기 수
    }

    [CreateAssetMenu(fileName = "Traps", menuName = "PigeonGame/Traps")]
    public class TrapTypeSet : ScriptableObject
    {
        public int version;
        public TrapDefinition[] traps;

        private void OnEnable()
        {
            InitializeData();
        }

        private void InitializeData()
        {
            if (traps == null || traps.Length == 0)
            {
                version = 1;
                traps = new TrapDefinition[]
                {
                    new TrapDefinition { id = "BREAD", name = "바삭빵덫", cost = 10, feedAmount = 20, pigeonSpawnCount = 3 },
                    new TrapDefinition { id = "SEEDS", name = "톡톡씨앗덫", cost = 15, feedAmount = 20, pigeonSpawnCount = 4 },
                    new TrapDefinition { id = "CORN", name = "노랑옥수수덫", cost = 25, feedAmount = 20, pigeonSpawnCount = 5 },
                    new TrapDefinition { id = "PELLET", name = "프리미엄알갱이덫", cost = 50, feedAmount = 20, pigeonSpawnCount = 6 },
                    new TrapDefinition { id = "SHINY", name = "반짝간식덫", cost = 100, feedAmount = 20, pigeonSpawnCount = 8 }
                };
            }
        }

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
 

