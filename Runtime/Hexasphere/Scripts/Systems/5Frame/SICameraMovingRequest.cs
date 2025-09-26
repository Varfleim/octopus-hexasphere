
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using GBB.Input;

namespace HS
{
    public class SICameraMovingRequest : IEcsRunSystem
    {
        readonly EcsWorldInject world = default;

        public void Run(IEcsSystems systems)
        {
            //Преобразуем запрос движения камеры
            CameraMovingRequestTransform();
        }

        readonly EcsFilterInject<Inc<RCameraMoving>> cameraMovingRFilter = default;
        readonly EcsPoolInject<RCameraMoving> cameraMovingRPool = default;
        void CameraMovingRequestTransform()
        {
            //Для каждого запроса движения камеры
            foreach (int requestEntity in cameraMovingRFilter.Value)
            {
                //Берём запрос
                ref RCameraMoving requestComp = ref cameraMovingRPool.Value.Get(requestEntity);

                //Запрашиваем движения камеры гексасферы
                HexasphereCameraMovingRequest(
                    requestComp.isHorizontal, requestComp.isVertical, requestComp.isZoom,
                    requestComp.value);

                //Удаляем запрос
                cameraMovingRPool.Value.Del(requestEntity);
            }
        }

        readonly EcsPoolInject<RHexasphereCameraMoving> hexasphereCameraMovingRPool = default;
        void HexasphereCameraMovingRequest(
            bool isHorizontal, bool isVertical, bool isZoom,
            float value)
        {
            //Создаём новую сущность и назначаем ей запрос движения камеры гексасферы
            int requestEntity = world.Value.NewEntity();
            ref RHexasphereCameraMoving requestComp = ref hexasphereCameraMovingRPool.Value.Add(requestEntity);

            //Заполняем данные запроса
            requestComp = new(
                isHorizontal, isVertical, isZoom,
                value);
        }
    }
}
