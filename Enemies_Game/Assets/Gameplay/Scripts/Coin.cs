using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    public AudioClip TakeCoin;
    public void Start()
    {
        if (Player.Instance != null)
            Player.Instance.MoveEvent += TryToHitPlayer;
    }

    protected void TryToHitPlayer()
    {
        var playerPos = Player.Instance.transform.position;
        var coinPos = transform.position;

        if(playerPos.x == coinPos.x && playerPos.z == coinPos.z)
        {
            Player.Instance.MoveEvent -= TryToHitPlayer;

            if (TakeCoin != null)
                SoundManager.Instance.PlaySound(TakeCoin);

            Debug.Log($"You take Coin! Wow");
            
            gameObject.SetActive(false);
        }
    }
}
