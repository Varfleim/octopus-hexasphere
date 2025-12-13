
using System.Collections.Generic;

using UnityEngine;

namespace HS.Hexasphere
{
    public class HexasphereData : MonoBehaviour
    {
        internal const float PHI = 1.61803399f;

        #region TempVariables
        internal readonly Dictionary<DHexaspherePoint, DHexaspherePoint> tempPoints = new();
        #endregion
    }
}
