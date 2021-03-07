using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Supercluster.KDTree;

namespace CaptureSystem
{

    public class CaptureViewCollection
    {
        // Start is called before the first frame update
        public List<Capture> captures;
        public KDTree<double, Capture> kdTree;

        public CaptureViewCollection()
        {
            captures = new List<Capture>();
        }

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

            // Define the metric function
            // This will be called many times make it as fast as possible
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