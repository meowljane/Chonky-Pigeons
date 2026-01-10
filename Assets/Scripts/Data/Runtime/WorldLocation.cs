using UnityEngine;
using System;

namespace PigeonGame.Data
{
    [Serializable]
    public class LocationDefinition
    {
        public string id;
        public string name;
        public int baseCrowdLevel;
        public float rareAlertChance;
    }

    [CreateAssetMenu(fileName = "Locations", menuName = "PigeonGame/Locations")]
    public class WorldLocationSet : ScriptableObject
    {
        public int version;
        public LocationDefinition[] locations;

        public LocationDefinition GetLocationById(string locationId)
        {
            foreach (var loc in locations)
            {
                if (loc.id == locationId)
                    return loc;
            }
            return null;
        }
    }
}


