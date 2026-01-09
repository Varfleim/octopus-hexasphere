
using UnityEngine;

using GBB;

namespace HS.Hexasphere.Input
{
    public class HexasphereInput_Submodule : GameSubmodule
    {
        [SerializeField]
        private HexasphereInput_Data hexasphereInput_Data;

        public override void Systems_Add(GameStartup startup)
        {
            //Добавляем системы инициализации

            //Добавляем покадровые системы
            #region PreFrame
            //Ввод с мыши
            startup.PreFrameSystem_Add(new S_Mouse_Input());
            #endregion

            //Добавляем системы рендеринга

            //Добавляем потиковые системы

        }

        public override void Data_Inject(GameStartup startup)
        {
            //Вводим данные
            startup.Data_Inject(hexasphereInput_Data);
        }
    }
}
