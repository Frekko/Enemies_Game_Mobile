using UnityEngine;
using UnityEngine.UI;

public class UIMarker : MonoBehaviour
{ 
    public Image icon;
    private bool _enabled = true;
    private Canvas _canvas;
    public virtual Vector3 currentPos { get; set; }

    public virtual bool IsEnabled => _enabled;
    public virtual bool IsVisible =>IsEnabled && UIUtils.IsPointInFront(currentPos);
    
    public void Remove()
    {
        _enabled = false;
        Destroy(gameObject);
    }
    
    public virtual void Setup(Canvas canvas)
    {
        _canvas = canvas;
    }
    
    public virtual void LateUpdate()
    {
        icon.gameObject.SetActive(IsVisible);
        if (!IsEnabled)
            return;
        
        transform.position = UIUtils.WorldToUISpace(_canvas, currentPos);
        
    }
}
