using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UniRx.Async;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

[Serializable]
public class ServerBundleInfo
{
    public string Name;
    public string Hash = "0";
}

[Serializable]
public class ServerBundles
{
    public List<ServerBundleInfo> BundlesInfo = new List<ServerBundleInfo>();
}

public class BundlesManager : MonoBehaviour, IProgress<float>
{
    public string assetsRootFolder = "Assets/Bundles";
    public string localBundlesSubfolder = "Bundles";
    [FormerlySerializedAs("bundlesRemoteURL")] public string bundlesRemoteURLFormat = "Bundles";
    //public string CacheSubfolder = "BundlesCache";
    public bool useBundlesInEditor = false;
    public static Action<string, float> LoadingStatusEvent;
    
    static BundlesManager _instance = null;
    Dictionary<string, AssetBundle> _loadedBundles = new Dictionary<string, AssetBundle>();
    ServerBundles ServerBundlesInfo = null;
    
    
   

    public static string AssetsRootFolder
    {
        get
        {
            if (BundlesManager.Instance != null) return BundlesManager.Instance.assetsRootFolder;
            return "Assets/Bundles";
        }
        
    }


    public static BundlesManager Instance => _instance;

    private void Awake()
    {
        _instance = this;
        GetServerBundlesInfo().Forget();
    }
    
    public T Load<T>(string path, string bundleName) where T: Object
    {
        var fullPath = FullAssetPath(path) ;

#if UNITY_EDITOR
        if (Application.isEditor && !useBundlesInEditor)
        {
            var res = AssetDatabase.LoadAssetAtPath<T>(fullPath);
            return res;
        }
#endif
        
        if (_loadedBundles.TryGetValue(bundleName, out var bundle))
        {
            return bundle.LoadAsset<T>(fullPath);
        }

        Debug.LogError($"Can't find bundle {bundleName}");
        return null;
    }
   

    public async UniTask<T> LoadAsync<T>(string path, IProgress<float> progress = null) where T : Object
    {
        return  await LoadAsync<T>(path, null, progress);
    }

    public async UniTask<T>  LoadAsync<T>(string path, string bundleName, IProgress<float> progress = null) where T: Object
    {
        string ext = ".prefab";
        var pathExt = Path.GetExtension(path);

        var fullPath = FullAssetPath(path) ;
        
        if (string.IsNullOrEmpty(pathExt)) 
            fullPath += ext;

#if UNITY_EDITOR

        if (Application.isEditor && !useBundlesInEditor)
        {
            var res = AssetDatabase.LoadAssetAtPath<T>(fullPath);
            return res;
        }
#endif

        var bundlePath = bundleName;
        
        if(string.IsNullOrEmpty(bundleName))
             bundlePath = BundleName(path);
        
        AssetBundle bundle = null;
            
        if (!_loadedBundles.ContainsKey(bundlePath))
        {
            await LoadBundle(bundlePath, progress);
        }

        if (_loadedBundles.TryGetValue(bundlePath, out bundle))
        {
            var loadReq = bundle.LoadAssetAsync(fullPath);
            await loadReq.ConfigureAwait(progress);

            var asset = loadReq.asset;
            if(asset == null) Debug.LogError($"Can't find asset {fullPath} in {bundlePath}");
            
            return (T)asset;
        }

        Debug.LogError($"There is no {bundlePath} bundle loaded");
        return null;
    }
    
