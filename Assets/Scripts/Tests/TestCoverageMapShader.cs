using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCoverageMapShader : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject camera;
    public GameObject light;

    public Material coverageMapMaterial1;
    public Material coverageMapMaterial2;

    public Material mapTextureMat;

    public GameObject defaultPlane;

    private GameObject textureMap;

    public float recWidth = 10;
    public float recHeight = 5;

    public int xSamples = 10;
    public int ySamples = 5;


    void Start()
    {
        Vector3 c = camera.transform.position;
        Vector3 l = light.transform.position;

        defaultPlane.GetComponent<Renderer>().sharedMaterial = coverageMapMaterial1;
        defaultPlane.GetComponent<Renderer>().sharedMaterial.SetMatrix("camNDCMat", camera.GetComponent<Camera>().projectionMatrix * camera.GetComponent<Camera>().worldToCameraMatrix);
        defaultPlane.GetComponent<Renderer>().sharedMaterial.SetVector("camPos", new Vector4(c.x, c.y, c.z, 1));
        defaultPlane.GetComponent<Renderer>().sharedMaterial.SetVector("lightPos", new Vector4(l.x, l.y, l.z, 1));
        defaultPlane.GetComponent<Renderer>().sharedMaterial.SetFloat("beta1", 10);
        defaultPlane.GetComponent<Renderer>().sharedMaterial.SetFloat("beta2", 20);
        defaultPlane.GetComponent<Renderer>().sharedMaterial.SetFloat("debug", 0);

        createTextureMap();

        textureMap.AddComponent<MeshRenderer>();
        textureMap.GetComponent<MeshRenderer>().enabled = true;
        textureMap.GetComponent<MeshRenderer>().material = coverageMapMaterial2;
        textureMap.GetComponent<MeshRenderer>().material.SetTexture("mapTexture", mapTextureMat.mainTexture);
        textureMap.GetComponent<MeshRenderer>().material.SetMatrix("camNDCMat", camera.GetComponent<Camera>().projectionMatrix * camera.GetComponent<Camera>().worldToCameraMatrix);
        textureMap.GetComponent<MeshRenderer>().material.SetVector("camPos", new Vector4(c.x, c.y, c.z, 1));
        textureMap.GetComponent<MeshRenderer>().material.SetVector("lightPos", new Vector4(l.x, l.y, l.z, 1));
        textureMap.GetComponent<MeshRenderer>().material.SetFloat("beta1", 10);
        textureMap.GetComponent<MeshRenderer>().material.SetFloat("beta2", 20);
        textureMap.GetComponent<MeshRenderer>().material.SetFloat("debug", 1);
    }

    // Update is called once per frame
    void Update()
    {

        Vector3 c = camera.transform.position;
        Vector3 l = light.transform.position;

        defaultPlane.GetComponent<Renderer>().sharedMaterial.SetMatrix("camNDCMat", camera.GetComponent<Camera>().projectionMatrix * camera.GetComponent<Camera>().worldToCameraMatrix);
        defaultPlane.GetComponent<Renderer>().sharedMaterial.SetVector("camPos", new Vector4(c.x, c.y, c.z, 1));
        defaultPlane.GetComponent<Renderer>().sharedMaterial.SetVector("lightPos", new Vector4(l.x, l.y, l.z, 1));

        var renderTexture = new RenderTexture(xSamples, ySamples, 16);

        RenderTexture activeRenderTexture = RenderTexture.active;
        var renderer = defaultPlane.GetComponent<MeshRenderer>();
        // var material = Instantiate(renderer.sharedMaterial);
        Graphics.Blit(mapTextureMat.mainTexture, renderTexture, renderer.sharedMaterial);

        RenderTexture.active = renderTexture;
        Texture2D frame = new Texture2D(renderTexture.width, renderTexture.height);
        frame.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0, false);
        frame.Apply();

        RenderTexture.active = activeRenderTexture;

        textureMap.GetComponent<MeshRenderer>().material.SetTexture("mapTexture", frame);
        textureMap.GetComponent<Renderer>().material.SetMatrix("camNDCMat", camera.GetComponent<Camera>().projectionMatrix * camera.GetComponent<Camera>().worldToCameraMatrix);
        textureMap.GetComponent<Renderer>().material.SetVector("camPos", new Vector4(c.x, c.y, c.z, 1));
        textureMap.GetComponent<Renderer>().material.SetVector("lightPos", new Vector4(l.x, l.y, l.z, 1));

    }

    void createTextureMap()
    {

        Vector3 topLeft = defaultPlane.transform.position;
        // topLeft.z += 10;
        topLeft.y += 1;

        textureMap = new GameObject("textureMap");
        textureMap.SetActive(false);



        MeshFilter meshFilter = textureMap.AddComponent<MeshFilter>();

        Mesh mesh = new Mesh();

        float dx = recWidth / (float)xSamples;
        float dy = recHeight / (float)ySamples;

        Vector3[] vertices = new Vector3[(ySamples + 1) * (xSamples + 1)];
        Vector3[] normals = new Vector3[(ySamples + 1) * (xSamples + 1)];
        Vector2[] uv = new Vector2[(ySamples + 1) * (xSamples + 1)];
        int[] tris = new int[(xSamples * ySamples * 2) * 3];
        int trisIdx = 0;

        int meshIdx = 0;

        for (float y = 0; y <= ySamples; y++)
        {
            for (float x = 0; x <= xSamples; x++)
            {

                Vector3 sampleVec = new Vector3(topLeft.x + x * dx, topLeft.y, topLeft.z + y * dy);
                // GameObject.Instantiate(ballPrefab, sampleVec, Quaternion.identity);
                vertices[meshIdx] = sampleVec;

                //mesh generation
                normals[meshIdx] = defaultPlane.transform.up;
                uv[meshIdx] = new Vector2(((x * dx) / recWidth), (y * dy) / recHeight);
                // uv[meshIdx] = new Vector2(0, 0);
                Debug.Log("MESH UVS " + uv[meshIdx]);

                if (x < xSamples && y < ySamples)
                {
                    tris[trisIdx] = meshIdx;
                    tris[trisIdx + 1] = meshIdx + xSamples + 1;
                    tris[trisIdx + 2] = meshIdx + 1;
                    trisIdx += 3;
                    tris[trisIdx] = meshIdx + 1;
                    tris[trisIdx + 1] = meshIdx + xSamples + 1;
                    tris[trisIdx + 2] = meshIdx + xSamples + 2;
                    trisIdx += 3;
                }
                meshIdx++;

            }



        }

        mesh.vertices = vertices;
        mesh.triangles = tris;
        mesh.normals = normals;
        mesh.uv = uv;

        meshFilter.mesh = mesh;

        textureMap.SetActive(true);


    }




}
