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

        private void UpdateWeightRange(SpeciesEncyclopediaData data, float weight)
        {
            data.isUnlocked = true;
            if (data.minWeight == float.MaxValue)
                data.minWeight = weight;
            else if (weight < data.minWeight)
                data.minWeight = weight;

            if (data.maxWeight == float.MinValue)
                data.maxWeight = weight;
            else if (weight > data.maxWeight)
                data.maxWeight = weight;
        }

        public void RecordPigeon(PigeonInstanceStats stats)
        {
            if (stats == null)
                return;

            PigeonSpecies speciesId = stats.speciesId;
            FaceType faceId = stats.faceId;
            float weight = stats.weight;

            if (!encyclopediaData.ContainsKey(speciesId))
                encyclopediaData[speciesId] = new SpeciesEncyclopediaData();

            SpeciesEncyclopediaData speciesData = encyclopediaData[speciesId];
            UpdateWeightRange(speciesData, weight);

            if (!speciesData.faces.ContainsKey(faceId))
                speciesData.faces[faceId] = new FaceEncyclopediaData();

            speciesData.faces[faceId].isUnlocked = true;
        }

        public SpeciesEncyclopediaData GetSpeciesData(PigeonSpecies speciesType)
        {
            return encyclopediaData.TryGetValue(speciesType, out var data) ? data : new SpeciesEncyclopediaData();
        }

        public FaceEncyclopediaData GetFaceData(PigeonSpecies speciesType, FaceType faceType)
        {
            SpeciesEncyclopediaData speciesData = GetSpeciesData(speciesType);
            return speciesData.faces.TryGetValue(faceType, out var data) ? data : new FaceEncyclopediaData();
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
                        isUnlocked = faceData.isUnlocked
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
                            isUnlocked = faceEntry.isUnlocked
                        };

                        speciesData.faces[faceEntry.faceId] = faceData;
                    }
                }
            }
        }
    }
}

