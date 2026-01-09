using UnityEngine;

using GBB;

namespace HS.Hexasphere.Render
{
    public class HexasphereRender_Submodule : GameSubmodule
    {
        [SerializeField]
        private HexasphereRender_Data hexasphereRender_Data;
        [SerializeField]
        private MapMode_Data mapMode_Data;

        public override void Systems_Add(GameStartup startup)
        {
            //Добавляем системы инициализации
            #region PreInit
            //Создание режимов карты
            startup.PreInitSystem_Add(new S_MapMode_Creation());
            #endregion

            //Добавляем покадровые системы
            #region Frame
            //Ввод в режимах карты
            startup.FrameSystem_Add(new S_MapMode_Input());
            #endregion

            //Добавляем системы рендеринга
            #region PreRender
            //Рассчёт визуализации режимов карты
            //Стандартный режим карты
            startup.PreRenderSystem_AddGroup(
                mapMode_Data.DefaultMapModeName,
                false,
                new S_DefaultMapMode_Render(),
                new SMT_DefaultMapMode_Render());
            #endregion
            #region PostRender
            //Рендер гексасферы при изменении
            startup.PostRenderSystem_Add(new S_Hexasphere_Render());
            #endregion

            //Добавляем потиковые системы

        }

        public override void Data_Inject(GameStartup startup)
        {
            //ТЕСТ
            hexasphereRender_Data.hoverProvinceHighlightMaterial.shaderKeywords = null;//= new string[] { ShaderParameters.SKW_HIGHLIGHT_TINT_BACKGROUND };
            hexasphereRender_Data.hoverProvinceHighlightMaterial.SetFloat(ShaderParameters.ColorShift, 1f);

            hexasphereRender_Data.currentProvinceHighlightMaterial.shaderKeywords = null;//= new string[] { ShaderParameters.SKW_HIGHLIGHT_TINT_BACKGROUND };
            hexasphereRender_Data.currentProvinceHighlightMaterial.SetFloat(ShaderParameters.ColorShift, 1f);
            //ТЕСТ

            //Вводим данные
            startup.Data_Inject(hexasphereRender_Data);

            //Вводим данные
            startup.Data_Inject(mapMode_Data);
        }
    }
}
