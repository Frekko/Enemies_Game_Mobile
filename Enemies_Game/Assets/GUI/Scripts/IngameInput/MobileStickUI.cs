using System;
using UnityEngine;
using UnityEngine.EventSystems;
[RequireComponent(typeof(UnityEngine.UI.AspectRatioFitter))]
public class MobileStickUI : UIElement,IDragHandler,IEndDragHandler,IPointerDownHandler,IPointerUpHandler {

    public RectTransform Background;
    public RectTransform Knob;
  

    public float offset = 1f;
    Vector2 PointPosition;

    public Action<Vector2> positionChanged;

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
    private void Update()
    {
        var pos = Vector2.zero;
        
        if (Input.GetKey(KeyCode.W)) pos.y += 1;
        if (Input.GetKey(KeyCode.S)) pos.y -= 1;
        if (Input.GetKey(KeyCode.A)) pos.x -= 1;
        if (Input.GetKey(KeyCode.D)) pos.x += 1;

        SetPointPosition(pos);
    }
#endif
    

    public void OnDrag(PointerEventData eventData)
    {
        SetPointPosition(new Vector2(
            (eventData.position.x - Background.position.x) / ((Background.rect.size.x - Knob.rect.size.x) / 2),
            (eventData.position.y - Background.position.y) / ((Background.rect.size.y - Knob.rect.size.y) / 2)));
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        SetPointPosition(Vector2.zero);
    }
    new public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        OnEndDrag(eventData);
    }
    
    void SetPointPosition(Vector2 pos)
    {
        var npos = (pos.magnitude > 1.0f) ? pos.normalized : pos;

        if (npos != PointPosition)
        {
            PointPosition = npos;

            Knob.transform.position = new Vector2(
                (PointPosition.x * ((Background.rect.size.x - Knob.rect.size.x) / 2) * offset) + Background.position.x,
                (PointPosition.y * ((Background.rect.size.y - Knob.rect.size.y) / 2) * offset) + Background.position.y);
            
            positionChanged?.Invoke(PointPosition);
        }
    }

    public void Reset()
    {
        SetPointPosition(Vector2.zero);
    }


    public void HandleStickEvents(Action<Vector2> position = null)
    {
        positionChanged += position;
    }
}