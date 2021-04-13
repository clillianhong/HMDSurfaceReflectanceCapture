using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;
using System.Runtime.InteropServices;

using UnityEngine.UIElements;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using MagicLeap.Core.StarterKit;
using MagicLeap;
using CaptureSystem;


namespace Simulation.Viewer
{

    public class UIManager : MonoBehaviour
    {
        // Start is called before the first frame update

        public GameObject focalPointPrefab;
        private MLInput.Controller controller;
        public GameObject HeadlockedCanvas;
        public GameObject controllerInput;

        public TMP_Text sessionNameText;

        public LFCaptureManager captureManager;
        public LightFieldViewManager viewManager;
        public GameObject viewManagerObj;

        private bool _privilegesBeingRequested = false;
        private MLControllerConnectionHandlerBehavior _controllerConnectionHandler = null;

        public GameObject focalPoint
        {
            get { return _focalPoint; }
            set { _focalPoint = value; }
        }



        private GameObject _focalPoint;
        enum UIState
        {
            MENU,
            CAPTURE_SETUP,
            CAPTURING,
            VIEWING
        }

        private UIState currentState;

        void Start()
        {
            MLInput.Start();
            controller = MLInput.GetController(MLInput.Hand.Left);
            MLInput.OnControllerButtonDown += OnButtonDown;

            currentState = UIState.MENU;

            CheckAllObjectsSet();

            viewManagerObj.SetActive(false);

            CheckPermissions();
        }

        void _CheckObjSet(Object obj, string desc)
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

            _CheckObjSet(focalPointPrefab, "focalPointPrefab");
            _CheckObjSet(HeadlockedCanvas, "headlocked canvas");
            _CheckObjSet(controllerInput, "statusText");
            _CheckObjSet(sessionNameText, "session name text mesh");
            _CheckObjSet(captureManager, "captureManager");
            _CheckObjSet(viewManager, "viewManager");
        }

        private void OnButtonDown(byte controllerId, MLInput.Controller.Button button)
        {
            if (MLInput.Controller.Button.Bumper == button)  // go back to menu 
            {
                // if (currentState == UIState.CAPTURE_SETUP)
                // {


                // }
                if (currentState == UIState.CAPTURING)
                {
                    //TODO stop capture, save session, return to MENU 
                    captureManager.StopCurrentSession();
                    currentState = UIState.MENU;
                }
                else if (currentState == UIState.VIEWING)
                {
                    //TODO stop current view session with session name
                    viewManager.StopCurrentSession();
                    currentState = UIState.MENU;
                }
            }
        }


        // Update is called once per frame
        void Update()
        {
            if (controller.TriggerValue > 0.5f)
            {
                if (currentState == UIState.MENU)
                {

                    RaycastHit hit;
                    if (Physics.Raycast(controllerInput.transform.position, controllerInput.transform.forward, out hit))
                    {
                        if (hit.transform.gameObject.name == "StartButton")
                        {
                            HeadlockedCanvas.SetActive(false);
                            StartCaptureSession();
                        }
                    }

                }
                else if (currentState == UIState.CAPTURE_SETUP) // create focal point! 
                {
                    Vector3 focalPos = controller.Position;
                    focalPoint = Instantiate(focalPointPrefab, focalPos, controller.Orientation);
                    captureManager.StartCapturing(focalPoint.transform.position);
                    currentState = UIState.CAPTURING;
                }
                // else if (currentState == UIState.CAPTURING)
                // {

                // }
                // else if (currentState == UIState.VIEWING)
                // {

                // }
            }


        }


        void StartCaptureSession()
        {
            currentState = UIState.CAPTURE_SETUP;
            captureManager.StartCaptureSession(sessionNameText.text);
        }


        private void OnDestroy()
        {
            MLInput.Stop();
            MLInput.OnControllerButtonDown -= OnButtonDown;
        }


        private void ListSessions()
        {
            // generate a list of buttons to view old sessions loaded from Assets/LightFieldOutput/

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
        void RequestPermission(MLPrivileges.Id id, string errorMessage, System.Action<MLResult> action)
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
            //TODO: call camera start capture to enable the camera and callbacks
#if PLATFORM_LUMIN
            MLInput.OnControllerButtonDown += OnButtonDown;
#endif
        }

        private void DefaultPermissionAction(MLResult result)
        {
            //do nothing
        }

    }

}