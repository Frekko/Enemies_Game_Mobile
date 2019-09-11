using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    public void Awake()
    {
        if (Instance == null)
            Instance = this;

        DontDestroyOnLoad(this);
    }

    public AudioSource Sound;

    public void PlaySound(AudioClip TakeCoin)
    {
        Sound.clip = TakeCoin;
        Sound?.Play();
    }
}
