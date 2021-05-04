using System;
using System.Collections.Generic;
using UnityEngine;
using Supercluster.KDTree;

namespace CaptureSystem
{

    /// <summary> 
    /// The coverage map represents each pixel on the map in three colors, which indicate the level of coverage.
    /// Coverage indicates the number of relevant samples having been taken of that pixel.
    /// Given thresholds b1 and b2 and capture theta:
    ///     thetaS < b1 -> blue channel set to 1 else 0 
    ///     b1 < thetaS < b2 -> green channel set to 1 else 0 
    ///     thetaS > b2 -> red channel set to 1 else 0 
    ///  Manager for all points within the region of interest, tracking their samplings status
    ///</summary> 

    public class CoverageMap : MonoBehaviour
    {

        // upper and lower bound angles for curve fitting 
        public int beta1;
        public int beta2;

        public Material coverageMapMaterial;

        public RenderTexture renderTexture;

        public GameObject previewCube;

        public GameObject ballPrefab;

        // Maps real world position to its sample data 
        public Dictionary<Vector3, SurfacePointData> samples;

        //real point positions of the sample ROI 
        private Vector3 _bottomLeftPos;

        public Vector3 bottomLeftPos
        {
            get { return _bottomLeftPos; }
        }
        private Vector3 _upperLeftPos;

        public Vector3 upperLeftPos
        {
            get { return _upperLeftPos; }
        }
        private Vector3 _bottomRightPos;
        public Vector3 bottomRightPos
        {
            get { return _bottomRightPos; }
        }
        private Vector3 _upperRightPos;
        public Vector3 upperRightPos
        {
            get { return _upperRightPos; }
        }

        public int xSamples; //pixel width of camera sample image  
        public int ySamples; //pixel height of camera sample image 

        private float recWidth;
        private float recHeight;

        private Vector3 rightVec;

        private Vector3 downVec;

        private Vector3 normalVec;
        private Vector3 centerPos;

        public KDTree<double, SurfacePointData> coverageTree;

        private bool active;
        private bool instantiated;

        private Texture2D samplesTexture;


        public event Action<string> OnUpdateMap = null;


        /// <summary> 
        ///     param bl: bottom left real world position of ROI 
        ///     param ul: upper left real world position of ROI  
        ///     param br: bottom right real world position of ROI  
        ///     param ur: upper right real world position of ROI  
        /// </summary>
        public void InitCoverageMap(Vector3 bl, Vector3 ul, Vector3 br, Vector3 ur, int numXSamples, int numYSamples)
        {

            centerPos = ((ur - bl) / 2f) + bl;
            _bottomLeftPos = bl;
            _upperLeftPos = ul;
            _bottomRightPos = br;
            _upperRightPos = ur;
            // xSamples = numXSamples;
            // ySamples = numYSamples;
            rightVec = (_upperRightPos - _upperLeftPos).normalized;
            downVec = (_bottomLeftPos - _upperLeftPos).normalized;
            normalVec = Vector3.Cross(rightVec, downVec);
            normalVec.Normalize();

            Debug.Log("bottom left pos " + _bottomLeftPos);
            Debug.Log("upper left pos " + _upperLeftPos);

            recWidth = (_upperRightPos - _upperLeftPos).magnitude;
            recHeight = (_upperRightPos - _bottomRightPos).magnitude;

            Debug.Log("recW " + recWidth);
            Debug.Log("recH " + recHeight);

            MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.enabled = true;
            meshRenderer.sharedMaterial = coverageMapMaterial;

            samplesTexture = new Texture2D(xSamples, ySamples);

            Debug.Log("MAPP TEXTURE");

            this.InitSamples();

            Debug.Log("Creating balls");

            //four corners and the normal vector 
            GameObject.Instantiate(ballPrefab, _upperRightPos, Quaternion.identity);
            GameObject.Instantiate(ballPrefab, _bottomLeftPos, Quaternion.identity);
            GameObject.Instantiate(ballPrefab, _upperLeftPos, Quaternion.identity);
            GameObject.Instantiate(ballPrefab, _bottomRightPos, Quaternion.identity);
            GameObject.Instantiate(ballPrefab, centerPos + (0.05f * normalVec), Quaternion.identity);

            gameObject.GetComponent<MeshRenderer>().sharedMaterial.SetFloat("beta1", beta1);
            gameObject.GetComponent<MeshRenderer>().sharedMaterial.SetFloat("beta2", beta2);
            gameObject.GetComponent<MeshRenderer>().sharedMaterial.SetInt("debug", 0);

            Texture2D clearTex = new Texture2D(xSamples, ySamples);
            for (int x = 0; x < xSamples; x++)
            {
                for (int y = 0; y < ySamples; y++)
                {
                    clearTex.SetPixel(x, y, new Color(0, 0, 0, 1f));
                }
            }
            clearTex.Apply();

            gameObject.GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_MapTexture", clearTex);

            // previewCube.GetComponent<MeshRenderer>().material.mainTexture = gameObject.GetComponent<MeshRenderer>().sharedMaterial.mainTexture;

            active = true;
            instantiated = false;
        }

