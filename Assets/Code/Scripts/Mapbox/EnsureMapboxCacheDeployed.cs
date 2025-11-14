using System.IO;
using UnityEngine;
using UnityEngine.Networking;

// On first run, copy a pre-populated cache.db from StreamingAssets to the Mapbox cache folder
public class EnsureMapboxCacheDeployed : MonoBehaviour
{
    // Mapbox's SQLite cache folder is Application.persistentDataPath + "/cache"
    private string CacheFolder => Path.Combine(Application.persistentDataPath, "cache");
    private string CacheDbPath => Path.Combine(CacheFolder, "cache.db");

    // Put your prebuilt cache at: Assets/StreamingAssets/mapbox-cache.db
    private string SourceStreamingAssetPath =>
#if UNITY_ANDROID && !UNITY_EDITOR
        Path.Combine(Application.streamingAssetsPath, "mapbox-cache.db");
#else
        Path.Combine(Application.streamingAssetsPath, "mapbox-cache.db");
#endif

    private void Awake()
    {

        if (!File.Exists(CacheDbPath))
        {
            Directory.CreateDirectory(CacheFolder);
#if UNITY_ANDROID && !UNITY_EDITOR
            // Android needs UnityWebRequest for StreamingAssets inside the APK
            StartCoroutine(CopyFromStreamingAssetsAndroid());
#else
            if (File.Exists(SourceStreamingAssetPath))
            {
                File.Copy(SourceStreamingAssetPath, CacheDbPath, overwrite: false);
                Debug.Log($"Deployed Mapbox cache to {CacheDbPath}");
            }
            else
            {
                Debug.Log("No prebuilt Mapbox cache found in StreamingAssets. Proceeding without.");
            }
#endif
        }
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private System.Collections.IEnumerator CopyFromStreamingAssetsAndroid()
    {
        using (UnityWebRequest req = UnityWebRequest.Get(SourceStreamingAssetPath))
        {
            yield return req.SendWebRequest();
#if UNITY_2020_2_OR_NEWER
            if (req.result != UnityWebRequest.Result.Success)
#else
            if (req.isNetworkError || req.isHttpError)
#endif
            {
                Debug.LogWarning($"Failed to load prebuilt cache from StreamingAssets: {req.error}");
                yield break;
            }
            File.WriteAllBytes(CacheDbPath, req.downloadHandler.data);
            Debug.Log($"Deployed Mapbox cache to {CacheDbPath}");
        }
    }
#endif
}