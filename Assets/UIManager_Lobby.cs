using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UIManager_Lobby : NetworkBehaviour
{
    [SerializeField] PlayerManager pm;

    [SerializeField] TextMeshProUGUI numPlayers;
    [SerializeField] Button startGame;

    [SerializeField] TextMeshProUGUI timerTime;

    [SerializeField] GameObject serverUI;
    [SerializeField] GameObject timerUI;

    [SerializeField] int startTimer_CountdownTime = 3;
    private int timer_InitialTime;

    GameManager gameManager = null;

    public static UIManager_Lobby instance;

    private void Start()
    {
        if (gameManager == null) 
        {
            gameManager = GameManager.instance;
        }

        startGame.onClick.AddListener(StartGame);

        timer_InitialTime = startTimer_CountdownTime;
    }

    private void Update()
    {
        UpdatePlayerCount();
        SetStartGameButton();
    }

    private void UpdatePlayerCount()
    {
        int count = pm.NumberOfPlayers;
        numPlayers.text = count.ToString();
    }

    private void SetStartGameButton()
    {
        if (pm.NumberOfPlayers > 1) 
        {
            startGame.interactable = true;
        }
        else
        {
            startGame.interactable = false;
        }
    }

    private void StartGame()
    {
        if (IsHost && pm.HasGameStarted == false)
        {
            gameManager.playerCounter = pm.NumberOfPlayers;

            SetValueOfLobbyUIRpc(false);

            pm.RespawnAllPlayers();            // Send all players to spawn points

            pm.FreezeAllPlayersRpc();               // Freeze movement of all players

            MessagePlayersToBeginCountdownRpc();
        }
        else
        {
            Debug.Log("NOT HOST");
        }
    }

    public void EndGame()
    {
        Debug.Log("GAME OVER!!!!!!");

        pm.SetGameStatusRpc(false);
        SetValueOfLobbyUIRpc(true);

        pm.RespawnAllPlayers();
        pm.UnfreezeAllPlayersRpc();
    }

    public void GameOverReset()
    {
        pm.SetGameStatusRpc(false);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SetValueOfLobbyUIRpc(bool value)
    {
        serverUI.SetActive(value);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SetValueOfTimerUIRpc(bool value)
    {
        timerUI.SetActive(value);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void MessagePlayersToBeginCountdownRpc()
    {
        StartCoroutine(TimerCountdown());    // Start round countdown
    }


    private IEnumerator TimerCountdown()
    {
        Debug.Log("Countdown Test START");

        while (startTimer_CountdownTime > 0) 
        {
            timerTime.text = startTimer_CountdownTime.ToString();
            startTimer_CountdownTime--;

            yield return new WaitForSeconds(1f);

            if (startTimer_CountdownTime <= 0)
            {
                pm.SetGameStatusRpc(true);
                pm.UnfreezeAllPlayersRpc();
                SetValueOfTimerUIRpc(false);
                StartCoroutine(gameManager.StartCountdown());
            }
        }
    }
}
