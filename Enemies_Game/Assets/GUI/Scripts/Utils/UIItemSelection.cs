using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemSelection : UIElement
{
    private Action<UIElement, object> _fillDelegate;
    List<object> _objectsToFill = new List<object>();
    private int _pageSize = 1;
    private Action<object> _optionSelected;
    
    
    private void Start()
    {
        Get("SelectionOptions")?.HandleChildren(SelectionOptionActivated);
        Get<UIElementSpinner>("PagesSpinner")?.HandlePageChanged(PageChanged);
    }

    private void SelectionOptionActivated(UIElement elem)
    {
        _optionSelected?.Invoke(elem.UserData);
    }

    public void SetData(string caption, params object[] elements)
    {
        if(elements == null || elements.Length <= 0) return;
        
        SetDataToObject("CaptionText", caption);
        _objectsToFill.Clear();
        _objectsToFill.AddRange(elements);
        
        _pageSize = Get("SelectionOptions").Children.Count;
        
        Get<UIElementSpinner>("PagesSpinner")
            .SetMaxPage((int)Math.Ceiling((double)_objectsToFill.Count / _pageSize));
        
        PageChanged(1);
    }

    private void PageChanged(int pageNum)
    {
        HideAll();
        
        var pageSegmentStart = (pageNum - 1) * _pageSize;
        var pageSegmentEnd = pageNum * _pageSize;

        if (pageSegmentEnd >= _objectsToFill.Count) pageSegmentEnd = _objectsToFill.Count;

        var list = Get("SelectionOptions");
        int elemtIdx = 0;
        for (int i = pageSegmentStart; i < pageSegmentEnd; i++)
        {
            var element = list.Get(elemtIdx);
            var data = _objectsToFill[i];
            
            element.Show(true);
            element.UserData = data; 
            
            _fillDelegate?.Invoke(element, data);
            elemtIdx++;
        }
    }

    public void HideAll()
    {
        foreach (var child in Get("SelectionOptions").Children)
        {
            child.Show(false);
        }
    }

    public UIItemSelection HandleFillElement(Action<UIElement, object> fillDelegate)
    {
        _fillDelegate += fillDelegate;
        return this;
    }

    public UIItemSelection HandleElementSelected(Action<object> onSelected)
    {
        _optionSelected += onSelected;
        return this;
    }



}
