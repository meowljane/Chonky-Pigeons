using UnityEngine;
using UnityEditor;
using System.IO;
using Newtonsoft.Json;

namespace PigeonGame.Data.Importers
{
    public static class GameDataImporter
    {
        private const string SOURCE_JSON_PATH = "Assets/GameData/SourceJson";
        private const string GENERATED_PATH = "Assets/GameData/Generated";

        private static void EnsureDirectoryExists(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parentPath = Path.GetDirectoryName(path).Replace('\\', '/');
                string folderName = Path.GetFileName(path);
                
                if (!AssetDatabase.IsValidFolder(parentPath))
                {
                    EnsureDirectoryExists(parentPath);
                }
                
                AssetDatabase.CreateFolder(parentPath, folderName);
            }
        }

        [MenuItem("Tools/Game Data/Import All")]
        public static void ImportAll()
        {
            Debug.Log("=== Game Data Import Started ===");
            
            // Generated 폴더가 없으면 생성
            EnsureDirectoryExists(GENERATED_PATH);
            
            int speciesCount = ImportSpecies();
            int facesCount = ImportFaces();
            int aiProfilesCount = ImportAIProfiles();
            int locationsCount = ImportLocations();
            int trapsCount = ImportTraps();

            Debug.Log($"=== Import Complete ===");
            Debug.Log($"Species: {speciesCount} updated");
            Debug.Log($"Faces: {facesCount} updated");
            Debug.Log($"AI Profiles: {aiProfilesCount} updated");
            Debug.Log($"Locations: {locationsCount} updated");
            Debug.Log($"Traps: {trapsCount} updated");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("Tools/Game Data/Import Species")]
        public static int ImportSpecies()
        {
            EnsureDirectoryExists(GENERATED_PATH);
            
            string jsonPath = Path.Combine(SOURCE_JSON_PATH, "pigeon_species.json");
            if (!File.Exists(jsonPath))
            {
                Debug.LogError($"JSON file not found: {jsonPath}");
                return 0;
            }

            string json = File.ReadAllText(jsonPath);
            var data = JsonConvert.DeserializeObject<SpeciesJsonData>(json);

            if (data == null || data.species == null)
            {
                Debug.LogError("Failed to parse species JSON");
                return 0;
            }

            // SO 생성 또는 업데이트
            string soPath = Path.Combine(GENERATED_PATH, "SpeciesSet.asset");
            PigeonSpeciesSet so = AssetDatabase.LoadAssetAtPath<PigeonSpeciesSet>(soPath);

            if (so == null)
            {
                so = ScriptableObject.CreateInstance<PigeonSpeciesSet>();
                AssetDatabase.CreateAsset(so, soPath);
            }

            // 기존 스프라이트 참조 보존
            var existingSprites = new System.Collections.Generic.Dictionary<string, Sprite>();
            if (so.species != null)
            {
                foreach (var existing in so.species)
                {
                    if (existing.icon != null)
                        existingSprites[existing.speciesId] = existing.icon;
                }
            }

            // 데이터 업데이트
            so.version = data.version;
            so.species = new SpeciesDefinition[data.species.Length];

            for (int i = 0; i < data.species.Length; i++)
            {
                var jsonSpecies = data.species[i];
                so.species[i] = new SpeciesDefinition
                {
                    speciesId = jsonSpecies.speciesId,
                    name = jsonSpecies.name,
                    rarityTier = jsonSpecies.rarityTier,
                    defaultObesityRange = new Vector2Int(jsonSpecies.defaultObesityRange[0], jsonSpecies.defaultObesityRange[1]),
                    locationPreference = jsonSpecies.locationPreference,
                    trapPreference = jsonSpecies.trapPreference,
                    icon = existingSprites.ContainsKey(jsonSpecies.speciesId) ? existingSprites[jsonSpecies.speciesId] : null
                };
            }

            EditorUtility.SetDirty(so);
            Debug.Log($"Imported {so.species.Length} species");
            return so.species.Length;
        }

