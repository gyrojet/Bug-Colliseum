using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    PlayerManager playerManager = null;
    UIManager_Lobby uiManager = null;
    public int playerCounter;
    [SerializeField] private TextMeshProUGUI countDown;
    [SerializeField] private GameObject timesUp;
    [SerializeField] private GameObject winner;


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
        //Debug.Log("PLAYER COUNT" + playerCounter.ToString());
        if (playerCounter == 1)
        {
            EndGame();
        }
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
        //Debug.Log(playerCounter.ToString());

        if (playerCounter == 1)
        {
            Debug.Log("You Win!!");
            winner.SetActive(true);
            //StopCoroutine(StartCountdown());
        }
        else
        {
            Debug.Log("Time is up!");
            timesUp.SetActive(true);
        }


    }


    //ADD UI DISPLAY 
    public IEnumerator StartCountdown()
    {
        float remainingTime = countdownTime;

        while (remainingTime > 0)
        {
            Debug.Log("Time left: " + remainingTime);
            countDown.text = remainingTime.ToString();
            yield return new WaitForSeconds(1f);
            remainingTime--;
        }

        EndGame();
    }
    /*
     * Create StartMatch()
     * re-enable player movement
     * start countdownTimer (coroutine - start the globalTimer)
     */

}
