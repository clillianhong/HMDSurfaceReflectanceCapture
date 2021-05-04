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
        public GameObject projCameraPrefab;
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

        private GameObject radiusLine;

        private LightField _lightField;

        private float radius;

        private MLInput.Controller _controller;
        private LFManagerState _currentState;

        private GameObject[] captureViewObjs;

        private int multiMode; //render KNN captures instead of nearest 

        private Camera _camera;
        #endregion

        // Start is called before the first frame update
        void Start()
        {
            _camera = Camera.main;
            _controller = MLInput.GetController(MLInput.Hand.Left);
            _currentState = LFManagerState.SETUP_FOCAL;
        }

        /// <summary>
        /// Creates focal point and projector plane at the current position of the controller 
        /// </summary>
        public void SetupFocal()
        {
            //create focal point and plane
            Debug.Log("Creating focal point and plane");
            Vector3 focalPos = _controller.Position;
            focalPoint = Instantiate(focalPointPrefab, focalPos, Quaternion.identity);
            _projectorPlane = Instantiate(projectorPlanePrefab, focalPos, _controller.Orientation);
            Debug.Log("Updating project plane invoking");
            InvokeRepeating("UpdateProjectorPlane", 2.0f, 0.3f);
        }

        /// <summary>
        /// Move focal point and focal plane to [position]
        /// </summary>
        /// <param name="position">location to move to</param>
        public void SnapFocalTo(Vector3 position)
        {
            focalPoint.transform.position = position;
            _projectorPlane.transform.position = position;
        }

        /// <summary>
        /// Loads the light field session and renders initial radius line 
        /// </summary>
        public void LoadSession()
        {
            Debug.Log("establishing radius");

            //create indicator for radius 
            radiusLine = new GameObject("Line");
            var lineRend = radiusLine.AddComponent<LineRenderer>();
            lineRend.startColor = Color.black;
            lineRend.endColor = Color.black;
            lineRend.startWidth = 0.005f;
            lineRend.endWidth = 0.005f;
            lineRend.positionCount = 2;
            lineRend.useWorldSpace = true;

            Vector3 radiusEnd = _controller.Position;
            radius = Vector3.Distance(radiusEnd, focalPoint.transform.position);
            lineRend.SetPosition(0, focalPoint.transform.position);
            lineRend.SetPosition(1, radiusEnd);

            //load light field with transformations
            if (preloadSession)
            {
                string jsonPath = Loader.PathFromSessionName(lightFieldName) + "/capture.json";
                _lightField = new LightField(JsonUtility.FromJson<LightFieldJsonData>(Simulation.Utils.Loader.LoadJsonText(jsonPath)), radius, focalPoint.transform.position, projCameraPrefab);
                captureViewObjs = new GameObject[_lightField.captures.Length];
                for (int i = 0; i < _lightField.captures.Length; i++)
                {
                    MLCaptureView captureView = _lightField.captures[i];
                    captureViewObjs[i] = createCapturePreviewObject(captureView);
                }
            }

        }

        /// <summary>
        /// Toggles multiMode uniform param from 0 to 1 
        /// </summary>
        public void ToggleMultimode()
        {
            multiMode = 1 - multiMode;
        }
        /// <summary>
        /// Deactivate current session of light field viewer: destroys all objects and sets current loaded light field to null 
        /// </summary>
        public void DeactivateSession()
        {
            Debug.Log("Destroying light viewing session.");
            Destroy(focalPoint);
            Destroy(_projectorPlane);
            Destroy(radiusLine);
            foreach (var cap in captureViewObjs)
            {
                Destroy(cap);
            }
            _lightField = null; //TODO : CHECK MIGHT CAUSE ERROR
            CancelInvoke("UpdateProjectorPlane");
        }
        /// <summary>
        /// Creates a capture preview object 
        /// </summary>
        /// <param name="captureView">The MLCaptureView to be rendered</param>
        /// <returns>Created game object</returns>
        GameObject createCapturePreviewObject(MLCaptureView captureView)
        {
            GameObject obj = GameObject.Instantiate(imagePreviewPrefab, captureView.realityCapturePosition, Quaternion.identity);
            Vector3 innerVec = (focalPoint.transform.position - obj.transform.position).normalized;
            obj.transform.up = -innerVec;
            obj.GetComponent<MeshRenderer>().material.mainTexture = captureView.texture;

            return obj;
        }


        /// <summary>
        /// Update projector plane by finding relevant nearest views and setting plane shader uniforms 
        /// NOTE: The shader assumes 3 nearest neighbors
        /// </summary>
        void UpdateProjectorPlane()
        {

            _projectorPlane.transform.up = -_camera.transform.forward;

            if (this.captureViewObjs != null)
            {
                int K = 3;
                MLCaptureView[] captures = this.FindNearestCapture(K, _camera.transform.position);

                for (int i = 0; i < K; i++)
                {
                    MLCaptureView capture = captures[i];
                    capture.texture.wrapMode = TextureWrapMode.Clamp;

                    _projectorPlane.GetComponent<Renderer>().sharedMaterial.SetTexture("_ProjTex" + (i + 1), capture.texture);
                    _projectorPlane.GetComponent<Renderer>().sharedMaterial.SetMatrix("projectM" + (i + 1), _camera.projectionMatrix * capture.worldToCameraMatrix);
                }

                _projectorPlane.GetComponent<Renderer>().sharedMaterial.SetMatrix("projectM", _camera.projectionMatrix * _camera.worldToCameraMatrix);
                _projectorPlane.GetComponent<Renderer>().sharedMaterial.SetInt("multiMode", multiMode);

            }

        }
        /// <summary>
        /// Find the nearest [K] captures to [position]
        /// </summary>
        /// <param name="K">Number of nearest neighbors</param>
        /// <param name="position">The position to calculate distance from</param>
        /// <returns>Array of capture view objects sorted closest to farthest</returns>
        public MLCaptureView[] FindNearestCapture(int k, Vector3 position)
        {

            if (_lightField.captures.Length < k)
            {
                k = _lightField.captures.Length;
            }

            var closestViews = new SortedSet<MLCaptureView>(new ByEuclidean(position));

            foreach (MLCaptureView view in _lightField.captures)
            {
                closestViews.Add(view);
            }

            MLCaptureView[] leastViews = new MLCaptureView[k];

            for (int i = 0; i < k; i++)
            {
                leastViews[i] = closestViews.Max;
                closestViews.Remove(leastViews[i]);
            }

            return leastViews;
        }

    }
    /// <summary>
    /// Comparator class used in nearest neighbors implementation 
    /// TODO: change to angular distance
    /// </summary>
    public class ByEuclidean : IComparer<MLCaptureView>
    {
        Vector3 cameraPosition;

        CaseInsensitiveComparer caseiComp = new CaseInsensitiveComparer();

        public ByEuclidean(Vector3 pos)
        {
            cameraPosition = pos;
        }

        public int Compare(MLCaptureView a, MLCaptureView b)
        {
            float distToA = Vector3.Distance(cameraPosition, a.realityCapturePosition);
            float distToB = Vector3.Distance(cameraPosition, b.realityCapturePosition);

            if (distToA < distToB)
            {
                return 1;
            }
            else
            {
                if (distToA > distToB)
                {
                    return -1;
                }
                else
                {
                    return 0;
                }
            }
        }
    }
}
