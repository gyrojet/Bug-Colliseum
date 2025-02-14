using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    // Get the reference from UIManager

    // When a player has 0 lives, call a method (a isDead bool) to change counter on GameManager / Counter has to be set up based on how many player we currently have


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // spawn players
        // calls StartMatch()
    }

    // Update is called once per frame
    void Update()
    {
        //check for players status
        //if only one alive call endGame
        //if time`s up, call EndGame
    }


    void EndGame()
    {
        //Handles globalTimer / UI? / which player won ( how do I check on that?)
    }

    /*
     * Create StartMatch()
     * re-enable player movement
     * start countdownTimer (coroutine - start the globalTimer)
     */

}
