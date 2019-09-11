using System;
using System.Collections;
using System.Collections.Generic;
using UniRx.Async;
using UnityEngine;

public class InfoPopup : PopupBase
{
   private Action onOK;

   public override void Init()
   {
      base.Init();
      uiRoot.SelectNext(1);
      uiRoot.Get("OKButton")?.SetOnActivated(OnOkClicked);
   }

   public InfoPopup OnOk(Action okAction)
   {
      if (okAction != null)
      {
         onOK = okAction;
      }

      return this;
   }

   public InfoPopup Text(string text)
   {
      uiRoot.SetDataToObject("InfoText", text);
      return this;
   }

   void OnOkClicked(UIElement element)
   {
      onOK?.Invoke();
      Hide();
   }

   public UniTask ShowAsync(string text = null)
   {
      if (!string.IsNullOrEmpty(text)) Text(text);
      
      var tcs = new UniTaskCompletionSource();
      OnOk(() => { tcs.TrySetResult(); }).Show();

      return tcs.Task;
   }
}
