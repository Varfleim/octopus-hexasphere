
using System.Collections.Generic;

using UnityEngine;

using Leopotam.EcsLite;

namespace HS.Hexasphere.Render
{
    public class MapMode_Data : MonoBehaviour
    {
        public string DefaultMapModeName
        {
            get
            {
                return defaultMapModeName;
            }
        }
        [SerializeField]
        private string defaultMapModeName;
        public List<Color> DefaultMapModeColors
        {
            get
            {
                return defaultMapModeColors;
            }
        }
        [SerializeField]
        private List<Color> defaultMapModeColors = new();
        public Color DefaultMapModeDefaultColor
        {
            get
            {
                return defaultMapModeDefaultColor;
            }
        }
        [SerializeField]
        private Color defaultMapModeDefaultColor;
        public bool DefaultMapModeDefaultState
        {
            get
            {
                return defaultMapModeDefaultState;
            }
        }
        [SerializeField]
        private bool defaultMapModeDefaultState;

        public EcsPackedEntity DefaultMapModePE
        {
            get
            {
                return defaultMapModePE;
            }
            internal set
            {
                defaultMapModePE = value;
            }
        }
        private EcsPackedEntity defaultMapModePE;
    }
}
