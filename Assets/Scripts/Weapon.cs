using System;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using System.Collections;
using Unity.VisualScripting;

public class Weapon : NetworkBehaviour
{
    //  Class Name        :  Weapon
    //  Developer           :   Tyler Law, Julia Polak & Walesca Borges
    //
    //  Synopsis          :  Handles weapon interactions, including collisions with players and triggering player deaths.
    //  Date                : February 19th, 2025


    public SpriteRenderer spriteRenderer;       // Handles the weapon's visual representation
    public CapsuleCollider2D collider;          // Collider component for detecting collisions

    public GameManager gameManager;             // Reference to the GameManager instance

    //  Method Name        :  Start
    //  Synopsis          :  Initializes the weapon's components.
    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();        // Gets the SpriteRenderer component
        collider = GetComponent<CapsuleCollider2D>();           // Gets the CapsuleCollider2D component
    }


    //  Method Name        :  OnCollisionExit2D
    //  Synopsis          :  Detects when the weapon stops colliding with a player.
    private void OnCollisionExit2D(Collision2D collision)
    {
        // Checks if the object exiting collision is a player
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            // If the player is not defending, trigger their death
            if (collision.gameObject.GetComponent<Player>().isDefending ==  false) 
            {
                collision.gameObject.GetComponent<Player>().Die(); // Calls the Die() function to handle player death
            }
        }
    }
}
