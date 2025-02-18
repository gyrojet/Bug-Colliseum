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
    [SerializeField] private List<Transform> matchSpawnPoints = new List<Transform>();

    [SerializeField] private List<Vector3> playerRotations = new List<Vector3>();

    [SerializeField] private List<Sprite> playerGraphics = new List<Sprite>();
    [SerializeField] private List<Sprite> playerWeapons = new List<Sprite>();
    [SerializeField] private List<Sprite> playerDefenseSprites = new List<Sprite>();

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
    public void RemovePlayerToList(GameObject playerToAdd)
    {
        playersInClient.Remove(playerToAdd);
    }

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

            for (int counter = 0; counter < playersInClient.Count; counter++)
            {
                playerTemp = playersInClient[counter].GetComponent<Player>();

                playerTemp.player_SR.color = new Color(1f, 1f, 1f, 1f);

                playerTemp.gameObject.layer = LayerMask.NameToLayer("Player");

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
            player.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
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

    public Vector3 GetPlayerRotation(int playerIndex)
    {
        return playerRotations[playerIndex];
    }

    public Sprite GetPlayerSprite(int playerIndex) 
    {
        return playerGraphics[playerIndex];
    }

    public Sprite GetPlayerDefenseSprite(int playerIndex)
    {
        return playerDefenseSprites[playerIndex];
    }

    public Sprite GetPlayerWeapon(int playerIndex)
    {
        return playerWeapons[playerIndex];
    }
}
