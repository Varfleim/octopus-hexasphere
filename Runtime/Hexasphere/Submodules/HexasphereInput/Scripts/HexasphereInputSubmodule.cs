
using UnityEngine;

using GBB;

namespace HS.Hexasphere.Input
{
    public class HexasphereInputSubmodule : GameSubmodule
    {
        [SerializeField]
        private HexasphereInputData hexasphereInputData;

        public override void AddSystems(GameStartup startup)
        {
            //Добавляем системы инициализации

            //Добавляем покадровые системы
            #region PreFrame
            //Ввод с мыши
            startup.AddPreFrameSystem(new SMouseInput());
            #endregion

            //Добавляем системы рендеринга

            //Добавляем потиковые системы

        }

        public override void InjectData(GameStartup startup)
        {
            //Вводим данные
            startup.InjectData(hexasphereInputData);
        }
    }
}
