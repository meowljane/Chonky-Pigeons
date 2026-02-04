using UnityEngine;
using System.Collections.Generic;
using PigeonGame.Data;
using PigeonGame.Save;

namespace PigeonGame.Gameplay
{
    public class EncyclopediaManager : MonoBehaviour
    {
        public static EncyclopediaManager Instance { get; private set; }

        [System.Serializable]
        public class FaceEncyclopediaData
        {
            public bool isUnlocked;
            public float minWeight = float.MaxValue;
            public float maxWeight = float.MinValue;
        }

        [System.Serializable]
        public class SpeciesEncyclopediaData
        {
            public bool isUnlocked;
            public float minWeight = float.MaxValue;
            public float maxWeight = float.MinValue;
            public Dictionary<FaceType, FaceEncyclopediaData> faces = new Dictionary<FaceType, FaceEncyclopediaData>();
        }

        private Dictionary<PigeonSpecies, SpeciesEncyclopediaData> encyclopediaData = new Dictionary<PigeonSpecies, SpeciesEncyclopediaData>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void RecordPigeon(PigeonInstanceStats stats)
        {
            if (stats == null)
                return;

            PigeonSpecies speciesId = stats.speciesId;
            FaceType faceId = stats.faceId;
            float weight = stats.weight;

            if (!encyclopediaData.ContainsKey(speciesId))
            {
                encyclopediaData[speciesId] = new SpeciesEncyclopediaData();
            }

            SpeciesEncyclopediaData speciesData = encyclopediaData[speciesId];
            speciesData.isUnlocked = true;

            if (speciesData.minWeight == float.MaxValue)
                speciesData.minWeight = weight;
            else if (weight < speciesData.minWeight)
                speciesData.minWeight = weight;

            if (speciesData.maxWeight == float.MinValue)
                speciesData.maxWeight = weight;
            else if (weight > speciesData.maxWeight)
                speciesData.maxWeight = weight;

            if (!speciesData.faces.ContainsKey(faceId))
            {
                speciesData.faces[faceId] = new FaceEncyclopediaData();
            }

            FaceEncyclopediaData faceData = speciesData.faces[faceId];
            faceData.isUnlocked = true;

            if (faceData.minWeight == float.MaxValue)
                faceData.minWeight = weight;
            else if (weight < faceData.minWeight)
                faceData.minWeight = weight;

            if (faceData.maxWeight == float.MinValue)
                faceData.maxWeight = weight;
            else if (weight > faceData.maxWeight)
                faceData.maxWeight = weight;
        }

        public SpeciesEncyclopediaData GetSpeciesData(PigeonSpecies speciesType)
        {
            if (encyclopediaData.ContainsKey(speciesType))
            {
                return encyclopediaData[speciesType];
            }
            return new SpeciesEncyclopediaData();
        }

        public FaceEncyclopediaData GetFaceData(PigeonSpecies speciesType, FaceType faceType)
        {
            SpeciesEncyclopediaData speciesData = GetSpeciesData(speciesType);
            if (speciesData.faces.ContainsKey(faceType))
            {
                return speciesData.faces[faceType];
            }
            return new FaceEncyclopediaData();
        }

        public Dictionary<PigeonSpecies, SpeciesEncyclopediaData> GetAllSpeciesData()
        {
            return encyclopediaData;
        }

        public EncyclopediaSaveData CreateSaveData()
        {
            var data = new EncyclopediaSaveData();

            foreach (var kvp in encyclopediaData)
            {
                var speciesId = kvp.Key;
                var speciesData = kvp.Value;

                var speciesEntry = new EncyclopediaSaveData.SpeciesEntry
                {
                    speciesId = speciesId,
                    isUnlocked = speciesData.isUnlocked,
                    minWeight = speciesData.minWeight,
                    maxWeight = speciesData.maxWeight
                };

                foreach (var faceKvp in speciesData.faces)
                {
                    var faceId = faceKvp.Key;
                    var faceData = faceKvp.Value;

                    var faceEntry = new EncyclopediaSaveData.FaceEntry
                    {
                        faceId = faceId,
                        isUnlocked = faceData.isUnlocked,
                        minWeight = faceData.minWeight,
                        maxWeight = faceData.maxWeight
                    };

                    speciesEntry.faces.Add(faceEntry);
                }

                data.species.Add(speciesEntry);
            }

            return data;
        }

        public void ApplySaveData(EncyclopediaSaveData data)
        {
            encyclopediaData.Clear();

            if (data == null || data.species == null)
                return;

            foreach (var speciesEntry in data.species)
            {
                if (!encyclopediaData.ContainsKey(speciesEntry.speciesId))
                {
                    encyclopediaData[speciesEntry.speciesId] = new SpeciesEncyclopediaData();
                }

                var speciesData = encyclopediaData[speciesEntry.speciesId];
                speciesData.isUnlocked = speciesEntry.isUnlocked;
                speciesData.minWeight = speciesEntry.minWeight;
                speciesData.maxWeight = speciesEntry.maxWeight;

                speciesData.faces = new Dictionary<FaceType, FaceEncyclopediaData>();

                if (speciesEntry.faces != null)
                {
                    foreach (var faceEntry in speciesEntry.faces)
                    {
                        var faceData = new FaceEncyclopediaData
                        {
                            isUnlocked = faceEntry.isUnlocked,
                            minWeight = faceEntry.minWeight,
                            maxWeight = faceEntry.maxWeight
                        };

                        speciesData.faces[faceEntry.faceId] = faceData;
                    }
                }
            }
        }
    }
}

