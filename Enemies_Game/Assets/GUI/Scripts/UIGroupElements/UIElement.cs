using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIElement : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
{
   
    
    public int GroupIdx = -1;
    public bool ActiveSelf = true;
    public bool NeedSelectionToInteract = false;
    public object UserData = null;
    public static UIElement currentModalRoot = null;

    public bool AutoIdx = false;

    public Action<bool, UIElement> SelectionChanged;
    public Action<UIElement> Activated;
    public Action<UIElement> Shown;
    public Action<UIElement> Hidden;

    [HideInInspector]
    public UIElement Parent;

    List<UIElement> _children = new List<UIElement>();
    private bool _isSelected = false;

    private bool _isInited = false;

    public List<UIElement> Children => _children;

    protected string Id
    {
        get => gameObject.name;
        set => gameObject.name = value;
    }
    
    public bool IsSelected
    {
        set
        {
            if(!IsActive) return;
            
            if (_isSelected != value)
            {
                SelectionChanged?.Invoke(value, this);
            }

            _isSelected = value;
        }

        get
        {
         return   _isSelected;
        }
    }

    public bool BlockedByModal => Parent == null && currentModalRoot != null && currentModalRoot != this;
    public bool BlockedBySelection =>  NeedSelectionToInteract && !IsSelected;

    public bool IsSelectable
    {
        get => GroupIdx >= 0;
    }

    public bool IsActive 
    {
        get
        {
            if (!gameObject.activeInHierarchy) return false;
            
            if (BlockedByModal) return false;
            if (Parent == null) return ActiveSelf;
            
            return Parent.IsActive && ActiveSelf;
        }
    }

    public bool IsVisible => gameObject.activeSelf;
    


    private void Awake()
    {
        Init();
    }

    public void SetModal(bool modal = true)
    {
        if (!modal && currentModalRoot == this) currentModalRoot = null;

        if(modal) currentModalRoot = this;
    }

    public void Show(bool show)
    {
        gameObject.SetActive(show);
        
        if(show) Shown?.Invoke(this);
        if(!show) Hidden?.Invoke(this);
    }

    public void ShowAllChildren(bool show)
    {
        foreach (var child in _children)
        {
            child.Show(show);
        }
    }


    public virtual void Init()
    {
        if(_isInited) return;
        _isInited = true;
        
        var parent = transform.parent;
        if (parent != null)
        {
            Parent = parent.GetComponent<UIElement>();
        }
        Parent?.RegisterChild(this);
        
        SetUserData(Id);
        if (AutoIdx) GroupIdx = transform.GetSiblingIndex();
        
        //SetData(Id);
    }

    public UIElement SetOnSelected(Action<bool, UIElement> action)
    {
        if (action == null) return this;
        SelectionChanged += action;
        return this;
    }

    public UIElement SelectMe()
    {
        if (Parent == null) return this;

        Parent.SelectChild(Id);
        return this;
    }

    public UIElement SetOnSelectedChildren(Action<bool, UIElement> action)
    {
        foreach (var child in _children)
        {
            child.SetOnSelected(action);
        }
        return this;
    }
    
    public UIElement HandleShow(Action<UIElement> show, Action<UIElement> hide = null)
    {
        Shown += show;
        
        if(hide != null)
            Hidden += hide;
        
        return this;
    }
    
    public UIElement SetOnActivated(Action<UIElement> action)
    {
        if (action == null) return this;
        Activated += action;
        return this;
    }
    
    public UIElement SetOnActivatedChildren(Action<UIElement> action)
    {
        foreach (var child in _children)
        {
            child.SetOnActivated(action);
        }
        return this;
    }

    public UIElement HandleChild(string id, Action<UIElement> activated = null, Action<bool, UIElement> selected = null)
    {
        var child = Get(id);
        child?.SetOnActivated(activated);
        child?.SetOnSelected(selected);
        
        return this;
    }
    
    public UIElement HandleChildren(Action<UIElement> activated = null, Action<bool, UIElement> selected = null)
    {
        foreach (var child in _children)
        {
            child?.SetOnActivated(activated);
            child?.SetOnSelected(selected);
        }
        return this;
    }

    public void RegisterChild(UIElement child)
    {
        _children.Add(child);
    }

    public UIElement SelectChild(string id)
    {
        foreach (var c in _children)
        {
            if(c.IsSelectable)
                c.IsSelected = c.Id == id;
        }

        return this;
    }
    
    public void SelectChild(int idx)
    {
        foreach (var c in _children)
        {
            if(c.IsSelectable)
                c.IsSelected = c.GroupIdx == idx;
        }
    }

    public void SelectNext(int step)
    {
        var selectedChild = GetSelectedChild();
        int currentIdx = selectedChild == null ? -1 : selectedChild.GroupIdx;

        int target = currentIdx + step;
        int min = int.MaxValue;
        int max = 0;

        foreach (var c in _children)
        {
            if(c.GroupIdx == target) {SelectChild(target); return;}

            if (c.GroupIdx >= 0 && c.GroupIdx > max) max = c.GroupIdx;
            if (c.GroupIdx >= 0 && c.GroupIdx < min) min = c.GroupIdx;
        }
        
        if(target > max) SelectChild(max);
        if(target < min) SelectChild(min);
    }

    public void Activate()
    {
        if(!IsActive || BlockedBySelection) return;
        
        if(IsSelectable) IsSelected = true;
        Activated?.Invoke(this);
        
    }

    UIElement GetSelectedChild()
    {
        return _children.Find(e => e._isSelected == true);
    }

    public void ActivateSelectedChild()
    {
        GetSelectedChild()?.Activate();
    }

    public T Get<T>(string path) where T: UIElement
    {
        var result = Get(path);
        return result as T;
    }

    public UIElement Get(int idx)
    {
        foreach (var elem in _children)
        {
            if (elem.GroupIdx == idx)
                return elem;
        }

        return null;
    }

    public UIElement Get(string path)
    {
        var rootObj = FindTransform(path);
        var currentRoot = rootObj?.GetComponent<UIElement>();
        
        if(currentRoot == null)
            Debug.LogError($"Can't find element {path} under {Id}");
        
        return currentRoot;
    }
    
    

    UIElement GetChild(string childName)
    {
        return _children.Find(e => e.Id == childName);
    }
    
    UIElement GetChild(int idx)
    {
        return _children.Find(e => e.GroupIdx == idx);
    }

    #region UIData
   public UIElement SetUserData(object data)
    {
        UserData = data;

        return this;
    }

   
   

    public Transform FindTransform(string path)
    {
        if (string.IsNullOrEmpty(path)) return null;
        
        Transform currentRoot = transform.Find(path);

        if (currentRoot == null)
        {
            var paths = path.Split(new []{'/'}, 2);
            var rootPath = paths[0];
            

            var childrenTransforms = GetComponentsInChildren<Transform>(true);
            Transform rootObj = null;
            foreach (var childrenTransform in childrenTransforms)
            {
                if (childrenTransform.name == rootPath)
                {
                    rootObj = childrenTransform;
                    currentRoot = rootObj;
                    break;
                }
            }

            if (rootObj != null && paths.Length > 1 && !string.IsNullOrEmpty(paths[1]))
            {
                var childrenPath = paths[1];
                currentRoot = rootObj.Find(childrenPath);
            }
        }

        return currentRoot;
    }

    public UIElement SetDataToObject(string path, params object[] data)
    {
        Transform currentRoot = FindTransform(path);

        if (currentRoot != null)
        {
            foreach (var o in data)
            {
                SetDataToObject(currentRoot.gameObject, o);
            }
        }
        else
        {
            Debug.LogError($"Can't find object {path} under {Id}");
        }
        
        return this;
    }
    
    public UIElement SetDataToObjectChildren(string path, params object[] data)
    {
        Transform currentRoot = FindTransform(path);

        if (currentRoot != null)
        {
            foreach (var o in data)
            {
                SetDataToObjectWithChildren(currentRoot.gameObject, o);
            }
        }
        else
        {
            Debug.LogError($"Can't find object {path} under {Id}");
        }
        
        return this;
    }

    public UIElement SetDataWithChildren( params object[] data)
    {
        foreach (var o in data)
        {
            SetDataToObjectWithChildren(this.gameObject, o);
        }
        
        return this;
    }

    public UIElement SetObjectVisible(string path, bool visible)
    {
        Transform currentRoot = FindTransform(path);

        if (currentRoot != null)
        {
            currentRoot.gameObject.SetActive(visible);
        }
        else
        {
            Debug.LogError($"Can't find object {path} under {Id}");
        }
        
        return this;
    }
    
    public UIElement SetObjectInteractable(string path, bool interactable)
    {
        Transform currentRoot = FindTransform(path);

        if (currentRoot != null)
        {
            var uiElement = currentRoot.GetComponent<Graphic>();
            if (uiElement != null) uiElement.raycastTarget = interactable;
        }
        else
        {
            Debug.LogError($"Can't find object {path} under {Id}");
        }
        
        return this;
    }

    public UIElement GetNextHiddenChildren()
    {
        var retVal = _children.Find(c => !c.IsVisible);
        return retVal;
    }



    void FillUIComponentsByType(GameObject obj, object data, Image image, Text text, TMPro.TextMeshProUGUI advText)
    {
        if (data is float f && image != null  && image.type == Image.Type.Filled)
        {
            image.fillAmount = f;
        }

        if (data is string s)
        {
            if (text != null) text.text = s;
            if (advText != null) advText.text = s;
        }
        
        if (data is Color color)
        {
            if (text != null) text.color = color;
            if (advText != null) advText.color = color;
            if (image != null) image.color = color;
        }
        
        if (data is Sprite sprite)
        {
            if (image != null) image.sprite = sprite;
        }

        if (data is bool visible)
        {
            obj.SetActive(visible);
        } 
    }

    void SetDataToObject(GameObject obj, object data)
    {
        if (obj == null) return;

        var image = obj.GetComponent<Image>();
        var text = obj.GetComponent<Text>();
        var advText = obj.GetComponent<TMPro.TextMeshProUGUI>();

        FillUIComponentsByType(obj, data, image, text, advText);
    }
    
    void SetDataToObjectWithChildren(GameObject obj, object data)
    {
        if (obj == null) return;

        var image = obj.GetComponentInChildren<Image>();
        var text = obj.GetComponentInChildren<Text>();
        var advText = obj.GetComponentInChildren<TMPro.TextMeshProUGUI>();

        FillUIComponentsByType(obj, data, image, text, advText);
    }

    #endregion

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (IsSelectable)
        {
            if (Parent != null)
            {
                SelectMe();
            }
            else
            {
                IsSelected = true;    
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Activate();
    }
    
    #region Utils
    #if UNITY_EDITOR

    [ContextMenu("Auto idx")]
    void AutoIdxNoRename()
    {
        GenerateAutoIdx();
    }
    
    [ContextMenu("Auto idx rename")]
    void AutoIdxRename()
    {
        GenerateAutoIdx(true);
    }

    void GenerateAutoIdx(bool rename = false)
    {
        int elementsCount = 0;
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            var element = child.GetComponent<UIElement>();

            if (element != null)
            {
                element.Id = "";
                if (rename)
                {
                    var id = name + elementsCount;
                    child.name = id;
                    element.Id = id;
                }

                element.GroupIdx = elementsCount;
                elementsCount++;
            }
        }
    }
    #endif
    
    #endregion

#if UNITY_EDITOR
    [MenuItem("GameObject/UIElement/HierarchyName", false, 0)]
    static void GetPath()
    {
        if(Selection.activeObject == null) return;
        
        var parents = new List<Transform>();

        var currentRoot = ((GameObject)Selection.activeObject).transform;

        while (currentRoot != null)
        {
            var parent = currentRoot.transform.parent;
            
            if(parent == null) break;
            
            parents.Add(currentRoot);
            currentRoot = parent;
        }
        
        parents.Reverse();

        var path = "";
        foreach (var o in parents)
        {
            path += o.name + "/";
        }

        path = path.TrimEnd('/');

        EditorGUIUtility.systemCopyBuffer = path;
        Debug.Log($"Element path {path}");
    }
#endif
}
