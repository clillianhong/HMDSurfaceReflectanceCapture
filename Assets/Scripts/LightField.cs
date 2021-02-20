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

        public Matrix4x4 simulationToRealityMat;
        public LightField(LightFieldJsonData data, float radius, Vector3 newFocalPoint, bool loadImages = true)
        {

            sessionName = data.sessionName;
            simFocalPoint = data.focalPoint;
            simSphereRadius = data.sphereRadius;
            captures = new MLCaptureView[data.captures.Length];
            simulationToRealityMat = ConstructSRMat(simSphereRadius, radius, simFocalPoint, newFocalPoint);

            for (int x = 0; x < captures.Length; x++)
            {
                CaptureJSONData captureData = data.captures[x];
                string pngPath = Loader.PathFromSessionName(data.sessionName) + "CaptureImages/" + captureData.imageFileName;
                captures[x] = new MLCaptureView(captureData, Loader.TextureFromPNG(pngPath), simulationToRealityMat);
            }

            Debug.Log("successfully loaded light field with " + captures.Length + " captures");

        }

        public Matrix4x4 ConstructSRMat(float simRadius, float realRadius, Vector3 S, Vector3 R)
        {
            Matrix4x4 T_SR = new Matrix4x4(
                new Vector4(1, 0, 0, 0),
                 new Vector4(0, 1, 0, 0),
                  new Vector4(0, 0, 1, 0),
                   new Vector4(R.x - S.x, R.y - S.y, R.z - S.z, 1)
            );

            Matrix4x4 T_Sfoc = new Matrix4x4(
                new Vector4(1, 0, 0, 0),
                 new Vector4(0, 1, 0, 0),
                  new Vector4(0, 0, 1, 0),
                   new Vector4(-R.x, -R.y, -R.z, 1)
            );

            Matrix4x4 T_Sfoc_inv = new Matrix4x4(
                new Vector4(1, 0, 0, 0),
                 new Vector4(0, 1, 0, 0),
                  new Vector4(0, 0, 1, 0),
                   new Vector4(R.x, R.y, R.z, 1)
            );

            Matrix4x4 S_SR = new Matrix4x4(
                new Vector4(realRadius / simRadius, 0, 0, 0),
                 new Vector4(0, realRadius / simRadius, 0, 0),
                  new Vector4(0, 0, realRadius / simRadius, 0),
                   new Vector4(0, 0, 0, 1)
            );

            return T_Sfoc_inv * S_SR * T_Sfoc * T_SR;

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
        public MLCaptureView(CaptureJSONData jSONData, Texture2D tex, Matrix4x4 simToRealityMat)
        {
            id = jSONData.imageFileName;
            texture = tex;
            simulationProjMatrix = jSONData.transform.projMatrix;
            transformData = jSONData.transform;

            texture = tex;
            simulationCapturePosition = jSONData.transform.position;
            realityCapturePosition = simToRealityMat * simulationCapturePosition;

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