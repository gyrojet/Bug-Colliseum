using NUnit.Framework;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    [SerializeField] private List<GameObject> playersInClient = new List<GameObject>();

    [SerializeField] public static PlayerManager pmInstance;

    public int NumberOfPlayers { get { return playersInClient.Count; } }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (pmInstance == null)
            pmInstance = this;
    }

    public void AddPlayerToList(GameObject playerToAdd)
    {
        playersInClient.Add(playerToAdd);
    }
}