        void Start()
        {
            CheckVariablesSet();
        }

        void Update()
        {
            if (active)
            {
                if (!instantiated)
                {

                    //four corners and normal vec
                    instantiated = true;

                }

                // RenderCoverageMap();
            }
        }

        public void UpdateCoverageMap(Matrix4x4 ndcCamMat, Vector3 camPos, Vector3 lightPos)
        {
            gameObject.GetComponent<MeshRenderer>().sharedMaterial.SetMatrix("camNDCMat", ndcCamMat);
            gameObject.GetComponent<MeshRenderer>().sharedMaterial.SetVector("camPos", new Vector4(camPos.x, camPos.y, camPos.z, 1));
            gameObject.GetComponent<MeshRenderer>().sharedMaterial.SetVector("lightPos", new Vector4(lightPos.x, lightPos.y, lightPos.z, 1));

            // var meshRenderer = gameObject.GetComponent<MeshRenderer>();
            // Texture2D tex = CreateMapTexture(camPos, lightPos);
            // meshRenderer.sharedMaterial.mainTexture = tex;
        }

        // public void updateShaderMapTexture()
        // {
        //     Texture2D clearTex = new Texture2D(xSamples, ySamples);
        //     for (int x = 0; x < xSamples; x++)
        //     {
        //         for (int y = 0; y < ySamples; y++)
        //         {
        //             if (x < y)
        //             {
        //                 clearTex.SetPixel(x, y, new Color(1, 0, 0, 1f));
        //             }
        //             else
        //             {
        //                 clearTex.SetPixel(x, y, new Color(0, 1, 0, 1f));
        //             }

        //         }
        //     }

        //     var renderTexture = new RenderTexture(xSamples, ySamples, 16);

        //     RenderTexture activeRenderTexture = RenderTexture.active;

        //     var renderer = gameObject.GetComponent<MeshRenderer>();
        //     var material = Instantiate(renderer.sharedMaterial);

        //     material.SetFloat("beta1", beta1);
        //     material.SetFloat("beta2", beta2);
        //     material.SetInt("debug", 0);

        //     material.SetMatrix("camNDCMat", gameObject.GetComponent<MeshRenderer>().sharedMaterial.GetMatrix("camNDCMat"));
        //     material.SetVector("camPos", gameObject.GetComponent<MeshRenderer>().sharedMaterial.GetVector("camPos"));
        //     material.SetVector("lightPos", gameObject.GetComponent<MeshRenderer>().sharedMaterial.GetVector("lightPos"));
        //     material.SetTexture("_MainTex", clearTex);

