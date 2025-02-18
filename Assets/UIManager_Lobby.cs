using System.Collections;
using TMPro;
using Unity.Multiplayer.Center.NetcodeForGameObjectsExample.DistributedAuthority;
using Unity.Netcode;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.UI;

public class UIManager_Lobby : NetworkBehaviour
{

    //  Class Name        :  UIManager_Lobby
    //  Developer           :   Tyler Law, Julia Polak & Walesca Borges
    //  Synopsis          :  Manages the UI in the game lobby, including player count, game start conditions, 
    //                       and countdown timers before starting the game.
    //  Date                : February 19th, 2025



    [SerializeField] PlayerManager playerManager;          // Reference to the PlayerManager script

    [SerializeField] TextMeshProUGUI numPlayers;            // UI element displaying number of players
    [SerializeField] Button startGame;                      // Start game button reference

    [SerializeField] TextMeshProUGUI timerTime;             // Countdown timer UI element

    [SerializeField] GameObject serverUI;                   // UI panel for server settings
    [SerializeField] GameObject timerUI;                    // UI panel for countdown timer

    [SerializeField] int startTimer_CountdownTime = 3;      // Initial countdown time before game starts
    private int timer_InitialTime;                          // Stores the initial countdown value for resetting

    GameManager gameManager = null;                         // Reference to the GameManager script

    public static UIManager_Lobby instance;                 // Singleton instance for UIManager_Lobby



    //  Method Name        :  Start
    //  Synopsis          :  Initializes the UI manager, sets up event listeners, and disables the game manager initially.
    private void Start()
    {
        if (gameManager == null) 
        {
            gameManager = GameManager.instance;               // Assigns GameManager reference
        }

        gameManager.enabled = false;                        // Ensures the game manager is disabled at the beginning


        startGame.onClick.AddListener(StartGame);           // Adds StartGame() function to the start button click event

        timer_InitialTime = startTimer_CountdownTime;       // Stores the initial countdown timer value
    
    }

    //  Method Name        :  Update
    //  Synopsis          :  Updates the player count display and start button interactability.
    private void Update()
    {
        UpdatePlayerCount();                            // Updates the number of players
        SetStartGameButton();                           // Enables or disables the start button based on player count
    }


    //  Method Name        :  UpdatePlayerCount
    //  Synopsis          :  Updates the UI to reflect the current number of players.
    private void UpdatePlayerCount()
    {
        int count = playerManager.NumberOfPlayers;      // Retrieves the number of players from PlayerManager
        numPlayers.text = count.ToString();             // Updates the UI text
    }


    //  Method Name        :  SetStartGameButton
    //  Synopsis          :  Enables the start game button only if there are more than one player.
    private void SetStartGameButton()
    {
        if (playerManager.NumberOfPlayers > 1) 
        {
            startGame.interactable = true;              // Enables button when enough players are present
        }
        else
        {
            startGame.interactable = false;             // Disables button if there aren't enough players
        }
    }



    //  Method Name        :  SetGameManagerToPlayersRpc
    //  Synopsis          :  Synchronizes the player counter with the GameManager across all clients.
    [Rpc(SendTo.ClientsAndHost)]
    private void SetGameManagerToPlayersRpc()
    {
        gameManager.playerCounter = playerManager.NumberOfPlayers;      // Updates GameManager's player counter

        gameManager.enabled = true;                                     // Enables the GameManager


    }


    //  Method Name        :  StartGame
    //  Synopsis          :  Handles the logic to start the game, including setting up UI and game states.
    private void StartGame()
    {
        if (IsHost && playerManager.HasGameStarted == false)            // Ensures only the host can start the game and only if it hasn't started yet
        {
            SetGameManagerToPlayersRpc();                           // Synchronizes game manager settings


            SetValueOfLobbyUIRpc(false);                            // Hides lobby UI

            playerManager.RespawnAllPlayers();                      // Move all players to spawn points

            playerManager.FreezeAllPlayersRpc();                     // Freezes players to prevent movement before the game starts
                                                                     
            MessagePlayersToBeginCountdownRpc();                    // Starts the countdown timer for all players
        }
        else
        {
            Debug.Log("NOT HOST");
        }
    }

    //  Method Name        :  EndGame
    //  Synopsis          :  Ends the game and resets the lobby UI.
    public void EndGame()
    {
        Debug.Log("GAME OVER!!!!!!");

        playerManager.SetGameStatusRpc(false);          // Updates game state to inactive
        SetValueOfLobbyUIRpc(true);                     // Re-enables the lobby UI

        playerManager.RespawnAllPlayers();              // Respawns all players
        playerManager.UnfreezeAllPlayersRpc();          // Unfreezes player movement
    }

    //  Method Name        :  GameOverReset
    //  Synopsis          :  Resets the game status without restarting the entire lobby.
    public void GameOverReset()
    {
        playerManager.SetGameStatusRpc(false);
    }


    //  Method Name        :  SetValueOfLobbyUIRpc
    //  Synopsis          :  Toggles the visibility of the lobby UI for all players.
    [Rpc(SendTo.ClientsAndHost)]
    public void SetValueOfLobbyUIRpc(bool value)
    {
        serverUI.SetActive(value);
    }

    //  Method Name        :  SetValueOfTimerUIRpc
    //  Synopsis          :  Toggles the visibility of the timer UI for all players.
    [Rpc(SendTo.ClientsAndHost)]
    private void SetValueOfTimerUIRpc(bool value)
    {
        timerUI.SetActive(value);
    }

    //  Method Name        :  MessagePlayersToBeginCountdownRpc
    //  Synopsis          :  Triggers the countdown timer on all clients.
    [Rpc(SendTo.ClientsAndHost)]
    private void MessagePlayersToBeginCountdownRpc()
    {
        StartCoroutine(TimerCountdown());    // Start round countdown
    }

    //  Method Name        :  TimerCountdown
    //  Synopsis          :  Handles the countdown timer before the game starts.
    private IEnumerator TimerCountdown()
    {
        Debug.Log("Countdown Test START");

        while (startTimer_CountdownTime > 0) 
        {
            timerTime.text = startTimer_CountdownTime.ToString();       // Updates the countdown UI
            startTimer_CountdownTime--;                                 // Decreases the countdown timer

            yield return new WaitForSeconds(1f);                        // Waits for 1 second

            if (startTimer_CountdownTime <= 0)
            {
                playerManager.SetGameStatusRpc(true);                   // Starts the game
                playerManager.UnfreezeAllPlayersRpc();                  // Unfreezes player movement
                SetValueOfTimerUIRpc(false);                            // Hides the countdown timer UI
                gameManager.StartCountdown();                           // Starts the in-game timer
            }
        }
    }


    //  Method Name        :  ResetToLobby
    //  Synopsis          :  Resets the game state and sends players back to the lobby.
    public IEnumerator ResetToLobby()
    {
        Debug.Log("Called ResetGame()");

        yield return new WaitForSeconds(3f);                            // Waits for 3 seconds before resetting

        playerManager.ReturnAllPlayersToSpawnPointRpc();                // Moves all players back to their spawn points

        playerManager.UnfreezeAllPlayersRpc();                          // Unfreezes player movement

        SetValueOfLobbyUIRpc(true);                                     // Re-enables the lobby UI
    }
}
}
