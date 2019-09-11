using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingPopup : PopupBase, IProgress<float>
{
   public override void Init()
   {
      base.Init();
   }

 

   public LoadingPopup Text(string text)
   {
      uiRoot.SetDataToObject("InfoText", text);
      return this;
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
