using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
Responsible for the projector plane setup 
*/

namespace Simulation
{
    public class OrbitViewManager : MonoBehaviour
    {

        public enum CameraType
        {
            View,
            Capture
        }

        public KeyCode changeCameraKey;

        public Camera viewCamera;
        public Camera captureCamera;
        public Transform focalPoint;

        [SerializeField, Range(1f, 20f)]
        public float distance = 5f;
        public GameObject projectorPlane;

        private GameObject _manager;
        private CaptureViewCollection _captures;

        CameraType currentlyControlling;

        // Start is called before the first frame update
        void Start()
        {
            currentlyControlling = CameraType.View;
            captureCamera.enabled = true;
            viewCamera.enabled = true;
            _manager = GameObject.Find("OrbitViewManager");
            _captures = _manager.GetComponent<CaptureViewCollection>();
        }


        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(changeCameraKey))
            {
                currentlyControlling = currentlyControlling == CameraType.View ? CameraType.Capture : CameraType.View;
                Debug.Log("Currently controlling " + currentlyControlling);
            }

        }

        void LateUpdate()
        {
            projectorPlane.transform.up = -viewCamera.transform.forward;
            UpdateProjector();
        }

        public CameraType Controlling() { return currentlyControlling; }

        private void UpdateProjector()
        {
            Simulation.CaptureView[] nearestCaptures = _captures.FindNearestCapture(1, viewCamera.transform.position);

            Simulation.CaptureView capture = nearestCaptures[0];
            if (capture.texture is null) { return; }

            capture.texture.wrapMode = TextureWrapMode.Clamp;

            projectorPlane.GetComponent<Renderer>().sharedMaterial.SetMatrix("projectM", capture.viewProjMatrix);
            projectorPlane.GetComponent<Renderer>().sharedMaterial.SetTexture("_ProjTex", capture.texture);
        }

    }

}