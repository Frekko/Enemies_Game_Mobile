using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchInputHelper : MonoBehaviour
{
    [HideInInspector]
    public static TouchInputHelper Instance;

    public Vector2 InputScale = new Vector2(1f, 0.5f);
    
    public Action<Vector2> dragStep;
    public bool smoothInput = true;

    [HideInInspector] public float sensScale = 1f;
    public float sensitivity = 1f;
    public AnimationCurve dragScaleCurve = AnimationCurve.Linear(0,1,1,1);
    
    private bool _touchLost = true;

    public bool TouchLost => _touchLost;

    private void Awake()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if(!gameObject.activeInHierarchy) return;

        if (GetFilteredTouchPos(out var pos))
        {
            if (_touchLost) _prevMousePos = pos;
            
            UpdatePos(pos);
            _touchLost = false;
        }
        else
        {
            _prevMousePos = pos;
            _touchLost = true;
        }
    }

    private float _prevStepLen = 0f;
    private float _prevStepMinLen = 0.1f;
    private float _smoothingScale = 1.3f;
    
    
    

   
    bool GetFilteredTouchPos(out Vector2 result)
    {
        result =  Vector2.zero;
        var inputLinePos = ControlLine();
        
        
        foreach (var t in Input.touches)
        {
            if (t.position.x > inputLinePos)
            {
                result = t.position;
                return true;
            }
        }
        
        if (Input.GetMouseButton(0) && Input.mousePosition.x > inputLinePos)
        {
            result = Input.mousePosition;
            return true;
        }

        return false;
    }

    private float _screenPart = 0.5f;
    float ControlLine()
    {
        if (Camera.main == null) return 0f;
        
        return Camera.main.pixelWidth * _screenPart;
    }

    Vector2 _prevMousePos = Vector2.zero;
    
    void UpdatePos(Vector2 pos)
    {
        var delta = pos - _prevMousePos;
        if (smoothInput)
        {
            if (_prevStepLen > Mathf.Epsilon && delta.magnitude / _prevStepLen > _smoothingScale)
            {
                delta = _prevStepLen * _smoothingScale * delta.normalized;
            }
        }

        dragStep?.Invoke(sensScale * sensitivity * ApplyCurve(Normalize(delta)) * InputScale);
        _prevMousePos = pos;
		
        _prevStepLen = delta.magnitude;

        //if (_prevStepLen < _prevStepMinLen) _prevStepLen = _prevStepMinLen;
    }
    
    Vector2 Normalize(Vector2 pos)
    {
        var camera = Camera.main;
        if(camera == null) return Vector2.zero;
        
        return new Vector2(pos.x / camera.pixelWidth, pos.y /camera.pixelHeight);
    }

    Vector2 ApplyCurve(Vector2 normalizedDelta)
    {
        float c = dragScaleCurve.Evaluate(Mathf.Abs(normalizedDelta.x));
        normalizedDelta.x *= c;
        return normalizedDelta;
    }
}
