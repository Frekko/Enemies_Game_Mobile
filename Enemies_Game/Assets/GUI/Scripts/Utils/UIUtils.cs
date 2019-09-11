using UnityEngine;

public class UIUtils 
{
	public static bool IsPointInFront(Vector3 worldPos)
	{
		var camera = Camera.main;
		if (camera == null) return true;
		
		var dirToPoint = worldPos - camera.transform.position;

		return Vector3.Dot(camera.transform.forward, dirToPoint) > 0f;
	}

	public static Vector3 WorldToUISpace(Canvas parentCanvas, Vector3 worldPos)
	{
		var camera = Camera.main;
		if (camera == null) return Vector3.zero;
		
		//Convert the world for screen point so that it can be used with ScreenPointToLocalPointInRectangle function
		Vector3 screenPos = camera.WorldToScreenPoint(worldPos);
		Vector2 movePos;

		//Convert the screenpoint to ui rectangle local point
		RectTransformUtility.ScreenPointToLocalPointInRectangle(parentCanvas.transform as RectTransform, screenPos, parentCanvas.worldCamera, out movePos);
		//Convert the local point to world point
		return parentCanvas.transform.TransformPoint(movePos);
	}
}
