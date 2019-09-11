using System;
using UniRx.Async.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;


public class UIDragRotationPanel :UIElement,IDragHandler,IEndDragHandler,IPointerDownHandler,IPointerUpHandler {

	public Vector2 rotationSpeed = Vector2.right;

	public Action<Vector2> dragStep;
	public Action<float> zoomStep;
	public Action<Vector2> beginDrag;
	public Action<Vector2> click;
	public bool smoothInput = true;
	
	
	private RectTransform _rect;

	private bool _isDrag = false;
	
	private Vector2 _prevPoint;
	private Vector2 _startPoint;
	public Action<Vector2> positionChanged;

  

	public void OnDrag(PointerEventData eventData)
	{
//		var delta = eventData.position - _prevPoint;
//		dragStep?.Invoke(Normalize(delta));
//		_prevPoint = eventData.position;
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		
	}
	new public void OnPointerDown(PointerEventData eventData)
	{
		
		_prevPoint = eventData.position;
		//_prevStepLen = _prevStepMinLen;
		//Debug.Log($"_touchId = {_touchId}");

		if (_touchId < 0)
		{
			if (Input.touches.Length > 0)
			{
				var touch = Input.touches[Input.touches.Length - 1];
				_touchId = touch.fingerId;
				_prevMousePos = touch.position;
			}
			else
			{
				_touchId = 0;
				_prevMousePos = ToVector2(Input.mousePosition);
			}
		}

		OnDrag(eventData);
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		OnEndDrag(eventData);
		_touchId = -1;
		_prevStepLen = 0f;
	}

	private int _touchId = -1;
	Vector2 _prevMousePos = Vector2.zero;

	private void Update()
	{
		if(_touchId < 0) return;
		
		foreach (var t in Input.touches)
		{
			if (t.fingerId == _touchId)
			{
				UpdatePos(t.position);
				break;
			}
		}

		if (Input.touches.Length <= 0)
		{
			UpdatePos(Input.mousePosition);
		}
	}

	private float _prevStepLen = 0f;
	private float _prevStepMinLen = 0.1f;
	private float _smoothingScale = 1.3f;
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

		dragStep?.Invoke(Normalize(delta));
		_prevMousePos = pos;
		
		_prevStepLen = delta.magnitude;

		//if (_prevStepLen < _prevStepMinLen) _prevStepLen = _prevStepMinLen;
	}

	Vector2 ToVector2(Vector3 pos)
	{
		return new Vector2(pos.x, pos.y);
	}


	private void Start()
	{
		_rect = GetComponent<RectTransform>();
	}


	Vector2 Normalize(Vector2 pos)
	{
		var rect = _rect.rect;
		return new Vector2(pos.x / rect.size.x, pos.y / rect.size.y);
	}



	public void NavigationRaw(Vector2 xy)
	{
		var rectSize = _rect.rect.size;
		var result = xy / rectSize * rotationSpeed;
		
		//Debug.Log($"NavigationRaw {xy}/{rectSize} * {rotationSpeed} = {result}");

		if (Input.touchCount > 1)
		{
			zoomStep?.Invoke(result.magnitude);
		}
		else
		{
			dragStep?.Invoke(result);	
		}
	}

	public void HandleDragEvents(Action<Vector2> dStep = null,  Action<Vector2> beginDrag = null ,   Action<Vector2> click = null, Action<float> zoom = null)
	{
		if (dStep != null) this.dragStep += dStep;
		if (beginDrag != null) this.beginDrag += beginDrag;
		if (click != null) this.click += click;
		if (zoom != null) this.zoomStep += zoom;
	}

}
