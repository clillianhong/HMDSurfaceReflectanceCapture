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

        private Vector3 rightVec;

        private Vector3 downVec;

        private Vector3 normalVec;

        public KDTree<double, SurfacePointData> coverageTree;

        private bool active;
        private bool instantiated;




        /// <summary> 
        ///     param bl: bottom left real world position of ROI 
        ///     param ul: upper left real world position of ROI  
        ///     param br: bottom right real world position of ROI  
        ///     param ur: upper right real world position of ROI  
        /// </summary>
        public void InitCoverageMap(Vector3 bl, Vector3 ul, Vector3 br, Vector3 ur, int numXSamples, int numYSamples)
        {

            Vector3 centerPos = ((ur - bl) / 2f) + bl;
            _bottomLeftPos = bl;
            _upperLeftPos = ul;
            _bottomRightPos = br;
            _upperRightPos = ur;
            xSamples = numXSamples;
            ySamples = numYSamples;
            rightVec = _upperRightPos - _upperLeftPos;
            rightVec.Normalize();
            downVec = _bottomLeftPos - _upperLeftPos;
            downVec.Normalize();
            normalVec = Vector3.Cross(rightVec, downVec);
            normalVec.Normalize();

            this.InitSamples();

            if (ballPrefab != null)
            {
                Debug.Log("Creating balls");
                GameObject.Instantiate(ballPrefab, _bottomLeftPos, Quaternion.identity);
                GameObject.Instantiate(ballPrefab, _bottomRightPos, Quaternion.identity);
                GameObject.Instantiate(ballPrefab, _upperLeftPos, Quaternion.identity);
                GameObject.Instantiate(ballPrefab, _upperRightPos, Quaternion.identity);
                GameObject.Instantiate(ballPrefab, centerPos, Quaternion.identity);
                GameObject.Instantiate(ballPrefab, 0.05f * normalVec + (centerPos), Quaternion.identity);

            }
            else
            {
                Debug.Log("The Ball prefab was null");
            }


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

            //TODO: clarify with #2 
            int sampleIdx = 0;
            for (int y = 0; y < ySamples; y++)
            {
                for (int x = 0; x < xSamples; x++)
                {

                    Vector3 sampleVec = vectorFromPixels(x, y);
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

        }

        /// <summary> 
        ///    Renders the entire coverage map  
        /// </summary>
        public void RenderCoverageMap()
        {


        }

        /// <summary> 
        ///     Updates all the surface point data when a new capture is taken 
        /// </summary>

        public void UpdateSurfaceData()
        {
            //calls UpdateSurfacePoint
            //calls 
        }

        /// <summary> Updates the SurfacePointData object associated with [surfacePoint] given a new [capture] </summary> 
        public void UpdateSurfacePoint(Vector3 surfacePoint, Capture capture)
        {
            SurfacePointData data;
            var success = samples.TryGetValue(surfacePoint, out data);
            if (!success)
            {
                return;
            }

            var ndcPoint = capture.cameraPose.projectionMat * capture.cameraPose.worldToCameraMat * surfacePoint;

            if (CameraController.IsVisible(ndcPoint))
            {
                var camPos = capture.cameraPose.position;

                var viewDir = camPos - surfacePoint;
                var lightDir = capture.pointLightPosition - surfacePoint;

                var halfVec = viewDir + lightDir;
                halfVec.Normalize();

                var thetaS = Vector3.Dot(halfVec, normalVec);

                int xPixel = (int)ndcPoint.x * Camera.main.pixelWidth;
                int yPixel = (int)ndcPoint.y * Camera.main.pixelHeight;
                // var color = capture.texture.GetPixel(xPixel, yPixel); //GETTING PIXEL VALUE

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
        }

        /// <summary> 
        ///     Updates the entire coverage map's color map texture when a new capture is taken 
        /// </summary>

        private void RetextureColorMap()
        {

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
        private Vector3 vectorFromPixels(int xPt, int yPt)
        {
            float widthDiff = (_upperRightPos - _upperLeftPos).magnitude / xSamples;
            float heightDiff = (_upperRightPos - _bottomRightPos).magnitude / ySamples;
            return rightVec * (xPt * widthDiff) + downVec * (yPt * heightDiff);
        }




    }



}