        //     Graphics.Blit(clearTex, renderTexture, material);
        //     RenderTexture.active = renderTexture;
        //     Texture2D frame = new Texture2D(renderTexture.width, renderTexture.height);
        //     frame.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0, false);
        //     frame.Apply();

        //     RenderTexture.active = activeRenderTexture;

        //     previewCube.GetComponent<MeshRenderer>().material.mainTexture = frame;

        //     // foreach (var color in frame.GetPixels())
        //     // {
        //     //     Debug.Log("FRAM TEXTYRE (" + color.r + ", " + color.g + ", " + color.b + ")");

        //     // }

        //     // Graphics.Blit(coverageMapMaterial., renderTexture);
        //     gameObject.GetComponent<MeshRenderer>().sharedMaterial.SetTexture("mapTexture", frame);
        // }

        public Tuple<Texture2D, float> OnCaptureTaken(Capture capture)
        {

            var output = UpdateSurfaceData(capture, true);

            Texture2D oldCoverageMapTex = (Texture2D)gameObject.GetComponent<MeshRenderer>().sharedMaterial.GetTexture("_MapTexture");
            Texture2D newCoverageMapTex = CreateCoverageMapTexture(capture, oldCoverageMapTex);

            Debug.Log("NEW COVERAGE MAP " + newCoverageMapTex.GetPixel(2, 2));

            previewCube.GetComponent<MeshRenderer>().material.mainTexture = newCoverageMapTex;

            gameObject.GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_MapTexture", newCoverageMapTex);

            return new Tuple<Texture2D, float>(newCoverageMapTex, output.Item2);
        }

        private void CheckVariablesSet()
        {
            if (beta1 == 0 || beta2 == 0)
            {
                Debug.Log("Beta values not set in CoverageMap");
                Environment.Exit(1);
            }
            if (xSamples == 0 || ySamples == 0)
            {
                Debug.Log("Sample width and height values not set in CoverageMap");
                Environment.Exit(1);
            }
        }
        /// <summary> 
        /// Fills in sample dictionary with all position vectors
        /// </summary>
        private void InitSamples()
        {
            var numSamples = xSamples * ySamples * 3;
            var treeData = new double[xSamples * ySamples][];
            var surfacePts = new SurfacePointData[xSamples * ySamples];
            samples = new Dictionary<Vector3, SurfacePointData>();

            //mesh generation
            MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();

            Mesh mesh = new Mesh();

            float dx = recWidth / (float)xSamples;
            float dy = recHeight / (float)ySamples;

            Vector3[] vertices = new Vector3[(ySamples + 1) * (xSamples + 1)];
            Vector3[] normals = new Vector3[(ySamples + 1) * (xSamples + 1)];
            Vector2[] uv = new Vector2[(ySamples + 1) * (xSamples + 1)];
            int[] tris = new int[(xSamples * ySamples * 2) * 3];
            int trisIdx = 0;

            int sampleIdx = 0;
            int meshIdx = 0;
            Debug.Log("ySamples " + ySamples);
            Debug.Log("xSamples " + xSamples);
            for (float y = 0; y <= ySamples; y++)
            {
                for (float x = 0; x <= xSamples; x++)
                {

                    Vector3 sampleVec = vectorFromPixels(x, y);
                    GameObject.Instantiate(ballPrefab, sampleVec, Quaternion.identity);
                    vertices[meshIdx] = sampleVec;

                    //mesh generation
                    normals[meshIdx] = normalVec;
                    uv[meshIdx] = new Vector2(((x * dx) / recWidth), (y * dy) / recHeight);
                    // uv[meshIdx] = new Vector2(0, 0);
                    // Debug.Log("MESH UVS " + uv[meshIdx]);

                    if (x < xSamples && y < ySamples)
                    {
                        tris[trisIdx] = meshIdx;
                        tris[trisIdx + 1] = meshIdx + 1;
                        tris[trisIdx + 2] = meshIdx + xSamples + 1;
                        trisIdx += 3;
                        tris[trisIdx] = meshIdx + 1;
                        tris[trisIdx + 1] = meshIdx + xSamples + 2;
                        tris[trisIdx + 2] = meshIdx + xSamples + 1;
                        trisIdx += 3;

                        SurfacePointData surfacePointData = new SurfacePointData(sampleVec);
                        if (!samples.ContainsKey(sampleVec))
                        {
                            samples.Add(sampleVec, surfacePointData);
                        }

                        var pt = new double[3];
                        pt[0] = sampleVec.x;
                        pt[1] = sampleVec.y;
                        pt[2] = sampleVec.z;
                        treeData[sampleIdx] = pt;
                        surfacePts[sampleIdx] = surfacePointData;
                        sampleIdx++;
                    }
                    meshIdx++;

                }
            }

            // The metric function for determining distance within the KDTree
            Func<double[], double[], double> L2Norm = (x, y) =>
            {
                double dist = 0f;
                for (int i = 0; i < x.Length; i++)
                {
                    dist += (x[i] - y[i]) * (x[i] - y[i]);
                }
                return dist;
            };

            coverageTree = new KDTree<double, SurfacePointData>(dimensions: 3, points: treeData, nodes: surfacePts, metric: L2Norm);
            Debug.Log("Successfully created KDTree!");

            mesh.vertices = vertices;
            mesh.triangles = tris;
            mesh.normals = normals;
            mesh.uv = uv;

            meshFilter.mesh = mesh;

            Debug.Log("Mesh Map created");

        }

