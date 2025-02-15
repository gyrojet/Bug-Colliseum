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

    

    private void Start()
    {
        startGame.onClick.AddListener(StartGame_TEST);

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

    private void StartGame_TEST()
    {
        if (IsHost)
        {
            SetValueOfLobbyUIRpc(false);

            pm.TEST_RespawnAllPlayers();            // Send all players to spawn points

            pm.FreezeAllPlayersRpc();               // Freeze movement of all players

            MessagePlayersToBeginCountdownRpc();
        }
        else
        {
            Debug.Log("NOT HOST");
        }

        
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
                pm.UnfreezeAllPlayersRpc();
                SetValueOfTimerUIRpc(false);
            }
        }
    }
}
