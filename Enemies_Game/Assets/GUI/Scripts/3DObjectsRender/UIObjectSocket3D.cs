using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ObjViewPreset
{
    public string Name;
    public GameObject target;
    public float rotationTarget;
    public float fov = -1f;
}

public class UIObjectSocket3D : MonoBehaviour
{
    public ObjViewPreset[] Presets;
    
    public Vector2 verticalCameraLimits = new Vector2(0.3f, 1.5f);
    public float rotationSpeed = -1000f;
    public float verticalMoveSpeed = -30f;
    
    public Camera uiCamera;
    public Transform socketPoint = null;
    
    private float _currentRotation = 0f;
    private float _currentHeight = 0f;
    private GameObject _currentObject;
    private float _startHeight = 0f;

    private void Awake()
    {
        _startHeight = uiCamera.transform.localPosition.y;
        _currentHeight = _startHeight;
        
        SetCamHeight(_currentHeight);
    }

    public void EnableRender(bool enable)
    {
        socketPoint.gameObject.SetActive(enable);
        uiCamera.enabled = enable;
    }

    public void SetObject(GameObject obj)
    {
        if(obj == null) return;
        
        DestroyObjInSocket();
        
        _currentObject = obj;
        _currentObject.transform.SetParent(socketPoint, false);
        ResetObjPos();
    }

    public void ResetObjPos()
    {
        socketPoint.rotation = Quaternion.identity;
        SetCamHeight(_startHeight);
    }

    public void SetToPreset(string pName)
    {
        if(string.IsNullOrEmpty(pName)) return;
        
        foreach (var preset in Presets)
        {
            if (preset.Name == pName)
            {
                var trm = uiCamera.transform;
                var presetTrm = preset.target.transform;
                
                trm.position = presetTrm.position;
                trm.forward = presetTrm.forward;
              
                SetRotation(preset.rotationTarget);

                if (preset.fov > 0f)
                {
                    uiCamera.fieldOfView = preset.fov;
                }
                
                SetCamHeight(presetTrm.position.y);
                return;
            }
        }
        
        Debug.LogError($"Can't find UIObjectSocket3D preset {pName}");
    }

    public void DestroyObjInSocket()
    {
        if(_currentObject != null)
            Destroy(_currentObject);
    }

    public void View(Vector2 step)
    {
        _currentRotation += step.x * rotationSpeed;
        SetRotation(_currentRotation);
        
        
        _currentHeight +=  step.y * verticalMoveSpeed;
        SetCamHeight(_currentHeight);
    }

    void SetRotation(float rot)
    {
        _currentRotation = rot;
        socketPoint.rotation = Quaternion.Euler(new Vector3(0f, _currentRotation, 0f));
    }

    void SetCamHeight(float val)
    {
        var camPos = uiCamera.transform.localPosition;
        _currentHeight = Mathf.Clamp(val, verticalCameraLimits.x, verticalCameraLimits.y);
        camPos.y = _currentHeight;
        uiCamera.transform.localPosition = camPos;
        
    }
}
