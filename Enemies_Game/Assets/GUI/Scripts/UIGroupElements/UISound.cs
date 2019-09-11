using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISound : MonoBehaviour
{
    public UIElement element;

    public void Start()
    {
        element.SetOnActivated(PlaySoundOnActivated);
    }

    public void PlaySoundOnActivated(UIElement uiElement)
    {
        UISoundManager.Instance?.PlayUISound(SoundType.Click);
    }
}
