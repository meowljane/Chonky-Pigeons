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
        [SerializeField] private MapTypeSet mapTypes;
        [SerializeField] private TrapTypeSet traps;
        [SerializeField] private UpgradeDefinitionSet upgradeDefinitions;
        [SerializeField] private DoorSet doorSet;

        public static GameDataRegistry Instance { get; private set; }

        public PigeonAIProfile AIProfiles => aiProfiles;
        public PigeonFaceSet Faces => faces;
        public PigeonSpeciesSet SpeciesSet => speciesSet;
        public TerrainTypeSet TerrainTypes => terrainTypes;
        public MapTypeSet MapTypes => mapTypes;
        public TrapTypeSet Traps => traps;
        public UpgradeDefinitionSet UpgradeDefinitions => upgradeDefinitions;
        public DoorSet DoorSet => doorSet;

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

