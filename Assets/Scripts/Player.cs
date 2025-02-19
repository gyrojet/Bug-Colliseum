using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using System.Collections;
using Unity.VisualScripting;
using TMPro;

public class Player : NetworkBehaviour
{

    //  class Name     :   Player
    //
    //  Developer      :   Tyler Law, Julia Polak & Walesca Borges
    //                          
    //  Synopsis       :   Manages player movement, attacks, dashes, defense, respawning, and networking.
    //  
    //  Date           :   February 19th, 2025


    [SerializeField] private Vector3 playerRotation;                                    // Player's Orientation
    [SerializeField] private float speed = 5f;                                          // Player's movespeed
    [SerializeField] private float originalSpeed = 5f;                                  // Used when dashing

    [SerializeField] public int playerIndex;                                            // Use this to get UI/Spawn points

    [SerializeField] private Transform player_RespawnPoint = null;                      // Player's assigned respawn point

    private Rigidbody2D rb;                                                             // Player's rigidbody

    [SerializeField] private Vector2 movement;                                          // Player's movement vector
    private bool isDashing;                                                             // Is the player dashing?

    [SerializeField] private float dashSpeed = 10f;                                     // Player's speed while dashing
    [SerializeField] private float dashDuration = 0.2f;                                 // Duration of a dash

    [SerializeField] private GameObject weapon;                                         // Player's weapon
    
    public bool isDefending = false;                                                    // Player is/is not blocking
    private bool isAttacking = false;                                                   // Player is/is not attacking

    public SpriteRenderer player_SR;                                                    // Sprite Renderer
    private Sprite player_Normal;                                                       // Default sprite
    private Sprite player_Defend;                                                       // Defense mode sprite

    [SerializeField] public int life = 3;                                               // PLayer's lives
    [SerializeField] private TextMeshPro lifes;                                         // Display lives         


    public bool isDead = false;                                                         // If player is dead

    [SerializeField] PlayerManager playerManager;                                       // References to player & game manager
    [SerializeField] GameManager gameManager;


    //  Property:  SpawnPoint
    //  Synopsis:  Gets or sets the player's respawn point
    public Transform SpawnPoint
    {
        get { return player_RespawnPoint; }                                             // Gets the player's respawn point
        set
        {
            player_RespawnPoint = value;                                                // Sets the player's respawn point
        }
    }


    //  Method Name       :  Awake
    //  Synopsis          :  Initializes references to managers and sets up player settings
    void Awake()
    {
        if (GameManager.Instance != null && GameManager.Instance.lifes != null)
        {
            GameManager.Instance.lifes.text = "LIVES: " + lifes.ToString();             // Update UI with number of lives
        }

        if (playerManager == null)
        {
            playerManager = PlayerManager.pmInstance;                                   // Fetches PlayerManager instance
        }

        if (gameManager == null)
        {
            gameManager = GameManager.instance;                                         // Fetches GameManager instance
        }

        try
        {
            playerManager.AddPlayerToList(this.gameObject);                             // Adds player to manager list
            playerIndex = playerManager.NumberOfPlayers - 1;                            // Assigns player index

            player_SR = GetComponent<SpriteRenderer>();                                 // Gets sprite renderer
            rb = GetComponent<Rigidbody2D>();                                           // Gets Rigidbody2D

            GetAndSetRotation();                                                        // Sets player rotation
            GetAndSetGraphics();                                                        // Assigns correct graphics
        }
        catch
        {
            Debug.Log("Unable to fetch Player Manager");                                // Log error if Player Manager is not available
        }

        rb = GetComponent<Rigidbody2D>();                                               // Ensures Rigidbody2D is assigned

    }


    //  Method Name       :  Update
    //  Synopsis          :  Handles input and player actions
    void Update()
    {   
        if (!IsOwner) return;                                                          // Ensures only the owner processes input

        if (!Application.isFocused) return;                                            // Ensures player is active


        if (Input.GetKeyDown(KeyCode.E) && !isDashing)                                 // Dash mechanic
        {
            StartCoroutine(Dash());
        }

        if (Input.GetMouseButtonDown(0) && isAttacking == false && playerManager.HasGameStarted == true && isDefending == false)            // Attack mechanic
        {
            CallAttackRpc();
        }

        if (Input.GetMouseButtonDown(1) && playerManager.HasGameStarted == true && isDefending == false && isAttacking == false)            // Defense mechanic
            CallDefendRpc();

        // Movement input handling
        movement.x = Input.GetAxisRaw("Horizontal");                                   // A/D or Left/Right
        movement.y = Input.GetAxisRaw("Vertical");                                     // W/S or Up/Down
    }


