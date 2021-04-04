using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCoverageMapShader : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject camera;
    public GameObject light;
    void Start()
    {
        Vector3 c = camera.transform.position;
        Vector3 l = light.transform.position;

        gameObject.GetComponent<Renderer>().sharedMaterial.SetVector("camPos", new Vector4(c.x, c.y, c.z, 1));
        gameObject.GetComponent<Renderer>().sharedMaterial.SetVector("lightPos", new Vector4(l.x, l.y, l.z, 1));
        gameObject.GetComponent<Renderer>().sharedMaterial.SetFloat("beta1", 30);
        gameObject.GetComponent<Renderer>().sharedMaterial.SetFloat("beta2", 70);

    }

    // Update is called once per frame
    void Update()
    {

        Vector3 c = camera.transform.position;
        Vector3 l = light.transform.position;

        gameObject.GetComponent<Renderer>().sharedMaterial.SetVector("camPos", new Vector4(c.x, c.y, c.z, 1));
        gameObject.GetComponent<Renderer>().sharedMaterial.SetVector("lightPos", new Vector4(l.x, l.y, l.z, 1));
    }
}
