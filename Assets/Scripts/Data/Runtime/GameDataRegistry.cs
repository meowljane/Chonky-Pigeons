using UnityEngine;

namespace PigeonGame.Data
{
    public class GameDataRegistry : MonoBehaviour
    {
        [Header("Data Assets")]
        [SerializeField] private PigeonAIProfile aiProfiles;
        [SerializeField] private PigeonFaceSet faces;
        [SerializeField] private PigeonSpeciesSet speciesSet;
        [SerializeField] private TerrainTypeSet terrainTypes;
        [SerializeField] private TrapTypeSet traps;
        [SerializeField] private UpgradeDefinitionSet upgradeDefinitions;

        public static GameDataRegistry Instance { get; private set; }

        public PigeonAIProfile AIProfiles => aiProfiles;
        public PigeonFaceSet Faces => faces;
        public PigeonSpeciesSet SpeciesSet => speciesSet;
        public TerrainTypeSet TerrainTypes => terrainTypes;
        public TrapTypeSet Traps => traps;
        public UpgradeDefinitionSet UpgradeDefinitions => upgradeDefinitions;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                
                // 런타임에 Dictionary 초기화
                InitializeData();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeData()
        {
            if (aiProfiles != null)
            {
                aiProfiles.OnAfterDeserialize();
            }
        }

        private void OnValidate()
        {
            if (aiProfiles != null)
                aiProfiles.OnAfterDeserialize();
        }
    }
}

