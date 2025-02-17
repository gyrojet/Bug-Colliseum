using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    PlayerManager playerManager = null;
    UIManager_Lobby uiManager = null;
    public int playerCounter;
    // Get the reference from UIManager / for countdown if we do it....maybe just for the playtime?

    // When a player has 0 lives, call a method (a isDead bool) to change counter on GameManager / Counter has to be set up based on how many player we currently have

    public float countdownTime = 180f;

    public static GameManager instance;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (instance == null)
            instance = this;

        if (playerManager == null)
            playerManager = PlayerManager.pmInstance;

        if (uiManager == null)
            uiManager = UIManager_Lobby.instance;

        //playerCounter = playerManager.NumberOfPlayers;
        // calls StartMatch()
        //StartCoroutine(StartCountdown());
    }

    // Update is called once per frame
    void Update()
    {
        //check for players status
        //if only one alive call endGame
        //if (playerCounter == 0)
        //    uiManager.EndGame();
        
        //if time`s up, call EndGame
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void GetPlayerCounterRpc()
    {
        playerCounter = playerManager.NumberOfPlayers;
    }


    void EndGame()
    {
        //Handles globalTimer / UI? / which player won ( how do I check on that?)

        if (playerCounter == 0)
        {
            string winner = playerManager.NumberOfPlayers.ToString();

            Debug.Log("Winner is :" + winner);
        }
        else
        {
            Debug.Log("Time is up!");

            
        }


    }


    //ADD UI DISPLAY 
    IEnumerator StartCountdown()
    {
        float remainingTime = countdownTime;

        while (remainingTime > 0)
        {
            Debug.Log("Time left: " + remainingTime);
            yield return new WaitForSeconds(1f);
            remainingTime--;
        }

        //EndGame();
    }
    /*
     * Create StartMatch()
     * re-enable player movement
     * start countdownTimer (coroutine - start the globalTimer)
     */

}
