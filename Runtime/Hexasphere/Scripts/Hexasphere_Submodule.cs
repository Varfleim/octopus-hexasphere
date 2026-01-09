
using UnityEngine;

using GBB;

namespace HS.Hexasphere
{
    internal class Hexasphere_Submodule : GameSubmodule
    {
        [SerializeField]
        private Hexasphere_Data hexasphere_Data;
        [SerializeField]
        private HexasphereCamera_Data hexasphereCamera_Data;

        public override void Systems_Add(GameStartup startup)
        {
            //Добавляем системы инициализации
            #region Init
            //Создание гексасферы по запросу
            startup.InitSystem_Add(new S_Hexasphere_Creation());
            #endregion

            //Добавляем покадровые системы
            #region Frame
            //Преобразование запроса движения камеры
            startup.FrameSystem_Add(new SI_Camera_MovingRequest());
            //Движение камеры
            startup.FrameSystem_Add(new S_Camera_Moving());
            #endregion

            //Добавляем системы рендеринга

            //Добавляем потиковые системы
            #region PreTick
            //Создание гексасферы по запросу
            startup.PreTickSystem_Add(new S_Hexasphere_Creation());
            #endregion
        }

        public override void Data_Inject(GameStartup startup)
        {
            //Вводим данные
            startup.Data_Inject(hexasphere_Data);

            //Вводим данные
            startup.Data_Inject(hexasphereCamera_Data);
        }
    }
}
