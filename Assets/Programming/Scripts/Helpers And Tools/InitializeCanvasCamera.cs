using UnityEngine;

namespace CoreCraft.Programming.HelpersAndTools
{
    public class InitializeCanvasCamera : MonoBehaviour
    {
        public SO_GameObject SOCamera;
        public Canvas Canvas;

        [Tooltip("This is based on the near clip plane of the camera")]
        public float OffSetPlaneDistance = 0.2f;

        private void Start()
        {
            Canvas.worldCamera = SOCamera.GameObject.GetComponent<Camera>();
            Canvas.planeDistance = SOCamera.GameObject.GetComponent<Camera>().nearClipPlane + OffSetPlaneDistance;
        }
    }
}