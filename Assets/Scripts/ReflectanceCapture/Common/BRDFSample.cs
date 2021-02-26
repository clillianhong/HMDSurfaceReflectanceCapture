using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CaptureSystem
{

    ///<summary>
    /// The output of the derenderer, a sample of the BRDF 
    ///</summary> 
    public struct BRDFSample
    {
        float brdf;
        Vector3 incidentDirection;
        Vector3 exitantDirection;
    }


}