using System;
using System.Collections.Generic;
using PigeonGame.Data;

namespace PigeonGame.Save
{
    [Serializable]
    public class SaveData
    {
        public int version = 1;
        public GameManagerSaveData game;
        public UpgradeSaveData upgrades;
        public EncyclopediaSaveData encyclopedia;
    }

    [Serializable]
    public class GameManagerSaveData
    {
        public int currentMoney;
        public List<TrapType> unlockedTraps = new List<TrapType>();
        public List<PigeonSpecies> unlockedSpecies = new List<PigeonSpecies>();
        public List<DoorType> unlockedDoors = new List<DoorType>();
        public List<PigeonInstanceSaveData> inventory = new List<PigeonInstanceSaveData>();
        public List<PigeonInstanceSaveData> exhibition = new List<PigeonInstanceSaveData>();
    }

    [Serializable]
    public class PigeonInstanceSaveData
    {
        public PigeonSpecies speciesId;
        public FaceType faceId;
        public float weight;
    }

    [Serializable]
    public class UpgradeSaveData
    {
        public int inventorySlotBonus;
        public int maxTrapCount;
        public int pigeonsPerMapUnlockedLevel;
        public int pigeonsPerMapSelectedValue;

        public bool hasIncreaseSpecies;
        public PigeonSpecies increaseSpecies;

        public bool hasDecreaseSpecies;
        public PigeonSpecies decreaseSpecies;
    }

    [Serializable]
    public class EncyclopediaSaveData
    {
        public List<SpeciesEntry> species = new List<SpeciesEntry>();

        [Serializable]
        public class SpeciesEntry
        {
            public PigeonSpecies speciesId;
            public bool isUnlocked;
            public float minWeight;
            public float maxWeight;
            public List<FaceEntry> faces = new List<FaceEntry>();
        }

        [Serializable]
        public class FaceEntry
        {
            public FaceType faceId;
            public bool isUnlocked;
            public float minWeight;
            public float maxWeight;
        }
    }
}

