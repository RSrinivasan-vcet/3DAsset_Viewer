using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AssetBundleDownloader
{
    public Action<object, bool> LoadAssetBundleCompleted = null;    
    //Dictionary<string, object> dicAssetData;
    //Dictionary<string, object> dicSceneAssetData;

    public AssetBundleDownloader()
    {
        //dicAssetData = new Dictionary<string, object>();
        //dicSceneAssetData = new Dictionary<string, object>();
    }

    public void DownloadAndCache_AssetBundle(string assetURL, bool isScene = false)
    {
        Coroutiner.StartCoroutine(DownloadAndCacheAPI(assetURL, isScene));
    }
    IEnumerator DownloadAndCacheAPI(string assetURL, bool isScene = false)
    {
        string manifestURL = assetURL + ".manifest";

        Debug.Log("assetURL : " + assetURL);
        Debug.Log("manifestURL : " + manifestURL);

        // Wait for the Caching system to be ready
        while (!Caching.ready)
        {
            yield return null;
        }

        // if you want to always load from server, can clear cache first
        //        Caching.ClearCache();

        // get current bundle hash from server, random value added to avoid caching
        UnityWebRequest www = UnityWebRequest.Get(manifestURL);
        // wait for load to finish
        yield return www.SendWebRequest();

        // if received error, exit
        if (www.isNetworkError == true)
        {
            Debug.LogError("www error: " + www.error);
            www.Dispose();
            www = null;
            yield break;
        }
        Debug.Log("www.downloadHandler.text : " + www.downloadHandler.text);
        // create empty hash string
        Hash128 hashString = (default(Hash128));// new Hash128(0, 0, 0, 0);

        // check if received data contains 'ManifestFileVersion'
        if (www.downloadHandler.text.Contains("ManifestFileVersion"))
        {
            // extract hash string from the received data, TODO should add some error checking here
            var hashRow = www.downloadHandler.text.ToString().Split("\n".ToCharArray())[5];
            hashString = Hash128.Parse(hashRow.Split(':')[1].Trim());

            if (hashString.isValid == true)
            {
                // we can check if there is cached version or not
                if (Caching.IsVersionCached(manifestURL, hashString) == true)
                {
                    Debug.Log("Bundle with this hash is already cached!");
                }
                else
                {
                    Debug.Log("No cached version founded for this hash..");
                }
            }
            else
            {
                // invalid loaded hash, just try loading latest bundle
                Debug.LogError("Invalid hash:" + hashString);
                yield break;
            }

        }
        else
        {
            Debug.LogError("Manifest doesn't contain string 'ManifestFileVersion': " + manifestURL  /*+".manifest"*/);
            AssetViewer.DisableLoaderEvent?.Invoke();
            yield break;
        }

        // now download the actual bundle, with hashString parameter it uses cached version if available
        www = UnityWebRequestAssetBundle.GetAssetBundle(assetURL + "?r=" + (UnityEngine.Random.value * 9999999), hashString, 0);
        
        // wait for load to finish
        yield return www.SendWebRequest();
        
        if (www.error != null)
        {
            Debug.LogError("www error: " + www.error);
            www.Dispose();
            www = null;
            yield break;
        }

        // get bundle from downloadhandler
        AssetBundle bundle = ((DownloadHandlerAssetBundle)www.downloadHandler).assetBundle;

        if (isScene)
        {
            //if (dicSceneAssetData?.Count > 0)
            //    dicSceneAssetData.Clear();

            //for (int i = 0; i < bundle.GetAllScenePaths().Length; i++)
            //{
            //    string[] sKey = bundle.GetAllScenePaths()[i].Split('/');
            //    string sKeyVal = sKey[sKey.Length - 1];
            //    //Debug.Log(i + " ======== " + sKeyVal);

            //    if (dicSceneAssetData.ContainsKey(sKeyVal))
            //    {
            //        dicSceneAssetData[sKeyVal] = bundle.GetAllScenePaths()[i];
            //    }
            //    else
            //    {
            //        dicSceneAssetData.Add(sKeyVal, bundle.GetAllScenePaths()[i]);
            //    }
            //}
            LoadAssetBundleCompleted?.Invoke(bundle.GetAllScenePaths()[0], isScene);
        }
        else
        {
            //if (dicAssetData?.Count > 0)
            //    dicAssetData.Clear();
            //for (int i = 0; i < bundle.GetAllAssetNames().Length; i++)
            //{
            //    string[] sKey = bundle.GetAllAssetNames()[i].Split('/');
            //    string sKeyVal = sKey[sKey.Length - 1];
            //    //Debug.Log(i + " ======== " + sKeyVal);

            //    if (dicAssetData.ContainsKey(sKeyVal))
            //    {
            //        dicAssetData[sKeyVal] = bundle.LoadAsset(bundle.GetAllAssetNames()[i]);
            //    }
            //    else
            //    {
            //        dicAssetData.Add(sKeyVal, bundle.LoadAsset(bundle.GetAllAssetNames()[i]));
            //    }

            //}
            LoadAssetBundleCompleted?.Invoke(bundle.LoadAsset(bundle.GetAllAssetNames()[0]), isScene);
        }
        www.Dispose();
        www = null;
        // try to cleanup memory
        Resources.UnloadUnusedAssets();
        bundle.Unload(false);
        bundle = null;
    }
}
