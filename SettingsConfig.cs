using BepInEx.Configuration;
using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace EnhancedTextures
{
    public static class SettingsConfig
    {
        [HarmonyPatch(typeof(MuckSettings.Settings), "Graphics")]
        [HarmonyPrefix]
        static void Graphics(MuckSettings.Settings.Page page)
        {
            bool defaultValue = true;
            if (PlayerPrefs.HasKey("useEnhancedTextures"))
            {
                defaultValue = PlayerPrefs.GetInt("useEnhancedTextures") != 0;
            }

            page.AddBoolSetting("Use Enhanced Textures", defaultValue, 
                async (value) => {

                    GameObject settings = GameObject.Find("Settings");
                    EscapeUI escapeUI = settings.GetComponent<EscapeUI>();
                    escapeUI.enabled = false;

                    if (SceneManager.GetActiveScene().name == "GameAfterLobby")
                    {
                        OtherInput.Instance.enabled = false;
                    }

                    Transform obj = Transform.Instantiate(Plugin.Overlay, Plugin.Overlay.position, Plugin.Overlay.rotation);

                    while (!Plugin.BundleLoading.IsCompleted)
                    {
                        await Task.Delay(10);
                    }

                    Plugin.Bundle.Unload(false);

                    PlayerPrefs.SetInt("useEnhancedTextures", value ? 1 : 0);
                    PlayerPrefs.SetInt("loadNewTextures", 1);

                    if (value)
                    {
                        Plugin.BundleLoading = Task.Run(
                            () =>
                            {
                                Plugin.Bundle = Plugin.GetAssetBundle("enhancedtextures");
                                Plugin.Log.LogDebug("Enhanced assets loaded");
                            });
                    }
                    else
                    {
                        Plugin.BundleLoading = Task.Run(
                            () =>
                            {
                                Plugin.Bundle = Plugin.GetAssetBundle("originaltextures");
                                Plugin.Log.LogDebug("Original assets loaded");
                            });
                    }

                    await Plugin.BundleLoading;

                    if (SceneManager.GetActiveScene().name == "GameAfterLobby")
                    {                        
                        AssetBundleLoadedEvent.OnAssetBundleLoaded();
                        OtherInput.Instance.enabled = true;
                    }

                    escapeUI.enabled = true;
                    
                    GameObject.Destroy(obj.gameObject);
                });
        }
    }
}
