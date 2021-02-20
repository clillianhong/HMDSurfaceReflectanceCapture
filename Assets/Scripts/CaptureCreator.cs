using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using Simulation.Utils;


namespace Simulation
{

public static class CaptureCreator 
{
    public static void CreateCaptureGameObject(Transform trans, Texture2D texOG, int width, int height, string objectName){
        GameObject captureObj = new GameObject(objectName);
        TextMesh textMesh = captureObj.AddComponent<TextMesh>();
        LineRenderer upAxisLine = new GameObject().AddComponent<LineRenderer>();
        LineRenderer forwardAxisLine = new GameObject().AddComponent<LineRenderer>();
        LineRenderer rightAxisLine = new GameObject().AddComponent<LineRenderer>();
        Texture2D tex = new Texture2D(texOG.width, texOG.height);
        Graphics.CopyTexture(texOG, tex);
        TextureScale.Point(tex, 10*width, 10*height);
        GameObject quad = CreateQuad(trans.position + new Vector3(0.1f,0.1f,0.1f), -trans.forward, width, height, objectName, tex);

        DrawLine(upAxisLine, trans.position, trans.position + trans.up.normalized * 2, Color.green);
        DrawLine(forwardAxisLine, trans.position, trans.position + trans.forward.normalized * 2, Color.blue);
        DrawLine(rightAxisLine, trans.position, trans.position + trans.right.normalized * 2, Color.red);

        textMesh.fontSize = 10;
        textMesh.text = objectName;
        textMesh.color = Color.black;
        captureObj.transform.position = trans.position;
    }

    public static GameObject CreateQuad(Vector3 blCorner, Vector3 normal, float width, float height, string name, Texture2D tex){
       
        GameObject gameObject = new GameObject(name);
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = new Material(Shader.Find("Standard"));
        meshRenderer.sharedMaterial.mainTexture = tex;

        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();

        Mesh mesh = new Mesh();

        int pixelWidth = tex.width;
        int pixelHeight = tex.height; 
        float dx = width/pixelWidth;
        float dy = height/pixelHeight;

        Vector3[] vertices = new Vector3[(pixelHeight+1) * (pixelWidth+1)];
        Vector3[] normals = new Vector3[(pixelHeight+1) * (pixelWidth+1)];
        Vector2[] uv = new Vector2[(pixelHeight+1) * (pixelWidth+1)];
        int[] tris = new int[(pixelWidth * pixelHeight * 2)*3]; 
        int trisIdx = 0;
        int idx = 0;

        for(float y = 0; y <= pixelHeight; y++){
            for (float x = 0; x <= pixelWidth; x++){
                vertices[idx] = new Vector3(blCorner.x + dx * x, blCorner.y + dy*y, blCorner.z);
                normals[idx]  = normal;
                uv[idx] = new Vector2(dx * x / width, dy*y / height);
                
                if (x < pixelWidth && y < pixelHeight){
                    tris[trisIdx] = idx;
                    tris[trisIdx+1] =idx + pixelWidth + 1;
                    tris[trisIdx+2] =  idx+1;
                    trisIdx += 3;
                    tris[trisIdx] = idx+1;
                    tris[trisIdx+1] = idx + pixelWidth + 1;
                    tris[trisIdx+2] = idx + pixelWidth+2;
                    trisIdx += 3;
                }
                idx++;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = tris;
        mesh.normals = normals;
        mesh.uv = uv;

        meshFilter.mesh = mesh;
        return gameObject;
    }

    public static void DrawLine(LineRenderer lr, Vector3 start, Vector3 end, Color color)
         {
             lr.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
             lr.sharedMaterial.SetColor("_Color", color);
             lr.startColor = color;
             lr.endColor = color;
             lr.SetWidth(0.1f, 0.1f);
             lr.SetPosition(0, start);
             lr.SetPosition(1, end);
         }

}

}


