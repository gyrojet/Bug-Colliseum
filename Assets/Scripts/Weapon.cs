using System;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using System.Collections;
using Unity.VisualScripting;

public class Weapon : NetworkBehaviour
{
    public SpriteRenderer spriteRenderer;
    public CircleCollider2D collider;
    
    public GameManager gameManager;
    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        collider = GetComponent<CircleCollider2D>();
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (collision.gameObject.GetComponent<Player>().isDefending ==  false) 
            {
                collision.gameObject.GetComponent<Player>().Die();
            }
        }
    }
}
