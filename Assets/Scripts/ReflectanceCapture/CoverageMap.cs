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

    class CoverageMap : MonoBehaviour
    {

        // upper and lower bound angles for curve fitting 
        public int beta1;
        public int beta2;

        // Maps real world position to its sample data 

        public Dictionary<Vector3, SurfacePointData> samples;

        //real point positions of the sample ROI 
        public Vector3 bottomLeftPos;
        public Vector3 upperLeftPos;
        public Vector3 bottomRightPos;
        public Vector3 upperRightPos;

        public int xSamples; //pixel width of camera sample image  
        public int ySamples; //pixel height of camera sample image 

        private Vector3 rightVec;

        private Vector3 downVec;

        private Vector3 normalVec;

        public KDTree<double, SurfacePointData> coverageTree;


        /// <summary> 
        ///     param bl: bottom left real world position of ROI 
        ///     param ul: upper left real world position of ROI  
        ///     param br: bottom right real world position of ROI  
        ///     param ur: upper right real world position of ROI  
        /// </summary>
        public CoverageMap(Vector3 bl, Vector3 ul, Vector3 br, Vector3 ur, int numXSamples, int numYSamples)
        {

            bottomLeftPos = bl;
            upperLeftPos = ul;
            bottomRightPos = br;
            upperRightPos = ur;
            xSamples = numXSamples;
            ySamples = numYSamples;
            Vector3 rightVec = upperRightPos - upperLeftPos;
            rightVec.Normalize();
            Vector3 downVec = bottomLeftPos - upperLeftPos;
            downVec.Normalize();
            normalVec = Vector3.Cross(downVec, rightVec);
            normalVec.Normalize();
            this.InitSamples();
        }

        void Start()
        {
            CheckVariablesSet();

        }

        private void CheckVariablesSet()
        {
            if (beta1 == 0 || beta2 == 0)
            {
                Debug.Log("Beta values not set in CoverageMap");
                Environment.Exit(1);
            }
        }
        /// <summary> 
        /// Fills in sample dictionary with all position vectors
        /// </summary>
        private void InitSamples()
        {
            var numSamples = xSamples * ySamples * 3;
            var treeData = new double[numSamples][];
            var surfacePts = new SurfacePointData[xSamples * ySamples];

            //TODO: clarify with #2 
            int sampleIdx = 0;
            for (int y = 0; y < ySamples; y++)
            {
                for (int x = 0; x < xSamples; x++)
                {

                    Vector3 sampleVec = vectorFromPixels(x, y);
                    SurfacePointData surfacePointData = new SurfacePointData(sampleVec);
                    samples.Add(sampleVec, surfacePointData);

                    var pt = new double[3];
                    pt[0] = sampleVec.x;
                    pt[1] = sampleVec.y;
                    pt[2] = sampleVec.z;
                    treeData[sampleIdx] = pt;
                    surfacePts[y * x + x] = surfacePointData;
                    sampleIdx += 3;

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

        /// update surfacepointdata given view dir and surface point 
        public void updateSurfacePoint(Vector3 surfacePoint, Capture capture)
        {
            SurfacePointData data;
            var success = samples.TryGetValue(surfacePoint, out data);
            if (!success)
            {
                return;
            }

            var camPos = capture.cameraPose.position;

            var viewDir = camPos - surfacePoint;
            var lightDir = capture.pointLightPosition - surfacePoint;

            var halfVec = viewDir + lightDir;
            halfVec.Normalize();

            var thetaS = Vector3.Dot(halfVec, normalVec);

            if (thetaS <= beta1 && !data.hasChannel(ChannelColor.BLUE))
            {

            }
            else if (thetaS > beta1 && thetaS < beta2 && !data.hasChannel(ChannelColor.GREEN))
            {

            }
            else if (thetaS >= beta2 && !data.hasChannel(ChannelColor.RED))
            {

            }

            //assert the tree also updated 

        }

        /// create custom texture based on surfacepointdata info 
        /// render this texture onto display plane 


        /// <summary> 
        ///     The position of the sample point given the 2D points on the sample rect (origin in top left corner)
        /// </summary>
        private Vector3 vectorFromPixels(int xPt, int yPt)
        {
            float widthDiff = (upperRightPos - upperLeftPos).magnitude / xSamples;
            float heightDiff = (upperRightPos - bottomRightPos).magnitude / ySamples;
            return rightVec * (xPt * widthDiff) + downVec * (yPt * heightDiff);
        }




    }



}