using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UILoading : UIScreenBase, IProgress<float>
{
    public override void Init()
    {
        base.Init();
        SetText("Loading".Lcz());
    }

    public void SetText(string text)
    {
        uiRoot.SetDataToObject("StatusText", text);
    }

    public void SetProgress(float progress)
    {
        uiRoot.SetDataToObject("Loading/Progress", progress);
    }

    public void Report(float value)
    {
        SetProgress(value);
    }
}