    //  Method Name       :  FixedUpdate
    //  Synopsis          :  Moves the player
    void FixedUpdate()
    {
        rb.linearVelocity = movement * speed;                                         // Move player using Rigidbody2D based on input
    }


    //  Method Name       :  Attack
    //  Synopsis          :  Handles attacking logic
    IEnumerator Attack()
    {
        isAttacking = true;                                                          // Ensures isAttacking is happening

        Vector3 originalPosition = weapon.transform.localPosition;                   // Save the original weapon position (relative to parent)

        // Move weapon slightly forward
        weapon.transform.localPosition += new Vector3(0f, 0.5f, 0);                  // Adjust the offset as needed
        weapon.GetComponent<Weapon>().collider.enabled = true;
        weapon.GetComponent<SpriteRenderer>().enabled = true;

        yield return new WaitForSeconds(0.1f);                                       // Attack duration

        weapon.transform.localPosition = originalPosition;                           // Return weapon to its original position
        weapon.GetComponent<Weapon>().collider.enabled = false;
        weapon.GetComponent<SpriteRenderer>().enabled = false;

        isAttacking = false;                                                         // Ensures isAttacking has ended

    }


    //  Method Name       :  Defend
    //  Synopsis          :  Handles defense logic
    IEnumerator Defend()
    {
        isDefending = true;                                                         // Ensures isDefending is happening

        player_SR.sprite = player_Defend;                                           // Plays the assigned sprite

        yield return new WaitForSeconds(1f);                                        // Defense duration

        isDefending = false;                                                        // Ensures isDefending has ended

        player_SR.sprite = player_Normal;                                           // Returns the regular sprite
    }


    //  Method Name       :  GetAndSetRotation
    //  Synopsis          :  Retrieves and applies the player's initial rotation based on their index.
    private void GetAndSetRotation()
    {
        playerRotation = playerManager.GetPlayerRotation(playerIndex);              // Gets the rotation assigned to this player
        transform.Rotate(playerRotation);                                           // Applies the retrieved rotation to the player
    }


    //  Method Name       :  GetAndSetGraphics
    //  Synopsis          :  Assigns the correct sprites for the player based on their index.
    private void GetAndSetGraphics()
    {
        player_Normal = playerManager.GetPlayerSprite(playerIndex);                // Gets player's normal sprite
        player_Defend = playerManager.GetPlayerDefenseSprites(playerIndex);        // Gets player's defending sprite

        player_SR.sprite = player_Normal;                                          // Sets player's default sprite

        weapon.GetComponent<SpriteRenderer>().sprite = playerManager.GetPlayerWeapon(playerIndex); // Sets player's weapon sprite
    }


    //  Method Name       :  CallAttackRpc
    //  Synopsis          :  Calls the attack function across all clients.
    [Rpc(SendTo.ClientsAndHost)]
    private void CallAttackRpc()
    {
        StartCoroutine(Attack());                                                  // Starts the attack coroutine
    }


    //  Method Name       :  CallDefendRpc
    //  Synopsis          :  Calls the defense function across all clients.
    [Rpc(SendTo.ClientsAndHost)]
    private void CallDefendRpc()
    {
        StartCoroutine(Defend());                                                  // Starts the defense coroutine
    }


    //  Method Name       :  Dash
    //  Synopsis          :  Temporarily increases player speed
    IEnumerator Dash()
    {
        isDashing = true;                                                          // Sets dashing state to true
        speed = dashSpeed;                                                         // Increases speed to dash speed

        yield return new WaitForSeconds(dashDuration);                             // Waits for the dash duration


        speed = originalSpeed;                                                     // Resets speed back to normal
        isDashing = false;                                                         // Ends dashing state
    }


