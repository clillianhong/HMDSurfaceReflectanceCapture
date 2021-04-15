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
        public GameObject smallDotPrefab;
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

            Debug.Log("headlocked canvas " + HeadlockedCanvas.active);

            CheckAllObjectsSet();

            viewManagerObj.SetActive(false);
            sessionNameText.text = "session1";

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
            _CheckObjSet(smallDotPrefab, "smallDotPrefab");
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
                    Debug.Log("STOP CAPTURING");
                    captureManager.SaveSession(sessionNameText.text);
                    captureManager.StopCurrentSession();

                    RestoreMenu();
                    CancelInvoke("CaptureImage");
                    currentState = UIState.MENU;
                }
                else if (currentState == UIState.VIEWING)
                {
                    //TODO stop current view session with session name
                    // viewManager.StopCurrentSession();
                    currentState = UIState.MENU;
                }
                else if (currentState == UIState.MENU)
                {
                    //START VIEWING 
                    currentState = UIState.VIEWING;
                    MLInput.Stop();
                    MLInput.OnControllerButtonDown -= OnButtonDown;
                    viewManagerObj.SetActive(true);
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
                            Debug.Log("DEACTIVATED HEADLOCKED CANVAS");
                            HeadlockedCanvas.SetActive(false);
                            currentState = UIState.CAPTURE_SETUP;

                        }
                    }

                }
                else if (currentState == UIState.CAPTURE_SETUP) // create focal point! 
                {
                    Vector3 focalPos = controller.Position;
                    focalPoint = Instantiate(focalPointPrefab, focalPos, controller.Orientation);
                    captureManager.focalPointPos = focalPoint.transform.position;
                    currentState = UIState.CAPTURING;

                    InvokeRepeating("CaptureImage", 2.0f, 2.0f);
                }
                // else if (currentState == UIState.CAPTURING)
                // {

                // }
                // else if (currentState == UIState.VIEWING)
                // {

                // }
            }

            if (currentState == UIState.CAPTURING)
            {


            }


        }

        void RestoreMenu()
        {
            HeadlockedCanvas.SetActive(true);
        }


        void StartCaptureSession()
        {
            captureManager.StartCaptureSession(sessionNameText.text);
        }

        void CaptureImage()
        {
            //draw dot on unit sphere 
            float SPHERE_RAD = 0.1f;
            Vector3 camDir = Camera.main.transform.position - focalPoint.transform.position;
            camDir.Normalize();
            camDir = camDir * SPHERE_RAD;

            Vector3 pointPos = focalPoint.transform.position + camDir;
            Instantiate(smallDotPrefab, pointPos, Quaternion.identity);

            //take an image and add to collection 
            captureManager.TriggerAsyncCapture();

            // captureManager.printStatus();
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

            StartCaptureSession();
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