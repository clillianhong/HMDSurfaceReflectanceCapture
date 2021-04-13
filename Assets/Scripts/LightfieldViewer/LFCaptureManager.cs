using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;
using UnityEngine.UIElements;

namespace Simulation.Viewer
{

    public class LFCaptureManager : MonoBehaviour
    {
        // Start is called before the first frame update

        /*
        input: 
        - sessionName 
        - degrees to take next capture 
        - record capture session and save it 
            - establish focal point for capture 
            -  click home to start capture 
                - add to collection 
                - potentially take image every 0.5 seconds      
                - automatically take image every time angle between you nearest capture increases by more than X degrees (kdTree!) 
                    - render dot on unit sphere around focal point
            - click home to stop capture 
        */

        private string currentSessionName;
        public float degreeDifference;

        void Start()
        {


            CheckAllObjectsSet();
        }

        void CheckAllObjectsSet()
        {
            if (degreeDifference <= 0)
            {
                degreeDifference = 5;
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void StartCaptureSession(string sessionName)
        {
            MLInput.OnControllerButtonDown += OnButtonDown;
        }

        public void StopCurrentSession()
        {
            MLInput.OnControllerButtonDown -= OnButtonDown;
        }

        private void OnButtonDown(byte controllerId, MLInput.Controller.Button button)
        {
            if (MLInput.Controller.Button.HomeTap == button)  // go back to menu 
            {

            }
        }
    }

}