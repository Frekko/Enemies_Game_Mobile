using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SearchTag : MonoBehaviour
{
    public string Tag = "SearchTag";
    
    public static GameObject Find(string tagName)
    {
        var tags = GameObject.FindObjectsOfType<SearchTag>();

        foreach (var tag in tags)
        {
            if (tag.Tag == tagName) return tag.gameObject;
        }

        return null;
    }
}
