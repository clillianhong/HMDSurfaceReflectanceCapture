using System;
using System.Linq;
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

        //controller for the main camera
        public CameraController cameraController;

        //controller of the capture view collection
        public CaptureViewController captureViewController;

        //controller of the user interface and ML device display
        private UserInterfaceController userInterfaceController;


        /////// CAMERA UI CODE ////////////////////
        //TODO: move the status text and all UI elements into the user interface controller 

        [SerializeField, Space, Tooltip("MLControllerConnectionHandlerBehavior reference.")]
        private MLControllerConnectionHandlerBehavior _controllerConnectionHandler = null;

        [SerializeField, Tooltip("The text used to display status information for the example.")]
        private Text _statusText = null;

        private bool _privilegesBeingRequested = false;

        /////// CAMERA UI CODE END ////////////////


        void Awake()
        {

            cameraController = GameObject.Find("CameraController").GetComponent<CameraController>();
            captureViewController = GameObject.Find("CaptureViewController").GetComponent<CaptureViewController>();
            captureViewController.OnCaptureCreated += OnCaptureCreated;
            userInterfaceController = GameObject.Find("UserInterfaceController").GetComponent<UserInterfaceController>();
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
                _statusText.text = "New capture!";
                captureViewController.collection.GenerateKDTree();
            }
            else if (_controllerConnectionHandler.IsControllerValid(controllerId) && MLInput.Controller.Button.HomeTap == button)
            {

                var nearestCapture = captureViewController.NearestNeighbor(Camera.main.transform.position);
                _statusText.text = "Nearest neighbor is capture " + nearestCapture.captureID;
                userInterfaceController.UpdateClosestPreview(nearestCapture);
            }
        }

        /// <summary>
        /// Callback after a new capture is registered in the collection - renders it as a small thumbnail 
        /// </summary>
        private void OnCaptureCreated()
        {
            //generate UI capture thumbnail 
            Capture newCapture = captureViewController.collection.captures?.ElementAt(captureViewController.collection.captures.Count - 1);
            GameObject previewObj = userInterfaceController.CreateCapturePreviewObject(newCapture);
            if (previewObj != null)
            {
                Debug.Log("preview object made at location " + previewObj.transform.position);
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
            _statusText.text = "THIS IS THE STATUS";
            // _statusText.text = string.Format("<color=#dbfb76><b>{0}</b></color>\n{1}: {2}\n",
            //     LocalizeManager.GetString("ControllerData"),
            //     LocalizeManager.GetString("Status"),
            //     LocalizeManager.GetString(ControllerStatus.Text));
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
            UnregisterCallbacks();
#endif
        }

        void OnApplicationPause(bool pause)
        {
            if (pause)
            {

#if PLATFORM_LUMIN
                UnregisterCallbacks();
#endif
            }
        }


        /// <summary>
        /// Unregisters all callback functions
        /// </summary>
        void UnregisterCallbacks()
        {
            captureViewController.OnCaptureCreated -= OnCaptureCreated;
            MLInput.OnControllerButtonDown -= OnButtonDown;
        }


        /// <summary>
        /// Checks if each object that needs to be set in the editor/OnAwake has been set.<!-- -->
        /// </summary>
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


        /// <summary>
        /// Attempts to acquire all necessary permissions from the user.<!-- -->
        /// </summary>
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
            RequestPermission(MLPrivileges.Id.CameraCapture, "Error: CaptureSystemController failed requesting privileges for camera capture, disabling script. Reason: {0}", DefaultPermissionAction);
            RequestPermission(MLPrivileges.Id.ControllerPose, "Error: CaptureSystemController failed requesting privileges for controller pose, disabling script. Reason: {0}", HandlePrivilegesDone);
            Debug.Log("Succeeded in requesting all privileges");

            _privilegesBeingRequested = true;

        }


        /// <summary>
        /// Abstraction for requesting a privilege using the MLPrivilegesStarterKit 
        /// </summary>
        void RequestPermission(MLPrivileges.Id id, string errorMessage, Action<MLResult> action)
        {
            MLResult result = MLPrivilegesStarterKit.RequestPrivilegesAsync(action, id);
#if PLATFORM_LUMIN
            if (!result.IsOk)
            {
                Debug.LogErrorFormat(errorMessage, result);
                MLPrivilegesStarterKit.Stop();
                enabled = false;
                return;
            }
#endif
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
                Debug.LogErrorFormat("Error: CaptureSystem failed to get requested privileges, disabling script. Reason: {0}", result);
                enabled = false;
                return;
            }
#endif

            cameraController.StartCapture(); //TODO: call camera start capture to enable the camera and callbacks
#if PLATFORM_LUMIN
            MLInput.OnControllerButtonDown += OnButtonDown;
#endif
        }

        /// <summary>
        /// Responds to privilege requester result, default action does nothing
        /// </summary>
        /// <param name="result"/>
        private void DefaultPermissionAction(MLResult result)
        {
            //do nothing
        }

    }

}
