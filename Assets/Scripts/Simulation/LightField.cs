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
        public Vector3 captureFocalPoint;
        public MLCaptureView[] captures;
        public float realRadius;
        public Vector3 realFocalPoint;
        public LightField(LightFieldJsonData data, float radius, Vector3 newFocalPoint, GameObject projCameraPrefab, bool loadImages = true)
        {
            sessionName = data.sessionName;
            captureFocalPoint = data.focalPoint;
            captures = new MLCaptureView[data.captures.Length];
            realRadius = radius;
            realFocalPoint = newFocalPoint;

            for (int x = 0; x < captures.Length; x++)
            {
                CaptureJSONData captureData = data.captures[x];
                float captureRadius = (captureFocalPoint - captureData.transform.position).magnitude;
                string pngPath = Loader.PathFromSessionName(data.sessionName) + "CaptureImages/" + captureData.imageFileName;
                captures[x] = new MLCaptureView(captureData, Loader.TextureFromPNG(pngPath), CreateTransformMat(captureRadius, realRadius, captureFocalPoint, newFocalPoint), newFocalPoint, projCameraPrefab);
            }

            Debug.Log("Successfully loaded light field with " + captures.Length + " captures.");
        }


        /// <summary>
        /// Creates matrix to transform the captures from capture space to the new real world space.
        /// </summary>
        /// <param name="captureRadius">captureRadius is the distance from the point of capture to the </param>
        /// <param name="realRadius">realRadius is the radius defined by the real world viewing sphere</param>
        /// <param name="C">Capture focal point</param>
        /// <param name="R">Real world focal point</param>
        /// <returns>Transformation matrix for capture -> world</returns>
        public Matrix4x4 CreateTransformMat(float captureRadius, float realRadius, Vector3 C, Vector3 R)
        {
            Matrix4x4 transOriginRMat = new Matrix4x4(
                new Vector4(1f, 0, 0, 0),
                 new Vector4(0, 1f, 0, 0),
                  new Vector4(0, 0, 1f, 0),
                   new Vector4(R.x - C.x, R.y - C.y, R.z - C.z, 1f)
            );

            Matrix4x4 transToOriginRMat = new Matrix4x4(
                new Vector4(1f, 0, 0, 0),
                 new Vector4(0, 1f, 0, 0),
                  new Vector4(0, 0, 1f, 0),
                   new Vector4(-R.x, -R.y, -R.z, 1f)
            );

            float scaleRatio = realRadius / captureRadius;

            Matrix4x4 scaleMat = new Matrix4x4(
               new Vector4(scaleRatio, 0, 0, 0),
                new Vector4(0, scaleRatio, 0, 0),
                 new Vector4(0, 0, scaleRatio, 0),
                  new Vector4(0, 0, 0, 1f)
           );

            Matrix4x4 transToOriginRMatInv = new Matrix4x4(
                new Vector4(1f, 0, 0, 0),
                 new Vector4(0, 1f, 0, 0),
                  new Vector4(0, 0, 1f, 0),
                   new Vector4(R.x, R.y, R.z, 1f)
            );

            return transToOriginRMatInv * scaleMat * transToOriginRMat * transOriginRMat;
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
        public Matrix4x4 captureProjMatrix;
        public Vector3 captureCapturePosition;
        public Vector3 realityCapturePosition;
        public TransformJSONData transformData;
        public Matrix4x4 worldToCameraMatrix;

        public MLCaptureView(CaptureJSONData jSONData, Texture2D tex, Matrix4x4 worldToCap, Vector3 originalFocalPointPos, GameObject camPrefab)
        {
            id = jSONData.imageFileName;
            texture = tex;
            captureProjMatrix = jSONData.transform.projMatrix;
            transformData = jSONData.transform;
            texture = tex;
            captureCapturePosition = jSONData.transform.position;
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