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

        /// <summary>
        /// The example is using threads on the call to MLCamera.CaptureRawImageAsync to alleviate the blocking
        /// call at the beginning of CaptureRawImageAsync, and the safest way to prevent race conditions here is to
        /// lock our access into the MLCamera class, so that we don't accidentally shut down the camera
        /// while the thread is attempting to work
        /// </summary>
        private object _cameraLockObject = new object();

        private string sessionName;

        public Vector3 focalPointPos;

        public List<CaptureView> captureViews;

        private int fileCounter;



        void Awake()
        {
            fileCounter = 0;
            captureViews = new List<CaptureView>();
            controller = GameObject.Find("Controller");
            CheckAllObjectsSet();
        }


        void _CheckObjSet(UnityEngine.Object obj, string desc)
        {
            if (obj == null)
            {
                Debug.LogError("Error: " + desc + " is not set, disabling script.");
                enabled = false;
                return;
            }
        }

        void CheckAllObjectsSet()
        {
            _CheckObjSet(controller, "controller");
        }


        /// <summary>
        /// Stop the camera, unregister callbacks, and stop input and privileges APIs.
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
        /// Captures a still image using the device's camera and returns
        /// the data path where it is saved.
        /// </summary>
        public void TriggerAsyncCapture()
        {
            if (_captureThread == null || (!_captureThread.IsAlive))
            {
                ThreadStart captureThreadStart = new ThreadStart(CaptureThreadWorker);
                _captureThread = new Thread(captureThreadStart);
                _captureThread.Start();
            }
            else
            {
                Debug.Log("Previous thread has not finished, unable to begin a new capture just yet.");
            }
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

        public void StartCaptureSession(string sessionName)
        {
            StartCapture();
            this.sessionName = sessionName;

        }

        public void SetFocalPoint(Vector3 position)
        {

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

                // add to list of captures 
                captureViews.Add(new Simulation.CaptureView("img_" + fileCount + ".png", texture, Camera.main.projectionMatrix * Camera.main.worldToCameraMatrix, transform, cameraTransform.position));
                //TODO: DO SOMETHING WITH TEXTURE   

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
            MLCamera.OnRawImageAvailable -= OnCaptureRawImageComplete;
        }


        /*  Saves a capture session within Assets/SimulatorCaptureSessions/sessionName  */
        public void SaveSession(string sessionName)
        {
            Debug.Log("START SAVING SESSION");
            LightFieldJsonData fieldData = new LightFieldJsonData();
            fieldData.sessionName = sessionName;
            fieldData.focalPoint = focalPointPos;
            fieldData.sphereRadius = 10; // THIS VALUE SHOULD NOT MATTER


            // string outputPath = Application.dataPath + "/LightFieldOutput";
            string outputPath = "/documents/C1/";
            string fieldPath = outputPath + "/" + sessionName;
            string imagePath = fieldPath + "/" + "CaptureImages";

            System.IO.Directory.CreateDirectory(fieldPath);
            System.IO.Directory.CreateDirectory(imagePath);

            // AssetDatabase.CreateFolder(outputPath, sessionName);  NOTE: DISABLED FOR BUILD
            // AssetDatabase.CreateFolder(fieldPath, "CaptureImages");
            // AssetDatabase.Refresh();

            CaptureJSONData[] captures = new CaptureJSONData[captureViews.Count];

            //save all images into CaptureImages folder and collect capture json data objects
            for (int x = 0; x < captureViews.Count; x++)
            {
                captures[x] = captureViews[x].jsonData;
                SaveToFile(captureViews[x].texture, imagePath, captures[x].imageFileName);

            }

            fieldData.captures = captures;

            string json = JsonUtility.ToJson(fieldData);

            //save light field JSON 
            File.WriteAllText(outputPath + "/" + sessionName + "/capture.json", json);

            Debug.Log("Saving session " + sessionName + " complete!");
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