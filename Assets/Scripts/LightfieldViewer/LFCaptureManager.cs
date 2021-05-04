// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
//
// Copyright (c) 2019-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Developer Agreement, located
// here: https://auth.magicleap.com/terms/developer
//
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.IO;
using UnityEngine.XR.MagicLeap;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using MagicLeap.Core.StarterKit;
using MagicLeap;

namespace Simulation.Viewer
{

    public class LFCaptureManager : MonoBehaviour
    {

        //ML remote controller
        public double captureAngle; //smallest angle allowed between two captures 
        public GameObject focalPointPrefab;
        public GameObject smallDotPrefab;

        private GameObject controller;
        private bool _isCameraConnected = false;

        private bool _isCapturing = false;

        public bool isCapturing
        {
            get { return _isCapturing; }
        }

        //whether capture mode has started after getting privileges
        private bool _hasStarted = false;

        private Thread _captureThread = null;

        private object _cameraLockObject = new object();

        public List<CaptureView> captureViews;

        private int fileCounter;
        public int sessionCounter;

        private string APP_ROOT_PATH = "/documents/C1/";
        //capture focalPoint
        public GameObject focalPoint
        {
            get { return _focalPoint; }
            set { _focalPoint = value; }
        }

        private GameObject _focalPoint;

        private LinkedList<Vector3> capturePositions;

        private bool captureAngleGood; //true if far away enough from nearest capture, false otherwise

        public bool CaptureAngleGood
        {
            get
            {
                return captureAngleGood;
            }
        }

        public bool ActivelyCapturing
        {
            get
            {
                return !(_captureThread == null || (!_captureThread.IsAlive));
            }
        }


        void Awake()
        {
            fileCounter = 0;
            captureViews = new List<CaptureView>();
            capturePositions = new LinkedList<Vector3>();
            controller = GameObject.Find("Controller");
            captureAngleGood = true;
            CheckAllObjectsSet();
            sessionCounter = 0;
            UpdateSessionNum();
        }

        public void UpdateSessionNum()
        {
            DirectoryInfo dir = new DirectoryInfo(APP_ROOT_PATH);
            sessionCounter = dir.GetDirectories().Length + 1;
        }

        /// <summary>
        /// Helper for checking if editor object is set 
        /// </summary>
        /// <param name="obj">object to be set</param>
        /// <param name="desc">description printed upon failing</param>
        void _CheckObjSet(UnityEngine.Object obj, string desc)
        {
            if (obj == null)
            {
                Debug.LogError("Error: " + desc + " is not set, disabling script.");
                enabled = false;
                return;
            }
        }

        /// <summary>
        /// Checks if editor objects are set 
        /// </summary>
        void CheckAllObjectsSet()
        {
            _CheckObjSet(controller, "controller");

            _CheckObjSet(smallDotPrefab, "smallDotPrefab");

            _CheckObjSet(focalPointPrefab, "focalPointPrefab");

            if (captureAngle <= 0)
            {
                Debug.LogError("Error: captureAngle is not set, disabling script.");
                enabled = false;
                return;
            }
        }


        /// <summary>
        /// On disabling game object: Stop the camera, unregister callbacks, and stop input and privileges APIs.
        /// </summary>
        void OnDisable()
        {
            lock (_cameraLockObject)
            {
                if (_isCameraConnected)
                {
#if PLATFORM_LUMIN
                    MLCamera.OnRawImageAvailable -= OnCaptureRawImageComplete;
#endif
                    _isCapturing = false;
                    DisableMLCamera();
                }
            }
        }

        /// <summary>
        /// Cannot make the assumption that a reality privilege is still granted after
        /// returning from pause. Return the application to the state where it
        /// requests privileges needed and clear out the list of already granted
        /// privileges. Also, disable the camera and unregister callbacks.
        /// </summary>
        void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                lock (_cameraLockObject)
                {
                    if (_isCameraConnected)
                    {
#if PLATFORM_LUMIN
                        MLCamera.OnRawImageAvailable -= OnCaptureRawImageComplete;
#endif

                        _isCapturing = false;

                        DisableMLCamera();
                    }
                }

