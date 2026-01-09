
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using GBB.Input;

namespace HS.Hexasphere
{
    public class SI_Camera_MovingRequest : IEcsRunSystem
    {
        readonly EcsWorldInject world = default;

        public void Run(IEcsSystems systems)
        {
            //Преобразуем запрос движения камеры
            Camera_MovingRequestTransform();
        }

        readonly EcsFilterInject<Inc<R_Camera_Moving>> camera_Moving_R_F = default;
        readonly EcsPoolInject<R_Camera_Moving> camera_Moving_R_P = default;
        void Camera_MovingRequestTransform()
        {
            //Для каждого запроса движения камеры
            foreach (int requestEntity in camera_Moving_R_F.Value)
            {
                //Берём запрос
                ref R_Camera_Moving requestComp = ref camera_Moving_R_P.Value.Get(requestEntity);

                //Запрашиваем движения камеры гексасферы
                HSCamera_Moving_Request(
                    requestComp.isHorizontal, requestComp.isVertical, requestComp.isZoom,
                    requestComp.value);

                //Удаляем запрос
                camera_Moving_R_P.Value.Del(requestEntity);
            }
        }

        readonly EcsPoolInject<R_HexasphereCamera_Moving> hSCamera_Moving_R_P = default;
        void HSCamera_Moving_Request(
            bool isHorizontal, bool isVertical, bool isZoom,
            float value)
        {
            //Создаём новую сущность и назначаем ей запрос движения камеры гексасферы
            int requestEntity = world.Value.NewEntity();
            ref R_HexasphereCamera_Moving requestComp = ref hSCamera_Moving_R_P.Value.Add(requestEntity);

            //Заполняем данные запроса
            requestComp = new(
                isHorizontal, isVertical, isZoom,
                value);
        }
    }
}
