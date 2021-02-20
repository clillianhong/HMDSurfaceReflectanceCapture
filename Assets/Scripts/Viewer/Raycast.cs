using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;


public class Raycast : MonoBehaviour
{

    public Transform ctransform; //camera transform 
    public GameObject prefab; //cube prefab 

    // Start is called before the first frame update
    void Start()
    {
        // Start accessing the ML World Ray API.
        MLRaycast.Start();


    }

    // Update is called once per frame
    void Update()
    {

        // Create a raycast parameters variable
        MLRaycast.QueryParams _raycastParams = new MLRaycast.QueryParams
        {
            // Update the parameters with our Camera's transform
            Position = ctransform.position,
            Direction = ctransform.forward,
            UpVector = ctransform.up,
            // Provide a size of our raycasting array (1x1)
            Width = 1,
            Height = 1
        };
        // Feed our modified raycast parameters and handler to our raycast request
        MLRaycast.Raycast(_raycastParams, HandleOnReceiveRaycast);


    }


    // Instantiate the prefab at the given point.
    // Rotate the prefab to match given normal.
    // Wait 2 seconds then destroy the prefab.
    private IEnumerator NormalMarker(Vector3 point, Vector3 normal)
    {
        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, normal);
        GameObject go = Instantiate(prefab, point, rotation);
        yield return new WaitForSeconds(2);
        Destroy(go);
    }

    // Use a callback to know when to run the NormalMaker() coroutine.
    void HandleOnReceiveRaycast(MLRaycast.ResultState state, UnityEngine.Vector3 point, Vector3 normal, float confidence)
    {
        if (state == MLRaycast.ResultState.HitObserved)
        {
            StartCoroutine(NormalMarker(point, normal));
        }
    }

    // When the prefab is destroyed, stop MLWorldRays API from running.
    private void OnDestroy()
    {
        MLRaycast.Stop();
    }



}
