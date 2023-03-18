using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EnhancedTextures.LobbyVisualsPatch
{
    public class PrefixesAndPostfixes
    {
        [HarmonyPatch(typeof(LobbyVisuals), "ExitGame")]
        [HarmonyPrefix]
        static void ExitGamePrefix()
        {
            // Avoid game crashing on exit
            Plugin.BundleLoading.Wait();
        }
    }
}
