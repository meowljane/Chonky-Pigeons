namespace PigeonGame.Data
{
    public class PigeonInstanceStats
    {
        public PigeonSpecies speciesId;
        public int obesity; 
        public float weight; 
        public FaceType faceId;

        public int bitePower;
        public float eatInterval;
        public float eatChance;
        public float playerAlertPerSec;
        public float crowdAlertPerNeighborPerSec;
        public int price;

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

