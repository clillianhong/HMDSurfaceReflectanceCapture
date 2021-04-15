using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;
using Simulation.Utils;

namespace Simulation.Viewer
{


    public class LightFieldViewManager : MonoBehaviour
    {


        public GameObject focalPointPrefab;

        public string lightFieldName;

        public GameObject projectorPlanePrefab;

        public GameObject imagePreviewPrefab;

        public bool preloadSession = false;

        public GameObject focalPoint
        {
            get { return _focalPoint; }
            set { _focalPoint = value; }
        }

        #region Private Variables

        private GameObject _focalPoint;

        private GameObject _projectorPlane;

        private LightField _lightField;

        private float radius;

        private MLInput.Controller _controller;
        private LFManagerState _currentState;

        private GameObject[] captureViewObjs;

        private Camera _camera;
        #endregion

        // Start is called before the first frame update
        void Start()
        {

            //start MLInput API 
            MLInput.Start();
            //assign callback
            MLInput.OnControllerButtonDown += OnButtonDown;
            MLInput.OnControllerButtonUp += OnButtonUp;
            _controller = MLInput.GetController(MLInput.Hand.Left);
            _currentState = LFManagerState.SETUP_FOCAL;
            Debug.Log("starting");

            _camera = Camera.main;
            InvokeRepeating("UpdateProjectorPlane", 2.0f, 0.3f);

        }

        void OnButtonDown(byte controllerId, MLInput.Controller.Button button)
        {
            if (button == MLInput.Controller.Button.Bumper)
            {
                switch (_currentState)
                {
                    case LFManagerState.SETUP_FOCAL:
                        Debug.Log("Creating focal point and plane");
                        //create focal point and plane 
                        Vector3 focalPos = _controller.Position;
                        focalPoint = Instantiate(focalPointPrefab, focalPos, Quaternion.identity);
                        _projectorPlane = Instantiate(projectorPlanePrefab, focalPos, _controller.Orientation);
                        transitionState(LFManagerState.SETUP_RADIUS);
                        break;
                    case LFManagerState.SETUP_RADIUS:
                        Debug.Log("establishing radius");

                        //create indicator for radius 
                        var line = new GameObject("Line").AddComponent<LineRenderer>();
                        line.startColor = Color.black;
                        line.endColor = Color.black;
                        line.startWidth = 0.005f;
                        line.endWidth = 0.005f;
                        line.positionCount = 2;
                        line.useWorldSpace = true;

                        Vector3 radiusEnd = _controller.Position;
                        radius = Vector3.Distance(radiusEnd, focalPoint.transform.position);
                        line.SetPosition(0, focalPoint.transform.position);
                        line.SetPosition(1, radiusEnd);

                        //load light field with transformations
                        if (preloadSession)
                        {
                            string jsonPath = Loader.PathFromSessionName(lightFieldName) + "/capture.json";
                            _lightField = new LightField(JsonUtility.FromJson<LightFieldJsonData>(Simulation.Utils.Loader.LoadJsonText(jsonPath)), radius, focalPoint.transform.position);
                            captureViewObjs = new GameObject[_lightField.captures.Length];
                            for (int i = 0; i < _lightField.captures.Length; i++)
                            {
                                MLCaptureView captureView = _lightField.captures[i];
                                captureViewObjs[i] = createCapturePreviewObject(captureView);

                            }
                        }

                        transitionState(LFManagerState.ACTIVE);

                        break;

                }

            }
        }

        GameObject createCapturePreviewObject(MLCaptureView captureView)
        {
            GameObject obj = GameObject.Instantiate(imagePreviewPrefab, captureView.realityCapturePosition, Quaternion.identity);
            Vector3 innerVec = (focalPoint.transform.position - obj.transform.position).normalized;
            obj.transform.up = -innerVec;
            obj.GetComponent<MeshRenderer>().material.mainTexture = captureView.texture;

            return obj;
        }

        void OnButtonUp(byte controllerId, MLInput.Controller.Button button)
        {
            if (button == MLInput.Controller.Button.Bumper)
            {

            }
            if (button == MLInput.Controller.Button.HomeTap)
            {

            }
        }


        // Update is called once per frame
        void Update()
        {

        }

        void LateUpdate()
        {

        }

        void UpdateProjectorPlane()
        {
            if (_currentState != LFManagerState.SETUP_FOCAL)
            {
                _projectorPlane.transform.up = -_camera.transform.forward;
            }
            if (this.captureViewObjs != null)
            {
                MLCaptureView capture = this.FindNearestCapture(1, _camera.transform.position);
                capture.texture.wrapMode = TextureWrapMode.Clamp;

                _projectorPlane.GetComponent<Renderer>().sharedMaterial.SetMatrix("projectM", _camera.projectionMatrix * _camera.worldToCameraMatrix);
                _projectorPlane.GetComponent<Renderer>().sharedMaterial.SetTexture("_ProjTex", capture.texture);
            }

        }

        public MLCaptureView FindNearestCapture(int k, Vector3 position)
        {

            MLCaptureView leastView = new MLCaptureView();
            float minDist = float.PositiveInfinity;

            foreach (MLCaptureView view in _lightField.captures)
            {
                float curDist = Vector3.Distance(position, view.realityCapturePosition);

                if (minDist > curDist)
                {
                    minDist = curDist;
                    leastView = view;
                }
            }

            return leastView;
        }

        void OnDestroy()
        {
            MLInput.OnControllerButtonDown -= OnButtonDown;
            MLInput.OnControllerButtonUp -= OnButtonUp;
            MLInput.Stop();
        }



        void transitionState(LFManagerState newState)
        {
            LFManagerState oldState = _currentState;
            _currentState = newState;
        }

        public void StopCurrentSession()
        {
            MLInput.OnControllerButtonDown -= OnButtonDown;
            MLInput.OnControllerButtonUp -= OnButtonUp;
        }
    }

}