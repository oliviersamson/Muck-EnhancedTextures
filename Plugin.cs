using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Xml.Linq;
using Unity.Audio;
using UnityEngine;
using UnityEngine.UI;

namespace EnhancedTextures
{
    public static class Globals
    {
        public const string PLUGIN_GUID = "muck.mrboxxy.enhancedtextures";
        public const string PLUGIN_NAME = "EnhancedTextures";
        public const string PLUGIN_VERSION = "1.0.0";
    }

    [BepInPlugin(Globals.PLUGIN_GUID, Globals.PLUGIN_NAME, Globals.PLUGIN_VERSION)]
    [BepInDependency("Terrain.MuckSettings")]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource Log;

        public static Task BundleLoading = default;

        public Harmony harmony;

        public static AssetBundle Bundle;

        public static Transform Overlay;

        private void Awake()
        {
            Log = base.Logger;

            Bundle = GetAssetBundle("uiassets");
            Overlay = Bundle.LoadAsset<GameObject>("AssetLoad").GetComponent<Transform>();
            Bundle.Unload(false);

            AssetBundleLoadedEvent.AssetBundleLoaded += TextureReplacement.PatchTextures;

            PlayerPrefs.SetInt("loadNewTextures", 1);
            if (!PlayerPrefs.HasKey("useEnhancedTextures"))
            {
                PlayerPrefs.SetInt("useEnhancedTextures", 1);              
            }

            bool useEnhancedTextures = PlayerPrefs.GetInt("useEnhancedTextures") != 0;
            if (useEnhancedTextures)
            {
                BundleLoading = Task.Run(
                    () =>
                    {
                        Bundle = GetAssetBundle("enhancedtextures");
                        Plugin.Log.LogDebug("Enhanced assets loaded");
                    });
            }
            else
            {
                BundleLoading = Task.Run(
                    () =>
                    {
                        Bundle = GetAssetBundle("originaltextures");
                        Plugin.Log.LogDebug("Original assets loaded");
                    });
            }

            // Plugin startup logic
            harmony = new Harmony(Globals.PLUGIN_NAME);

            harmony.PatchAll(typeof(PrefixesAndPostfixes));
            Log.LogInfo("Patching TextureData.GenerateTextureArray()");
            Log.LogInfo("Patching GameManager.Start()");

            harmony.PatchAll(typeof(SettingsConfig));
            Log.LogInfo("Patching MuckSettings.Settings.Graphics()");

            harmony.PatchAll(typeof(LobbyVisualsPatch.PrefixesAndPostfixes));
            Log.LogInfo("Patching LobbyVisuals.ExitGame()");
        }

        public static AssetBundle GetAssetBundle(string name)
        {
            var execAssembly = Assembly.GetExecutingAssembly();

            var resourceName = execAssembly.GetManifestResourceNames().Single(str => str.EndsWith(name));

            using (var stream = execAssembly.GetManifestResourceStream(resourceName))
            {
                return AssetBundle.LoadFromStream(stream);
            }
        }
    }

    class PrefixesAndPostfixes
    {
        [HarmonyPatch(typeof(TextureData), "GenerateTextureArray")]
        [HarmonyPrefix]
        static bool GenerateTextureArrayPrefix(ref Texture2DArray __result, Texture2D[] textures)
        {
            bool useEnhancedTextures = PlayerPrefs.GetInt("useEnhancedTextures") != 0;

            if (useEnhancedTextures)
            {
                Texture2DArray texture2DArray = new Texture2DArray(textures[0].width, textures[0].height, textures.Length, TextureFormat.RGB565, true);
                for (int i = 0; i < textures.Length; i++)
                {
                    if (textures[i])
                    {
                        texture2DArray.SetPixels(textures[i].GetPixels(), i);
                    }
                }
                texture2DArray.Apply();
                __result = texture2DArray;
                return false;
            }

            return true;
        }

        [HarmonyPatch(typeof(GameManager), "Start")]
        [HarmonyPrefix]
        static void StartPrefix()
        {
            if (PlayerPrefs.GetInt("loadNewTextures") == 0)
            {
                return;
            }

            TextureReplacement.PatchTextures();
        }
    }
}
