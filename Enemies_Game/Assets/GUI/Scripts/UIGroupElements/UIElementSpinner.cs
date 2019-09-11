using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIElementSpinner : UIElement
{
    private int _currentPage = 1;
    private int _maxPage = 1;

    private Action<int> _pageChanged;

    private void Start()
    {
        Get("LeftButton")?.SetOnActivated(LeftButtonClicked);
        Get("RightButton")?.SetOnActivated(RightButtonClicked);
    }

    public UIElementSpinner HandlePageChanged(Action<int> action)
    {
        Debug.Log($"HandlePageChanged {action}");
        
        _pageChanged += action;
        return this;
    }

    public UIElementSpinner SetMaxPage(int maxPage)
    {
        _currentPage = 0;
        _maxPage = maxPage;

        if (_maxPage <= 0) _maxPage = 1;
        
        SetPage(1);
        return this;
    }

    private void RightButtonClicked(UIElement obj)
    {
        var nextPage = _currentPage + 1;
        SetPage(nextPage);
    }

    private void LeftButtonClicked(UIElement obj)
    {
        var prevPage = _currentPage - 1;
        SetPage(prevPage);
    }

    public UIElementSpinner SetPage(int page)
    {
        var prevPage = _currentPage;
        _currentPage = page;
        if (_currentPage < 1) _currentPage = 1;
        if (_currentPage > _maxPage)
        {
            _currentPage = _maxPage;
        }

        Get("LeftButton")?.Show(_currentPage > 1);
        Get("RightButton")?.Show(_currentPage < _maxPage);
        SetDataToObject("Text", $"{_currentPage}/{_maxPage}");
        
        if (_currentPage != prevPage)
        {
            _pageChanged?.Invoke(_currentPage);
        }
        return this;
    }
}