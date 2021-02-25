using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;


public class ButtonScript : MonoBehaviour
{

    private GameObject _cube;
    private Quaternion _originalOrientation;
    private Vector3 _rotation = new Vector3(0, 0, 0);
    private const float _rotationSpeed = 30.0f;
    private MLInput.Controller _controller;


    // Start is called before the first frame update
    void Start()
    {
        _cube = GameObject.Find("InputButtonDemoCube");

        _originalOrientation = _cube.transform.rotation;

        //start MLInput API 
        MLInput.Start();
        //assign callback
        MLInput.OnControllerButtonDown += OnButtonDown;
        MLInput.OnControllerButtonUp += OnButtonUp;
        _controller = MLInput.GetController(MLInput.Hand.Left);
    }

    // Update is called once per frame
    void Update()
    {
        _cube.transform.Rotate(_rotation, _rotationSpeed * Time.deltaTime);
        CheckTrigger();
    }

    void OnButtonDown(byte controllerId, MLInput.Controller.Button button)
    {
        if (button == MLInput.Controller.Button.Bumper)
        {
            _rotation.y = 90;
        }
    }

    void OnButtonUp(byte controllerId, MLInput.Controller.Button button)
    {
        if (button == MLInput.Controller.Button.Bumper)
        {
            _rotation.y = 0;
        }
        if (button == MLInput.Controller.Button.HomeTap)
        {
            _cube.transform.rotation = _originalOrientation;
        }
    }

    //continuously checks the status of the trigger

    void CheckTrigger()
    {
        if (_controller.TriggerValue > 0.2f)
        {
            _rotation.x = 90;
        }
        else
        {
            _rotation.x = 0;
        }
    }



    void OnDestroy()
    {
        MLInput.OnControllerButtonDown -= OnButtonDown;
        MLInput.OnControllerButtonUp -= OnButtonUp;
        MLInput.Stop();
    }


}
