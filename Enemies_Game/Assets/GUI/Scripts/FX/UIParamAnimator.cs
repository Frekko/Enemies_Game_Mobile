using System;
using System.Collections;
using System.Collections.Generic;
using UniRx.Async;
using UnityEngine;
using UnityEngine.UI;

public static class UIElementExtention
{
    public static void PlayAnimationOn(this UIElement thiz, string path = null, bool reversed = false)
    {
        var obj = thiz.FindTransform(path);
        var animator = obj?.GetComponent<UIParamAnimator>();

        animator?.PlayAll(reversed);
    }
    
    public static void PlayAnimation(this UIElement thiz, bool reversed = false)
    {
        var obj = thiz.gameObject;
        var animator = obj.GetComponent<UIParamAnimator>();

        animator?.PlayAll(reversed);
    }
    
    public static void ResetAnimationOn(this UIElement thiz, string path = null)
    {
        var obj = thiz?.FindTransform(path);
        var animator = obj?.GetComponent<UIParamAnimator>();

        animator?.ResetAll();
    }
    
    public static void ResetAnimation(this UIElement thiz)
    {
        var obj = thiz.gameObject;
        var animator = obj.GetComponent<UIParamAnimator>();

        animator?.ResetAll();
    }
}

[Serializable]
public class UIParamAnimation
{
    enum PlayState
    {
        Paused,
        Active
    }
    
    public enum LoopType
    {
        None,
        Loop,
        PingPong
    }
    
    public string name;
    public AnimationCurve curve;
    public float duration = 1f;
    public float delay = 0f;

    public Color startColor = UnityEngine.Color.white;
    public Color endColor = UnityEngine.Color.white;

    public LoopType loopType = LoopType.None;

    public Action<UIParamAnimation> update;
    public Action<UIParamAnimation> finished;
    public Action<UIParamAnimation> started;

    private float _remainTime = 0f;
    private float _remainDelay = 0f;
    private bool _reverse = false;

    private PlayState _state = PlayState.Active;

    private UniTaskCompletionSource _tcs;

    public bool Reversed => _reverse;
    public bool IsFinished => _remainTime <= 0f;
    

    public float CurrentPosition => _reverse ?  _remainTime / duration : 1f - _remainTime / duration;

    #region Register methods
    public UIParamAnimation OnUpdate(Action<UIParamAnimation> func)
    {
        if (func != null)
        {
            update += func;
        }

        return this;
    }
    
    public UIParamAnimation OnStart(Action<UIParamAnimation> func)
    {
        if (func != null)
        {
            started += func;
        }

        return this;
    }
    
    public UIParamAnimation OnFinished(Action<UIParamAnimation> func)
    {
        if (func != null)
        {
            finished += func;
        }

        return this;
    }
    #endregion

    public void Reset()
    {
        _state = PlayState.Paused;
        _remainTime = duration;
        _remainDelay = delay;
        _reverse = false;
        
        finished?.Invoke(this);
        update?.Invoke(this);
        

        _tcs?.TrySetResult();
    }
    
    public void ToEnd()
    {
        _state = PlayState.Active;
        _remainTime = 0;
        
        finished?.Invoke(this);
        update?.Invoke(this);
        

        _tcs?.TrySetResult();
    }

    public async UniTask Play(bool reversed = false)
    {
        _state = PlayState.Active;
        _reverse = reversed;
        _remainTime = duration;
        _remainDelay = delay;
        
        _tcs = new UniTaskCompletionSource();

        if (loopType != LoopType.None)
            _tcs.TrySetResult();
        
        await _tcs.Task;
    }


