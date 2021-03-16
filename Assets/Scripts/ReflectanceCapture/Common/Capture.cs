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
        public TransformData cameraPose; //pose of the camera at time of capture

        // Start is called before the first frame update

        public Capture(string ID, Texture2D tex, float theS, Transform camTrans, Vector3 lightPos, Matrix4x4 projMat, Matrix4x4 worldToCam)
        {
            captureID = ID;
            texture = tex;
            thetaS = theS;
            cameraPose = new TransformData();
            cameraPose.forward = camTrans.forward;
            cameraPose.up = camTrans.up;
            cameraPose.position = camTrans.position;
            cameraPose.right = camTrans.right;
            cameraPose.rotation = camTrans.rotation;
            cameraPose.projectionMat = projMat;
            cameraPose.worldToCameraMat = worldToCam;

            pointLightPosition = lightPos;
        }

    }

    [System.Serializable]
    public class TransformData
    {
        public Vector3 forward;
        public Vector3 up;
        public Vector3 right;
        public Vector3 position;

        public Quaternion rotation;

        public Matrix4x4 projectionMat;
        public Matrix4x4 worldToCameraMat;
    }
}