
using UnityEngine;

namespace HS
{
    internal class HexasphereCameraData : MonoBehaviour
    {
        public Transform hexasphereCamera;
        public Transform swiwel;
        public Transform stick;
        public UnityEngine.Camera camera;

        public float rotationSpeed;
        public float rotationAngleY;
        public float rotationAngleX;
        public float minAngleX;
        public float maxAngleX;

        public float zoom;
        public float stickMinZoom;
        public float stickMaxZoom;
        public float swiwelMinZoom;
        public float swiwelMaxZoom;
    }
}
