using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFloatingTextSpawner : MonoBehaviour
{
	public GameObject prefab;
	public Canvas canvas;

	public static UIFloatingTextSpawner Instance;
	
	private void Awake()
	{
		if (canvas == null)
			canvas = GetComponentInParent<Canvas>();

		Instance = this;
	}
	
	public static void Spawn(string text, Vector3 startPos, Vector3 flyDir, Color color, float ttl = 1f)
	{
		if (Instance != null)
		{
			Instance.SpawnText(text, startPos, flyDir, color, ttl);
		}
	}

	public void SpawnText(string text, Vector3 startPos, Vector3 flyDir)
	{
		SpawnText(text, startPos,flyDir,  Color.white);
	}

	public void SpawnText(string text, Vector3 startPos,  Vector3 flyDir, Color color, float ttl = 1f)
	{
		var instance = Instantiate(prefab, transform);
		var script = instance.GetComponent<UIFloatingText>();
		
		script.Setup(text, canvas, startPos, flyDir,color, ttl);
	}

}
