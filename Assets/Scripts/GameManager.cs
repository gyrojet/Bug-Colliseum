using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{

    //  class Name        :   GameManager
    //
    //  Developer         :   Tyler Law, Julia Polak & Walesca Borges
    //                          
    //
    //  Synopsis          :   Manages the game state, including player count, countdown timer, 
    //                        and endgame conditions.
    //
    //  Date              :   February 19th, 2025


    [SerializeField] PlayerManager playerManager = null;                                // Reference to PlayerManager script
    [SerializeField] UIManager_Lobby uiManager = null;                                  // Reference to UIManager_Lobby script

    public int playerCounter;                                                           // Tracks the number of active players

    [SerializeField] private TextMeshProUGUI countDown;                                 // UI Element for displaying countdown
    [SerializeField] private GameObject timesUp;                                        // UI element for "Time's Up" message
    [SerializeField] private GameObject winner;                                         // UI element for displaying the winner

    public float countdownTime = 180f;                                                  // Total countdown time in seconds

    public static GameManager instance;                                                 // Singleton instance

    public Coroutine timerCoroutine;                                                    // Stores reference to the countdown coroutine

    public static GameManager Instance { get; private set; }                            // Public static property for accessing the instance
    [SerializeField] public TextMeshProUGUI lifes;                                      // UI element for displaying player lives

    void Awake()
    {
        Instance = this;                                                                // Assign the singleton instance
    }


    //  method Name    :  Start 
    //  Synopsis       :  Initializes singleton instance and references to required managers
    void Start()
    {
        if (instance == null)
            instance = this;                                                            // Assigns singleton instance if not already set

        if (playerManager == null)
            playerManager = PlayerManager.pmInstance;                                   // Fetches PlayerManager instance

        if (uiManager == null)
            uiManager = UIManager_Lobby.instance;                                       // Fetches UIManager instance

    }


    //  Method Name     :  FixedUpdate
    //  Synopsis        :  Constantly checks player count and stops the game if necessary
    void FixedUpdate()
    {
        if (playerCounter <= 1)                                                         // Ends game if there is only one or no players left
        {
            EndGame();                                                                  // Calls function to handle game-ending logic
            RpcStopTimerRpc();                                                          // Stops countdown timer for all players
        }
    }


    //  Method Name       :  GetPlayerCounterRpc
    //  Synopsis          :  Retrieves the number of players on the network

    [Rpc(SendTo.ClientsAndHost)]
    public void GetPlayerCounterRpc()
    {
        playerCounter = playerManager.NumberOfPlayers;                                   // Gets the player count from PlayerManager
    }


    //  Method Name       :  EndGame
    //  Synopsis          :  Determines the end game condition and updates UI accordingly
    void EndGame()
    {
        if (playerCounter <= 1)                                                          // If only one player remains, declare them the winner
        {
            winner.SetActive(true);                                                      // Display the winner UI element
        }
        else
        {
            timesUp.SetActive(true);                                                     // Display the "Time's Up" Menu
        }
    }


    //  Method Name       :  RpcStopTimerRpc
    //  Synopsis          :  Stops the countdown timer across all clients
    [Rpc(SendTo.ClientsAndHost)]
    private void RpcStopTimerRpc()
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);                                             // Stop the running coroutine
            timerCoroutine = null;                                                     // Clear the reference
        }
        Debug.Log("Timer Stopped for all players.");
    }


    //  Method Name       :  StartCountdown
    //  Synopsis          :  Begins the countdown timer
    public void StartCountdown()
    {
        timerCoroutine = StartCoroutine(Countdown());                                  // Start the countdown coroutine
    }


    //  Method Name  :  Countdown
    //  Synopsis     :  Handles countdown logic and updates UI display
    private IEnumerator Countdown()
    {
        float remainingTime = countdownTime;                                           // Set initial countdown time

        while (remainingTime > 0)
        {
            Debug.Log("Time left: " + remainingTime);
            countDown.text = remainingTime.ToString();                                 // Update UI display with remaining time
            yield return new WaitForSeconds(1f);                                       // Wait for 1 second
            remainingTime--;                                                           // Decrement timer
        }

        EndGame();                                                                     // End the game when countdown reaches zero
    }
}