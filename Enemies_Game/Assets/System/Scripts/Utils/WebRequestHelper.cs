using UnityEngine;
using UnityEngine.Networking;
using System.Net;
using System.Text;
using UniRx.Async;

public static class WebRequestHelper
{

    public static bool sslBypass = false;
    
    public static UnityWebRequest PrepareGetRequestExact(string serverEndPointURL, string route,  params object[] args)
    {
        var targetUrlWithParams = string.Format("{0}/{1}", serverEndPointURL, route);;

        if (args != null)
        {
            foreach (var arg in args)
            {
                targetUrlWithParams += string.Format("/{0}", arg);
            }
        }

        var request = UnityWebRequest.Get(targetUrlWithParams);
        request.certificateHandler = new SSLHandler();

        return request;
    }
    
    public static UnityWebRequest PreparePostJson(string serverEndPointURL, string route,  object requestObject)
    {
        var targetUrlWithParams = string.Format("{0}/{1}", serverEndPointURL, route);;

        var request = new UnityWebRequest(targetUrlWithParams, "POST");

        if (requestObject != null)
        {
            var bodyJsonString = JsonUtility.ToJson(requestObject);
            Debug.Log($"PreparePostJson {targetUrlWithParams} -> {bodyJsonString}");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(bodyJsonString);
            request.uploadHandler = (UploadHandler) new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
        }

        if(sslBypass)
            request.certificateHandler = new SSLHandler();

        return request;
    }

    public static async UniTask<string> GetTextAsync(string url)
    {
        var request = UnityWebRequest.Get(url);
        return await GetTextAsync(request);
    }

    public static async UniTask<string> GetTextAsync(UnityWebRequest request)
    {
        var op = await request.SendWebRequest();
        
        if (request.isNetworkError)
        {
            var reason =   $"Network error: {request.error} for {request.url}";
            Debug.LogError(reason);

            throw new WebException(reason);
        }
        
        var isFail = (request.responseCode != 200);

        if (isFail)
        {
            var requestResult = request.downloadHandler != null ? request.downloadHandler.text : request.error;
            var reason = $"Request error: [{requestResult}][{request.responseCode}]for {request.url}";
            
            throw new WebException(reason);
        }

        return op.downloadHandler.text;
    }
}