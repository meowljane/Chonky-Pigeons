namespace PigeonGame.Data
{
    public class PigeonInstanceStats
    {
        public PigeonSpecies speciesId;
        public int obesity; // 게임 로직용 (1~5)
        public float weight; // 실제 무게 (kg, 1.0~5.0)
        public FaceType faceId;

        // 계산된 최종 스탯
        public int bitePower;
        public float eatInterval;
        public float eatChance;
        public float playerAlertPerSec;
        public float crowdAlertPerNeighborPerSec;
        // detectionRadius, warnThreshold, backoffThreshold, fleeThreshold, backoffDistance, alertDecayPerSec는 PigeonMovement에서 관리 (모든 tier 통일)
        // alertWeight는 PigeonMovement에서 관리 (모든 비둘기 공통)
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
                weight = this.weight,
                faceId = this.faceId,
                bitePower = this.bitePower,
                eatInterval = this.eatInterval,
                eatChance = this.eatChance,
                playerAlertPerSec = this.playerAlertPerSec,
                crowdAlertPerNeighborPerSec = this.crowdAlertPerNeighborPerSec,
                price = this.price
            };
        }
    }
}


