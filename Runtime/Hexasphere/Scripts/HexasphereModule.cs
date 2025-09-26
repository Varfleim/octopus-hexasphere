
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
            //��������� ������� �������������
            #region PreInit
            //�������� ������� �����
            startup.AddPreInitSystem(new SMapModesCreation());

            //��������� ���������� �� �������
            startup.AddPreInitSystem(new SHexasphereGeneration());
            #endregion

            //��������� ���������� �������
            #region Frame
            //���� � ������� �����
            startup.AddFrameSystem(new SMapModesInput());

            //�������������� ������� �������� ������
            startup.AddFrameSystem(new SICameraMovingRequest());
            //�������� ������
            startup.AddFrameSystem(new SCameraMoving());
            #endregion

            //��������� ������� ����������
            #region PreRender
            //������� ������������ ������� �����
            //����������� ����� �����
            startup.AddPreRenderSystemGroup(
                defaultMapModeName,
                false,
                new SDefaultMapModeRender(),
                new SMTDefaultMapModeRender());
            #endregion
            #region PostRender
            //������ ���������� ��� ���������
            startup.AddPostRenderSystem(new SHexasphereRender());
            #endregion

            //��������� ��������� �������

        }

        public override void InjectData(GameStartup startup)
        {
            //������ ��������� ������ ����������
            HexasphereData hexasphereData = startup.AddDataObject().AddComponent<HexasphereData>();

            //��������� � ���� ������
            //�����
            hexasphereData.hexasphereScale = hexasphereScale;
            HexasphereData.ExtrudeMultiplier = extrudeMultiplier;

            //GO
            HexasphereData.HexasphereGO = startup.mapObject;
            HexasphereData.HexasphereCollider = startup.mapCollider as SphereCollider;

            //�������
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

            //������ ������
            startup.InjectData(hexasphereData);

            //������ ��������� ������ ������ ����������
            HexasphereCameraData hexasphereCameraData = startup.AddDataObject().AddComponent<HexasphereCameraData>();

            //��������� � ���� ������
            //�������
            hexasphereCameraData.hexasphereCamera = startup.mapCamera;
            hexasphereCameraData.swiwel = startup.swiwel;
            hexasphereCameraData.stick = startup.stick;
            hexasphereCameraData.camera = startup.camera;

            //����������
            hexasphereCameraData.rotationSpeed = rotationSpeed;
            hexasphereCameraData.minAngleX = minAngleX;
            hexasphereCameraData.maxAngleX = maxAngleX;

            hexasphereCameraData.stickMinZoom = stickMinZoom;
            hexasphereCameraData.stickMaxZoom = stickMaxZoom;
            hexasphereCameraData.swiwelMinZoom = swiwelMinZoom;
            hexasphereCameraData.swiwelMaxZoom = swiwelMaxZoom;

            //������ ������
            startup.InjectData(hexasphereCameraData);

            //������ ��������� ������ ������� �����
            MapModeData mapModeData = startup.AddDataObject().AddComponent<MapModeData>();

            //��������� � ���� ������
            mapModeData.defaultMapModeName = defaultMapModeName;
            mapModeData.defaultMapModeColors = defaultMapModeColors;
            mapModeData.defaultMapModeDefaultColor = defaultMapModeDefaultColor;

            //������ ������
            startup.InjectData(mapModeData);
        }
    }
}
