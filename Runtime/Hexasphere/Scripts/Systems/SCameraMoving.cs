
using UnityEngine;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace HS.Hexasphere
{
    public class SCameraMoving : IEcsRunSystem
    {
        readonly EcsCustomInject<HexasphereCameraData> hexasphereCameraData = default;

        public void Run(IEcsSystems systems)
        {
            //Двигаем камеру
            CameraMoving();
        }

        readonly EcsFilterInject<Inc<R_HexasphereCameraMoving>> hexasphereCameraMovingRFilter = default;
        readonly EcsPoolInject<R_HexasphereCameraMoving> hexasphereCameraMovingRPool = default;
        void CameraMoving()
        {
            //Для каждого запроса
            foreach (int requestEntity in hexasphereCameraMovingRFilter.Value)
            {
                //Берём запрос
                ref R_HexasphereCameraMoving requestComp = ref hexasphereCameraMovingRPool.Value.Get(requestEntity);

                //Если запрашивается горизонтальное вращение
                if (requestComp.isHorizontal == true)
                {
                    //Выполняем его
                    AdjustRotationY(requestComp.value);
                }
                //Иначе, если запрашивается вертикальное вращение
                else if (requestComp.isVertical == true)
                {
                    //Выполняем его
                    AdjustRotationX(requestComp.value);
                }
                //Иначе, если запрашивается приближение
                else if (requestComp.isZoom == true)
                {
                    //Выполняем его
                    AdjustZoom(requestComp.value);
                }

                //Удаляем запрос
                hexasphereCameraMovingRPool.Value.Del(requestEntity);
            }
        }

        void AdjustRotationY(
            float rotationDelta)
        {
            //Рассчитываем угол вращения
            hexasphereCameraData.Value.rotationAngleY -= rotationDelta * hexasphereCameraData.Value.RotationSpeed * Time.deltaTime;

            //Выравниваем угол
            if (hexasphereCameraData.Value.rotationAngleY < 0f)
            {
                hexasphereCameraData.Value.rotationAngleY += 360f;
            }
            else if (hexasphereCameraData.Value.rotationAngleY >= 360f)
            {
                hexasphereCameraData.Value.rotationAngleY -= 360f;
            }

            //Применяем вращение
            hexasphereCameraData.Value.HeasphereCamera.localRotation = Quaternion.Euler(
                0f, hexasphereCameraData.Value.rotationAngleY, 0f);
        }

        void AdjustRotationX(
            float rotationDelta)
        {
            //Рассчитываем угол вращения
            hexasphereCameraData.Value.rotationAngleX += rotationDelta * hexasphereCameraData.Value.RotationSpeed * Time.deltaTime;

            //Выравниваем угол
            hexasphereCameraData.Value.rotationAngleX = Mathf.Clamp(
                hexasphereCameraData.Value.rotationAngleX, hexasphereCameraData.Value.MinAngleX, hexasphereCameraData.Value.MaxAngleX);

            //Применяем вращение
            hexasphereCameraData.Value.Swiwel.localRotation = Quaternion.Euler(
                hexasphereCameraData.Value.rotationAngleX, 0f, 0f);
        }

        void AdjustZoom(
            float zoomDelta)
        {
            //Рассчитываем приближение камеры
            hexasphereCameraData.Value.zoom = Mathf.Clamp01(hexasphereCameraData.Value.zoom + zoomDelta * Time.deltaTime);

            //Рассчитываем расстояние 
            float zoomDistance = Mathf.Lerp(
                hexasphereCameraData.Value.StickMinZoom, hexasphereCameraData.Value.StickMaxZoom, hexasphereCameraData.Value.zoom);

            //Применяем приближение
            hexasphereCameraData.Value.Stick.localPosition = new(0f, 0f, zoomDistance);
        }
    }
}
