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
        public Dictionary<Vector3, PointSampleData> samples;

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


    /// <summary> 
    /// CMSample represent sample metadata for a single point in the ROI 
    ///     param: [beta1] is the lower bound angle for the exponential specular component 
    ///     param: [beta2] is the upper bound angle for the exponential specular component 
    ///     param: [worldPosition] is the world position of the PointSampleData (somewhere within the ROI)
    ///     param: [sampleCaptures] is a dictionary mapping the color channel to a CapturePoint that fulfills the inequality for that channel
    /// The channels are set as follows: 
    /// Channel.RED -> capture theta > b2 
    /// Channel.BLUE -> capture theta < b1 
    /// Channel.GREEN -> b1 < capture theta < b2 
    /// </summary>
    struct PointSampleData
    {
        int beta1;
        int beta2;

        Vector3 worldPosition;

        Dictionary<string, CapturePoint> sampleCaptures;

        public CMSample(int b1, int b2, Vector3 worldPos)
        {
            beta1 = b1;
            beta2 = b2;
            sampleCaptures = new Dictionary<string, CapturePoint>();
            worldPosition = worldPos;
        }

    }


    /// <summary> 
    /// Stores the location of a specific sample point in an image
    ///     param: [captureID] -> the ID of the capture
    ///     param: [x] the x coord of this point in that capture image 
    ///     param: [y] the y coord of this point in that capture image 
    /// </summary>
    struct CapturePoint
    {
        string captureID;
        int x;
        int y;
    }




}