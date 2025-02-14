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
    [SerializeField] private List<GameObject> playersInClient = new List<GameObject>();
    [SerializeField] private List<Transform> playerPos = new List<Transform>();

    [SerializeField] public static PlayerManager pmInstance;

    [Header("Buttons")]
    [SerializeField] Button setSpawnPoints;
    
    [SerializeField] TMP_InputField joinCodeField;


    public int NumberOfPlayers { get { return playersInClient.Count; } }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (pmInstance == null)
            pmInstance = this;

        setSpawnPoints.onClick.AddListener(SetPositionsOfClientsRpc);
    }

    public void AddPlayerToList(GameObject playerToAdd)
    {
        playersInClient.Add(playerToAdd);
    }

    public void RemovePlayerFromList(GameObject playerToRemove)
    {
        playersInClient.Remove(playerToRemove);
    }

   
    private void SetPositionsOfClientsRpc()
    {
        try
        {
            Debug.Log("RUN");
            for (int counter = 0; counter > playersInClient.Count; counter++)
            {
                //[INetworkSerializable] Transform tf = playerPos[counter];
                //playersInClient[counter].GetComponent<Player>().SetNewTransform();
            }
        }
        catch (Exception ex) 
        {
            Debug.Log(ex);
        }
    }
}
