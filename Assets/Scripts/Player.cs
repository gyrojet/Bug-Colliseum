using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using System.Collections;
using Unity.VisualScripting;

public class Player : NetworkBehaviour
{
   [SerializeField] private float speed = 5f;
    [SerializeField] private float originalSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 movement;
    private bool isDashing;
    [SerializeField] private float dashSpeed = 10f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private GameObject weapon;
    [SerializeField] private GameObject shield;
    private bool isDefending = false;
    private Vector3 shieldOriginalPosition;
    private int life = 3;

    PlayerManager playerManager;

    //WE MIGHT CHANGE FOR SPAWNING AT THE BEGGINING OF LEVELS SO WE USE THIS METHOD
    //public override void OnNetworkSpawn()
    //{
    //    base.OnNetworkSpawn();
    //    Initialize();
    //}

    void Start()
    {
        if (playerManager == null)
        {
            playerManager = PlayerManager.pmInstance;
        }

        try
        {
            playerManager.AddPlayerToList(this.gameObject);
        }
        catch
        {
            Debug.Log("Unable to fetch Player Manager");
        }

        rb = GetComponent<Rigidbody2D>();
        shieldOriginalPosition = shield.transform.localPosition; // Store shield's starting position

    }

    void Update()
    {
        Debug.Log($"IsOwner: {IsOwner}, Local Player ID: {NetworkManager.Singleton.LocalClientId}, Object Owner ID: {OwnerClientId}");

        if (!IsOwner) return;

        if (!Application.isFocused) return;

        // Get Input

        if (Input.GetKeyDown(KeyCode.E) && !isDashing)
        {
            StartCoroutine(Dash());
        }
        if (Input.GetMouseButtonDown(0) )
            StartCoroutine(Attack());

        if (Input.GetMouseButtonDown(1))
            StartCoroutine(Defend());
        if (Input.GetMouseButtonUp(1))
            LowerShield();

        movement.x = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right
        movement.y = Input.GetAxisRaw("Vertical");   // W/S or Up/Down

        //movement = movement.normalized; // Prevents diagonal speed boost

        if (IsClient && Input.GetKeyDown(KeyCode.P))
        {
            // Client -> Server because PingRpc sends to Server
            PingRpc(10);
        }
    }

    void FixedUpdate()
    {
        // Move player using Rigidbody2D
        rb.linearVelocity = movement * speed;
    }

    IEnumerator Attack()
    {
        Vector3 originalPosition = weapon.transform.localPosition; // Save the original position (relative to parent)

        // Move weapon slightly forward
        weapon.transform.localPosition += new Vector3(0.5f, 0, 0); // Adjust the offset as needed
        yield return new WaitForSeconds(0.1f); // Pause for a short time

        // Return weapon to its original position
        weapon.transform.localPosition = originalPosition;

    }

    IEnumerator Defend()
    {
        isDefending = true;

        // Move weapon slightly forward
        shield.transform.localPosition += new Vector3(0, 0.5f, 0); // Adjust the offset as needed
        yield return new WaitForSeconds(1f); // Pause for a short time

        // Return weapon to its original position
        LowerShield();


    }

    void LowerShield()
    {
        shield.transform.localPosition = shieldOriginalPosition;

        isDefending = false;
    }
    IEnumerator Dash()
    {
        isDashing = true;
        speed = dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        speed = originalSpeed;
        isDashing=false;
    }

    [Rpc(SendTo.NotMe)]
    public void PingRpc(int pingCount)
    {
        // Server -> Clients because PongRpc sends to NotServer
        // Note: This will send to all clients.
        // Sending to the specific client that requested the pong will be discussed in the next section.
        PongRpc(pingCount, "PONG!");
    }

    [Rpc(SendTo.ClientsAndHost)]
    void PongRpc(int pingCount, string message)
    {
        Debug.Log($"Received pong from server for ping {pingCount} and message {message}");
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
       
            if (collision.gameObject.CompareTag("Weapon"))
            {

                Debug.Log("Collide with weapon");

            Damage();


            }
        if (collision.gameObject.CompareTag("Shield") && isDefending)
        {

            Debug.Log("Collide with shield/defended");



        }
    }

    private void OnApplicationQuit()
    {
        playerManager.RemovePlayerFromList(this.gameObject);
    }

    void Damage()
    {
        life--;
        Debug.Log("current life " + life.ToString());
    }

}
