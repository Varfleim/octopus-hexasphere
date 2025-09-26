
using System.Collections.Generic;

using UnityEngine;

using Leopotam.EcsLite;

namespace HS
{
    internal class MapModeData : MonoBehaviour
    {
        public string defaultMapModeName;
        public EcsPackedEntity defaultMapModePE;
        public List<Color> defaultMapModeColors = new();
        public Color defaultMapModeDefaultColor;
    }
}
