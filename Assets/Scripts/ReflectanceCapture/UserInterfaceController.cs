using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CaptureSystem
{
    public class UserInterfaceController : MonoBehaviour
    {
        public GameObject captureViewThumbnailPrefab;

        [SerializeField, Tooltip("Object with the image of the closest capture to the camera.")]
        private GameObject _closestObject = null;


        void Awake()
        {
            if (_closestObject == null)
            {
                Debug.LogError("Error: CameraController._closestObject is not set, disabling script.");
                enabled = false;
                return;
            }

            // This is made active when we have a captured image to show.
            _closestObject.SetActive(false);
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void UpdateClosestPreview(Capture nearestCapture)
        {

            Renderer renderer = _closestObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.mainTexture = nearestCapture.texture;
            }
        }

        public GameObject CreateCapturePreviewObject(Capture captureView)
        {
            GameObject obj = GameObject.Instantiate(captureViewThumbnailPrefab, captureView.cameraPose.position, captureView.cameraPose.rotation);

            Texture2D previewTex = new Texture2D(8, 8);
            byte[] imgData = captureView.texture.GetRawTextureData();
            var success = previewTex.LoadImage(imgData);
            if (success)
            {
                obj.GetComponent<MeshRenderer>().material.mainTexture = captureView.texture;

                //upon first capture 
                if (!_closestObject.activeInHierarchy)
                {
                    _closestObject.SetActive(true);
                    UpdateClosestPreview(captureView);
                }
                else
                {
                    Debug.Log("Failed to load preview image object texture.");
                }

            }


            return obj;
        }

        public GameObject CreateImagePreviewObject(Vector3 position, Quaternion rotation, Texture2D image)
        {
            GameObject obj = GameObject.Instantiate(captureViewThumbnailPrefab, position, rotation);

            Texture2D previewTex = new Texture2D(8, 8);
            byte[] imgData = image.GetRawTextureData();
            var success = previewTex.LoadImage(imgData);
            if (success)
            {
                obj.GetComponent<MeshRenderer>().material.mainTexture = image;
            }
            Debug.Log("successfully created image preview object");
            return obj;
        }


    }

}
