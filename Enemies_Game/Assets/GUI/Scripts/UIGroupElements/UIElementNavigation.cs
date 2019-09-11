using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UIElement))]
public class UIElementNavigation : MonoBehaviour
{
    public Vector2 repeatTime = new Vector2(0.5f, 0.1f);
    public float repeatTimeAccelStep = 0.1f;

	public KeyCode prevKey = KeyCode.UpArrow;
	public KeyCode nextKey = KeyCode.DownArrow;
	
	public KeyCode prevRowKey = KeyCode.LeftArrow;
	public KeyCode nextRowKey = KeyCode.RightArrow;

	public KeyCode submitButton = KeyCode.Return;
	public KeyCode cancelButton = KeyCode.Escape;

	public bool transpose = false;
    public bool inverseH = false;
    public bool inverseV = false;
	public int rowCount = 1;
	
    public Vector2 speedMult = Vector2.one;

    public bool isActive = true;
	
    private float _remainTime = 0f;
    private float _currentRepeatTime = 0.5f;

	private UIElement _targetElement;

	private void Awake()
	{
		_targetElement = GetComponent<UIElement>();
	}

	private void Start()
	{
		_targetElement?.SelectChild(0);
	}

	protected virtual void InputUpdate()
	{
		if (!isActive || !_targetElement.IsActive || _targetElement.BlockedBySelection) return;
		
		if (GetInputValues(out var axisValH, out var axisValV))
		{
			_remainTime -= Time.deltaTime;

			if (_remainTime <= 0f)
			{
				_currentRepeatTime = Mathf.Clamp(_currentRepeatTime - repeatTimeAccelStep, repeatTime.y, repeatTime.x);
				_remainTime = _currentRepeatTime;

				var signH = Mathf.Abs(axisValH) > 0.5f ? Mathf.Sign(axisValH) * (inverseH ? -1f : 1f) : 0f;
				var signV = Mathf.Abs(axisValV) > 0.5f ? Mathf.Sign(axisValV) * (inverseV ? -1f : 1f) : 0f;

				NavigationStep((int) signH, (int) signV);
			}
		}
		else
		{
			_remainTime = 0f;
			_currentRepeatTime = repeatTime.x;
		}

		if (SubmitButtonUp())
		{
			_targetElement?.ActivateSelectedChild();
		}
		
		if (CancelButtonUp())
		{
			//CancelPressed();
		}
	}


	private void ResetInput()
	{
		Input.ResetInputAxes();
	}

	void NavigationStep(int x, int y)
	{
		var curX = transpose ? y : x;
		var curY = transpose ? x : y;
		
		if (rowCount < 0) rowCount = 0;
		var step =  curY * rowCount + curX;
		
		_targetElement.SelectNext(step);
	}

	protected bool GetInputValues(out float axisH, out float axisV)
	{
		axisH = 0f;
		axisV = 0f;

		bool hasAnyInput = false;
		if (Input.GetKey(prevKey)) { hasAnyInput = true; axisH = -1f;}
		if (Input.GetKey(nextKey)) { hasAnyInput = true; axisH = 1f;}
		if (Input.GetKey(prevRowKey)) { hasAnyInput = true; axisV = -1f;}
		if (Input.GetKey(nextRowKey)) { hasAnyInput = true; axisV = 1f;}
		
    
		return hasAnyInput;
	}

	protected bool CancelButtonUp()
	{
		return Input.GetKeyUp(cancelButton);
	}
   
	protected bool SubmitButtonUp()
	{
		return Input.GetKeyUp(submitButton);
	}


    // Update is called once per frame
    void Update()
    {
        InputUpdate();
    }
}
