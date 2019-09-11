using System;
using System.Collections;
using System.Collections.Generic;
using UniRx.Async;
using UnityEngine;

public class BundlesManagerUtils
{
    public static async UniTask ResourceUpToDate(string path, bool isScene = false)
    {

//        if (!await ShowDownloadDialog())
//            throw new OperationCanceledException();

        
        try
        {
            if (!BundlesManager.Instance.IsBundlesCached(BundlesManager.BundleName( path, isScene)))
                ShowLoadingProgress("", 0);

            await BundlesManager.Instance.LoadBundleForResource(path, isScene, UIManager.Popup<LoadingPopup>());

            HideLoadingProgress();
            Debug.Log($"ResourcesUpToDate -> Done ");
        }
        catch (Exception e)
        {
            Debug.LogError($"ResourceUpToDate error: for {e.Message} -> {e.StackTrace}");
            HideLoadingProgress();
            await UIManager.Popup<InfoPopup>().Text("#RESOURCE_NOT_FOUND".Lcz()).ShowAsync();
            throw;
        }
    }
    
    public static async UniTask BundleUpToDate(string bundleName)
    {

//        if (!await ShowDownloadDialog())
//            throw new OperationCanceledException();

        
        try
        {
            if (!BundlesManager.Instance.IsBundlesCached(bundleName))
                ShowLoadingProgress("", 0);

            await  BundlesManager.Instance.DownloadBundles(bundleName);

            HideLoadingProgress();
            Debug.Log($"ResourcesUpToDate -> Done ");
        }
        catch (Exception e)
        {
            Debug.LogError($"ResourceUpToDate error: for {e.Message} -> {e.StackTrace}");
            HideLoadingProgress();
            await UIManager.Popup<InfoPopup>().Text("#RESOURCE_NOT_FOUND".Lcz()).ShowAsync();
            throw;
        }
    }

    public static async UniTask ResourcesUpToDate(params string[] bundlesNames)
    {
        Debug.Log($"Check ResourcesUpToDate");
//        if (BundlesManager.Instance.IsBundlesCached(bundlesNames))
//        {
//            return;
//        }

        
//        if (!await ShowDownloadDialog())
//            throw new OperationCanceledException();


        try
        {
            BundlesManager.LoadingStatusEvent += ShowLoadingProgress;
            Debug.Log($"ResourcesUpToDate -> Try download ");

            ShowLoadingProgress("", 0);

            await BundlesManager.Instance.DownloadBundles(bundlesNames);

            Debug.Log($"ResourcesUpToDate -> Done ");
            
            return;
        }
        catch (Exception e)
        {
            Debug.LogError($"ResourceUpToDate error: for {e.Message} -> {e.StackTrace}");
            throw;
        }
        finally
        {
            BundlesManager.LoadingStatusEvent -= ShowLoadingProgress;
            HideLoadingProgress();
        }
    }

    static UniTask<bool> ShowDownloadDialog()
    {
        return UIManager.Popup<YesNoPopup>().Text("#UPDATE_RESOURCE".Lcz()).ShowAsync();
    }

    static void ShowLoadingProgress(string status, float progress)
    {
        var popup = UIManager.Popup<LoadingPopup>();
        if (popup == null) return;
        
        if (popup.IsHidden) popup.Show();
        
        popup.Text("#DOWNLOADING_CONTENT".Lcz() + " " + status).SetProgress(progress);
    }

    static void HideLoadingProgress()
    {
        UIManager.Popup<LoadingPopup>()?.Hide();
    }
}