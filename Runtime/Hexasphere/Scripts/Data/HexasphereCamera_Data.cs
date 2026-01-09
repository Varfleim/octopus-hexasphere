
using UnityEngine;

namespace HS.Hexasphere
{
    public class HexasphereCamera_Data : MonoBehaviour
    {
        public Transform HeasphereCamera
        {
            get
            {
                return hexasphereCamera;
            }
        }
        [SerializeField]
        private Transform hexasphereCamera;
        public Transform Swiwel
        {
            get
            {
                return swiwel;
            }
        }
        [SerializeField]
        private Transform swiwel;
        public Transform Stick
        {
            get
            {
                return stick;
            }
        }
        [SerializeField]
        private Transform stick;
        public Camera Camera
        {
            get
            {
                return camera;
            }
        }
        [SerializeField]
        private Camera camera;

        public float RotationSpeed
        {
            get
            {
                return rotationSpeed;
            }
        }
        [SerializeField]
        private float rotationSpeed;
        public float MinAngleX
        {
            get
            {
                return minAngleX;
            }
        }
        [SerializeField]
        private float minAngleX;
        public float MaxAngleX
        {
            get
            {
                return maxAngleX;
            }
        }
        [SerializeField]
        private float maxAngleX;
        internal float rotationAngleX;
        internal float rotationAngleY;

        public float StickMinZoom
        {
            get
            {
                return stickMinZoom;
            }
        }
        [SerializeField]
        private float stickMinZoom;
        public float StickMaxZoom
        {
            get
            {
                return stickMaxZoom;
            }
        }
        [SerializeField]
        private float stickMaxZoom;
        public float SwiwelMinZoom
        {
            get
            {
                return swiwelMinZoom;
            }
        }
        [SerializeField]
        private float swiwelMinZoom;
        public float SwiwelMaxZoom
        {
            get
            {
                return swiwelMaxZoom;
            }
        }
        [SerializeField]
        private float swiwelMaxZoom;
        internal float zoom;
    }
}