    //  Method Name       :  PingRpc
    //  Synopsis          :  Sends a ping request from the server to all clients except itself.
    [Rpc(SendTo.NotMe)]
    public void PingRpc(int pingCount)
    {
        // Server -> Clients because PongRpc sends to NotServer
        // Note: This will send to all clients.
        PongRpc(pingCount, "PONG!");                                               // Calls PongRpc with a response message
    }


    //  Method Name       :  PongRpc
    //  Synopsis          :  Receives a response from the server to confirm the ping was received.
    [Rpc(SendTo.ClientsAndHost)]
    void PongRpc(int pingCount, string message)
    {
        Debug.Log($"Received pong from server for ping {pingCount} and message {message}");
    }


    //  Method Name       :  SetNewTransform
    //  Synopsis          :  Moves the player to a new transform position, adjusting constraints if necessary.
    public void SetNewTransform(Vector3 newTransform)
    {
        if (newTransform != null) 
        {
            rb.constraints = RigidbodyConstraints2D.None;                                   // Unlocks movement constraints

            gameObject.transform.position = new Vector3(newTransform.x, newTransform.y, 0); // Moves player to new position
            gameObject.transform.rotation = Quaternion.Euler(playerRotation);               // Applies player's rotation
        }
        else
        { 
            Debug.Log($"Transform {newTransform} is null!");                                // Logs error if transform is invalid
        
        }
       
    }


    //  Method Name       :  Die
    //  Synopsis          :  Handles player death and triggers respawn or elimination.
    //  Note              :  Bug: Life sometimes decreases twice due to an unknown issue.
    //                       Developers are aware, but due to a tight deadline, it could not be fully resolved.
    public void Die()
    {
        PlayerDeathEventRpc();                                                              // Calls death event RPC to notify all clients
    }


    //  Method Name       :  PlayerDeathEventRpc
    //  Synopsis          :  Handles player death event on all clients, removing player from active play.
    [Rpc(SendTo.ClientsAndHost)]
    private void PlayerDeathEventRpc()
    {
        gameObject.layer = LayerMask.NameToLayer("IgnoreLayer");                             // Moves player to an "Ignore" layer to prevent interactions
           
        Vector3 deathZone = new Vector3(9999999f, 9999999f, 0f);                             // Moves player out of the game area

        SetNewTransform(deathZone);                                                          // Applies the movement
        
        rb.constraints = RigidbodyConstraints2D.FreezeAll;                                   // Prevents further movement

        if (isDead == false)                                                                 // Ensures the death event only happens once
        {
            life -= 1;                                                                       // Decreases player's life count
            Debug.Log($"Current Life: {life}");
             GameManager.Instance.lifes.text =  "LIVES: " + life.ToString();

            if (life > 0)
                Invoke("RespawnPlayerRpc", 2.5f);                                            // Triggers respawn after a delay
        
        else if (life <= 0)                                                                  // If life reaches zero or less, player is eliminated
            {
                Debug.Log("A Player has been killed!");
                isDead = true;                                                               // Sets player to dead
                gameManager.playerCounter--;                                                 // Decreases the total player count
            }
        }
    }


    //  Method Name       :  RespawnPlayerRpc
    //  Synopsis          :  Respawns the player at their spawn point after a delay.
    [Rpc(SendTo.ClientsAndHost)]
    private void RespawnPlayerRpc()
    {
        player_SR.color = new Color(1f, 0f, 0f, 0.2f);                                       // Makes player semi-transparent upon respawn

        SetNewTransform(SpawnPoint.transform.position);                                      // Moves player to their respawn point

        rb.constraints = RigidbodyConstraints2D.None;                                        // Unlocks movement
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;                              // Prevents unwanted rotation

        Invoke("ResetPlayerCollisionRpc", 2.5f);                                             // Restores player collision after a delay
}
    //  Method Name       :  ResetPlayerCollisionRpc
    //  Synopsis          :  Restores player's collision settings after respawning.
    [Rpc(SendTo.ClientsAndHost)]
    private void ResetPlayerCollisionRpc()
    {
        player_SR.color = new Color(1f, 1f, 1f, 1f);                                         // Restores player's visibility

        gameObject.layer = LayerMask.NameToLayer("Player");                                  // Sets layer back to "Player" to allow normal interactions
    }
}