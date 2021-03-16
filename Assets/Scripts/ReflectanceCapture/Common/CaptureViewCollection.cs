using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Supercluster.KDTree;

namespace CaptureSystem
{

    /// <summary>
    /// A collection of Captures that can be organized into a KDTree for ease of finding the nearest neighbor. 
    /// </summary>

    public class CaptureViewCollection
    {
        // Start is called before the first frame update
        public Dictionary<string, Capture> captures;
        public KDTree<double, Capture> kdTree;

        public CaptureViewCollection()
        {
            captures = new Dictionary<string, Capture>();
        }

        /// <summary>
        /// GenerateKDTree creates a KDTree of 3 dimensions, out of their world coordinate positions, with each position being associated with a Capture node.<!-- -->
        /// </summary>
        public void GenerateKDTree()
        {
            Debug.Log("Beginning KDTree generation");
            var data = new double[captures.Count][];
            var idx = 0;
            foreach (KeyValuePair<string, Capture> entry in captures)
            {
                var pt = new double[3];
                pt[0] = entry.Value.cameraPose.position.x;
                pt[1] = entry.Value.cameraPose.position.y;
                pt[2] = entry.Value.cameraPose.position.z;
                data[idx] = pt;
                idx++;
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

            kdTree = new KDTree<double, Capture>(dimensions: 3, points: data, nodes: new List<Capture>(captures.Values).ToArray(), metric: L2Norm);
            Debug.Log("Successfully created KDTree!");
        }



    }

}