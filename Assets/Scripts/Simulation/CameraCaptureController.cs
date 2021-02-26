using UnityEngine;
using System.IO;
using UnityEditor;
using System.Linq;

/***
Responsible for the capture, saving, and display of images taken by the capture camera.
*/

namespace Simulation
{
    public class CameraCaptureController : MonoBehaviour
    {
        public int fileCounter;
        public string sessionName;
        public KeyCode screenshotKey;

        public int captureWidth;
        public int captureHeight;

        CaptureViewCollection collectionManager;
        private Camera _camera;

        //autocapture
        public bool autoCapture;
        public KeyCode triggerOrbitCaptureKey;
        public int divisions;
        public string lightFieldName;
        bool currentlyCapturing;
        OrbitViewManager viewManager;

        private void Start()
        {
            collectionManager = GameObject.Find("OrbitViewManager").GetComponent<CaptureViewCollection>();
            viewManager = GameObject.Find("OrbitViewManager").GetComponent<OrbitViewManager>();
            _camera = gameObject.GetComponent<Camera>();
            currentlyCapturing = false;
        }
        private void LateUpdate()
        {
            if (autoCapture)
            {
                if (Input.GetKeyDown(screenshotKey))
                {
                    Debug.Log("Autocapture on, not capturing image.");
                }
                if (!currentlyCapturing && Input.GetKeyDown(triggerOrbitCaptureKey))
                {
                    currentlyCapturing = true;
                    Debug.Log("Starting orbit capture");
                    collectionManager.captureViews.Clear();
                    StartOrbitingCapture();
                    SaveSession(sessionName);
                }
            }
            else
            {
                if (Input.GetKeyDown(screenshotKey))
                {
                    CaptureAndDisplay(true);

                }
            }

        }


        public void CaptureAndDisplay(bool renderCapture)
        {
            //get current camera image 

            RenderTexture activeRenderTexture = RenderTexture.active;
            RenderTexture.active = _camera.targetTexture;

            _camera.Render();

            Texture2D image = new Texture2D(_camera.targetTexture.width, _camera.targetTexture.height);
            image.ReadPixels(new Rect(0, 0, _camera.targetTexture.width, _camera.targetTexture.height), 0, 0);
            image.Apply();
            RenderTexture.active = activeRenderTexture;

            //get current camera pose 

            Transform cameraTransform = _camera.gameObject.transform;

            int fileCount = fileCounter;
            fileCounter++;

            // add to list of captures 
            collectionManager.captureViews.Add(new Simulation.CaptureView("img_" + fileCount + ".png", image, _camera.projectionMatrix * _camera.worldToCameraMatrix, transform, cameraTransform.position));

            if (renderCapture)
            {

                createCapturePoseLabel(cameraTransform, image, fileCount);
            }
        }

        public void createCapturePoseLabel(Transform trans, Texture2D tex, int fileCount)
        {
            CaptureCreator.CreateCaptureGameObject(trans, tex, captureWidth, captureHeight, "capture-" + fileCount);
        }



        /// <summary>
        /// Save texture as a PNG within assets/outputFolder/
        /// </summary>
        /// 
        public void SaveToFile(Texture2D image, string outputFolder, string imageName)
        {
            byte[] bytes = image.EncodeToJPG();
            File.WriteAllBytes(outputFolder + "/" + imageName, bytes);
        }


        void StartOrbitingCapture()
        {
            currentlyCapturing = true;
            for (int theta = 0; theta < 360; theta += divisions)
            {
                Vector3 worldX = transform.TransformDirection(Vector3.right);
                _camera.gameObject.transform.RotateAround(viewManager.focalPoint.position, worldX, 360 / divisions);
                _RotateHorizontal360();
            }


        }

        void _RotateHorizontal360()
        {

            for (int theta = 0; theta < 360; theta += divisions)
            {
                _camera.gameObject.transform.RotateAround(viewManager.focalPoint.position, Vector3.up, 360 / divisions);
                CaptureAndDisplay(false);
            }
        }

        /*  Saves a capture session within Assets/SimulatorCaptureSessions/sessionName  */
        public void SaveSession(string sessionName)
        {
            LightFieldJsonData fieldData = new LightFieldJsonData();
            fieldData.sessionName = sessionName;
            fieldData.focalPoint = viewManager.focalPoint.position;
            fieldData.sphereRadius = viewManager.distance;


            // string outputPath = Application.dataPath + "/LightFieldOutput";
            string outputPath = "Assets/LightFieldOutput";
            string fieldPath = outputPath + "/" + sessionName;
            string imagePath = fieldPath + "/" + "CaptureImages";

            AssetDatabase.CreateFolder(outputPath, sessionName);
            AssetDatabase.CreateFolder(fieldPath, "CaptureImages");
            AssetDatabase.Refresh();

            CaptureJSONData[] captures = new CaptureJSONData[collectionManager.captureViews.Count];

            //save all images into CaptureImages folder and collect capture json data objects
            for (int x = 0; x < collectionManager.captureViews.Count; x++)
            {
                captures[x] = collectionManager.captureViews[x].jsonData;
                SaveToFile(collectionManager.captureViews[x].texture, imagePath, captures[x].imageFileName);

            }

            fieldData.captures = captures;

            string json = JsonUtility.ToJson(fieldData);

            //save light field JSON 
            File.WriteAllText(outputPath + "/" + sessionName + "/capture.json", json);

            Debug.Log("Saving session " + sessionName + " complete!");
            currentlyCapturing = false;

        }


    }
}
