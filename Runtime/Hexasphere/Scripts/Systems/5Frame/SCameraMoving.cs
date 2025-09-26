
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
            //������� ������
            CameraMoving();
        }

        readonly EcsFilterInject<Inc<RHexasphereCameraMoving>> hexasphereCameraMovingRFilter = default;
        readonly EcsPoolInject<RHexasphereCameraMoving> hexasphereCameraMovingRPool = default;
        void CameraMoving()
        {
            //��� ������� �������
            foreach (int requestEntity in hexasphereCameraMovingRFilter.Value)
            {
                //���� ������
                ref RHexasphereCameraMoving requestComp = ref hexasphereCameraMovingRPool.Value.Get(requestEntity);

                //���� ������������� �������������� ��������
                if (requestComp.isHorizontal == true)
                {
                    //��������� ���
                    AdjustRotationY(requestComp.value);
                }
                //�����, ���� ������������� ������������ ��������
                else if (requestComp.isVertical == true)
                {
                    //��������� ���
                    AdjustRotationX(requestComp.value);
                }
                //�����, ���� ������������� �����������
                else if (requestComp.isZoom == true)
                {
                    //��������� ���
                    AdjustZoom(requestComp.value);
                }

                //������� ������
                hexasphereCameraMovingRPool.Value.Del(requestEntity);
            }
        }

        void AdjustRotationY(
            float rotationDelta)
        {
            //������������ ���� ��������
            hexasphereCameraData.Value.rotationAngleY -= rotationDelta * hexasphereCameraData.Value.rotationSpeed * Time.deltaTime;

            //����������� ����
            if (hexasphereCameraData.Value.rotationAngleY < 0f)
            {
                hexasphereCameraData.Value.rotationAngleY += 360f;
            }
            else if (hexasphereCameraData.Value.rotationAngleY >= 360f)
            {
                hexasphereCameraData.Value.rotationAngleY -= 360f;
            }

            //��������� ��������
            hexasphereCameraData.Value.hexasphereCamera.localRotation = Quaternion.Euler(
                0f, hexasphereCameraData.Value.rotationAngleY, 0f);
        }

        void AdjustRotationX(
            float rotationDelta)
        {
            //������������ ���� ��������
            hexasphereCameraData.Value.rotationAngleX += rotationDelta * hexasphereCameraData.Value.rotationSpeed * Time.deltaTime;

            //����������� ����
            hexasphereCameraData.Value.rotationAngleX = Mathf.Clamp(
                hexasphereCameraData.Value.rotationAngleX, hexasphereCameraData.Value.minAngleX, hexasphereCameraData.Value.maxAngleX);

            //��������� ��������
            hexasphereCameraData.Value.swiwel.localRotation = Quaternion.Euler(
                hexasphereCameraData.Value.rotationAngleX, 0f, 0f);
        }

        void AdjustZoom(
            float zoomDelta)
        {
            //������������ ����������� ������
            hexasphereCameraData.Value.zoom = Mathf.Clamp01(hexasphereCameraData.Value.zoom + zoomDelta * Time.deltaTime);

            //������������ ���������� 
            float zoomDistance = Mathf.Lerp(
                hexasphereCameraData.Value.stickMinZoom, hexasphereCameraData.Value.stickMaxZoom, hexasphereCameraData.Value.zoom);

            //��������� �����������
            hexasphereCameraData.Value.stick.localPosition = new(0f, 0f, zoomDistance);
        }
    }
}