                _hasStarted = false;
            }
        }


        public void printStatus()
        {
            Debug.Log(captureViews.Count + " captures taken");
        }

        /// <summary>
        /// Checks if the camera is captureAngle away from all current Captures before triggering capture thread 
        /// </summary>
        public void CheckCaptureConditions()
        {

            Vector3 camPos = Camera.main.transform.position;
            LinkedListNode<Vector3> currCapturePos = capturePositions.First;

            while (currCapturePos != null)
            {
                Vector3 toCam = camPos - focalPoint.transform.position;
                Vector3 toCap = currCapturePos.Value - focalPoint.transform.position;
                double theta = Math.Acos(Vector3.Dot(toCam, toCap) / (toCam.magnitude * toCap.magnitude)) / Math.PI * 180;
                if (theta < captureAngle)
                {
                    //found closest point -> remove and move to front of list, return without taking capture \
                    Debug.Log("Capture found that is too close!");
                    capturePositions.AddFirst(new LinkedListNode<Vector3>(currCapturePos.Value));
                    capturePositions.Remove(currCapturePos);
                    captureAngleGood = false;
                    return;
                }
                currCapturePos = currCapturePos.Next;
            }
            captureAngleGood = true;
            // if all captures are more than captureAngle away, trigger capture
            TriggerAsyncCapture();

        }


        /// <summary>
        /// Starts thread to capture a still image using the device's camera 
        /// </summary>
        public void TriggerAsyncCapture()
        {


            if (!ActivelyCapturing)
            {
                Debug.Log("Triggering async image capture!");
                ThreadStart captureThreadStart = new ThreadStart(CaptureThreadWorker);
                _captureThread = new Thread(captureThreadStart);
                _captureThread.Start();
            }
            else
            {
                Debug.Log("Previous thread has not finished, unable to begin a new capture just yet.");
            }
            return;
        }





        /// <summary>
        /// Connects the MLCamera component and instantiates a new instance
        /// if it was never created.
        /// </summary>
        private void EnableMLCamera()
        {
#if PLATFORM_LUMIN
            lock (_cameraLockObject)
            {
                MLResult result = MLCamera.Start();
                if (result.IsOk)
                {
                    result = MLCamera.Connect();
                    _isCameraConnected = true;
                }
                else
                {
                    Debug.LogErrorFormat("Error: ImageCaptureExample failed starting MLCamera, disabling script. Reason: {0}", result);
                    enabled = false;
                    return;
                }
            }
#endif
        }

        /// <summary>
        /// Disconnects the MLCamera if it was ever created or connected.
        /// </summary>
        private void DisableMLCamera()
        {
#if PLATFORM_LUMIN
            lock (_cameraLockObject)
            {
                if (MLCamera.IsStarted)
                {
                    MLCamera.Disconnect();
                    // Explicitly set to false here as the disconnect was attempted.
                    _isCameraConnected = false;
                    MLCamera.Stop();
                }
            }
#endif
        }

        public void StartCaptureSession()
        {
            StartCapture();

        }

        public void SetFocalPoint(Vector3 position)
        {
            focalPoint = Instantiate(focalPointPrefab, position, Quaternion.identity);
        }

        /// <summary>
        /// Once privileges have been granted, enable the camera and callbacks.
        /// </summary>
        public void StartCapture()
        {
            if (!_hasStarted)
            {
                lock (_cameraLockObject)
                {
                    EnableMLCamera();

#if PLATFORM_LUMIN
                    MLCamera.OnRawImageAvailable += OnCaptureRawImageComplete;
#endif
                }

                Debug.Log("CAMERA SUCCESSFULLY ENABLED");

                _hasStarted = true;
            }
        }

        public void SetFocalPointColor(Color color)
        {
            focalPoint.GetComponent<Renderer>().material.color = color;
        }



        /// <summary>
        /// Handles the event of a new image getting captured.
        /// </summary>
        /// <param name="imageData">The raw data of the image.</param>
        private void OnCaptureRawImageComplete(byte[] imageData)
        {
            lock (_cameraLockObject)
            {
                _isCapturing = false;
            }
            // between uninitalized captures and error texture
            Texture2D texture = new Texture2D(Camera.main.pixelWidth, Camera.main.pixelHeight);
            bool status = texture.LoadImage(imageData);

            if (status)
            {

                Transform cameraTransform = Camera.main.transform;

                int fileCount = fileCounter;
                fileCounter++;

                string imageFileName = "img_" + fileCount + ".png";

                CaptureJSONData jsonData = new CaptureJSONData();
                jsonData.imageFileName = imageFileName;
                jsonData.transform = new TransformJSONData();
                jsonData.transform.forward = cameraTransform.forward;
                jsonData.transform.up = cameraTransform.up;
                jsonData.transform.right = cameraTransform.right;
                jsonData.transform.position = cameraTransform.position;
                Debug.Log("captured position!! " + cameraTransform.position);
                jsonData.transform.projMatrix = Camera.main.projectionMatrix * Camera.main.worldToCameraMatrix;

                // add to list of captures 
                capturePositions.AddFirst(jsonData.transform.position);
                captureViews.Add(new Simulation.CaptureView(imageFileName, texture, Camera.main.projectionMatrix * Camera.main.worldToCameraMatrix, jsonData));

                float SPHERE_RAD = 0.1f;
                Vector3 camDir = Camera.main.transform.position - focalPoint.transform.position;
                camDir.Normalize();
                camDir = camDir * SPHERE_RAD;

                Vector3 pointPos = focalPoint.transform.position + camDir;
                GameObject dot = Instantiate(smallDotPrefab, pointPos, Quaternion.identity);
                dot.transform.SetParent(focalPoint.transform);

            }
        }

        /// <summary>
        /// Worker function to call the API's Capture function
        /// </summary>
        private void CaptureThreadWorker()
        {
#if PLATFORM_LUMIN
            lock (_cameraLockObject)
            {
                if (MLCamera.IsStarted && _isCameraConnected)
                {
                    MLResult result = MLCamera.CaptureRawImageAsync();
                    if (result.IsOk)
                    {
                        _isCapturing = true;
                    }
                }
            }
#endif
        }

        public void StopCurrentSession()
        {
            _isCapturing = false;
            focalPoint.SetActive(false);
            Destroy(focalPoint);
            MLCamera.OnRawImageAvailable -= OnCaptureRawImageComplete;
        }


        /*  Saves a capture session within Assets/SimulatorCaptureSessions/sessionName  */
        public void SaveSession()
        {
            Debug.Log("START SAVING SESSION");
            LightFieldJsonData fieldData = new LightFieldJsonData();
            fieldData.sessionName = "session" + sessionCounter;
            sessionCounter++;
            fieldData.focalPoint = focalPoint.transform.position;
            fieldData.sphereRadius = 10; // THIS VALUE SHOULD NOT MATTER


            // string outputPath = Application.dataPath + "/LightFieldOutput";

            string fieldPath = APP_ROOT_PATH + "/" + fieldData.sessionName;
            string imagePath = fieldPath + "/" + "CaptureImages";

            System.IO.Directory.CreateDirectory(fieldPath);
            System.IO.Directory.CreateDirectory(imagePath);

            // AssetDatabase.CreateFolder(APP_ROOT_PATH, sessionName);  NOTE: DISABLED FOR BUILD
            // AssetDatabase.CreateFolder(fieldPath, "CaptureImages");
            // AssetDatabase.Refresh();

            CaptureJSONData[] captures = new CaptureJSONData[captureViews.Count];

            //save all images into CaptureImages folder and collect capture json data objects
            for (int x = 0; x < captureViews.Count; x++)
            {
                captures[x] = captureViews[x].jsonData;
                Debug.Log("saving capture " + captures[x].transform.position);
                SaveToFile(captureViews[x].texture, imagePath, captures[x].imageFileName);

            }

            fieldData.captures = captures;

            string json = JsonUtility.ToJson(fieldData);

            //save light field JSON 
            File.WriteAllText(APP_ROOT_PATH + "/" + fieldData.sessionName + "/capture.json", json);

            Debug.Log("Saving session " + fieldData.sessionName + " complete!");
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

    }

}