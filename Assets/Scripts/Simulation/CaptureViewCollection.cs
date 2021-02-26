using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**

Manages a collection of CaptureViews taken by the capture camera 

*/

namespace Simulation
{
    public class CaptureViewCollection : MonoBehaviour
    {
        // Start is called before the first frame update

        // public List<Simulation.CaptureView> captureViews;

        public Transform focalPoint;
        public Transform captureCamera;

        public List<CaptureView> captureViews;

        public Vector3 comparisonPoint;

        void Start()
        {
            captureViews = new List<CaptureView>();
            comparisonPoint = focalPoint.position + new Vector3(Vector3.Distance(captureCamera.position, focalPoint.position), 0, 0);
        }


        public CaptureView[] FindNearestCapture(int k, Vector3 position)
        {

            CaptureView[] o = new CaptureView[k];
            CaptureView leastView = new CaptureView();
            float minDist = float.PositiveInfinity;

            foreach (CaptureView view in captureViews)
            {
                float curDist = Vector3.Distance(position, view.capturePosition);

                if (minDist > curDist)
                {
                    minDist = curDist;
                    leastView = view;
                }
            }

            o[0] = leastView;

            return o;
        }

    }



}
