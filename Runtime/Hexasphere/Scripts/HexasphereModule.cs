
using System.Collections.Generic;

using UnityEngine;

using GBB;

namespace HS
{
    [CreateAssetMenu]
    internal class HexasphereModule : GameModule
    {
        #region Hexasphere
        public float hexasphereScale;
        public float extrudeMultiplier;
        #endregion

        #region HexasphereRender
        public Material provinceMaterial;
        public Material provinceColoredMaterial;
        public float gradientIntensity;
        public Color tileTintColor;
        public Color ambientColor;
        public float minimumLight;

        public Material thinEdgesMaterial;
        public Color thinEdgesColor;
        [Range(0, 2f)]
        public float thinEdgesColorIntensity;
        
        public Material thickEdgesMaterial;
        public Color thickEdgesColor;
        [Range(0, 2f)]
        public float thickEdgesColorIntensity;

        public Material hoverProvinceHighlightMaterial;
        public Material currentProvinceHighlightMaterial;
        #endregion

        #region Camera
        public float rotationSpeed;
        public float minAngleX;
        public float maxAngleX;

        public float stickMinZoom;
        public float stickMaxZoom;
        public float swiwelMinZoom;
        public float swiwelMaxZoom;
        #endregion

        #region MapMode
        public string defaultMapModeName;
        public List<Color> defaultMapModeColors = new();
        public Color defaultMapModeDefaultColor;
        #endregion

        public override void AddSystems(GameStartup startup)
        {
            //Добавляем системы инициализации
            #region PreInit
            //Создание режимов карты
            startup.AddPreInitSystem(new SMapModesCreation());

            //Генерация гексасферы по запросу
            startup.AddPreInitSystem(new SHexasphereGeneration());
            #endregion

            //Добавляем покадровые системы
            #region Frame
            //Ввод в режимах карты
            startup.AddFrameSystem(new SMapModesInput());

            //Преобразование запроса движения камеры
            startup.AddFrameSystem(new SICameraMovingRequest());
            //Движение камеры
            startup.AddFrameSystem(new SCameraMoving());
            #endregion

            //Добавляем системы рендеринга
            #region PreRender
            //Рассчёт визуализации режимов карты
            //Стандартный режим карты
            startup.AddPreRenderSystemGroup(
                defaultMapModeName,
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
            //Создаём компонент данных гексасферы
            HexasphereData hexasphereData = startup.AddDataObject().AddComponent<HexasphereData>();

            //Переносим в него данные
            //Сфера
            hexasphereData.hexasphereScale = hexasphereScale;
            HexasphereData.ExtrudeMultiplier = extrudeMultiplier;

            //GO
            HexasphereData.HexasphereGO = startup.mapObject;
            HexasphereData.HexasphereCollider = startup.mapCollider as SphereCollider;

            //Шейдеры
            hexasphereData.provinceMaterial = provinceMaterial;
            hexasphereData.provinceColoredMaterial = provinceColoredMaterial;
            hexasphereData.gradientIntensity = gradientIntensity;
            hexasphereData.tileTintColor = tileTintColor;
            hexasphereData.ambientColor = ambientColor;
            hexasphereData.minimumLight = minimumLight;

            hexasphereData.thinEdgesMaterial = thinEdgesMaterial;
            hexasphereData.thinEdgesColor = thinEdgesColor;
            hexasphereData.thinEdgesColorIntensity = thinEdgesColorIntensity;

            hexasphereData.thickEdgesMaterial = thickEdgesMaterial;
            hexasphereData.thickEdgesColor = thickEdgesColor;
            hexasphereData.thickEdgesColorIntensity = thickEdgesColorIntensity;
            
            hexasphereData.hoverProvinceHighlightMaterial = hoverProvinceHighlightMaterial;
            hexasphereData.hoverProvinceHighlightMaterial.shaderKeywords = null;//= new string[] { ShaderParameters.SKW_HIGHLIGHT_TINT_BACKGROUND };
            hexasphereData.hoverProvinceHighlightMaterial.SetFloat(ShaderParameters.ColorShift, 1f);

            hexasphereData.currentProvinceHighlightMaterial = currentProvinceHighlightMaterial;
            hexasphereData.currentProvinceHighlightMaterial.shaderKeywords = null;//= new string[] { ShaderParameters.SKW_HIGHLIGHT_TINT_BACKGROUND };
            hexasphereData.currentProvinceHighlightMaterial.SetFloat(ShaderParameters.ColorShift, 1f);

            //Вводим данные
            startup.InjectData(hexasphereData);

            //Создаём компонент данных камеры гексасферы
            HexasphereCameraData hexasphereCameraData = startup.AddDataObject().AddComponent<HexasphereCameraData>();

            //Переносим в него данные
            //Объекты
            hexasphereCameraData.hexasphereCamera = startup.mapCamera;
            hexasphereCameraData.swiwel = startup.swiwel;
            hexasphereCameraData.stick = startup.stick;
            hexasphereCameraData.camera = startup.camera;

            //Переменные
            hexasphereCameraData.rotationSpeed = rotationSpeed;
            hexasphereCameraData.minAngleX = minAngleX;
            hexasphereCameraData.maxAngleX = maxAngleX;

            hexasphereCameraData.stickMinZoom = stickMinZoom;
            hexasphereCameraData.stickMaxZoom = stickMaxZoom;
            hexasphereCameraData.swiwelMinZoom = swiwelMinZoom;
            hexasphereCameraData.swiwelMaxZoom = swiwelMaxZoom;

            //Вводим данные
            startup.InjectData(hexasphereCameraData);

            //Создаём компонент данных режимов карты
            MapModeData mapModeData = startup.AddDataObject().AddComponent<MapModeData>();

            //Переносим в него данные
            mapModeData.defaultMapModeName = defaultMapModeName;
            mapModeData.defaultMapModeColors = defaultMapModeColors;
            mapModeData.defaultMapModeDefaultColor = defaultMapModeDefaultColor;

            //Вводим данные
            startup.InjectData(mapModeData);
        }
    }
}
