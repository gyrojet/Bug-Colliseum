using System;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using System.Collections;
using Unity.VisualScripting;

public class Weapon : NetworkBehaviour
{
    public SpriteRenderer spriteRenderer;
    public CapsuleCollider2D collider;
    [SerializeField] Player player;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        collider = GetComponent<CapsuleCollider2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        try
        {
            if (collision.gameObject.tag == "Player")
            {
                if (player.isDefending == false)
                {
                    if (collision.gameObject.GetComponent<Player>().IsPlayerBlocking == false)
                    {
                        collision.gameObject.GetComponent<Player>().Die();
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
}
