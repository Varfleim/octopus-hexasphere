
using UnityEngine;

using GBB;

namespace HS.Hexasphere
{
    internal class HexasphereSubmodule : GameSubmodule
    {
        [SerializeField]
        private HexasphereData hexasphereData;
        [SerializeField]
        private HexasphereCameraData hexasphereCameraData;

        public override void AddSystems(GameStartup startup)
        {
            //Добавляем системы инициализации
            #region Init
            //Создание гексасферы по запросу
            startup.AddInitSystem(new SHexasphereCreation());
            #endregion

            //Добавляем покадровые системы
            #region Frame
            //Преобразование запроса движения камеры
            startup.AddFrameSystem(new SICameraMovingRequest());
            //Движение камеры
            startup.AddFrameSystem(new SCameraMoving());
            #endregion

            //Добавляем системы рендеринга

            //Добавляем потиковые системы
            #region PreTick
            //Создание гексасферы по запросу
            startup.AddPreTickSystem(new SHexasphereCreation());
            #endregion
        }

        public override void InjectData(GameStartup startup)
        {
            //Вводим данные
            startup.InjectData(hexasphereData);

            //Вводим данные
            startup.InjectData(hexasphereCameraData);
        }
    }
}
