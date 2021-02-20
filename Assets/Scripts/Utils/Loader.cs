using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Simulation.Utils
{

    public static class Loader
    {

        public static string LoadJsonText(string jsonPath)
        {
            using (StreamReader r = new StreamReader(jsonPath))
            {
                var json = r.ReadToEnd();
                return json;
            }
        }

        public static string PathFromSessionName(string sessionName)
        {
            return "Assets/LightFieldOutput/" + sessionName + "/";
        }

        public static Texture2D TextureFromPNG(string pngPath)
        {
            Texture2D tex = new Texture2D(2, 2);
            byte[] fileData = File.ReadAllBytes(pngPath);
            tex.LoadImage(fileData);
            return tex;
        }
    }


}