    public  async UniTask LoadSceneAsync(string path, IProgress<float> progress = null, LoadSceneMode mode = LoadSceneMode.Single)
    {

#if UNITY_EDITOR

        if (Application.isEditor && !useBundlesInEditor)
        {
            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>($"{AssetsRootFolder}/Scenes/{path}/{path}.scene");
            await SceneManager.LoadSceneAsync(path, mode).ConfigureAwait(progress);
            return;
        }
#endif

        var bundlePath = BundleName(path, true);
        AssetBundle bundle = null;
            
        if (!_loadedBundles.ContainsKey(bundlePath))
        {
            await LoadBundle(bundlePath, progress);
        }

        if (_loadedBundles.TryGetValue(bundlePath, out bundle))
        {
            if (bundle.isStreamedSceneAssetBundle)
            { 
                await SceneManager.LoadSceneAsync(path, mode).ConfigureAwait(progress); //Bundle already loaded
            }
            else
            {
                Debug.LogError($"{bundlePath} is't streamed scene bundle ");
            }
        }
        else
        {
            Debug.LogError($"There is no {bundlePath} bundle loaded");
        }
    }

    public async UniTask LoadBundle(string bundleName, IProgress<float> progress = null)
    {
        await DownloadBundle(bundleName, progress);
    }

    public async UniTask LoadBundleForResource(string path, bool isScene = false, IProgress<float> progress = null)
    {
        await LoadBundle(BundleName(path,isScene), progress);
    }

    string FullAssetPath(string path)
    {
        return $"{assetsRootFolder}/{path}";
    }

    string LocalBundlePath(string bundleName)
    {
        if(string.IsNullOrEmpty(localBundlesSubfolder)) 
            return $"{Application.streamingAssetsPath}/{bundleName}";
        
        return $"{Application.streamingAssetsPath}/{localBundlesSubfolder}/{bundleName}";
    }
    
    string RemoteBundlePath(string bundleName)
    {
        return $"{string.Format(bundlesRemoteURLFormat, Application.platform)}{bundleName}";
    }

    public void UnloadBundle(string bundleName, bool all = true)
    {
        if(_loadedBundles.ContainsKey(bundleName))
            _loadedBundles[bundleName]?.Unload(all);
    }

    public static string BundleName(string resourcePath, bool isScene = false)
    {
        if(isScene) return  $"scene_{resourcePath}".ToLowerInvariant();
        
        var dir = Path.GetDirectoryName(resourcePath);

        return dir.Trim('\\').Replace('\\', '/').Replace('/', '_').ToLowerInvariant();
    }

    public Hash128 GetRemoteBundleHash(string bundleName)
    {
        var retVal = new Hash128();

        if (ServerBundlesInfo != null && ServerBundlesInfo.BundlesInfo != null)
        {
            var bundleInfo = ServerBundlesInfo.BundlesInfo.Find(i => i.Name == bundleName);
            if (bundleInfo != null)
                retVal = Hash128.Parse(bundleInfo.Hash);
        }

        return retVal;
    }
    
    public bool IsBundlesCached(params string[] bundleNames)
    {
        if(Application.isEditor && !useBundlesInEditor) return true;

        foreach (var bundleName in bundleNames)
        {
            if (_loadedBundles.ContainsKey(bundleName)) continue;
            
            if (!Caching.IsVersionCached(RemoteBundlePath(bundleName), GetRemoteBundleHash(bundleName)))
                return false;
        }

        Debug.Log($"IsBundlesCached -> All cached");
        return true;
    }

    public async UniTask DownloadBundle(string bundleName, IProgress<float> progress = null)
    {
        Debug.Log($"Try DownloadBundle {bundleName}");
        
        if(Application.isEditor && !useBundlesInEditor) return;
        if(_loadedBundles.ContainsKey(bundleName)) return;
        
        var remotePath = RemoteBundlePath(bundleName);
        var hash = GetRemoteBundleHash(bundleName);

        await LoadBundleRequest(bundleName, remotePath, hash, progress);
    }

    public async UniTask DownloadBundles(params string[] bundleNames )
    {
        _currentBundleIdx = 0;
        _chunkSize = 1f / bundleNames.Length;

        for (int i = 0; i < bundleNames.Length; i++)
        {
            var bundleName = bundleNames[i];
            _loadingStatus = $"{i+1}/{bundleNames.Length} {bundleName}";

            //if(CheckIsBundleCached(bundleName)) continue;
            
            await DownloadBundle(bundleName, this);
            _currentBundleIdx = i;
        }
    }

