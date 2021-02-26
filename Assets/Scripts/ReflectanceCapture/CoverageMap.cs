using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> 
/// The coverage map represents each pixel on the map in three colors, which indicate the level of coverage.
/// Coverage indicates the number of relevant samples having been taken of that pixel.
/// Given thresholds b1 and b2 and capture theta:
///     thetaS < b1 -> blue channel set to 1 else 0 
///     b1 < thetaS < b2 -> green channel set to 1 else 0 
///     thetaS > b2 -> red channel set to 1 else 0 
///</summary> 

namespace CaptureSystem
{

    public class CoverageMap : MonoBehaviour
    {


        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }


        // Texture2D generateCoverageMapTexture()
        // {

                
        // }

    }



}
