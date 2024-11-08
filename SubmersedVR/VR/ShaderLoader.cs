using System.Collections.Generic;
using UnityEngine;

namespace Valve.VR
{
    public class ShaderLoader
    {
        private static bool initialized = false;
        private static readonly Dictionary<string, Shader> shaders = [];

        public static bool Initialize(string assetBundlePath)
        {
            return loadAllShadersFromAssetBundle(assetBundlePath);
        }

        public static Shader GetShader(string name)
        {
            Debug.Log("GetShader( " + name + " )");
            if (!initialized)
            {
                Debug.Log("GetShader called before Initializing.");
                return null;
            }
            if (!shaders.ContainsKey(name))
            {
                Debug.Log("shaders dictionary does not contain shader: " + name);
                return null;
            }
            return shaders[name];
        }

        private static bool loadAllShadersFromAssetBundle(string assetBundlePath)
        {
            AssetBundle loadedAssetBundle = AssetBundle.LoadFromFile(assetBundlePath);
            Debug.Log("loadAllShadersFromAssetBundle called.");
            foreach (Object a in loadedAssetBundle.LoadAllAssets())
            {
                Debug.Log($"{a.name}");
                shaders[a.name] = a as Shader;
            }
            initialized = true;
            return true;
        }

    }
}