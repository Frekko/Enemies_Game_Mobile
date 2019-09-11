using System;
using System.Collections;
using System.Collections.Generic;
using UniRx.Async;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class UIFades : MonoBehaviour
{
	public static UIFades Instance => _instance;

	private static UIFades _instance;

	public Image image;
	public UIParamAnimator animator;

	private CanvasGroup _canvasGroup;

	private string animKey = "Alpha";

	private void Awake()
	{
		_instance = this;
		if (image == null) image = GetComponent<Image>();
		_canvasGroup = GetComponent<CanvasGroup>();
		DontDestroyOnLoad(gameObject);

		animator.Anim(animKey)?.OnUpdate(paramAnimation => _canvasGroup.alpha = paramAnimation.Float());
	}

	public async UniTask FadeIn()
	{
		await animator.Anim(animKey).Play();
	}

	public async UniTask FadeOut()
	{
		await animator.Anim(animKey).Play(true);
	}
}
