
using System.Collections.Generic;

using UnityEngine;

using Leopotam.EcsLite;

namespace HS.Hexasphere
{
    public class Hexasphere_Data : MonoBehaviour
    {
        internal const float PHI = 1.61803399f;

        #region TempVariables
        internal readonly Dictionary<D_HexaspherePoint, D_HexaspherePoint> tempPoints = new();
        #endregion

        /// <summary>
        /// Публичная функция, поскольку запрашивается из модуля игры
        /// </summary>
        /// <param name="sR_P"></param>
        /// <param name="mapEntity"></param>
        /// <param name="hexasphereSubdivisions"></param>
        public static void HS_Creation_SR(
            EcsPool<SR_Hexasphere_Creation> sR_P,
            int mapEntity,
            int hexasphereSubdivisions)
        {
            //Назначаем сущности запрос создания гексасферы и заполняем его данные
            ref SR_Hexasphere_Creation requestComp = ref sR_P.Add(mapEntity);
            requestComp = new(hexasphereSubdivisions);
        }
    }
}
