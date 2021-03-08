using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CaptureSystem
{

    public class CaptureViewCollection
    {
        // Start is called before the first frame update
        public List<Capture> captures;

        public CaptureViewCollection()
        {
            captures = new List<Capture>();
        }

    }

}