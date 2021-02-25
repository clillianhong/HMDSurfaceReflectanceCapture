using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Simulation
{
    public class CameraController : MonoBehaviour
    {
        // Start is called before the first frame update
        [SerializeField]
        public Transform focus;

        [SerializeField, Range(1f, 20f)]
        float distance = 5f;
        public float speed;
        public OrbitViewManager.CameraType type;


        OrbitViewManager viewManager;

        void Start()
        {
            viewManager = GameObject.Find("OrbitViewManager").GetComponent<OrbitViewManager>();

        }

        // Update is called once per frame
        void Update()
        {

            if (viewManager.Controlling() == type)
            {
                Vector3 worldX = transform.TransformDirection(Vector3.right);
                Vector3 worldY = transform.TransformDirection(Vector3.up);
                transform.RotateAround(focus.position, worldY, -Input.GetAxis("Horizontal") * speed * Time.deltaTime);
                transform.RotateAround(focus.position, worldX, Input.GetAxis("Vertical") * speed * Time.deltaTime);
                transform.LookAt(focus);
            }
        }




    }

}