        /// <summary> 
        ///     Updates all the surface point data when a new capture is taken 
        /// </summary>

        public Tuple<Texture2D, float> UpdateSurfaceData(Capture capture, bool updateTexture = false)
        {
            int numFullySampled = 0;

            for (int y = 0; y < ySamples; y++)
            {
                for (int x = 0; x < xSamples; x++)
                {
                    Vector3 samplePt = vectorFromPixels(x, y);
                    SurfacePointData data = UpdateSurfacePoint(samplePt, capture);
                    if (updateTexture && data != null)
                    {
                        Color col = new Color();
                        col.a = 1;
                        int channels = 0;
                        if (data.hasChannel(ChannelColor.BLUE))
                        {
                            col.b = 1;
                            channels++;
                        }
                        if (data.hasChannel(ChannelColor.GREEN))
                        {
                            col.g = 1;
                            channels++;
                        }
                        if (data.hasChannel(ChannelColor.RED))
                        {
                            col.r = 1;
                            channels++;
                        }

                        samplesTexture.SetPixel(x, y, col);
                        if (channels == 3)
                        {
                            numFullySampled++;
                        }
                        // Debug.Log("updated color " + col);
                    }
                }
            }


            return new Tuple<Texture2D, float>(samplesTexture, numFullySampled / ((float)ySamples * xSamples));
            //calls UpdateSurfacePoint
            //calls 
        }

