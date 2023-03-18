using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EnhancedTextures
{
    public delegate void AssetBundleLoadedEventHandler();

    public static class AssetBundleLoadedEvent
    {
        public static event AssetBundleLoadedEventHandler AssetBundleLoaded;

        public static void OnAssetBundleLoaded()
        {
            AssetBundleLoadedEventHandler handler = AssetBundleLoaded;

            if (handler != null)
            {
                handler();
            }
        }
    }
}
