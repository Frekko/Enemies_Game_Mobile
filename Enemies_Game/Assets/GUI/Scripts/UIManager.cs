using System;
using System.Collections;
using System.Collections.Generic;
using UniRx.Async;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public List<UIScreenBase> Screens;
    public List<PopupBase> Popups;
    public UIScreenBase startScreen = null;

    Dictionary<string, UIObjectSocket3D> _visuals3D = new Dictionary<string, UIObjectSocket3D>();
    
    public static UIManager Instance;

    public static Stack<UIScreenBase[]> PrevScreens = new Stack<UIScreenBase[]>();
    //TODO: popups
    
    private void Awake()
    {
        Instance = this;
        Screens = new List<UIScreenBase>(GetComponentsInChildren<UIScreenBase>(true));
        Popups = new List<PopupBase>(GetComponentsInChildren<PopupBase>(true));
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        foreach (var screen in Screens)
        {
            try
            {
                screen.Init();
                screen.Hide();
            }
            catch (Exception e)
            {
                Debug.LogError($"{screen.name} error: {e.Message} -> {e.StackTrace}");
            }
           
        }
        
        if(startScreen != null)
            ShowScreenInst(startScreen);
    }
    
    public static async UniTask FadeIn()
    {
        if (UIFades.Instance == null) return;
        await UIFades.Instance.FadeIn();
    }
    
    public static async UniTask FadeOut()
    {
        if (UIFades.Instance == null) return;
        await UIFades.Instance.FadeOut();
    }
    
    public static void FadeOutFire()
    {
        if (UIFades.Instance == null) return;
        UIFades.Instance.FadeOut();
    }
    
    public static void FadeInFire()
    {
        if (UIFades.Instance == null) return;
        UIFades.Instance.FadeIn();
    }



    public static T Screen<T>(string name = null) where T: UIScreenBase
    {
        if (!string.IsNullOrEmpty(name))
        {
            return Instance.Screens.Find(s => s.name == name) as T;
        }
        
        return Instance.Screens.Find(s => s is T) as T;
    }
    
    public static T Popup<T>(string name = null) where T: PopupBase
    {
        if (!string.IsNullOrEmpty(name))
        {
            return Instance.Popups.Find(s => s.name == name) as T;
        }
        
        return Instance.Popups.Find(s => s is T) as T;
    }

  

    public static T ShowScreenInst<T>() where T: UIScreenBase
    {
       UIScreenBase retVal = null;
      
        foreach (var screen in Instance.Screens)
        {
            if (screen is T)
            {
                ShowScreenInst(screen);
                retVal = screen;
            }
        }
        
        if(retVal == null) Debug.LogError($"Can't find {typeof(T)} screen under {Instance.gameObject.name}");
        
        return retVal as T;
    }

    public static void ShowScreenInst(UIScreenBase screen)
    {
        foreach (var s in Instance.Screens)
        {
            if (s == screen)
            {
                if (screen.IsHidden) screen.Show();
            }
            else
            {
                if(!(s is PopupBase))
                    HideScreen(s);
            }
        }
        
    }

    public static void ClearScreensHistory()
    {
        PrevScreens.Clear();
    }

    public static UIObjectSocket3D Get3DSocket(string key = "")
    {
        UIObjectSocket3D retVal = null;
        if (Instance._visuals3D.ContainsKey(key) && Instance._visuals3D[key] != null)
        {
            retVal = Instance._visuals3D[key];
        }
        else 
        {
            var objects = GameObject.FindObjectsOfType<UIObjectSocket3D>();
            foreach (var o in objects)
            {
                Instance._visuals3D[o.name] = o;
                if (o.name == key)
                    retVal = o;
            }
        }

        return retVal;
    }

    public static bool SetObjectTo3DSocket(string socketName, GameObject obj, string preset = "default")
    {
        var socket3D = Get3DSocket(socketName);
        socket3D?.SetObject(obj);
        socket3D?.SetToPreset(preset);

        return socket3D != null;
    }

   

    public static void HideAll()
    {
        foreach (var screen in Instance.Screens)
        {
            HideScreen(screen);
        }
    }
    
    public static void HideAllExceptPopups()
    {
        foreach (var screen in Instance.Screens)
        {
            if(!(screen is PopupBase))
                HideScreen(screen);
        }
    }


    static void HideScreen(UIScreenBase screen)
    {
        if(!screen.IsHidden) screen.Hide();
    }

    public static void KeepInHistory(params UIScreenBase[] screens)
    {
        PrevScreens.Push(screens);
    }

    public static void ShowPrevScreens()
    {
        if (PrevScreens.Count > 0)
        {
            var screens = PrevScreens.Pop();

            foreach (var screen in screens)
            {
                HideAllExceptPopups();
                screen.Show();
            }
        }
    }

}
