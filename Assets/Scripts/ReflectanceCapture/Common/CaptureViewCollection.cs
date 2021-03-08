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
        public List<Capture> captures;
        public KDTree<double, Capture> kdTree;

        public CaptureViewCollection()
        {
            captures = new List<Capture>();
        }

        /// <summary>
        /// GenerateKDTree creates a KDTree of 3 dimensions, out of their world coordinate positions, with each position being associated with a Capture node.<!-- -->
        /// </summary>
        public void GenerateKDTree()
        {
            Debug.Log("Beginning KDTree generation");
            var data = new double[captures.Count][];
            for (int i = 0; i < captures.Count; i++)
            {
                var pt = new double[3];
                pt[0] = captures[i].cameraPose.position.x;
                pt[1] = captures[i].cameraPose.position.y;
                pt[2] = captures[i].cameraPose.position.z;
                data[i] = pt;
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

            kdTree = new KDTree<double, Capture>(dimensions: 3, points: data, nodes: captures.ToArray(), metric: L2Norm);
            Debug.Log("Successfully created KDTree!");
        }

        

    }

}