using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SmartEnemy : Character
{
    public int StepCount = 1;

    public void Start()
    {
        if (Player.Instance != null)
            Player.Instance.MoveEvent += OnPlayerMoved;
    }

    private void OnPlayerMoved()
    {
        for(var i = 0; i < StepCount; i++)
        {
            var playerPos = Player.Instance.transform.position;
            var ourPos = transform.position;
            var direction = Player.Instance.transform.position - transform.position;

            Moving move = Moving.Nothing;
            bool isMoving = false;

            if (Math.Abs(direction.x) > Math.Abs(direction.z))
            {
                if (playerPos.x > ourPos.x && CheckForTraps(Moving.DownRight))
                {
                    move = Moving.DownRight;
                    isMoving = true;
                }

                if (playerPos.x < ourPos.x && CheckForTraps(Moving.UpLeft))
                {
                    move = Moving.UpLeft;
                    isMoving = true;
                }
            }

            if (!isMoving)
            {
                if (playerPos.z < ourPos.z && CheckForTraps(Moving.DownLeft))
                {
                    move = Moving.DownLeft;
                    isMoving = true;
                }

                if (playerPos.z > ourPos.z && !isMoving && CheckForTraps(Moving.UpRight))
                {
                    move = Moving.UpRight;
                    isMoving = true;
                }
            }

            if (!isMoving)
            {
                if (playerPos.x > ourPos.x && CheckForTraps(Moving.DownRight))
                {
                    move = Moving.DownRight;
                    isMoving = true;
                }

                if (playerPos.x < ourPos.x && CheckForTraps(Moving.UpLeft))
                {
                    move = Moving.UpLeft;
                    isMoving = true;
                }
            }

            if (isMoving)
                Move(move);

            if(TryToHitPlayer())
            {
                Player.Instance?.Death();
                Debug.Log($"You are dead!");
                return;
            }
        }
    }

    public override void Move(Moving move)
    {
        base.Move(move);
    }
}
