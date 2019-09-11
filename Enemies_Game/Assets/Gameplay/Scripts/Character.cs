using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Moving
{
    Nothing,
    UpLeft,
    UpRight,
    DownLeft,
    DownRight
}

public class Character : MonoBehaviour
{
    public LayerMask TrapsLayer;
    public LayerMask WallsLayer;
    public AudioClip DeathSound;

    private float _stepSize = 1f;
    private Vector3 legsVector = new Vector3(1f, 0f, 1f);

    public virtual void Move(Moving move)
    {
        var direction = GetDirection(move);

        if (Physics.Raycast(transform.position, direction, out var wall, _stepSize*2, WallsLayer))
        {
            Debug.Log($"{name}: It's wall.");
            return;
        }

        transform.position = transform.position + direction * _stepSize;
        if (Physics.Raycast(transform.position, Vector3.down, out var hit, _stepSize*2, TrapsLayer))
        {
            this.Death();
        }
    }

    protected bool TryToHitPlayer()
    {
        var playerPos = Player.Instance.transform.position;
        var dist = Vector3.Distance(playerPos, transform.position);
        return dist < 1f;
    }

    protected bool CheckForTraps(Moving move)
    {
        var newPos = transform.position + GetDirection(move) * _stepSize;
        if (Physics.Raycast(newPos, Vector3.down, out var hit, _stepSize*2, TrapsLayer))
        {
            Debug.Log($"{name}: No way! It's too dangerous!");
            return false;
        }

        return true;
    }

    protected Vector3 GetDirection(Moving move)
    {
        var direction = new Vector3();
        switch (move)
        {
            case Moving.UpLeft:
                {
                    direction = new Vector3(-1f, 0f, 0f);
                    break;
                }
            case Moving.UpRight:
                {
                    direction = new Vector3(0f, 0f, 1f);
                    break;
                }
            case Moving.DownLeft:
                {
                    direction = new Vector3(0f, 0f, -1f);
                    break;
                }
            case Moving.DownRight:
                {
                    direction = new Vector3(1f, 0f, 0f);
                    break;
                }
        }

        return direction;
    }

    public virtual void Death()
    {
        if (DeathSound != null)
            SoundManager.Instance.PlaySound(DeathSound);
        
        gameObject.SetActive(false);
        Debug.Log($"{name} is dead!");
    }

    public Vector3 Mult(Vector3 a, Vector3 b)
    {
        return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
    }
}
