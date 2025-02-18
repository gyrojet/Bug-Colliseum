using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    //  class Name    :   GameManager
    //
    //  Developer           :   Tyler Law, Julia Polak & Walesca Borges
    //                          
    //
    //  Synopsis            : Manages the game state, including player count, countdown timer, 
    //                          and endgame conditions.
    //
    //  Date                : February 19th, 2025

    // References to other manager scripts
    PlayerManager playerManager = null;
    UIManager_Lobby uiManager = null;

    // Tracks the number of active players
    public int playerCounter;

    // UI Elements
    [SerializeField] private TextMeshProUGUI countDown;
    [SerializeField] private GameObject timesUp;
    [SerializeField] private GameObject winner;


    // Total countdown time in seconds
    public float countdownTime = 180f;

    // Singleton instance
    public static GameManager instance;

    // Stores reference to the countdown coroutine
    public Coroutine timerCoroutine;


   //  method Name    :  Start 
    //  Synopsis   : Initializes singleton instance and references to required managers
    void Start()
    {
        if (instance == null)
            instance = this; // Assigns singleton instance if not already set

        if (playerManager == null)
            playerManager = PlayerManager.pmInstance; // Fetches PlayerManager instance

        if (uiManager == null)
            uiManager = UIManager_Lobby.instance;  // Fetches UIManager instance

    }

 //  Method Name        :  FixedUpdate
    //  Synopsis          :  Constantly checks player count and stops the game if necessary
    void FixedUpdate()
    {
        // Ends game if there is only one or no players left
        if (playerCounter <= 1)
        {
            EndGame(); // Calls function to handle game-ending logic
            RpcStopTimerRpc(); // Stops countdown timer for all players
        }
    }

    //  Method Name        :  GetPlayerCounterRpc
    //  Synopsis          :  Retrieves the number of players on the network

    [Rpc(SendTo.ClientsAndHost)]
    public void GetPlayerCounterRpc()
    {
        playerCounter = playerManager.NumberOfPlayers; // Gets the player count from PlayerManager
    }

    //  Method Name        :  EndGame
    //  Synopsis          :  Determines the end game condition and updates UI accordingly
    void EndGame()
    {
       
        // If only one player remains, declare them the winner
        if (playerCounter <= 1)
        {
            Debug.Log("You Win!!");
            //winner.SetActive(true);

            ResetGame();
        }
        else
        {
            Debug.Log("Time is up!");
            timesUp.SetActive(true); // Displays the time-up UI
        }


    }

   
    private void ResetGame()
    {
        
    }

    //  Method Name        :  RpcStopTimerRpc
    //  Synopsis          :  Stops the countdown timer across all clients
    [Rpc(SendTo.ClientsAndHost)]
    private void RpcStopTimerRpc()
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine); // Stops the running coroutine
            timerCoroutine = null; // Clears the reference
        }
        Debug.Log("Timer Stopped for all players.");
    }

    //  Method Name        :  StartCountdown
    //  Synopsis          :  Begins the countdown timer
    public void StartCountdown()
    {
        timerCoroutine = StartCoroutine(Countdown()); // Starts the countdown coroutine
    }

 //  Method Name  :  Countdown
    //  Synopsis  :  Handles countdown logic and updates UI display
    private IEnumerator Countdown()
    {
        float remainingTime = countdownTime;  // Sets initial time

        while (remainingTime > 0)
        {
            Debug.Log("Time left: " + remainingTime);
            countDown.text = remainingTime.ToString();      // Updates UI display
            yield return new WaitForSeconds(1f);         // Waits for 1 second
            remainingTime--;                            // Decrements timer
        }

        EndGame();                          // Ends the game when countdown reaches zero
    }
       

    }
