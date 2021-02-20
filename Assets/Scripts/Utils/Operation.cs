using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Simulation.Utils
{

    public static class OP
    {
        public static Vector3 MultPoint(Matrix4x4 matrix, Vector3 pt)
        {
            Vector4 res = matrix * new Vector4(pt.x, pt.y, pt.z, 1);
            return new Vector3(res.x, res.y, res.z);
        }
    }

}
