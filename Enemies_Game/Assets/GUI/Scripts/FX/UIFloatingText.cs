using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIFloatingText : MonoBehaviour
{
	public TMPro.TextMeshProUGUI textField;
	public float gravity = 0f;
	public float damping = 1f;
	
	private Vector3 _flyVector = Vector3.up;
	private Vector3 _current3Dpos;
	private Canvas _canvas;

	private float _speed = 0f;
	private CanvasGroup _canvasGroup;
	private float _fadeSpeed = 1f;

	public void Setup(string text, Canvas rootCanvas, Vector3 startPos, Vector3 flyDir, Color color, float ttl =1f)
	{
		Destroy(gameObject, ttl);

		_flyVector = flyDir;
		_speed = _flyVector.magnitude;
		
		_current3Dpos = startPos;
		_canvas = rootCanvas;

		textField.text = text;
		textField.color = color;

		_canvasGroup = GetComponent<CanvasGroup>();
		_fadeSpeed = 1f / ttl;
	}

	private void Update()
	{
		_speed -= damping * Time.deltaTime;
		_flyVector += gravity * Time.deltaTime * Vector3.up;
		if (_flyVector.magnitude > _speed) _flyVector = _flyVector.normalized * _speed;
		
		_current3Dpos += _flyVector * Time.deltaTime;
		transform.position = UIUtils.WorldToUISpace(_canvas, _current3Dpos);

		if (_canvasGroup != null) _canvasGroup.alpha -= _fadeSpeed * Time.deltaTime;
	}
}
