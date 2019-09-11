using System;
using UnityEngine;

public class UIIcons : MonoBehaviour
{
    public static string GetIconVisualFolder(string visualId, string section = null)
    {
        var retVal = $"Icons";
        
        if (!string.IsNullOrEmpty(section))
        {
            retVal = $"Icons/{section}";
        }
        
        var types = visualId.Split(new[] {'_'},  2, StringSplitOptions.RemoveEmptyEntries);
        var final = "";
        
        if(types != null && types.Length > 1 ) final = types[0];
       
        if (!string.IsNullOrEmpty(final))
        {
            retVal =$"{retVal}/{final}";
        }
        
        
        return retVal;
    }

    public static string GetIconPath(string visualId, string section = null)
    {
        var folder = GetIconVisualFolder(visualId, section);

        return $"{folder}/UI_{visualId}";
    }

    public static Sprite Icon(string resourceId, string section = "Items")
    {
        if (string.IsNullOrEmpty(resourceId)) return null;
        
        var fileExtJpg = ".jpg"; 
        var fileExtPng = ".png"; //All icons expected to be in .png format
        var iconsBundle = "icons";
        var path = GetIconPath(resourceId, section);
        
        try
        {
            var texture = BundlesManager.Instance.Load<Texture2D>(path + fileExtJpg, iconsBundle);
            
            if(texture == null)
                texture = BundlesManager.Instance.Load<Texture2D>(path + fileExtPng, iconsBundle);
            
            if(texture != null) return Sprite.Create(texture, new Rect(0,0, texture.width, texture.height), new Vector2(0.5f, 0.5f) );

            Debug.LogError($"Can't load Texture2D  {path}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Can't load Texture2D  for  {path}");
        }

        return UIStyles.WIPIcon();
    }
}
