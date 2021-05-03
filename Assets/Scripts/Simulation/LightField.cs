using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Simulation.Utils;

namespace Simulation
{

    public enum LFManagerState
    {
        SETUP_FOCAL,
        SETUP_RADIUS,

        ACTIVE,
        STOPPED
    }

    public class LightField
    {
        public string sessionName;
        public Vector3 simFocalPoint;
        public float simSphereRadius;
        public MLCaptureView[] captures;

        //transform from sim to real 
        public Matrix4x4 simulationToRealityMat;

        //translate from sim to real origin 

        //translate to the real origin
        public Matrix4x4 transOriginRMat;
        public Matrix4x4 transToOriginRMat;

        //translate back from the real origin
        public Matrix4x4 transToOriginRMatInv;

        //scale from sim to real 
        public Matrix4x4 scaleMat;

        public float realRadius;
        public Vector3 realFocalPoint;
        public LightField(LightFieldJsonData data, float radius, Vector3 newFocalPoint, bool loadImages = true)
        {

            sessionName = data.sessionName;
            simFocalPoint = data.focalPoint;
            captures = new MLCaptureView[data.captures.Length];

            realRadius = radius;
            realFocalPoint = newFocalPoint;

            Matrix4x4 simToRealMat = CreateTransformMat(simSphereRadius, radius, simFocalPoint, newFocalPoint);

            for (int x = 0; x < captures.Length; x++)
            {
                CaptureJSONData captureData = data.captures[x];
                Debug.Log("loaded position " + captureData.transform.position);

                float simRadius = (simFocalPoint - captureData.transform.position).magnitude;
                Debug.Log("simRadius " + simRadius);
                Debug.Log("real radius " + realRadius);

                string pngPath = Loader.PathFromSessionName(data.sessionName) + "CaptureImages/" + captureData.imageFileName;
                captures[x] = new MLCaptureView(captureData, Loader.TextureFromPNG(pngPath), CreateTransformMat(simRadius, realRadius, simFocalPoint, newFocalPoint));
                Debug.Log("reality position " + captures[x].realityCapturePosition);
            }

            Debug.Log("successfully loaded light field with " + captures.Length + " captures");

        }


        /// S -> simulation focal point 
        /// R -> real focal point 
        public Matrix4x4 CreateTransformMat(float simRadius, float realRadius, Vector3 S, Vector3 R)
        {
            transOriginRMat = new Matrix4x4(
                new Vector4(1f, 0, 0, 0),
                 new Vector4(0, 1f, 0, 0),
                  new Vector4(0, 0, 1f, 0),
                   new Vector4(R.x - S.x, R.y - S.y, R.z - S.z, 1f)
            );

            transToOriginRMat = new Matrix4x4(
                new Vector4(1f, 0, 0, 0),
                 new Vector4(0, 1f, 0, 0),
                  new Vector4(0, 0, 1f, 0),
                   new Vector4(-R.x, -R.y, -R.z, 1f)
            );

            float scaleRatio = realRadius / simRadius;

            scaleMat = new Matrix4x4(
               new Vector4(scaleRatio, 0, 0, 0),
                new Vector4(0, scaleRatio, 0, 0),
                 new Vector4(0, 0, scaleRatio, 0),
                  new Vector4(0, 0, 0, 1f)
           );

            transToOriginRMatInv = new Matrix4x4(
                new Vector4(1f, 0, 0, 0),
                 new Vector4(0, 1f, 0, 0),
                  new Vector4(0, 0, 1f, 0),
                   new Vector4(R.x, R.y, R.z, 1f)
            );

            return transToOriginRMatInv * scaleMat * transToOriginRMat * transOriginRMat;

        }

        // [TransformSimToReal] is the scaled position of simulation vector [S]
        public Vector3 TransformSimToReal(Vector3 S, Matrix4x4 simToReal)
        {
            return OP.MultPoint(simToReal, S);
        }
    }

    [System.Serializable]
    public class LightFieldJsonData
    {

        public string sessionName;
        public Vector3 focalPoint;
        public float sphereRadius;
        public CaptureJSONData[] captures;

    }

    public struct CaptureView
    {
        public string id;
        public Texture2D texture;
        public Matrix4x4 viewProjMatrix;

        public CaptureJSONData jsonData;

        public Vector3 capturePosition;


        //initialize when capturing in simulation
        public CaptureView(string imageFileName, Texture2D tex, Matrix4x4 vpm, CaptureJSONData data)
        {
            id = imageFileName;
            texture = tex;
            viewProjMatrix = vpm;
            capturePosition = data.transform.position;
            jsonData = data;
        }


    }

    public struct MLCaptureView
    {

        public string id;
        public Texture2D texture;
        public Matrix4x4 simulationProjMatrix;
        public Vector3 simulationCapturePosition;

        public Vector3 realityCapturePosition;

        public TransformJSONData transformData;

        public Matrix4x4 worldToCameraMatrix;

        //initialize with json data object
        public MLCaptureView(CaptureJSONData jSONData, Texture2D tex, Matrix4x4 worldToCap, Vector3 originalFocalPointPos, GameObject camPrefab)
        {
            id = jSONData.imageFileName;
            texture = tex;
            simulationProjMatrix = jSONData.transform.projMatrix;
            transformData = jSONData.transform;

            texture = tex;
            simulationCapturePosition = jSONData.transform.position;
            realityCapturePosition = OP.MultPoint(worldToCap, jSONData.transform.position);
            var captureCam = GameObject.Instantiate(camPrefab, realityCapturePosition, Quaternion.identity);
            var camComponent = captureCam.GetComponent<Camera>();
            camComponent.transform.LookAt(originalFocalPointPos);
            worldToCameraMatrix = camComponent.cameraToWorldMatrix;
            GameObject.Destroy(captureCam);

        }



    }



    [System.Serializable]
    public class CaptureJSONData
    {
        public string imageFileName;

        public TransformJSONData transform;


    }
    [System.Serializable]
    public class TransformJSONData
    {
        public Vector3 forward;
        public Vector3 up;
        public Vector3 right;
        public Vector3 position;
        public Matrix4x4 projMatrix;
    }

}