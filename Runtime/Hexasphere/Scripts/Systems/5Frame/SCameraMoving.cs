
using UnityEngine;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace HS
{
    public class SCameraMoving : IEcsRunSystem
    {
        readonly EcsCustomInject<HexasphereCameraData> hexasphereCameraData = default;

        public void Run(IEcsSystems systems)
        {
            //Двигаем камеру
            CameraMoving();
        }

        readonly EcsFilterInject<Inc<RHexasphereCameraMoving>> hexasphereCameraMovingRFilter = default;
        readonly EcsPoolInject<RHexasphereCameraMoving> hexasphereCameraMovingRPool = default;
        void CameraMoving()
        {
            //Для каждого запроса
            foreach (int requestEntity in hexasphereCameraMovingRFilter.Value)
            {
                //Берём запрос
                ref RHexasphereCameraMoving requestComp = ref hexasphereCameraMovingRPool.Value.Get(requestEntity);

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
            hexasphereCameraData.Value.rotationAngleY -= rotationDelta * hexasphereCameraData.Value.rotationSpeed * Time.deltaTime;

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
            hexasphereCameraData.Value.hexasphereCamera.localRotation = Quaternion.Euler(
                0f, hexasphereCameraData.Value.rotationAngleY, 0f);
        }

        void AdjustRotationX(
            float rotationDelta)
        {
            //Рассчитываем угол вращения
            hexasphereCameraData.Value.rotationAngleX += rotationDelta * hexasphereCameraData.Value.rotationSpeed * Time.deltaTime;

            //Выравниваем угол
            hexasphereCameraData.Value.rotationAngleX = Mathf.Clamp(
                hexasphereCameraData.Value.rotationAngleX, hexasphereCameraData.Value.minAngleX, hexasphereCameraData.Value.maxAngleX);

            //Применяем вращение
            hexasphereCameraData.Value.swiwel.localRotation = Quaternion.Euler(
                hexasphereCameraData.Value.rotationAngleX, 0f, 0f);
        }

        void AdjustZoom(
            float zoomDelta)
        {
            //Рассчитываем приближение камеры
            hexasphereCameraData.Value.zoom = Mathf.Clamp01(hexasphereCameraData.Value.zoom + zoomDelta * Time.deltaTime);

            //Рассчитываем расстояние 
            float zoomDistance = Mathf.Lerp(
                hexasphereCameraData.Value.stickMinZoom, hexasphereCameraData.Value.stickMaxZoom, hexasphereCameraData.Value.zoom);

            //Применяем приближение
            hexasphereCameraData.Value.stick.localPosition = new(0f, 0f, zoomDistance);
        }
    }
}
