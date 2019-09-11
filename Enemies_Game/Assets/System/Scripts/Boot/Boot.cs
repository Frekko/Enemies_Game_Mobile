using System;
using System.IO;
using Game;
using UniRx.Async;
using UnityEngine;


public class Boot : MonoBehaviour
{
    public int targetFrameRate = 60;
    public string[] updateBundles;

    public bool useRemoteConfigs = true;
    public string localBaseCfgURL;
//    public string remoteBaseCfgURL;
//    public string charactersCfgURL;
//    public string userCfgURL;
//    public string cosmeticsCfgURL;
//    public string questsCfgURL;
//    public string lootboxesCfgURL;

    public GameObject debugConsolePrefab;

   


//    private static string _baseUrlKey = "configs_base";
//    private static string _charCfgUrlKey = "char_configs";
//    private static string _userCfgUrlKey = "user_configs";
//    private static string _cosmeticsCfgUrlKey = "cosmetics_configs";
//    private static string _questsCfgUrlKey = "quests_configs";
//    private static string _lootboxCfgUrlKey = "lootboxes_configs";

    void Start()
    {
        Init().Forget();
    }

    async UniTaskVoid Init()
    {

        var localUrl =
            $"file://{Path.GetFullPath(Path.Combine(Application.dataPath, localBaseCfgURL))}{Path.DirectorySeparatorChar}";

#if !UNITY_EDITOR
        useRemoteConfigs = true;
#endif

//        PlayerPrefs.SetString(_baseUrlKey, useRemoteConfigs ? remoteBaseCfgURL : localUrl);
//        PlayerPrefs.SetString(_charCfgUrlKey, charactersCfgURL);
//        PlayerPrefs.SetString(_userCfgUrlKey, userCfgURL);
//        PlayerPrefs.SetString(_cosmeticsCfgUrlKey, cosmeticsCfgURL);
//        PlayerPrefs.SetString(_questsCfgUrlKey, questsCfgURL);
//        PlayerPrefs.SetString(_lootboxCfgUrlKey, lootboxesCfgURL);
//        PlayerPrefs.Save();

       // await LoadConfigs();

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        {
            //GraphicsSettings.renderPipelineAsset = Resources.Load<RenderPipelineAsset>("Settings/LWRP-HighQuality");
        }
#else
        {
            //TODO: graphics settings

        var screenWidth = Screen.currentResolution.width;
        var screenHeight = Screen.currentResolution.height;
        
        Screen.SetResolution(screenWidth/2, screenHeight/2, true);


            //GraphicsSettings.renderPipelineAsset = Resources.Load<RenderPipelineAsset>("Settings/LWRP-MediumQuality");
            Application.targetFrameRate = targetFrameRate;
            //QualitySettings.vSyncCount = 0;
        }
#endif

        var console = Instantiate(debugConsolePrefab);

        try
        {
            await BundlesManagerUtils.ResourcesUpToDate(updateBundles);
        }
        catch (Exception e)
        {
            UIManager.Popup<InfoPopup>().Text("Can't update data").Show();
        }
        finally
        {
            GameFlow.Init();
            GameFlow.MainMenu();
            //await GameFlow.Login(await LocalUser.GetLogin(), LocalUser.GetPassword());
        }
    }

//    public static async UniTask LoadConfigs()
//    {
//        var baseCfg = PlayerPrefs.GetString(_baseUrlKey);
//        var charactersCfg = baseCfg + PlayerPrefs.GetString(_charCfgUrlKey);
//        var userCfg = baseCfg + PlayerPrefs.GetString(_userCfgUrlKey);
//        var cosmeticsCfg = baseCfg + PlayerPrefs.GetString(_cosmeticsCfgUrlKey);
//        var questsCfg = baseCfg + PlayerPrefs.GetString(_questsCfgUrlKey);
//        var lootboxesCfg = baseCfg + PlayerPrefs.GetString(_lootboxCfgUrlKey);
//        try
//        {
//            await GameConfigsProvider.LoadCfg<CharacterCardCfg>(charactersCfg, TextLoader);
//            await GameConfigsProvider.LoadCfg<GameConfigUpgradable>(userCfg, TextLoader);
//            await GameConfigsProvider.LoadCfg<ItemCfg>(cosmeticsCfg, TextLoader);
//            await GameConfigsProvider.LoadCfg<QuestCfg>(questsCfg, TextLoader);
//            await GameConfigsProvider.LoadCfg<LootBoxConfig>(lootboxesCfg, TextLoader);
//            //var cards = GameConfigsProvider.GetConfigs<CharacterCardCfg>("Cards");
//        }
//        catch (Exception e)
//        {
//            Debug.LogError($"LoadConfigs exception {e.Message} \n {e.StackTrace}");
//        }
//    }

    static async UniTask<string> TextLoader(string url)
    {
        if (!url.Contains(":"))
        {
            url = $"file://{Application.streamingAssetsPath}/{url}";
        }

        Debug.Log($"URL = {url}");

        return await WebRequestHelper.GetTextAsync(url);
    }
}