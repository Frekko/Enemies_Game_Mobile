using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIScreenBase : MonoBehaviour
{
	public GameObject screenRoot;
	public UIElement uiRoot;

	public bool IsHidden => !screenRoot.activeInHierarchy;

	private Canvas _canvas;
	public Canvas Canvas => GetComponentInParent<Canvas>();

	public virtual void Init()
	{
		_canvas = GetComponentInParent<Canvas>();
		var elements = GetComponentsInChildren<UIElement>(true);

		foreach (var uiElement in elements)
		{
			uiElement.Init();
		}
	}

	public virtual void Show()
	{
		screenRoot?.SetActive(true);
	}

	public virtual void Hide()
	{
		screenRoot?.SetActive(false);
	}
}
