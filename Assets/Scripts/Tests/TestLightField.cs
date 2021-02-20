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
            TestAxisAlignedSRTransform(20, 15, new Vector3(-1, -1, -1), new Vector3(1, 0, 0), ">>>>Test Axis Aligned SR Transform - Sim origin at -1,-1,-1 - Real Origin at 1,0,0 - real radius 0.75x");
            TestAxisAlignedSRTransform(20, 15, new Vector3(-1, -1, -1), new Vector3(-1, 0, 0), ">>>>Test Axis Aligned SR Transform - Sim origin at -1,-1,-1 - Real Origin at -1,0,0 - real radius 0.75x");

            //radius less than
            TestAxisAlignedSRTransform(20, 15, new Vector3(10, 10, 10), new Vector3(0, 0, 0), ">>>>Test Axis Aligned SR Transform - Real Origin at 0,0,0 - real radius 0.75x");
            TestAxisAlignedSRTransform(20, 15, new Vector3(10, 10, 10), new Vector3(5, 5, 5), ">>>>Test Axis Aligned SR Transform - Real Origin at 5,5,5 - real radius 0.75x");

            //Along Unit Vector Diagonal
            TestUnitVectorAlignedSRTransform(20, 20, new Vector3(10, 10, 10), new Vector3(0, 0, 0), ">>>>Unit Vector Diagonal - Real Origin at 0,0,0");
            TestUnitVectorAlignedSRTransform(20, 20, new Vector3(10, 10, 10), new Vector3(5, 5, 5), ">>>>Unit Vector Diagonal - Real Origin at 5,5,5");
            TestUnitVectorAlignedSRTransform(20f, 40f, new Vector3(10, 10, 10), new Vector3(0f, 0f, 0f), ">>>>Unit Vector Diagonal - Real Origin at 1,0,0 - real radius 2x");
            TestUnitVectorAlignedSRTransform(20, 40, new Vector3(10, 10, 10), new Vector3(-1, 0, 0), ">>>>Unit Vector Diagonal - Real Origin at -1,0,0 - real radius 2x");

        }

        private void TestSRCapturePos(Vector3 simPos, Vector3 realPos, string descr)
        {
            bool eq = simPos == realPos;
            Debug.Assert(eq, simPos + "");
            if (eq) { Debug.Log("TEST " + descr + " PASSED"); }
            else { Debug.Log("FAILED " + descr + ", outVec " + simPos + " does not equal " + realPos); }
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

            LightField lightField = new LightField(lfJson, realRadius, realFocal);

            TestSRCapturePos(lightField.TransformSimToReal(c1), new Vector3(realFocal.x, realFocal.y, realRadius + realFocal.z), descr + " z axis");
            TestSRCapturePos(lightField.TransformSimToReal(c2), new Vector3(realFocal.x, realRadius + realFocal.y, realFocal.z), descr + " y axis");
            TestSRCapturePos(lightField.TransformSimToReal(c3), new Vector3(realRadius + realFocal.x, realFocal.y, realFocal.z), descr + " x axis");


        }

        private void TestUnitVectorAlignedSRTransform(float simRadius, float realRadius, Vector3 simFocal, Vector3 realFocal, string descr)
        {

            float simDiagonal = Mathf.Sqrt(simRadius * simRadius) / 3;
            float realDiagonal = Mathf.Sqrt(realRadius * realRadius) / 3;

            Vector3 c1 = new Vector3(simFocal.x + simDiagonal, simFocal.y + simDiagonal, simFocal.z + simDiagonal);

            LightFieldJsonData lfJson = new LightFieldJsonData();
            lfJson.sessionName = "test2";
            lfJson.focalPoint = simFocal;
            lfJson.sphereRadius = simRadius;

            //don't actually load just check matrix 
            lfJson.captures = new CaptureJSONData[0];

            LightField lightField = new LightField(lfJson, realRadius, realFocal);

            TestSRCapturePos(lightField.TransformSimToReal(c1), new Vector3(realFocal.x + realDiagonal, realFocal.y + realDiagonal, realFocal.z + realDiagonal), descr);

        }
    }
}
