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
            public int minWeight = int.MaxValue;
            public int maxWeight = int.MinValue;
        }

        [System.Serializable]
        public class SpeciesEncyclopediaData
        {
            public bool isUnlocked;
            public int minWeight = int.MaxValue;
            public int maxWeight = int.MinValue;
            public Dictionary<string, FaceEncyclopediaData> faces = new Dictionary<string, FaceEncyclopediaData>();
        }

        private Dictionary<string, SpeciesEncyclopediaData> encyclopediaData = new Dictionary<string, SpeciesEncyclopediaData>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
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

            string speciesId = stats.speciesId;
            string faceId = stats.faceId;
            int weight = stats.obesity;

            // Species 데이터 가져오기 또는 생성
            if (!encyclopediaData.ContainsKey(speciesId))
            {
                encyclopediaData[speciesId] = new SpeciesEncyclopediaData();
            }

            SpeciesEncyclopediaData speciesData = encyclopediaData[speciesId];
            speciesData.isUnlocked = true;

            // Species 전체 무게 업데이트
            if (speciesData.minWeight == int.MaxValue)
                speciesData.minWeight = weight;
            else if (weight < speciesData.minWeight)
                speciesData.minWeight = weight;
                
            if (speciesData.maxWeight == int.MinValue)
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
            if (faceData.minWeight == int.MaxValue)
                faceData.minWeight = weight;
            else if (weight < faceData.minWeight)
                faceData.minWeight = weight;
                
            if (faceData.maxWeight == int.MinValue)
                faceData.maxWeight = weight;
            else if (weight > faceData.maxWeight)
                faceData.maxWeight = weight;
        }

        /// <summary>
        /// Species 도감 데이터 가져오기
        /// </summary>
        public SpeciesEncyclopediaData GetSpeciesData(string speciesId)
        {
            if (encyclopediaData.ContainsKey(speciesId))
            {
                return encyclopediaData[speciesId];
            }
            return new SpeciesEncyclopediaData();
        }

        /// <summary>
        /// Face 도감 데이터 가져오기
        /// </summary>
        public FaceEncyclopediaData GetFaceData(string speciesId, string faceId)
        {
            SpeciesEncyclopediaData speciesData = GetSpeciesData(speciesId);
            if (speciesData.faces.ContainsKey(faceId))
            {
                return speciesData.faces[faceId];
            }
            return new FaceEncyclopediaData();
        }

        /// <summary>
        /// 모든 Species 데이터 가져오기
        /// </summary>
        public Dictionary<string, SpeciesEncyclopediaData> GetAllSpeciesData()
        {
            return encyclopediaData;
        }
    }
}

