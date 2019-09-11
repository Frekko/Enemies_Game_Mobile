using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SoundType
{
    Click,
    YouWin,
    YouLose,
    MVP
}

public enum MusicType
{
    Background,
    FightOpening,
    FightEnding
}

public class UISoundManager : MonoBehaviour
{
    [Header("UI Sounds")]
    public List<UISound> sounds;
    [Space]
    public AudioSource UISoundPlayer;
    
    [Header("UI Music")]
    
    public List<UIMusic> tracks;
    [Space]
    public AudioSource UIMusicPlayer;
    private bool _isFadingOut;
    private float _defaultMusicVol;
    private float _fadeOutSpeed = 1f;
    
    [HideInInspector]
    public static UISoundManager Instance;
    
    [Serializable]
    public class UISound
    {
        public AudioClip clip;
        public SoundType type;
    }
    
    [Serializable]
    public class UIMusic
    {
        public AudioClip clip;
        public MusicType type;
        public bool loop;
    }
    
    private void Awake()
    {
        Instance = this;
        _defaultMusicVol = UIMusicPlayer.volume;
    }
    
    public void Update()
    {
        if (tracks.Count == 0)
            return;

        if (_isFadingOut)
        {
            _isFadingOut = MusicFadingOut(UIMusicPlayer);
            return;
        }

        if (UIMusicPlayer.clip != null && _currentMusic.loop && !UIMusicPlayer.isPlaying)
        {
            UIMusicPlayer.Play();
        }
    }

    #region UISounds

    public void PlayUISound(SoundType type)
    {
        var sound = sounds?.Find(s => s.type == type);
        if (sound != null)
        {
            UISoundPlayer.clip = sound.clip;
            UISoundPlayer.Play();
        }
    }
    
    #endregion
    
    #region UIMusic

    private UIMusic _currentMusic;

    public void PlayUIMusic(MusicType type, bool useFadeOut = true)
    {
        var track = tracks?.Find(s => s.type == type);
        var clip = track?.clip;
        if (clip == null)
        {
            Debug.Log($"Can't find clip with type \"{type.ToString()}\" in UISoundManager");
            return;
        }

        if (_currentMusic != null && _currentMusic == track)
            return;

        _currentMusic = track;

        if (useFadeOut && UIMusicPlayer.isPlaying)
        {
            _isFadingOut = true;
            return;
        }

        _isFadingOut = false;
        StartUIMusic();
    }

    public void StopCurrentMusic(bool useFadeOut = true)
    {
        if (useFadeOut)
            _isFadingOut = true;
        else
            UIMusicPlayer.Stop();
    }

    private void StartUIMusic()
    {
        UIMusicPlayer.clip = _currentMusic.clip;
        UIMusicPlayer.volume = _defaultMusicVol;
        UIMusicPlayer.Play();
    }
    
    public bool MusicFadingOut(AudioSource track)
    {
        
        if (track.volume > 0)
        {
            track.volume -= _defaultMusicVol/_fadeOutSpeed * Time.deltaTime;
            return true;
        }

        if (_currentMusic.clip != track.clip)
            StartUIMusic();

        return false;
    }

    #endregion
   
}
