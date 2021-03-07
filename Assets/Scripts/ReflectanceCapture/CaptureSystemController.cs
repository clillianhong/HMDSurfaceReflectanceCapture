using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.XR.MagicLeap;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using MagicLeap.Core.StarterKit;
using MagicLeap;

namespace CaptureSystem
{
    public class CaptureSystemController : MonoBehaviour
    {
        // Start is called before the first frame update

        public CameraController cameraController;

        public CaptureViewController captureViewController;

        private UserInterface userInterface;


        /////// CAMERA UI CODE ////////////////////


        [SerializeField, Space, Tooltip("MLControllerConnectionHandlerBehavior reference.")]
        private MLControllerConnectionHandlerBehavior _controllerConnectionHandler = null;

        [SerializeField, Tooltip("The text used to display status information for the example.")]
        private Text _statusText = null;


        private bool _privilegesBeingRequested = false;

        /////// CAMERA UI CODE END ////////////////


        void Awake()
        {

            // cameraController = GameObject.Find("CameraController").GetComponent<CameraController>();
            // captureViewController = GameObject.Find("CaptureViewController").GetComponent<CaptureViewController>();
            CheckObjectsSet();
            CheckPermissions();



        }
        /// <summary>
        /// Handles the event for button down.
        /// </summary>
        /// <param name="controllerId">The id of the controller.</param>
        /// <param name="button">The button that is being pressed.</param>
        private void OnButtonDown(byte controllerId, MLInput.Controller.Button button)
        {
            if (_controllerConnectionHandler.IsControllerValid(controllerId) && MLInput.Controller.Button.Bumper == button && !cameraController.isCapturing)
            {
                cameraController.TriggerAsyncCapture();
                _statusText.text = "Async capture triggered!";
                // Debug.Log("Async capture triggered!");
            }
        }

        void Start()
        {

        }

        /// <summary>
        /// Display privilege error if necessary or update status text.
        /// </summary>
        private void Update()
        {
            // UpdateStatusText();
        }

        /// <summary>
        /// Updates examples status text.
        /// </summary>
        private void UpdateStatusText()
        {
            _statusText.text = string.Format("<color=#dbfb76><b>{0}</b></color>\n{1}: {2}\n",
                LocalizeManager.GetString("ControllerData"),
                LocalizeManager.GetString("Status"),
                LocalizeManager.GetString(ControllerStatus.Text));
        }

        /// <summary>
        /// Cleans up the component.
        /// </summary>
        void OnDestroy()
        {
            if (_privilegesBeingRequested)
            {
                _privilegesBeingRequested = false;

                MLPrivilegesStarterKit.Stop();
            }
        }

        /// <summary>
        /// unregister callbacks, and stop input and privileges APIs.
        /// </summary>
        void OnDisable()
        {
#if PLATFORM_LUMIN
            MLInput.OnControllerButtonDown -= OnButtonDown;
#endif
        }

        void OnApplicationPause(bool pause)
        {
            if (pause)
            {

#if PLATFORM_LUMIN
                MLInput.OnControllerButtonDown -= OnButtonDown;
#endif
            }
        }



        void CheckObjectsSet()
        {
            if (_controllerConnectionHandler == null)
            {
                Debug.LogError("Error: CaptureSystemController._controllerConnectionHandler is not set, disabling script.");
                enabled = false;
                return;
            }

            if (_statusText == null)
            {
                Debug.LogError("Error: CaptureSystemController._statusText is not set, disabling script.");
                enabled = false;
                return;
            }

            if (cameraController == null)
            {
                Debug.LogError("Error: CaptureSystemController.cameraController is not set, disabling script.");
                enabled = false;
                return;
            }

            if (captureViewController == null)
            {
                Debug.LogError("Error: CaptureSystemController.captureViewController is not set, disabling script.");
                enabled = false;
                return;
            }



        }

        void CheckPermissions()
        {
            // Before enabling the Camera, the scene must wait until the privilege has been granted.
            MLResult result = MLPrivilegesStarterKit.Start();
#if PLATFORM_LUMIN
            if (!result.IsOk)
            {
                Debug.LogErrorFormat("Error: CaptureSystemController failed starting MLPrivilegesStarterKit, disabling script. Reason: {0}", result);
                enabled = false;
                return;
            }
#endif

            result = MLPrivilegesStarterKit.RequestPrivilegesAsync(HandlePrivilegesDone, MLPrivileges.Id.CameraCapture);
#if PLATFORM_LUMIN
            if (!result.IsOk)
            {
                Debug.LogErrorFormat("Error: CaptureSystemController failed requesting privileges, disabling script. Reason: {0}", result);
                MLPrivilegesStarterKit.Stop();
                enabled = false;
                return;
            }

#endif

            _privilegesBeingRequested = true;

        }

        /// <summary>
        /// Responds to privilege requester result.
        /// </summary>
        /// <param name="result"/>
        private void HandlePrivilegesDone(MLResult result)
        {
            _privilegesBeingRequested = false;
            MLPrivilegesStarterKit.Stop();

#if PLATFORM_LUMIN
            if (result != MLResult.Code.PrivilegeGranted)
            {
                Debug.LogErrorFormat("Error: ImageCaptureExample failed to get requested privileges, disabling script. Reason: {0}", result);
                enabled = false;
                return;
            }
#endif

            Debug.Log("Succeeded in requesting all privileges");
            cameraController.StartCapture(); //TODO: call camera start capture to enable the camera and callbacks
#if PLATFORM_LUMIN
            MLInput.OnControllerButtonDown += OnButtonDown;
#endif
        }

    }

}