        [MenuItem("Tools/Game Data/Import Faces")]
        public static int ImportFaces()
        {
            EnsureDirectoryExists(GENERATED_PATH);
            
            string jsonPath = Path.Combine(SOURCE_JSON_PATH, "pigeon_faces.json");
            if (!File.Exists(jsonPath))
            {
                Debug.LogError($"JSON file not found: {jsonPath}");
                return 0;
            }

            string json = File.ReadAllText(jsonPath);
            var data = JsonConvert.DeserializeObject<FacesJsonData>(json);

            if (data == null || data.faces == null)
            {
                Debug.LogError("Failed to parse faces JSON");
                return 0;
            }

            string soPath = Path.Combine(GENERATED_PATH, "Faces.asset");
            PigeonFace so = AssetDatabase.LoadAssetAtPath<PigeonFace>(soPath);

            if (so == null)
            {
                so = ScriptableObject.CreateInstance<PigeonFace>();
                AssetDatabase.CreateAsset(so, soPath);
            }

            so.version = data.version;
            so.faces = new FaceDefinition[data.faces.Length];

            for (int i = 0; i < data.faces.Length; i++)
            {
                var jsonFace = data.faces[i];
                so.faces[i] = new FaceDefinition
                {
                    id = jsonFace.id,
                    name = jsonFace.name,
                    priceMultiplier = jsonFace.priceMultiplier
                };
            }

            EditorUtility.SetDirty(so);
            Debug.Log($"Imported {so.faces.Length} faces");
            return so.faces.Length;
        }

        [MenuItem("Tools/Game Data/Import AI Profiles")]
        public static int ImportAIProfiles()
        {
            EnsureDirectoryExists(GENERATED_PATH);
            
            string jsonPath = Path.Combine(SOURCE_JSON_PATH, "pigeon_ai_profiles.json");
            if (!File.Exists(jsonPath))
            {
                Debug.LogError($"JSON file not found: {jsonPath}");
                return 0;
            }

            string json = File.ReadAllText(jsonPath);
            var data = JsonConvert.DeserializeObject<AIProfilesJsonData>(json);

            if (data == null)
            {
                Debug.LogError("Failed to parse AI profiles JSON");
                return 0;
            }

            string soPath = Path.Combine(GENERATED_PATH, "AIProfiles.asset");
            PigeonAIProfile so = AssetDatabase.LoadAssetAtPath<PigeonAIProfile>(soPath);

            if (so == null)
            {
                so = ScriptableObject.CreateInstance<PigeonAIProfile>();
                AssetDatabase.CreateAsset(so, soPath);
            }

            so.version = data.version;
            so.obesityRule = data.obesityRule;
            so.rarityBasePrice = data.rarityBasePrice;
            so.tiers = data.tiers;
            so.stressToEatModifier = data.stressToEatModifier;

            so.OnBeforeSerialize();
            EditorUtility.SetDirty(so);
            Debug.Log("Imported AI Profiles");
            return 1;
        }

        [MenuItem("Tools/Game Data/Import Locations")]
        public static int ImportLocations()
        {
            EnsureDirectoryExists(GENERATED_PATH);
            
            string jsonPath = Path.Combine(SOURCE_JSON_PATH, "world_locations.json");
            if (!File.Exists(jsonPath))
            {
                Debug.LogError($"JSON file not found: {jsonPath}");
                return 0;
            }

            string json = File.ReadAllText(jsonPath);
            var data = JsonConvert.DeserializeObject<LocationsJsonData>(json);

            if (data == null || data.locations == null)
            {
                Debug.LogError("Failed to parse locations JSON");
                return 0;
            }

            string soPath = Path.Combine(GENERATED_PATH, "Locations.asset");
            WorldLocationSet so = AssetDatabase.LoadAssetAtPath<WorldLocationSet>(soPath);

            if (so == null)
            {
                so = ScriptableObject.CreateInstance<WorldLocationSet>();
                AssetDatabase.CreateAsset(so, soPath);
            }

            so.version = data.version;
            so.locations = new LocationDefinition[data.locations.Length];

            for (int i = 0; i < data.locations.Length; i++)
            {
                var jsonLoc = data.locations[i];
                so.locations[i] = new LocationDefinition
                {
                    id = jsonLoc.id,
                    name = jsonLoc.name,
                    baseCrowdLevel = jsonLoc.baseCrowdLevel,
                    rareAlertChance = jsonLoc.rareAlertChance
                };
            }

            EditorUtility.SetDirty(so);
            Debug.Log($"Imported {so.locations.Length} locations");
            return so.locations.Length;
        }