    public void Update(float dt)
    {
        if (_remainDelay > 0f && _remainDelay <= dt)
        {
            started?.Invoke(this);
        }

        _remainDelay -= dt;
        if(_remainDelay > 0f) return;

        _remainDelay = 0f;
        
        if(_state == PlayState.Paused) return;
        
        var prevTime = _remainTime;

        _remainTime -= dt;

        if (_remainTime <= 0f && prevTime > 0f)
        {
            switch (loopType)
            {
                case LoopType.Loop:
                    Play();
                    break;
                case LoopType.PingPong:
                    Play(!_reverse);
                    break;
                default:
                    finished?.Invoke(this);
                    _tcs.TrySetResult();
                    break;
            }
            update?.Invoke(this);
        }

        if (_remainTime <= 0f)
        {
            _remainTime = 0f;
        }
        else
        {
            update?.Invoke(this);
        }
    }

    #region Getters
    public float Float()
    {
        return curve.Evaluate(CurrentPosition);
    }
    
    public Vector3 Scale()
    {
        var val = Float();
        return new Vector3(val, val, val);
    }
    
    public Color Color()
    {
        var val = Float();
        return UnityEngine.Color.Lerp(startColor, endColor, val);
    }
    #endregion

  
}

public class UIParamAnimator : MonoBehaviour
{
    public List<UIParamAnimation> anims = new List<UIParamAnimation>();
    public bool autoBind = true;
    public bool playOnStart = false;
    
    public enum  BindType
    {
        Color,
        Size,
        Alpha,
        Visibility
    }

    private void Awake()
    {
        _initialPosition = transform.position;
        if (autoBind) AutoBind();
    }

    private void Start()
    {
        if(playOnStart) PlayAll();
    }

    void Update()
    {
        foreach (var anim in anims)
        {
            anim.Update(Time.deltaTime);
        }
    }

    public UIParamAnimation Anim(string animName = null)
    {
        if (string.IsNullOrEmpty(animName)) return anims[0];
        
        return anims.Find(a => a.name == animName);
    }

    public void PlayAll(bool reversed = false)
    {
        foreach (var anim in anims)
        {
            anim?.Play(reversed);
        }
    }
    
    public void ResetAll()
    {
        foreach (var anim in anims)
        {
            anim?.Reset();
        }
    }

    void AutoBind()
    {
        foreach (var anim in anims)
        {
            switch (anim.name)
            {
                case "Color": BindByType(BindType.Color, anim); break;
                case "Size": BindByType(BindType.Size, anim); break;
                case "Alpha": BindByType(BindType.Alpha, anim); break;
                case "Visibility": BindByType(BindType.Visibility, anim); break;
            }
        }
    }

    Vector3 _initialPosition = Vector3.zero;
    void BindByType(BindType type, UIParamAnimation anim)
    {
        var obj = gameObject;
        var image = obj.GetComponent<Image>();
        var text = obj.GetComponent<Text>();
        var advText = obj.GetComponent<TMPro.TextMeshProUGUI>();
        var canvasGroup = obj.GetComponent<CanvasGroup>();

        switch (type)
        {
            case BindType.Color:
                anim.OnUpdate(paramAnimation =>
                {
                    if(image != null) image.color = paramAnimation.Color();
                    if(text != null) text.color = paramAnimation.Color();
                    if(advText != null) advText.color = paramAnimation.Color();
                });
                break;
            case BindType.Size:
                anim.OnUpdate(paramAnimation =>
                {
                    transform.localScale = paramAnimation.Scale();
                });
                break;
            case BindType.Alpha:
                anim.OnUpdate(paramAnimation =>
                {
                    if (canvasGroup != null) canvasGroup.alpha = paramAnimation.Float();
                });
                break;
            case BindType.Visibility:
                anim.OnStart(paramAnimation =>
                {
                    gameObject.SetActive(!paramAnimation.Reversed);
                });
                anim.OnFinished(paramAnimation =>
                {
                    gameObject.SetActive(paramAnimation.Reversed);
                });
                break;
        }
    }
    
    #region  Editor

    [ContextMenu("Play All")]
    void PlayAllEditor()
    {
       PlayAll();
    }
    
    [ContextMenu("Reset")]
    void ResetEditor()
    {
        ResetAll();
    }

    #endregion
}
