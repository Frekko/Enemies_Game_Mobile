using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Character
{
    public List<Moving> Route;
    private int _currentStep;

    public void OnEnable()
    {
        if (Player.Instance != null)
            Player.Instance.MoveEvent += OnPlayerMoved;
    }

    private void OnPlayerMoved()
    {
        if (_currentStep >= Route.Count)
        {
            _currentStep = 0;
        }

        Move(Route[_currentStep]);
        _currentStep++;

        if (TryToHitPlayer())
        {
            Player.Instance?.Death();
            Debug.Log($"You are dead!");
            return;
        }
    }

    public override void Move(Moving move)
    {
        base.Move(move);
    }
}
