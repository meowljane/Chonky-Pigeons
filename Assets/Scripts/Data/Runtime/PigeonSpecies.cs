using UnityEngine;
using System.Collections.Generic;

namespace PigeonGame.Data
{
    [System.Serializable]
    public class LocationPreference
    {
        public int PARK;
        public int PLAZA;
        public int ALLEY;
        public int HARBOR;
        public int ROOFTOP;
    }

    [System.Serializable]
    public class TrapPreference
    {
        public int BREAD;
        public int SEEDS;
        public int CORN;
        public int PELLET;
        public int SHINY;
    }

    [System.Serializable]
    public class SpeciesDefinition
    {
        public string speciesId;
        public string name;
        public int rarityTier;
        public Vector2Int defaultObesityRange;
        public LocationPreference locationPreference;
        public TrapPreference trapPreference;
        public Sprite icon; // 에디터에서 직접 할당
    }

    [CreateAssetMenu(fileName = "SpeciesSet", menuName = "PigeonGame/Species Set")]
    public class PigeonSpeciesSet : ScriptableObject
    {
        public int version;
        public SpeciesDefinition[] species;

        public SpeciesDefinition GetSpeciesById(string speciesId)
        {
            foreach (var s in species)
            {
                if (s.speciesId == speciesId)
                    return s;
            }
            return null;
        }
    }
}


