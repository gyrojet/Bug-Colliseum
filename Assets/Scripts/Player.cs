using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using System.Collections;
using Unity.VisualScripting;

public class Player : NetworkBehaviour
{
    [SerializeField] private Vector3 playerRotation;                                    // Player's Orientation
    //private Sprite playerGraphics;
    [SerializeField] private float speed = 5f;                                          // Player's movespeed
    [SerializeField] private float originalSpeed = 5f;                                  // Used when dashing

    [SerializeField] public int playerIndex;                                            // Use this to get UI/Spawn points

    [SerializeField] private Transform player_RespawnPoint = null;                      // Player's assigned respawn point

    public Transform SpawnPoint                                                         // Gets/sets respawn
    {
        get { return player_RespawnPoint; }
        set
        {
            player_RespawnPoint = value;
        }
    }

    private Rigidbody2D rb;                                                             // Player's rigidbody

    [SerializeField] private Vector2 movement;                                          // Player's movement vector
    private bool isDashing;                                                             // Is the player dashing?

    [SerializeField] private float dashSpeed = 10f;                                     // Player's speed while dashing
    [SerializeField] private float dashDuration = 0.2f;                                 // Duration of a dash

    [SerializeField] private GameObject weapon;                                         // Player's weapon
    
    public bool isDefending = false;                                                    // Player is/is not blocking
    private bool isAttacking = false;                                                   // Player is/is not attacking

    public SpriteRenderer player_SR;                                                    // Sprite Renderer
    private Sprite player_Normal;
    private Sprite player_Defend;

    [SerializeField] private int life = 3;                                              // PLayer's lives
   
    public bool isDead = false;                                                         // If player is dead

    [SerializeField] PlayerManager playerManager;                                       // References to player, game manager
    [SerializeField] GameManager gameManager;



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

        if (gameManager == null)
        {
            gameManager = GameManager.instance;
        }

        try
        {
            playerManager.AddPlayerToList(this.gameObject);
            playerIndex = playerManager.NumberOfPlayers - 1;

            player_SR = GetComponent<SpriteRenderer>();
            rb = GetComponent<Rigidbody2D>();

            GetAndSetRotation();
            GetAndSetGraphics();
            
        }
        catch
        {
            Debug.Log("Unable to fetch Player Manager");
        }

        rb = GetComponent<Rigidbody2D>();

    }

    void Update()
    {
        if (!IsOwner) return;

        if (!Application.isFocused) return;

        if (Input.GetKeyDown(KeyCode.E) && !isDashing)
        {
            StartCoroutine(Dash());
        }

        if (Input.GetMouseButtonDown(0) && isAttacking == false && playerManager.HasGameStarted == true)
        {
            CallAttackRpc();
        }

        if (Input.GetMouseButtonDown(1) && playerManager.HasGameStarted == true && isDefending == false)
            CallDefendRpc();

        movement.x = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right
        movement.y = Input.GetAxisRaw("Vertical");   // W/S or Up/Down
    }

    void FixedUpdate()
    {
        // Move player using Rigidbody2D
        rb.linearVelocity = movement * speed;
    }

    IEnumerator Attack()
    {
        isAttacking = true;

        Vector3 originalPosition = weapon.transform.localPosition; // Save the original position (relative to parent)

        // Move weapon slightly forward
        weapon.transform.localPosition += new Vector3(0f, 0.5f, 0); // Adjust the offset as needed
        weapon.GetComponent<Weapon>().collider.enabled = true;
        weapon.GetComponent<SpriteRenderer>().enabled = true;

        yield return new WaitForSeconds(0.1f); // Pause for a short time

        // Return weapon to its original position
        weapon.transform.localPosition = originalPosition;
        weapon.GetComponent<Weapon>().collider.enabled = false;
        weapon.GetComponent<SpriteRenderer>().enabled = false;

        isAttacking = false;

    }

    IEnumerator Defend()
    {
        isDefending = true;

        Debug.Log("Defense UP!");

        yield return new WaitForSeconds(1f); // Pause for a short time

        isDefending = false;

        Debug.Log("Defense DOWN!");
    }

    void LowerShield()
    {
        //shield.transform.localPosition = shieldOriginalPosition;

        //if (shield.GetComponent<CapsuleCollider2D>().enabled != false)
        //    shield.GetComponent <CapsuleCollider2D>().enabled = false;

        isDefending = false;
    }

    
    private void GetAndSetRotation()
    {
        playerRotation = playerManager.GetPlayerRotation(playerIndex);
        transform.Rotate(playerRotation);
    }

    private void GetAndSetGraphics()
    {
        player_Normal = playerManager.GetPlayerSprite(playerIndex);
        // player_Defend = get defend idk

        player_SR.sprite = player_Normal;

        weapon.GetComponent<SpriteRenderer>().sprite = playerManager.GetPlayerWeapon(playerIndex);
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
            rb.constraints = RigidbodyConstraints2D.None;

            Debug.Log("FunctionHit!");
            gameObject.transform.position = new Vector3(newTransform.x, newTransform.y, 0);
            gameObject.transform.rotation = Quaternion.Euler(playerRotation);
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
    //public bool IsDead()
    //{
    //    if (life == 0)
    //        return isDead = true;

    //}

    [Rpc(SendTo.ClientsAndHost)]
    private void PlayerDeathEventRpc()
    {
        gameObject.layer = LayerMask.NameToLayer("IgnoreLayer");

        Vector3 deathZone = new Vector3(9999999f, 9999999f, 0f);
        
        SetNewTransform(deathZone);

        rb.constraints = RigidbodyConstraints2D.FreezeAll;

        if (isDead == false)
        {
            life -= 1;
            Debug.Log($"Current Life: {life}");

            if (life > 0)
                Invoke("RespawnPlayerRpc", 2.5f);
            else if (life <= 0)
            {
                Debug.Log("A Player has been killed!");
                isDead = true;
                gameManager.playerCounter--;
                Debug.Log("PLAYER COUNTER" +  gameManager.playerCounter);
            }
            //Debug.Log("Ur dead lol");
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void RespawnPlayerRpc()
    {
        player_SR.color = new Color(1f, 0f, 0f, 0.2f);

        SetNewTransform(SpawnPoint.transform.position);

        rb.constraints = RigidbodyConstraints2D.None;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        Invoke("ResetPlayerCollisionRpc", 2.5f);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ResetPlayerCollisionRpc()
    {
        player_SR.color = new Color(1f, 1f, 1f, 1f);

        gameObject.layer = LayerMask.NameToLayer("Player");
    }

}