    public async UniTask LoadBundleRequest(string bundleName, string remotePath,  Hash128 hash, IProgress<float> progress = null)
    {
        Debug.Log($"Try DownloadBundle {bundleName} from {remotePath}");
        using (var request = UnityWebRequestAssetBundle.GetAssetBundle(remotePath, hash, 0))
        {
            await request.SendWebRequest().ConfigureAwait(progress);

            if (!request.isHttpError && !request.isNetworkError)
            {
                var content = DownloadHandlerAssetBundle.GetContent(request);
                if (content != null)
                {
                    _loadedBundles[bundleName] = content;
                }
                else
                {
                    Debug.LogError($"{bundleName} Already loaded");
                }

                if (request.responseCode == 200) Debug.Log($"Downloaded bundle {bundleName} from {remotePath}");
                if (request.responseCode == 0) Debug.Log($"Downloaded bundle {bundleName} from cache ");
            }
            else
            {
                var msg = $"Can't download asset bundle {bundleName} from {remotePath}-> {request.error}";
                Debug.LogError(msg);
                throw  new IOException(msg);
            }
        }
    }

    public void ClearCache()
    {
        Caching.ClearCache();
    }

    public async UniTaskVoid GetServerBundlesInfo()
    {
        if(Application.isEditor) return;
        
        var manifestUrl = RemoteBundlePath("manifest.json");

        using (var requiest = UnityWebRequest.Get(manifestUrl))
        {
            await requiest.SendWebRequest();

            if (requiest.isHttpError || requiest.isNetworkError)
            {
                Debug.LogError($"Can't load ServerBundlesInfo from {manifestUrl} -> {requiest.error}");
                return;
            }

            var json = requiest.downloadHandler.text;
            try
            {
                ServerBundlesInfo = JsonUtility.FromJson<ServerBundles>(json);
                Debug.Log($"Loaded ServerBundlesInfo from {manifestUrl} -> {json}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Can't parse ServerBundlesInfo from {manifestUrl} -> {json}");
            }
        }

    }
 
   

#if UNITY_EDITOR
    [MenuItem("Assets/Bundles/Set name",false, 60)]
    static void SetAssetBundlesFromFileNames()
    {
       
        if (Selection.assetGUIDs.Length > 0) 
        {
            foreach (Object asset in Selection.objects) 
            {
                string path = AssetDatabase.GetAssetPath (asset);
                AssetImporter assetImporter = AssetImporter.GetAtPath (path);
                var newPath = path.Replace(AssetsRootFolder, "") +"/";
                assetImporter.SetAssetBundleNameAndVariant(BundleName(newPath), ""); 
            }
        }
        else 
        {
            Debug.Log ("No Assets Selected");
        }
    }
    
    [MenuItem("Assets/Bundles/Set scene name",false, 60)]
    static void SetAssetBundlesSceneFromFileNames()
    {
       
        if (Selection.assetGUIDs.Length > 0) 
        {
            foreach (Object asset in Selection.objects) 
            {
                string path = AssetDatabase.GetAssetPath (asset);
                AssetImporter assetImporter = AssetImporter.GetAtPath (path);
                var newPath = path.Replace(AssetsRootFolder +"/Scenes", "");
                assetImporter.SetAssetBundleNameAndVariant("scene_" + BundleName(newPath), ""); 
            }
        }
        else 
        {
            Debug.Log ("No Assets Selected");
        }
    }
#endif

    #region Loading progress
    private float _currentProgress = 0f;
    private string _loadingStatus = "";
    int _currentBundleIdx = 0;
    float _chunkSize = 1f;
    
    public void Report(float value)
    {
        _currentProgress = (_currentBundleIdx + value) * _chunkSize;
        LoadingStatusEvent?.Invoke(_loadingStatus, _currentProgress);
    }
    #endregion
}
