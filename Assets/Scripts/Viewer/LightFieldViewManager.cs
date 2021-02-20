using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;
using Simulation.Utils;

namespace Simulation.Viewer
{

    public enum LFManagerState
    {
        SETUP_FOCAL,
        SETUP_RADIUS,

        ACTIVE,
        STOPPED
    }
    public class LightFieldViewManager : MonoBehaviour
    {

        public GameObject focalPointPrefab;

        public string lightFieldName;
        public GameObject projectorPlanePrefab;


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
                        focalPoint = Instantiate(focalPointPrefab, focalPos, _controller.Orientation);
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
                        string jsonPath = Loader.PathFromSessionName(lightFieldName) + "/capture.json";
                        _lightField = new LightField(JsonUtility.FromJson<LightFieldJsonData>(Simulation.Utils.Loader.LoadJsonText(jsonPath)), radius, focalPoint.transform.position);

                        transitionState(LFManagerState.ACTIVE);

                        break;

                }

            }
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
    }

}