using System;
using System.Collections.Generic;
using UnityEngine;

namespace CaptureSystem
{

    /// <summary> 
    /// BRDFSample represent sample metadata for a single real world point in the ROI 
    ///     param: [beta1] is the lower bound angle for the exponential specular component 
    ///     param: [beta2] is the upper bound angle for the exponential specular component 
    ///     param: [worldPosition] is the world position of the PointSampleData (somewhere within the ROI)
    ///     param: [sampleCaptures] is a dictionary mapping the color channel to a CapturePoint that fulfills the inequality for that channel
    /// The channels are set as follows: 
    /// Channel.RED -> capture theta > b2 
    /// Channel.BLUE -> capture theta < b1 
    /// Channel.GREEN -> b1 < capture theta < b2 
    /// </summary>
    public class SurfacePointData
    {

        Vector3 worldPosition;

        Dictionary<string, ImageSamplePoint> sampleCaptures;

        public SurfacePointData(Vector3 worldPos)
        {
            sampleCaptures = new Dictionary<string, ImageSamplePoint>();
            worldPosition = worldPos;
        }

    }


    /// <summary> 
    /// Stores the location of a specific sample point in an image
    ///     param: [captureID] -> the ID of the capture
    ///     param: [x] the x coord of this point in that capture image 
    ///     param: [y] the y coord of this point in that capture image 
    /// </summary>
    struct ImageSamplePoint
    {
        string captureID;
        int x;
        int y;
    }
}