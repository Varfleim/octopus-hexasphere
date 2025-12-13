
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using GBB.Input;

namespace HS.Hexasphere
{
    public class SICameraMovingRequest : IEcsRunSystem
    {
        readonly EcsWorldInject world = default;

        public void Run(IEcsSystems systems)
        {
            //Преобразуем запрос движения камеры
            CameraMovingRequestTransform();
        }

        readonly EcsFilterInject<Inc<R_CameraMoving>> cameraMovingRFilter = default;
        readonly EcsPoolInject<R_CameraMoving> cameraMovingRPool = default;
        void CameraMovingRequestTransform()
        {
            //Для каждого запроса движения камеры
            foreach (int requestEntity in cameraMovingRFilter.Value)
            {
                //Берём запрос
                ref R_CameraMoving requestComp = ref cameraMovingRPool.Value.Get(requestEntity);

                //Запрашиваем движения камеры гексасферы
                HexasphereCameraMovingRequest(
                    requestComp.isHorizontal, requestComp.isVertical, requestComp.isZoom,
                    requestComp.value);

                //Удаляем запрос
                cameraMovingRPool.Value.Del(requestEntity);
            }
        }

        readonly EcsPoolInject<R_HexasphereCameraMoving> hexasphereCameraMovingRPool = default;
        void HexasphereCameraMovingRequest(
            bool isHorizontal, bool isVertical, bool isZoom,
            float value)
        {
            //Создаём новую сущность и назначаем ей запрос движения камеры гексасферы
            int requestEntity = world.Value.NewEntity();
            ref R_HexasphereCameraMoving requestComp = ref hexasphereCameraMovingRPool.Value.Add(requestEntity);

            //Заполняем данные запроса
            requestComp = new(
                isHorizontal, isVertical, isZoom,
                value);
        }
    }
}
