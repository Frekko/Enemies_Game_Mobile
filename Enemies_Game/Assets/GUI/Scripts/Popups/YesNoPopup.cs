using System;
using UniRx.Async;

public class YesNoPopup : PopupBase
{
    private Action onOK;
    private Action onCancel;

    public override void Init()
    {
        base.Init();
        uiRoot.SelectNext(1);
        uiRoot.Get("OKButton").SetOnActivated(OnOkClicked);
        uiRoot.Get("CancelButton").SetOnActivated(OnCancelClicked);
    }

    public YesNoPopup OnOk(Action okAction)
    {
        if (okAction != null)
        {
            onOK = okAction;
        }

        return this;
    }

    public YesNoPopup OnCancel(Action cancelAction)
    {
        if (cancelAction != null)
        {
            onCancel = cancelAction;
        }

        return this;
    }

    public YesNoPopup Text(string text)
    {
        uiRoot.SetDataToObject("InfoText", text);
        return this;
    }

    void OnOkClicked(UIElement element)
    {
        onOK?.Invoke();
        Hide();
    }

    void OnCancelClicked(UIElement element)
    {
        onCancel?.Invoke();
        Hide();
    }

    public UniTask<bool> ShowAsync()
    {
        var tcs = new UniTaskCompletionSource<bool>();
        
        OnOk(() => { tcs.TrySetResult(true); });
        OnCancel(() => { tcs.TrySetResult(false); });
        Show();

        return tcs.Task;
    }
}