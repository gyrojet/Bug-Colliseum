using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using System.Collections;

public class Player : NetworkBehaviour
{
   [SerializeField] private float speed = 5f;
    [SerializeField] private float originalSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 movement;
    private bool isDashing;
    [SerializeField] private float dashSpeed = 10f;
    [SerializeField] private float dashDuration = 0.2f;


    //WE MIGHT CHANGE FOR SPAWNING AT THE BEGGINING OF LEVELS SO WE USE THIS METHOD
    //public override void OnNetworkSpawn()
    //{
    //    base.OnNetworkSpawn();
    //    Initialize();
    //}

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if(!IsOwner || !Application.isFocused) return;
        // Get Input

        if (Input.GetKeyDown(KeyCode.E) && !isDashing)
        {
            StartCoroutine(Dash());
        }

        movement.x = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right
        movement.y = Input.GetAxisRaw("Vertical");   // W/S or Up/Down

        movement = movement.normalized; // Prevents diagonal speed boost
    }

    void FixedUpdate()
    {
        // Move player using Rigidbody2D
        rb.linearVelocity = movement * speed;
    }

    IEnumerator Dash()
    {
        isDashing = true;
        speed = dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        speed = originalSpeed;
        isDashing=false;
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Weapon"))
        {

            Debug.Log("Collide with weapon");

            //add damage to this player


        }
    }



}
