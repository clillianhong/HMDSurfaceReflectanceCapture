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

        public GameObject sessionButtonPrefab;
        public GameObject HeadlockedCanvas;
        public GameObject controllerInput;
        private MLInput.Controller controller;
        public LFCaptureManager captureManager;
        public LightFieldViewManager viewManager;
        public GameObject viewManagerObj;

        private bool _privilegesBeingRequested = false;
        private MLControllerConnectionHandlerBehavior _controllerConnectionHandler = null;

        private HashSet<string> sessionNames;
        public List<GameObject> sessionButtons;
        private UIState currentState;
        private int coolDown;
        private bool prevCaptureAngleGood;

        //timers for repeat invoking of image capture system 
        private float CHECK_CAPTURE_ATTEMPT_TIME = 0.2f; //check every 0.2 seconds if not actively capturing 
        private float timer = 0.0f; //the current time

        //the last time we attempted to invoke capturing 
        private float lastCaptureAttemptTime = 0.0f;


        enum UIState
        {
            MENU,
            CAPTURE_SETUP,
            CAPTURING,
            VIEW_FOCAL_SETUP,
            VIEW_RAD_SETUP,
            VIEWING
        }


        void Start()
        {
            MLInput.Start();
            controller = MLInput.GetController(MLInput.Hand.Left);
            MLInput.OnControllerButtonDown += OnButtonDown;
            currentState = UIState.MENU;
            prevCaptureAngleGood = true;

            viewManagerObj.SetActive(false);
            coolDown = 0;
            sessionNames = new HashSet<string>();

            CheckAllObjectsSet();
            CheckPermissions();
            CreateSavedSessionButtons();

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
            _CheckObjSet(HeadlockedCanvas, "headlocked canvas");
            _CheckObjSet(controllerInput, "statusText");
            _CheckObjSet(captureManager, "captureManager");
            _CheckObjSet(viewManager, "viewManager");
            _CheckObjSet(sessionButtonPrefab, "sessionButtonPrefab");
        }

        void CreateSavedSessionButtons()
        {
            captureManager.UpdateSessionNum();
            int numSessions = captureManager.sessionCounter - 1;
            Vector3 currentButtonPos;
            float heightDiff = 0.02f;
            float localScale = 0.001f;
            currentButtonPos = HeadlockedCanvas.transform.position;
            currentButtonPos.y -= 0.01f;

            Debug.Log("Creating saved session " + numSessions);

            for (int x = 1; x <= numSessions; x++)
            {
                string name = "session" + x;
                GameObject button = Instantiate(sessionButtonPrefab, currentButtonPos, HeadlockedCanvas.transform.rotation);
                button.name = name;
                button.transform.localScale = button.transform.localScale * localScale;
                button.GetComponentInChildren<TMP_Text>().text = name;
                button.transform.SetParent(HeadlockedCanvas.transform);
                Debug.Log("button created at " + currentButtonPos);
                currentButtonPos.y -= heightDiff;
                sessionNames.Add(name);
                sessionButtons.Add(button);
            }
        }


        private void OnButtonDown(byte controllerId, MLInput.Controller.Button button)
        {
            if (button == MLInput.Controller.Button.HomeTap)
            {
                if (currentState != UIState.MENU)
                {
                    Debug.Log("Current state " + currentState + " transitioning to Menu");
                    TransitionState(UIState.MENU);
                }

            }
            else if (MLInput.Controller.Button.Bumper == button)  // go back to menu 
            {
                if (currentState == UIState.VIEW_FOCAL_SETUP && coolDown == 0)
                {
                    coolDown = 50;
                    viewManager.SetupFocal();
                    TransitionState(UIState.VIEW_RAD_SETUP);
                }
                else if (currentState == UIState.VIEW_RAD_SETUP && coolDown == 0)
                {
                    TransitionState(UIState.VIEWING);
                }
                else if (currentState == UIState.VIEWING && coolDown == 0)
                {
                    coolDown = 50;
                    Debug.Log("TOGGLING MULTIMODE");
                    viewManager.ToggleMultimode();
                }

            }

        }



        // Update is called once per frame
        void Update()
        {
            if (currentState == UIState.CAPTURING)
            {
                timer += Time.deltaTime;
                if (!captureManager.ActivelyCapturing && timer - lastCaptureAttemptTime > CHECK_CAPTURE_ATTEMPT_TIME)
                {
                    AttemptImageCapture();
                    lastCaptureAttemptTime = timer;
                }
            }

            if (coolDown > 0)
            {
                coolDown--;
            }

            if (controller.TriggerValue > 0.5f)
            {
                if (currentState == UIState.MENU)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(controllerInput.transform.position, controllerInput.transform.forward, out hit))
                    {
                        Debug.Log("button hit " + hit.transform.gameObject.name);
                        if (hit.transform.gameObject.name == "StartButton")
                        {
                            TransitionState(UIState.CAPTURE_SETUP);
                            coolDown = 100;
                        }
                        else if (sessionNames.Contains(hit.transform.gameObject.name))
                        {
                            Debug.Log("helloooo " + hit.transform.gameObject.name);
                            viewManager.lightFieldName = hit.transform.gameObject.name;
                            TransitionState(UIState.VIEW_FOCAL_SETUP);
                            coolDown = 100;
                        }

                    }
                }
                else if (currentState == UIState.VIEWING)
                {
                    viewManager.SnapFocalTo(controllerInput.transform.position);
                }
                else if (coolDown == 0 && currentState == UIState.CAPTURE_SETUP) // create focal point! 
                {
                    TransitionState(UIState.CAPTURING);
                }
            }

        }

        void TransitionState(UIState newState)
        {
            switch (newState)
            {
                case UIState.MENU:
                    if (currentState == UIState.CAPTURING)
                    {
                        timer = 0.0f;
                        lastCaptureAttemptTime = 0.0f;
                        captureManager.SaveSession();
                        captureManager.StopCurrentSession();
                        foreach (GameObject button in sessionButtons)
                        {
                            GameObject.Destroy(button);
                        }
                        sessionButtons.Clear();
                        sessionNames.Clear();
                        CreateSavedSessionButtons();
                    }
                    else if (currentState == UIState.VIEWING || currentState == UIState.VIEW_FOCAL_SETUP || currentState == UIState.VIEW_RAD_SETUP)
                    {
                        viewManager.DeactivateSession();
                    }

                    HeadlockedCanvas.SetActive(true);

                    break;
                case UIState.CAPTURE_SETUP:
                    HeadlockedCanvas.SetActive(false);

                    break;
                case UIState.CAPTURING:
                    captureManager.SetFocalPoint(controller.Position);
                    captureManager.SetFocalPointColor(new Color(0, 1, 0, 1));
                    break;
                case UIState.VIEW_FOCAL_SETUP:
                    HeadlockedCanvas.SetActive(false);
                    viewManagerObj.SetActive(true);
                    break;
                case UIState.VIEWING:
                    viewManager.LoadSession();
                    break;

            }

            currentState = newState;
        }


        void StartCaptureSession()
        {
            captureManager.StartCaptureSession();
        }


        void AttemptImageCapture()
        {
            Debug.Log("ATTEMPTING IMAGE CAPTURE");
            captureManager.CheckCaptureConditions();
            if (!prevCaptureAngleGood && captureManager.CaptureAngleGood)
            {
                captureManager.SetFocalPointColor(new Color(0, 1, 0, 1));
            }
            else if (prevCaptureAngleGood && !captureManager.CaptureAngleGood)
            {
                captureManager.SetFocalPointColor(new Color(1, 0, 0, 1));
            }

            prevCaptureAngleGood = captureManager.CaptureAngleGood;

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