using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CaptureSystem
{
    public class Capture : MonoBehaviour
    {

        public string captureID;


        public float thetaS; // the angle between the half vector and surface normal 

        public Transform lightPose; //pose of the light at time of capture 
        public Transform cameraPose; //pose of the camera at time of capture

        // Start is called before the first frame update

        public Capture(string ID)
        {
            captureID = ID;
        }
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}