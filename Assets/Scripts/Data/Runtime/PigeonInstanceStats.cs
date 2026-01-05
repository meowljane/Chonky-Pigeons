namespace PigeonGame.Data
{
    public class PigeonInstanceStats
    {
        public string speciesId;
        public int obesity;
        public string faceId;

        // 계산된 최종 스탯
        public int bitePower;
        public float eatInterval;
        public float eatChance;
        public float personalSpaceRadius;
        public float playerAlertPerSec;
        public float crowdAlertPerNeighborPerSec;
        public float alertDecayPerSec;
        public float warnThreshold;
        public float backoffThreshold;
        public float fleeThreshold;
        public float backoffDuration;
        public float backoffDistance;
        public float crowdWeight;
        public float playerWeight;
        public int price;
    }
}