        [MenuItem("Tools/Game Data/Import Traps")]
        public static int ImportTraps()
        {
            EnsureDirectoryExists(GENERATED_PATH);
            
            string jsonPath = Path.Combine(SOURCE_JSON_PATH, "trap_types.json");
            if (!File.Exists(jsonPath))
            {
                Debug.LogError($"JSON file not found: {jsonPath}");
                return 0;
            }

            string json = File.ReadAllText(jsonPath);
            var data = JsonConvert.DeserializeObject<TrapsJsonData>(json);

            if (data == null || data.traps == null)
            {
                Debug.LogError("Failed to parse traps JSON");
                return 0;
            }

            string soPath = Path.Combine(GENERATED_PATH, "Traps.asset");
            TrapTypeSet so = AssetDatabase.LoadAssetAtPath<TrapTypeSet>(soPath);

            if (so == null)
            {
                so = ScriptableObject.CreateInstance<TrapTypeSet>();
                AssetDatabase.CreateAsset(so, soPath);
            }

            so.version = data.version;
            so.traps = new TrapDefinition[data.traps.Length];

            for (int i = 0; i < data.traps.Length; i++)
            {
                var jsonTrap = data.traps[i];
                so.traps[i] = new TrapDefinition
                {
                    id = jsonTrap.id,
                    name = jsonTrap.name,
                    cost = jsonTrap.cost,
                    feedAmount = jsonTrap.feedAmount
                };
            }

            EditorUtility.SetDirty(so);
            Debug.Log($"Imported {so.traps.Length} traps");
            return so.traps.Length;
        }

        // JSON 데이터 구조체들
        [System.Serializable]
        private class SpeciesJsonData
        {
            public int version;
            public SpeciesJsonItem[] species;
        }

        [System.Serializable]
        private class SpeciesJsonItem
        {
            public string speciesId;
            public string name;
            public int rarityTier;
            public int[] defaultObesityRange;
            public LocationPreference locationPreference;
            public TrapPreference trapPreference;
        }

        [System.Serializable]
        private class FacesJsonData
        {
            public int version;
            public FaceJsonItem[] faces;
        }

        [System.Serializable]
        private class FaceJsonItem
        {
            public string id;
            public string name;
            public float priceMultiplier;
        }

        [System.Serializable]
        private class AIProfilesJsonData
        {
            public int version;
            public ObesityRule obesityRule;
            public System.Collections.Generic.Dictionary<int, int> rarityBasePrice;
            public System.Collections.Generic.Dictionary<int, RarityTierProfile> tiers;
            public StressToEatModifier stressToEatModifier;
        }

        [System.Serializable]
        private class LocationsJsonData
        {
            public int version;
            public LocationJsonItem[] locations;
        }

        [System.Serializable]
        private class LocationJsonItem
        {
            public string id;
            public string name;
            public int baseCrowdLevel;
            public float rareAlertChance;
        }

        [System.Serializable]
        private class TrapsJsonData
        {
            public int version;
            public TrapJsonItem[] traps;
        }

        [System.Serializable]
        private class TrapJsonItem
        {
            public string id;
            public string name;
            public int cost;
            public int feedAmount;
        }
    }
}

