using System.IO;
using UnityEditor;
using UnityEngine;

namespace AssetBundles
{
    public class CreateAssetBundles
    {
        [MenuItem("Assets/Build AssetBundles")]
        static void BuildAssetBundlesAll()
        {
            //Debug.LogWarning("Building AssetBundles for target: "+ EditorUserBuildSettings.activeBuildTarget);
            string assetBundleDirectory = "_Bundles/AssetBundles/" + AssetBundlesUtil.GetPlatformName();
            if (!Directory.Exists(assetBundleDirectory))
            {
                Directory.CreateDirectory(assetBundleDirectory);
            }

            BuildPipeline.BuildAssetBundles(assetBundleDirectory, BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);
            Debug.LogWarning("Assetbundles built to dir: " + assetBundleDirectory);
        }       
    }
}