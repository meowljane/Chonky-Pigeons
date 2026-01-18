using UnityEngine;
using System.Collections.Generic;
using PigeonGame.Data;

namespace PigeonGame.Gameplay
{
    /// <summary>
    /// 도감 데이터 관리
    /// </summary>
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

        /// <summary>
        /// 비둘기를 잡았을 때 도감에 기록
        /// </summary>
        public void RecordPigeon(PigeonInstanceStats stats)
        {
            if (stats == null)
                return;

            PigeonSpecies speciesId = stats.speciesId;
            FaceType faceId = stats.faceId;
            float weight = stats.weight;

            // Species 데이터 가져오기 또는 생성
            if (!encyclopediaData.ContainsKey(speciesId))
            {
                encyclopediaData[speciesId] = new SpeciesEncyclopediaData();
            }

            SpeciesEncyclopediaData speciesData = encyclopediaData[speciesId];
            speciesData.isUnlocked = true;

            // Species 전체 무게 업데이트
            if (speciesData.minWeight == float.MaxValue)
                speciesData.minWeight = weight;
            else if (weight < speciesData.minWeight)
                speciesData.minWeight = weight;
                
            if (speciesData.maxWeight == float.MinValue)
                speciesData.maxWeight = weight;
            else if (weight > speciesData.maxWeight)
                speciesData.maxWeight = weight;

            // Face 데이터 가져오기 또는 생성
            if (!speciesData.faces.ContainsKey(faceId))
            {
                speciesData.faces[faceId] = new FaceEncyclopediaData();
            }

            FaceEncyclopediaData faceData = speciesData.faces[faceId];
            faceData.isUnlocked = true;

            // Face 무게 업데이트
            if (faceData.minWeight == float.MaxValue)
                faceData.minWeight = weight;
            else if (weight < faceData.minWeight)
                faceData.minWeight = weight;
                
            if (faceData.maxWeight == float.MinValue)
                faceData.maxWeight = weight;
            else if (weight > faceData.maxWeight)
                faceData.maxWeight = weight;
        }

        /// <summary>
        /// Species 도감 데이터 가져오기
        /// </summary>
        public SpeciesEncyclopediaData GetSpeciesData(PigeonSpecies speciesType)
        {
            if (encyclopediaData.ContainsKey(speciesType))
            {
                return encyclopediaData[speciesType];
            }
            return new SpeciesEncyclopediaData();
        }

        /// <summary>
        /// Face 도감 데이터 가져오기
        /// </summary>
        public FaceEncyclopediaData GetFaceData(PigeonSpecies speciesType, FaceType faceType)
        {
            SpeciesEncyclopediaData speciesData = GetSpeciesData(speciesType);
            if (speciesData.faces.ContainsKey(faceType))
            {
                return speciesData.faces[faceType];
            }
            return new FaceEncyclopediaData();
        }

        /// <summary>
        /// 모든 Species 데이터 가져오기
        /// </summary>
        public Dictionary<PigeonSpecies, SpeciesEncyclopediaData> GetAllSpeciesData()
        {
            return encyclopediaData;
        }
    }
}

