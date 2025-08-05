using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace EnderPearl
{
    public class CustomAtlases
    {

        public FAtlas LoadAtlases(string imageName)
        {
            Texture2D? texture = this.LoadTexture(imageName);
            return Futile.atlasManager.LoadAtlasFromTexture(imageName, texture, false);
        }


        public Texture2D? LoadTexture(string imageName)
        {
            this.pluginPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            this.assetsPath = Path.Combine(this.pluginPath, "../assets");
            string text = Path.Combine(Application.streamingAssetsPath, this.assetsPath, imageName + ".png");
            bool flag = File.Exists(text);
            if (flag)
            {
                byte[] data = File.ReadAllBytes(text);
                Texture2D texture2D = new Texture2D(2, 2);
                bool flag2 = texture2D.LoadImage(data);
                if (flag2)
                {
                    texture2D.filterMode = FilterMode.Point;
                    return texture2D;
                }
                Debug.LogError("Failed to load texture from file: " + text);
            }
            else
            {
                Debug.LogError("File not found at path: " + text);
            }
            return null;
        }


        private string? pluginPath;


        private string? assetsPath;
    }
}
