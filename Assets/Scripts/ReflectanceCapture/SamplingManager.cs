using System;
using System.Collections.Generic;
using UnityEngine;

namespace CaptureSystem
{

    /// <summary> 
    /// Manager for all points within the region of interest, tracking their samplings status
    /// </summary> 
    class SamplingManager : MonoBehaviour
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

        public int xPixels; //pixel width of camera sample image  
        public int yPixels; //pixel height of camera sample image 

        /// <summary> 
        ///     param bl: bottom left real world position of ROI 
        ///     param ul: upper left real world position of ROI  
        ///     param br: bottom right real world position of ROI  
        ///     param ur: upper right real world position of ROI  
        /// </summary>
        public SamplingManager(Vector3 bl, Vector3 ul, Vector3 br, Vector3 ur, int xPix, int yPix)
        {
            bottomLeftPos = bl;
            upperLeftPos = ul;
            bottomRightPos = br;
            upperRightPos = ur;
            xPixels = xPix;
            yPixels = yPix;
            this.InitSamples();
        }
        /// <summary> 
        /// Fills in sample dictionary with all position vectors, discretizing based 
        /// </summary>
        private void InitSamples()
        {

            Vector3 widthDiff = (upperRightPos - upperLeftPos) / xPixels;
            Vector3 heightDiff = (upperRightPos - bottomRightPos) / yPixels;

            //TODO: clarify with #2 


        }


    }



}