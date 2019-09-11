using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class Player : Character
{
    public static Player Instance;

    public Action MoveEvent;
    public Action DeadEvent;

    public void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public override void Move(Moving move)
    {
        base.Move(move);
        MoveEvent?.Invoke();
    }

    public override void Death()
    {
        DeadEvent?.Invoke();
        base.Death();
        SceneManager.LoadScene("SampleScene");
    }
}
