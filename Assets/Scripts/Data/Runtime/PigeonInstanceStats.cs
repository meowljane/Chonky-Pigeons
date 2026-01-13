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
        public float alertWeight; // 플레이어 및 군집 Alert 가중치 (통일)
        public int price;

        /// <summary>
        /// 인벤토리 저장용 복사본 생성
        /// </summary>
        public PigeonInstanceStats Clone()
        {
            return new PigeonInstanceStats
            {
                speciesId = this.speciesId,
                obesity = this.obesity,
                faceId = this.faceId,
                bitePower = this.bitePower,
                eatInterval = this.eatInterval,
                eatChance = this.eatChance,
                personalSpaceRadius = this.personalSpaceRadius,
                playerAlertPerSec = this.playerAlertPerSec,
                crowdAlertPerNeighborPerSec = this.crowdAlertPerNeighborPerSec,
                alertDecayPerSec = this.alertDecayPerSec,
                warnThreshold = this.warnThreshold,
                backoffThreshold = this.backoffThreshold,
                fleeThreshold = this.fleeThreshold,
                backoffDuration = this.backoffDuration,
                backoffDistance = this.backoffDistance,
                alertWeight = this.alertWeight,
                price = this.price
            };
        }
    }
}


