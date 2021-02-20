using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using Simulation.Utils;

namespace Simulation.Tests
{
    public class TestLightField : MonoBehaviour
    {
        // A Test behaves as an ordinary method

        void Start()
        {
            TestSRMatrix();
        }

        public void TestSRMatrix()
        {

            //no radius difference
            TestAxisAlignedSRTransform(20, 20, new Vector3(10, 10, 10), new Vector3(0, 0, 0), ">>>>Test Axis Aligned SR Transform - Real Origin at 0,0,0");
            TestAxisAlignedSRTransform(20, 20, new Vector3(10, 10, 10), new Vector3(5, 5, 5), ">>>>Test Axis Aligned SR Transform - Real Origin at 5,5,5");
            TestAxisAlignedSRTransform(20, 20, new Vector3(10, 10, 10), new Vector3(1, 0, 0), ">>>>Test Axis Aligned SR Transform - Real Origin at 1,0,0");
            TestAxisAlignedSRTransform(20, 20, new Vector3(10, 10, 10), new Vector3(-1, 0, 0), ">>>>Test Axis Aligned SR Transform - Real Origin at -1,0,0");

            //radius greater than
            TestAxisAlignedSRTransform(20, 40, new Vector3(10, 10, 10), new Vector3(0, 0, 0), ">>>>Test Axis Aligned SR Transform - Real Origin at 0,0,0 - real radius 2x");
            TestAxisAlignedSRTransform(20, 40, new Vector3(10, 10, 10), new Vector3(5, 5, 5), ">>>>Test Axis Aligned SR Transform - Real Origin at 5,5,5 - real radius 2x");

            //radius less than
            TestAxisAlignedSRTransform(20, 15, new Vector3(10, 10, 10), new Vector3(0, 0, 0), ">>>>Test Axis Aligned SR Transform - Real Origin at 0,0,0 - real radius 0.75x");
            TestAxisAlignedSRTransform(20, 15, new Vector3(10, 10, 10), new Vector3(5, 5, 5), ">>>>Test Axis Aligned SR Transform - Real Origin at 5,5,5 - real radius 0.75x");

            //big ass radius
            TestAxisAlignedSRTransform(20, 15, new Vector3(-1, -1, -1), new Vector3(1, 0, 0), ">>>>Test Axis Aligned SR Transform - Sim origin at -1,-1,-1 - Real Origin at 1,0,0 - real radius 5x");
            TestAxisAlignedSRTransform(20, 15, new Vector3(-1, -1, -1), new Vector3(-1, 0, 0), ">>>>Test Axis Aligned SR Transform - Sim origin at -1,-1,-1 - Real Origin at -1,0,0 - real radius 5x");

        }

        private void TestSRCapturePos(Matrix4x4 mat, Vector3 simPos, Vector3 realPos, string descr)
        {
            Vector3 outVec = OP.MultPoint(mat, simPos);
            bool eq = Vector3.Equals(outVec, realPos);
            Debug.Assert(eq, outVec + "");
            if (eq) { Debug.Log("TEST " + descr + " PASSED"); }
        }

        private void TestAxisAlignedSRTransform(float simRadius, float realRadius, Vector3 simFocal, Vector3 realFocal, string descr)
        {

            Vector3 c1 = new Vector3(simFocal.x, simFocal.y, simFocal.z + simRadius);

            Vector3 c2 = new Vector3(simFocal.x, simFocal.y + simRadius, simFocal.z);

            Vector3 c3 = new Vector3(simFocal.x + simRadius, simFocal.y, simFocal.z);

            LightFieldJsonData lfJson = new LightFieldJsonData();
            lfJson.sessionName = "test1";
            lfJson.focalPoint = simFocal;
            lfJson.sphereRadius = simRadius;

            //don't actually load just check matrix 
            lfJson.captures = new CaptureJSONData[0];

            Debug.Log("real " + realFocal);

            LightField lightField = new LightField(lfJson, realRadius, realFocal);

            TestSRCapturePos(lightField.simulationToRealityMat, c1, new Vector3(realFocal.x, realFocal.y, realRadius + realFocal.z), descr + " z axis");
            TestSRCapturePos(lightField.simulationToRealityMat, c2, new Vector3(realFocal.x, realRadius + realFocal.y, realFocal.z), descr + " y axis");
            TestSRCapturePos(lightField.simulationToRealityMat, c3, new Vector3(realRadius + realFocal.x, realFocal.y, realFocal.z), descr + " x axis");


        }
    }
}
