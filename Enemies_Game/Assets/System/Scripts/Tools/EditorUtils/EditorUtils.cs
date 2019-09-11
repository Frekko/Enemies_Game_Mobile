

#if UNITY_EDITOR
using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class EditorUtils 
{
    public static void       StartBackgroundTask(IEnumerator update, Action end = null)
    {
        EditorApplication.CallbackFunction   closureCallback = null;
 
        closureCallback = () =>
        {
            try
            {
                if (update.MoveNext() == false)
                {
                    if (end != null)
                        end();
                    EditorApplication.update -= closureCallback;
                }
            }
            catch (Exception ex)
            {
                if (end != null)
                    end();
                Debug.LogException(ex);
                EditorApplication.update -= closureCallback;
            }
        };
 
        EditorApplication.update += closureCallback;
    }

    public static void WebRequest(string url, Action<string> result = null)
    {
        StartBackgroundTask(WebRequestHandler(url, result));
    }

    static IEnumerator   WebRequestHandler(string url, Action<string> result = null)
    {
        using (UnityWebRequest w = UnityWebRequest.Get(url))
        {
            yield return w.SendWebRequest();
 
            while (w.isDone == false)
                yield return null;
 
            var resultText = w.downloadHandler.text;
            
            if(w.isHttpError || w.isNetworkError)
                resultText = w.error;
            
            result?.Invoke(resultText);
        }
    }
}
#endif
