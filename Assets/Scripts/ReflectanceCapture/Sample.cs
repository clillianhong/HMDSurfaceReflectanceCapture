using System;
using System.Collections.Generic;
using UnityEngine;

namespace CaptureSystem
{

    class SampleManager : MonoBehaviour
    {
        public int beta1;
        public int beta2;
        private Dictionary<Tuple<float, float>, CMSample> _samples;


    }


    /// <summary> 
    /// CMSample represent sample metadata for a single point in the ROI 
    /// </param> [beta1] is the lower bound angle for the exponential specular component 
    /// </param> [beta2] is the upper bound angle for the exponential specular component 
    /// </param> [captureIDs] is a dictionary mapping the color channel to a capture that fulfills the inequality for that channel
    ///      The channels are set as follows: 
    ///      Channel.RED -> capture theta > b2 
    ///      Channel.BLUE -> capture theta < b1 
    ///      Channel.GREEN -> b1 < capture theta < b2 
    /// </summary>
    struct CMSample
    {

        int beta1;
        int beta2;
        Dictionary<string, string> captureIDs;

        public CMSample(int b1, int b2)
        {
            beta1 = b1;
            beta2 = b2;
            captureIDs = new Dictionary<string, string>();
        }


    }




}