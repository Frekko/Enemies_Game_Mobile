using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIIngame : UIScreenBase
{
    public override void Init()
    {
        base.Init();

        uiRoot.Get("UpLeftButton").SetOnActivated(MoveUpLeft);
        uiRoot.Get("UpRightButton").SetOnActivated(MoveUpRight);
        uiRoot.Get("DownLeftButton").SetOnActivated(MoveDownLeft);
        uiRoot.Get("DownRightButton").SetOnActivated(MoveDownRight);
    }

    public override void Show()
    {
        base.Show();
    }

    void MoveUpLeft(UIElement element)
    {
        Player.Instance?.Move(Moving.UpLeft);
    }

    void MoveUpRight(UIElement element)
    {
        Player.Instance?.Move(Moving.UpRight);
    }

    void MoveDownLeft(UIElement element)
    {
        Player.Instance?.Move(Moving.DownLeft);
    }

    void MoveDownRight(UIElement element)
    {
        Player.Instance?.Move(Moving.DownRight);
    }

    public void PlayerIsDead()
    {
        RestartScene();
    }

    async void RestartScene()
    {
        SceneManager.LoadScene("SampleScene");
    }
}
