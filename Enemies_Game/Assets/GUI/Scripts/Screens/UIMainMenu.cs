using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIMainMenu : UIScreenBase
{
    public override void Init()
    {
        base.Init();

        uiRoot.Get("PlayButton").SetOnActivated(OnPlayActivated);
    }

    private void OnPlayActivated(UIElement obj)
    {
        Debug.Log("Play button activated");
        SceneManager.LoadScene("SampleScene");
        ShowIngameUI();
    }

    public override void Hide()
    {
        base.Hide();
    }

    async void ShowIngameUI()
    {
        //Hide();
        UIManager.KeepInHistory(this);
        await UIManager.FadeIn();
        UIManager.ShowScreenInst<UIIngame>();
        await UIManager.FadeOut();
    }
}
