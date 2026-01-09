
using UnityEngine;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace HS.Hexasphere
{
    public class S_Camera_Moving : IEcsRunSystem
    {
        readonly EcsCustomInject<HexasphereCamera_Data> hexasphereCamera_Data = default;

        public void Run(IEcsSystems systems)
        {
            //Двигаем камеру
            Camera_Moving();
        }

        readonly EcsFilterInject<Inc<R_HexasphereCamera_Moving>> hSCamera_Moving_R_F = default;
        readonly EcsPoolInject<R_HexasphereCamera_Moving> hSCamera_Moving_R_P = default;
        void Camera_Moving()
        {
            //Для каждого запроса
            foreach (int requestEntity in hSCamera_Moving_R_F.Value)
            {
                //Берём запрос
                ref R_HexasphereCamera_Moving requestComp = ref hSCamera_Moving_R_P.Value.Get(requestEntity);

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
                hSCamera_Moving_R_P.Value.Del(requestEntity);
            }
        }

        void AdjustRotationY(
            float rotationDelta)
        {
            //Рассчитываем угол вращения
            hexasphereCamera_Data.Value.rotationAngleY -= rotationDelta * hexasphereCamera_Data.Value.RotationSpeed * Time.deltaTime;

            //Выравниваем угол
            if (hexasphereCamera_Data.Value.rotationAngleY < 0f)
            {
                hexasphereCamera_Data.Value.rotationAngleY += 360f;
            }
            else if (hexasphereCamera_Data.Value.rotationAngleY >= 360f)
            {
                hexasphereCamera_Data.Value.rotationAngleY -= 360f;
            }

            //Применяем вращение
            hexasphereCamera_Data.Value.HeasphereCamera.localRotation = Quaternion.Euler(
                0f, hexasphereCamera_Data.Value.rotationAngleY, 0f);
        }

        void AdjustRotationX(
            float rotationDelta)
        {
            //Рассчитываем угол вращения
            hexasphereCamera_Data.Value.rotationAngleX += rotationDelta * hexasphereCamera_Data.Value.RotationSpeed * Time.deltaTime;

            //Выравниваем угол
            hexasphereCamera_Data.Value.rotationAngleX = Mathf.Clamp(
                hexasphereCamera_Data.Value.rotationAngleX, hexasphereCamera_Data.Value.MinAngleX, hexasphereCamera_Data.Value.MaxAngleX);

            //Применяем вращение
            hexasphereCamera_Data.Value.Swiwel.localRotation = Quaternion.Euler(
                hexasphereCamera_Data.Value.rotationAngleX, 0f, 0f);
        }

        void AdjustZoom(
            float zoomDelta)
        {
            //Рассчитываем приближение камеры
            hexasphereCamera_Data.Value.zoom = Mathf.Clamp01(hexasphereCamera_Data.Value.zoom + zoomDelta * Time.deltaTime);

            //Рассчитываем расстояние 
            float zoomDistance = Mathf.Lerp(
                hexasphereCamera_Data.Value.StickMinZoom, hexasphereCamera_Data.Value.StickMaxZoom, hexasphereCamera_Data.Value.zoom);

            //Применяем приближение
            hexasphereCamera_Data.Value.Stick.localPosition = new(0f, 0f, zoomDistance);
        }
    }
}
