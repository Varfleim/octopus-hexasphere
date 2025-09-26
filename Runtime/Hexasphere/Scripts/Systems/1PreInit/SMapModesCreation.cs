
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace HS
{
    public class SMapModesCreation : IEcsInitSystem
    {
        readonly EcsWorldInject world = default;


        readonly EcsPoolInject<GBB.Map.SRMapModeCreation> mapModeCreationSRPool = default;

        readonly EcsPoolInject<GBB.Map.RMapModeUpdateColorsListSecond> mapModeUpdateColorsListSecondRPool = default;


        readonly EcsCustomInject<MapModeData> mapModeData = default;

        public void Init(IEcsSystems systems)
        {
            //������ ����������� ����� �����
            DefaultMapModeCreation();
        }

        readonly EcsPoolInject<CDefaultMapMode> defaultMapModePool = default;
        void DefaultMapModeCreation()
        {
            //������ ����� �������� � ��������� �� ��������� ������������ ������ �����
            int mapModeEntity = world.Value.NewEntity();
            ref CDefaultMapMode defaultMapMode = ref defaultMapModePool.Value.Add(mapModeEntity);

            //��������� PE ������������ ������ �����
            mapModeData.Value.defaultMapModePE = world.Value.PackEntity(mapModeEntity);

            //����������� ���������� �������� ���������� ������ �����
            GBB.Map.MapModeData.MapModeCreationRequest(
                mapModeCreationSRPool.Value,
                mapModeEntity, mapModeData.Value.defaultMapModeName,
                false);

            //����������� ��������� ���������� ������ ������ ������ �����
            GBB.Map.MapModeData.MapModeUpdateColorsListSecondRequest(
                world.Value,
                mapModeUpdateColorsListSecondRPool.Value,
                mapModeData.Value.defaultMapModePE,
                mapModeData.Value.defaultMapModeColors, mapModeData.Value.defaultMapModeDefaultColor);
        }
    }
}
