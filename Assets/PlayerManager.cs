using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Unity.Services.Multiplayer;
using UnityEngine.UI;
using TMPro;

public class PlayerManager : NetworkBehaviour
{
    private bool hasGameStarted;

    public bool HasGameStarted
    {
        get { return hasGameStarted; }
        set
        {
            hasGameStarted = value;
        }
    }

    [SerializeField] private List<GameObject> playersInClient = new List<GameObject>();
    [SerializeField] private List<Transform> playerPos = new List<Transform>();

    [SerializeField] private List<Transform> matchSpawnPoints = new List<Transform>();

    [SerializeField] public static PlayerManager pmInstance;

    [SerializeField] TMP_InputField joinCodeField;


    public int NumberOfPlayers { get { return playersInClient.Count; } }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (pmInstance == null)
            pmInstance = this;

        hasGameStarted = false;
    }

    public void AddPlayerToList(GameObject playerToAdd)
    {
        playersInClient.Add(playerToAdd);
    }

    public void RemovePlayerFromList(GameObject playerToRemove)
    {
        playersInClient.Remove(playerToRemove);
    }

    //[Rpc(SendTo.ClientsAndHost)]
    //public void SetPositionRpc()
    //{
    //    try
    //    {
    //        Debug.Log("RUN");
    //        for (int counter = 0; counter < playersInClient.Count; counter++)
    //        {
    //            Transform tf = playerPos[counter];
    //            playersInClient[counter].GetComponent<Player>().SetNewTransform(tf);
    //        }
    //    }
    //    catch (Exception ex) 
    //    {
    //        Debug.Log(ex);
    //    }
    //}

    [Rpc(SendTo.ClientsAndHost)]
    public void AssignPlayerSpawnPointRpc()
    {
        try
        {
            Debug.Log("RUN");

            for (int counter = 0; counter < playersInClient.Count; counter++)
            {
                if (playersInClient[counter].GetComponent<Player>().SpawnPoint == null)
                {
                    Transform spawnPointToSet = matchSpawnPoints[counter];

                    playersInClient[counter].GetComponent<Player>().SpawnPoint = spawnPointToSet;

                    Debug.Log($"Set transform for player: {playersInClient[counter].GetComponent<Player>().playerIndex}");
                }
                else
                {
                    Debug.Log("Spawn point already exists for this player.");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void ReturnAllPlayersToSpawnPointRpc()
    {
        try
        {
            Player playerTemp;

            for (int counter = 0;counter < playersInClient.Count; counter++)
            {
                playerTemp = playersInClient[counter].GetComponent<Player>();

                playerTemp.SetNewTransform(playerTemp.SpawnPoint.transform.position);

                Debug.Log($"Set transform for player: {playerTemp.playerIndex}");
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void FreezeAllPlayersRpc()
    {
        foreach (GameObject player in playersInClient) 
        {
            player.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void UnfreezeAllPlayersRpc()
    {
        foreach (GameObject player in playersInClient)
        {
            player.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SetGameStatusRpc(bool value)
    {
        hasGameStarted = value;
    }

    public void RespawnAllPlayers()
    {
        AssignPlayerSpawnPointRpc();
        ReturnAllPlayersToSpawnPointRpc();
    }
}
