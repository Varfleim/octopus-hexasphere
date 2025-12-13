using UnityEngine;

using GBB;

namespace HS.Hexasphere.Render
{
    public class HexasphereRenderSubmodule : GameSubmodule
    {
        [SerializeField]
        private HexasphereRenderData hexasphereRenderData;
        [SerializeField]
        private MapModeData mapModeData;

        public override void AddSystems(GameStartup startup)
        {
            //Добавляем системы инициализации
            #region PreInit
            //Создание режимов карты
            startup.AddPreInitSystem(new SMapModesCreation());
            #endregion

            //Добавляем покадровые системы
            #region Frame
            //Ввод в режимах карты
            startup.AddFrameSystem(new SMapModesInput());
            #endregion

            //Добавляем системы рендеринга
            #region PreRender
            //Рассчёт визуализации режимов карты
            //Стандартный режим карты
            startup.AddPreRenderSystemGroup(
                mapModeData.DefaultMapModeName,
                false,
                new SDefaultMapModeRender(),
                new SMTDefaultMapModeRender());
            #endregion
            #region PostRender
            //Рендер гексасферы при изменении
            startup.AddPostRenderSystem(new SHexasphereRender());
            #endregion

            //Добавляем потиковые системы

        }

        public override void InjectData(GameStartup startup)
        {
            //ТЕСТ
            hexasphereRenderData.hoverProvinceHighlightMaterial.shaderKeywords = null;//= new string[] { ShaderParameters.SKW_HIGHLIGHT_TINT_BACKGROUND };
            hexasphereRenderData.hoverProvinceHighlightMaterial.SetFloat(ShaderParameters.ColorShift, 1f);

            hexasphereRenderData.currentProvinceHighlightMaterial.shaderKeywords = null;//= new string[] { ShaderParameters.SKW_HIGHLIGHT_TINT_BACKGROUND };
            hexasphereRenderData.currentProvinceHighlightMaterial.SetFloat(ShaderParameters.ColorShift, 1f);
            //ТЕСТ

            //Вводим данные
            startup.InjectData(hexasphereRenderData);

            //Вводим данные
            startup.InjectData(mapModeData);
        }
    }
}
