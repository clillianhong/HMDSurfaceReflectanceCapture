using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CaptureSystem
{
    public class UserInterfaceController : MonoBehaviour
    {
        public GameObject captureViewThumbnailPrefab;


        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public GameObject CreateCapturePreviewObject(Capture captureView)
        {
            GameObject obj = GameObject.Instantiate(captureViewThumbnailPrefab, captureView.cameraPose.position, captureView.cameraPose.rotation);
            obj.GetComponent<MeshRenderer>().material.mainTexture = captureView.texture;
            return obj;
        }


    }

}
