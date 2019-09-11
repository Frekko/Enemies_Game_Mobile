using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIStyles : MonoBehaviour
{
    public Sprite wipIcon;
    [Serializable]
    public class StyleData
    {
        public string Name;
        public Color Color;
    }
    public static UIStyles Instance;

    public List<StyleData> Styles = new List<StyleData>();
  
    private void Awake()
    {
        Instance = this;
    }

    public static Color Color(string styleName)
    {
        if(Instance == null) return UnityEngine.Color.white;

        var style = Instance.Styles.Find(s => s.Name == styleName);
        if (style != null)
        {
            return style.Color;
        }
        return UnityEngine.Color.white;
    }
    
    public static string ColorHex(string styleName)
    {
        var color = Color(styleName);

        return "#"+ColorUtility.ToHtmlStringRGBA(color);
    }
    
    public static string ColoredText(string styleName, string text)
    {
        var colorHex = ColorHex(styleName);

        return $"<color={colorHex}>{text}</color>";
    }
    
    public static Sprite WIPIcon()
    {
        return Instance?.wipIcon;
    }
}
