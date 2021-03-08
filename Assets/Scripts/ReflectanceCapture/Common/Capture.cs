using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CaptureSystem
{
    public class Capture
    {

        public string captureID;

        public Texture2D texture;

        public float thetaS; // the angle between the half vector and surface normal 

        public Vector3 pointLightPosition; //pose of the light at time of capture 
        public Transform cameraPose; //pose of the camera at time of capture

        // Start is called before the first frame update

        public Capture(string ID, Texture2D tex, float theS, Transform camTrans, Vector3 lightPos)
        {
            captureID = ID;
            texture = tex;
            thetaS = theS;
            cameraPose = camTrans;
            pointLightPosition = lightPos;
        }

    }
}