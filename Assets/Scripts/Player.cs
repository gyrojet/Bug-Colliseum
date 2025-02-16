using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using System.Collections;
using Unity.VisualScripting;

public class Player : NetworkBehaviour
{
    private Camera player_MainCam;

    [SerializeField] private float speed = 5f;
    [SerializeField] private float originalSpeed = 5f;

    [SerializeField] public int playerIndex;                                            // Use this to get UI/Spawn points

    [SerializeField] private Transform player_RespawnPoint = null;

    public Transform SpawnPoint
    {
        get { return player_RespawnPoint; }
        set
        {
            player_RespawnPoint = value;
        }
    }

    public bool IsPlayerBlocking { get { return isDefending; } }

    private Rigidbody2D rb;
    [SerializeField] private Vector2 movement;
    private bool isDashing;
    [SerializeField] private float dashSpeed = 10f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private GameObject weapon;
    [SerializeField] private GameObject shield;
    private bool isDefending = false;
    private bool isAttacking = false;
    private Vector3 shieldOriginalPosition;
    private SpriteRenderer player_SR;
    [SerializeField] private int life = 3;

    [SerializeField] PlayerManager playerManager;



    //NetworkVariable<Transform> 

    //WE MIGHT CHANGE FOR SPAWNING AT THE BEGGINING OF LEVELS SO WE USE THIS METHOD
    //public override void OnNetworkSpawn()
    //{
    //    base.OnNetworkSpawn();
    //    Initialize();
    //}

    
    void Awake()
    {
        if (playerManager == null)
        {
            playerManager = PlayerManager.pmInstance;
        }

        try
        {
            playerManager.AddPlayerToList(this.gameObject);
            playerIndex = playerManager.NumberOfPlayers - 1;

            player_SR = GetComponent<SpriteRenderer>();

            player_MainCam = Camera.main;
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
        //Debug.Log($"IsOwner: {IsOwner}, Local Player ID: {NetworkManager.Singleton.LocalClientId}, Object Owner ID: {OwnerClientId}");

        if (!IsOwner) return;

        if (!Application.isFocused) return;

        if (Input.GetKeyDown(KeyCode.E) && !isDashing)
        {
            StartCoroutine(Dash());
        }

        if (Input.GetMouseButtonDown(0) && isAttacking == false)
        {
            CallAttackRpc();
        }

        if (Input.GetMouseButtonDown(1))
            CallDefendRpc();

        if (Input.GetMouseButtonUp(1))
            CallLowerShieldRpc();

        movement.x = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right
        movement.y = Input.GetAxisRaw("Vertical");   // W/S or Up/Down

        FlipPlayerRpc();

        // Test, remove later
        if (Input.GetKeyDown(KeyCode.R) && playerManager.HasGameStarted == true)
        {
            Die();
        }
        else
        {
            Debug.Log("Please start the game first!");
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void FlipPlayerRpc()
    {
        if (movement.x < 0) 
        {
           
        }
        else if (movement.x > 0)
        {
            player_SR.flipX = true;
        }
        else if (movement.y < 0)
        {
            player_SR.flipY = false;
        }
        else if (movement.y > 0)
        {
            player_SR.flipY = true;
        }
    }

    void FixedUpdate()
    {
        // Move player using Rigidbody2D
        rb.linearVelocity = movement * speed;
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void UpdateLookPositionRpc()
    {
        LookAtCursor();
    }

    private void LookAtCursor()
    {
        Vector3 mousePosition = (Vector2)player_MainCam.ScreenToWorldPoint(Input.mousePosition);

        float angleRad = Mathf.Atan2(
            mousePosition.y - transform.position.y,
            mousePosition.x - transform.position.x);

        float angleDeg = (180 / Mathf.PI * angleRad - 90);

        transform.rotation = Quaternion.Euler(0f, 0f, angleDeg);
    }

    IEnumerator Attack()
    {
        isAttacking = true;

        Vector3 originalPosition = weapon.transform.localPosition; // Save the original position (relative to parent)

        // Move weapon slightly forward
        weapon.transform.localPosition += new Vector3(0f, 0.5f, 0); // Adjust the offset as needed
        weapon.GetComponent<Weapon>().collider.enabled = true;

        yield return new WaitForSeconds(0.1f); // Pause for a short time

        // Return weapon to its original position
        weapon.transform.localPosition = originalPosition;
        weapon.GetComponent<Weapon>().collider.enabled = false;
        isAttacking = false;

    }

    IEnumerator Defend()
    {
        isDefending = true;

        // Move weapon slightly forward
        shield.transform.localPosition += new Vector3(0, 0.5f, 0); // Adjust the offset as needed
        shield.GetComponent<CapsuleCollider2D>().enabled = true;

        yield return new WaitForSeconds(1f); // Pause for a short time

        // Return weapon to its original position
        LowerShield();


    }

    void LowerShield()
    {
        shield.transform.localPosition = shieldOriginalPosition;

        if (shield.GetComponent<CapsuleCollider2D>().enabled != false)
            shield.GetComponent <CapsuleCollider2D>().enabled = false;

        isDefending = false;
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void CallAttackRpc()
    {
        StartCoroutine(Attack());
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void CallDefendRpc()
    {
        StartCoroutine(Defend());
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void CallLowerShieldRpc()
    {
        LowerShield();
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

    
    // Gets new transform and moves player's location to that point
    public void SetNewTransform(Vector3 newTransform)
    {
        if (newTransform != null) 
        {
            Debug.Log("FunctionHit!");
            gameObject.transform.position = new Vector3(newTransform.x, newTransform.y, 0);
            gameObject.transform.rotation = Quaternion.identity;
        }
        else
        {
            Debug.Log($"Transform {newTransform} is null!");
        }
       
    }

    

    public void Die()
    {
        PlayerDeathEventRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void PlayerDeathEventRpc()
    {
        Vector3 deathZone = new Vector3(9999999f, 9999999f, 0f);
        
        SetNewTransform(deathZone);

        gameObject.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;

        gameObject.layer = LayerMask.NameToLayer("IgnoreLayer");

        life--;
        Debug.Log($"Current Life: {life}");

        if (life > 0)
            Invoke("RespawnPlayerRpc", 2.5f);
        else
            Debug.Log("Ur dead lol");
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void RespawnPlayerRpc()
    {
        player_SR.color = new Color(1f, 0f, 0f, 0.2f);

        SetNewTransform(SpawnPoint.transform.position);

        gameObject.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;

        Invoke("ResetPlayerCollisionRpc", 2.5f);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ResetPlayerCollisionRpc()
    {
        player_SR.color = new Color(1f, 1f, 1f, 1f);

        gameObject.layer = LayerMask.NameToLayer("Player");
    }

    //private void OnCollisionEnter2D(Collision2D collision)
    //{
    //    if (collision.gameObject.tag == "Weapon")
    //    {
    //        PlayerDeathEvent();
    //    }
    //}
}
