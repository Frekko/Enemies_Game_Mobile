using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS
using UniRx.Async;
using UnityEngine;

#endif

public static class GameConfigsProvider
{
    static Dictionary<string, GameConfig> _configs = new Dictionary<string, GameConfig>();
    static List<string> _wildCards = new List<string>();
    public static bool verboseLog = true;


    public static T GetConfig<T>(string id) where T : GameConfig
    {
        var isWildcard = GetKeyWithWildcard(id, out var key);
        if (_configs.TryGetValue(key, out var cfg))
        {
            if (cfg is T result)
            {
                if(isWildcard) result.Id = id;
                
                return result;
            }

            throw new Exception(
                $"Cfg type mismatch: [{id}] [type:{cfg?.GetType().FullName}] expected [{typeof(T).FullName}]");
        }

        //throw new Exception($"Can't find cfg id: {id}");
        return null;
    }

    public static List<T> GetConfigs<T>(Predicate<T> predicate = null) where T : GameConfig
    {
        var configs = new List<T>();
        foreach (var config in _configs.Values)
        {
            if (config is T gameConfig)
            {
                if (predicate == null || predicate(gameConfig))
                {
                    configs.Add(gameConfig);
                }
            }
        }

        return configs;
    }

    private const string _wildmaskMark = "*";
    public static void ParseCfgs<T>(string json) where T : GameConfig
    {
        var type = typeof(T).Name;

        GameConfigs<T> cfgs = null;
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS
        cfgs = JsonUtility.FromJson<GameConfigs<T>>(json);
#else
      cfgs = Utf8Json.JsonSerializer.Deserialize<GameConfigs<T>>(json);
#endif

        foreach (var config in cfgs.Configs)
        {
            var id = config.Id;
            _configs[id] = config;
            Log($"  >Loaded config  {config.Id}");

            if (id.Contains(_wildmaskMark))
            {
                _wildCards.Add(id);
                Log($"  >>Loaded wildcard config  {config.Id}");
            }
        }
    }

    static bool GetKeyWithWildcard(string key, out string result)
    {
        result = key;
        if (string.IsNullOrEmpty(key)) return false;
        
        var wildCard = _wildCards.Find(k=> key.Contains(k.Replace(_wildmaskMark, String.Empty)));

        if (!string.IsNullOrEmpty(wildCard))
        {
            result = wildCard;
            return true;
        }

        return false;
    }

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS
    public static async UniTask<bool> LoadCfg<T>(string url, Func<string, UniTask<string>> loader = null) where T : GameConfig
    #else
    public static async Task<bool> LoadCfg<T>(string url, Func<string, Task<string>> loader = null) where T : GameConfig
#endif
    {
        var type = typeof(T).Name;
        Log($"Loading configs  {type} from {url}");
        try
        {
            string json = "{}";
            if (loader != null)
            {
                json = await loader(url);
            }
            else
            {
                if (url.Contains("//"))
                {
                    if (_client == null) _client = new HttpClient();
                    json = await _client.GetStringAsync(url);
                }
                else
                {
                    json = LoadFromFile(url);
                }
            }

            if (!string.IsNullOrEmpty(json))
            {
                ParseCfgs<T>(json);
            }
        }
        catch (Exception e)
        {
            Log($"LoadCfg error {url} -> {type}", e);
            return false;
        }

        return true;
    }

    private static string LoadFromFile(string path)
    {
        var fullpath = $"{Directory.GetCurrentDirectory()}/{path}";
        Log($"  >Try to load configs from {fullpath}");

        return File.ReadAllText(path);
    }


    public static void Clear()
    {
        _configs.Clear();
    }

    public static void Log(string text, Exception e = null)
    {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS
        if (e != null)
        {
            Debug.LogError($"{text} -> {e.Message}\n {e.StackTrace}");
        }
        else if (verboseLog)
        {
            Debug.Log(text);
        }

#else
      if (e != null)
      {
         ServerLog.Error($"{text} -> {e.Message}\n {e.StackTrace}");
      }
      else
      {
         if (text.Contains(">"))
         {
            if(verboseLog)
               ServerLog.Debug(text);
         }
         else
         {
            ServerLog.Info(text);
         }
      }
#endif
    }


    private static HttpClient _client;

    public static void Dispose()
    {
        _client?.Dispose();
    }
}