        /// <summary> Updates the SurfacePointData object associated with [surfacePoint] given a new [capture] </summary> 
        public SurfacePointData UpdateSurfacePoint(Vector3 surfacePoint, Capture capture)
        {
            SurfacePointData data;
            var success = samples.TryGetValue(surfacePoint, out data);
            if (!success)
            {
                return null;
            }

            var ndcPoint = capture.cameraPose.projectionMat * capture.cameraPose.worldToCameraMat * surfacePoint;

            if (CameraController.IsVisible(ndcPoint))
            {
                var camPos = capture.cameraPose.position;

                var viewDir = camPos - surfacePoint;
                var lightDir = capture.pointLightPosition - surfacePoint;

                var halfVec = viewDir + lightDir;
                halfVec.Normalize();

                var thetaS = 180 * Mathf.Acos(Vector3.Dot(halfVec, normalVec)) / Mathf.PI;

                var viewPoint = Camera.main.WorldToViewportPoint(surfacePoint);

                int xPixel = (int)(viewPoint.x * Camera.main.pixelWidth);
                int yPixel = (int)(viewPoint.y * Camera.main.pixelHeight);
                var color = capture.texture.GetPixel(xPixel, yPixel); //GETTING PIXEL VALUE
                // Debug.Log("PIX VAL " + color + " at pixel x=" + xPixel + " y=" + yPixel);
                // Debug.Log("thetaS " + thetaS);

                if (thetaS <= beta1 && !data.hasChannel(ChannelColor.BLUE))
                {
                    data.addCapture(ChannelColor.BLUE, capture.captureID, xPixel, yPixel);
                }
                else if (thetaS > beta1 && thetaS < beta2 && !data.hasChannel(ChannelColor.GREEN))
                {
                    data.addCapture(ChannelColor.GREEN, capture.captureID, xPixel, yPixel);
                }
                else if (thetaS >= beta2 && !data.hasChannel(ChannelColor.RED))
                {
                    data.addCapture(ChannelColor.RED, capture.captureID, xPixel, yPixel);
                }

            }
            return data;
        }

        /// <summary> 
        ///     Updates the entire coverage map's color map texture when a new capture is taken 
        /// </summary>

        private Texture2D CreateCoverageMapTexture(Capture capture, Texture2D oldCoverageMapTex)
        {

            Texture2D coverageMapTexture = new Texture2D(xSamples, ySamples);

            for (int y = 0; y < ySamples; y++)
            {
                for (int x = 0; x < xSamples; x++)
                {
                    Vector3 surfacePoint = vectorFromPixels(x, y);
                    Vector4 ndc4 = capture.cameraPose.projectionMat * capture.cameraPose.worldToCameraMat * surfacePoint;
                    Vector3 ndc = new Vector3(ndc4.x, ndc4.y, ndc4.z);
                    Color pixelColor = oldCoverageMapTex.GetPixel(x, y);
                    pixelColor.a = 1;

                    // if (ndc.x <= 1 && ndc.x >= -1 && ndc.y <= 1 && ndc.y >= -1 && ndc.z <= 1 && ndc.z >= -1)
                    // {
                    Vector3 viewDir = (capture.cameraPose.position - surfacePoint).normalized;
                    Vector3 lightDir = (capture.pointLightPosition - surfacePoint).normalized;
                    Vector3 halfVec = (viewDir + lightDir).normalized;

                    float thetaS = 180.0f * ((float)Math.Acos(Vector3.Dot(halfVec, normalVec))) / (float)Math.PI;
                    // Debug.Log("theta " + thetaS + " b1 " + beta1 + " b2 " + beta2);

                    if (thetaS <= beta1)
                    {
                        // Debug.Log("PIXEL IS green");
                        pixelColor.g = 1.0f;
                    }
                    if (thetaS > beta1 && thetaS < beta2)
                    {

                        // Debug.Log("PIXEL IS blue");
                        pixelColor.b = 1.0f;
                    }
                    if (thetaS >= beta2)
                    {
                        // Debug.Log("PIXEL IS RED");
                        pixelColor.r = 1.0f;
                    }
                    // }

                    coverageMapTexture.SetPixel(x, y, pixelColor);

                }
            }
            coverageMapTexture.Apply();

            return coverageMapTexture;


        }

        /// <summary> 
        ///     Updates the coverage map's bullseye outline given the current main camera information 
        /// </summary>

        private void RetextureBullseyeOutline()
        {

        }


        /// <summary> 
        ///     The position of the sample point given the 2D points on the sample rect (origin in top left corner)
        /// </summary>
        private Vector3 vectorFromPixels(float xPt, float yPt)
        {
            float widthDiff = recWidth / (float)xSamples;
            float heightDiff = recHeight / (float)ySamples;
            return (rightVec * (xPt * widthDiff)) + (downVec * (yPt * heightDiff)) + _upperLeftPos;
        }


    }



}