using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace EnhancedTextures
{
    public static class TextureReplacement
    {
        public static void PatchTextures()
        {
            while (!Plugin.BundleLoading.IsCompleted)
            {
                Thread.Sleep(10);
            }

            bool useEnhancedTextures = PlayerPrefs.GetInt("useEnhancedTextures") != 0;        

            // Patch ground texture
            if (useEnhancedTextures)
            {
                Texture2D.Destroy(MapGenerator.Instance.textureData.layers[0].texture);
                MapGenerator.Instance.textureData.layers[0].texture = Plugin.Bundle.LoadAsset<Texture2D>("Grass_up");
                Texture2D.Destroy(MapGenerator.Instance.textureData.layers[2].texture);
                MapGenerator.Instance.textureData.layers[2].texture = Plugin.Bundle.LoadAsset<Texture2D>("Stony ground_up");
            }
            else
            {
                Texture2D.Destroy(MapGenerator.Instance.textureData.layers[0].texture);
                MapGenerator.Instance.textureData.layers[0].texture = Plugin.Bundle.LoadAsset<Texture2D>("Grass");
                Texture2D.Destroy(MapGenerator.Instance.textureData.layers[2].texture);
                MapGenerator.Instance.textureData.layers[2].texture = Plugin.Bundle.LoadAsset<Texture2D>("Stony ground");
            }

            MapGenerator.Instance.textureData.ApplyToMaterial(MapGenerator.Instance.terrainMaterial);

            // Patch objects texture
            foreach (Material mat in Resources.FindObjectsOfTypeAll(typeof(Material)) as Material[])
            {
                string matName = mat.name.Replace(" (Instance)", "");
                if (TexturesToPatch.Textures.TryGetValue(matName, out Dictionary<string, string> maps))
                {
                    foreach (KeyValuePair<string, string> map in maps)
                    {
                        string assetName = map.Value;
                        if (useEnhancedTextures)
                        {
                            assetName += "_up";
                        }

                        //Texture2D.Destroy(mat.GetTexture(map.Key));
                        mat.SetTexture(map.Key, Plugin.Bundle.LoadAsset<Texture2D>(assetName));
                    }
                }
            }

            PlayerPrefs.SetInt("loadNewTextures", 0);
        }
    }
}
