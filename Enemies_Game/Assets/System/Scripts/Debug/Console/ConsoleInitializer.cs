using System;
using Opencoding.CommandHandlerSystem;
using UniRx.Async;
using UnityEngine;

public class ConsoleInitializer : MonoBehaviour
{
    private int _screenHeight = 0;
    private int _screenWidth = 0;

    private void Start()
    {
        _screenHeight = Screen.currentResolution.height;
        _screenWidth = Screen.currentResolution.width;

        CommandHandlers.RegisterCommandHandlers(this);
        CommandHandlers.BeforeCommandExecutedHook = BeforeCommandHook;
//            ConsoleBindsListener.Instance.LoadBinds();
    }

    #region Cheats

    [CommandHandler(Description = "Example ", Name = "ze_example")]
    public void Example()
    {
        async UniTaskVoid Force()
        {
           Debug.Log("Example");
        }
        
        Force().Forget();
    }

    [CommandHandler(Description = "touch input sensetivity ", Name = "i_sensitivity")]
    public void Sensetivity(float scale = 1f)
    {
        if (TouchInputHelper.Instance != null)
            TouchInputHelper.Instance.sensitivity = scale;
    }

    [CommandHandler(Description = "Screen resolution preset ", Name = "g_resolution")]
    public void ScreenResolution(int height = 0)
    {
        try
        {
            switch (height)
            {
                case 0:
                    Screen.SetResolution(_screenWidth, _screenHeight, true);
                    Debug.Log($"Canged resolution to {_screenWidth} x {_screenWidth}");
                    break;

                case 2:
                    Screen.SetResolution(_screenWidth / 2, _screenHeight / 2, true);
                    Debug.Log($"Canged resolution to {_screenWidth / 2} x {_screenWidth / 2}");
                    break;

                case 1080:
                    Screen.SetResolution(1920, 1080, true);
                    Debug.Log($"Canged resolution to {1920} x {1080}");
                    break;

                case 720:
                    Screen.SetResolution(1280, 720, true);
                    Debug.Log($"Canged resolution to {1280} x {1280}");
                    break;

                case 640:
                    Screen.SetResolution(960, 640, true);
                    Debug.Log($"Canged resolution to {960} x {640}");
                    break;

                default:
                    Debug.LogError($"Unknown preset");
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Can't switch resolution to {height} -> {e.Message}");
        }
    }

    [CommandHandler(Description = "Clear asset bundles cache", Name = "clear_cache")]
    public void ClearCache()
    {
        if (Caching.ClearCache())
        {
            Debug.Log("Cache cleared");
        }
        else
        {
            Debug.LogWarning("Cache clear failed");
        }
    }

//    [CommandHandler(Description = "Start configs reloading", Name = "reload_configs")]
//    public void ReloadConfigs()
//    {
//        Boot.LoadConfigs().Forget();
//    }

    #endregion


    private bool BeforeCommandHook(CommandHandler commandHandler, string[] commandParams)
    {
        //_aliasesStr.TryGetValue(commandHandler.CommandName, out _selectedAliasCode);
        return true;
    }
}