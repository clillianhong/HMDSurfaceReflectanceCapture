using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Simulation.Utils;

namespace Simulation
{

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
            simSphereRadius = data.sphereRadius;
            captures = new MLCaptureView[data.captures.Length];
            SetMatrices(simSphereRadius, radius, simFocalPoint, newFocalPoint);
            realRadius = radius;
            realFocalPoint = newFocalPoint;

            for (int x = 0; x < captures.Length; x++)
            {
                CaptureJSONData captureData = data.captures[x];
                string pngPath = Loader.PathFromSessionName(data.sessionName) + "CaptureImages/" + captureData.imageFileName;
                captures[x] = new MLCaptureView(captureData, Loader.TextureFromPNG(pngPath), TransformSimToReal(captureData.transform.position));
            }

            Debug.Log("successfully loaded light field with " + captures.Length + " captures");

        }

        public void SetMatrices(float simRadius, float realRadius, Vector3 S, Vector3 R)
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

            simulationToRealityMat = transToOriginRMatInv * scaleMat * transToOriginRMat * transOriginRMat;

        }

        // [TransformSimToReal] is the scaled position of simulation vector [S]
        public Vector3 TransformSimToReal(Vector3 S)
        {
            return OP.MultPoint(this.simulationToRealityMat, S);
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
        public CaptureView(string imageFileName, Texture2D tex, Matrix4x4 vpm, Transform trans, Vector3 pos)
        {
            id = imageFileName;
            texture = tex;
            viewProjMatrix = vpm;
            capturePosition = pos;
            jsonData = new CaptureJSONData();
            jsonData.imageFileName = imageFileName;
            jsonData.transform = new TransformJSONData();
            jsonData.transform.forward = trans.forward;
            jsonData.transform.up = trans.up;
            jsonData.transform.right = trans.right;
            jsonData.transform.position = trans.position;
            jsonData.transform.projMatrix = vpm;
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

        //initialize with json data object
        public MLCaptureView(CaptureJSONData jSONData, Texture2D tex, Vector3 realityPosition)
        {
            id = jSONData.imageFileName;
            texture = tex;
            simulationProjMatrix = jSONData.transform.projMatrix;
            transformData = jSONData.transform;

            texture = tex;
            simulationCapturePosition = jSONData.transform.position;
            realityCapturePosition = realityPosition;

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