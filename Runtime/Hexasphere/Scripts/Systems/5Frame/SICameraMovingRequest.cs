
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
            //����������� ������ �������� ������
            CameraMovingRequestTransform();
        }

        readonly EcsFilterInject<Inc<RCameraMoving>> cameraMovingRFilter = default;
        readonly EcsPoolInject<RCameraMoving> cameraMovingRPool = default;
        void CameraMovingRequestTransform()
        {
            //��� ������� ������� �������� ������
            foreach (int requestEntity in cameraMovingRFilter.Value)
            {
                //���� ������
                ref RCameraMoving requestComp = ref cameraMovingRPool.Value.Get(requestEntity);

                //����������� �������� ������ ����������
                HexasphereCameraMovingRequest(
                    requestComp.isHorizontal, requestComp.isVertical, requestComp.isZoom,
                    requestComp.value);

                //������� ������
                cameraMovingRPool.Value.Del(requestEntity);
            }
        }

        readonly EcsPoolInject<RHexasphereCameraMoving> hexasphereCameraMovingRPool = default;
        void HexasphereCameraMovingRequest(
            bool isHorizontal, bool isVertical, bool isZoom,
            float value)
        {
            //������ ����� �������� � ��������� �� ������ �������� ������ ����������
            int requestEntity = world.Value.NewEntity();
            ref RHexasphereCameraMoving requestComp = ref hexasphereCameraMovingRPool.Value.Add(requestEntity);

            //��������� ������ �������
            requestComp = new(
                isHorizontal, isVertical, isZoom,
                value);
        }
    }
}
