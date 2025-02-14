using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //spawn players
        //start timer countdown
    }

    // Update is called once per frame
    void Update()
    {
     //check for players status
     //if only one alive call endGame
    }


    void EndGame()
    {
        //Handles timer / UI? / which player won ( how do I check on that?)
    }

}
