using System;
using System.Collections.Generic;
using UnityEngine;
using MagicLeap;

namespace CaptureSystem
{
    public class CaptureViewController : MonoBehaviour
    {

        private CaptureViewCollection _collection;

        public event Action OnCaptureCreated = null;

        private int nextIDNum;
        public CaptureViewCollection collection
        {
            get { return _collection; }
        }

        void Awake()
        {
            _collection = new CaptureViewCollection();
        }
        // Start is called before the first frame update
        void Start()
        {
            nextIDNum = 0;
        }

        // Update is called once per frame
        void Update()
        {

        }

        /// <summary> 
        /// Creates a capture view object and adds it to the capture collection 
        /// </summary> 
        public void CreateCaptureView(Texture2D texture, Transform camTransform, Vector3 lightPos)
        {
            float thetaS = 0; //TODO: ACTUALLY CALCULATE THETA S 

            Capture newCapture = new Capture("" + nextIDNum, texture, thetaS, camTransform, lightPos);
            nextIDNum++;
            _collection.captures.Add(newCapture);
            Debug.Log("Capture added to collection");
            OnCaptureCreated?.Invoke();
        }
    }

}
