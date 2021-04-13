using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;
using UnityEngine.UIElements;
using UnityEngine.UI;
using TMPro;


namespace Simulation.Viewer
{

    public class UIManager : MonoBehaviour
    {
        // Start is called before the first frame update
        private MLInput.Controller controller;
        public GameObject HeadlockedCanvas;
        public GameObject controllerInput;

        public TMP_Text sessionNameText;

        public LFCaptureManager captureManager;
        public LightFieldViewManager viewManager;

        enum UIState
        {
            MENU,
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
            _CheckObjSet(sessionNameText, "session name text mesh");
            _CheckObjSet(captureManager, "captureManager");
            _CheckObjSet(viewManager, "viewManager");
        }

        private void OnButtonDown(byte controllerId, MLInput.Controller.Button button)
        {
            if (MLInput.Controller.Button.HomeTap == button)  // go back to menu 
            {
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
                RaycastHit hit;
                if (Physics.Raycast(controllerInput.transform.position, controllerInput.transform.forward, out hit))
                {
                    if (hit.transform.gameObject.name == "StartButton")
                    {
                        StartCaptureSession();
                    }
                }
            }


        }

        void StartCaptureSession()
        {
            HeadlockedCanvas.SetActive(false);
            currentState = UIState.CAPTURING;
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

    }

}