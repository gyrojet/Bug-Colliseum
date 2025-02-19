using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Unity.Services.Multiplayer;
using UnityEngine.UI;
using TMPro;
using Unity.Multiplayer.Center.NetcodeForGameObjectsExample.DistributedAuthority;

public class PlayerManager : NetworkBehaviour
{
    private bool hasGameStarted;                                                                    // Dictates if game is active

    public bool HasGameStarted                                                                      // Public field to set hasGameStarted
    {
        get { return hasGameStarted; }
        set
        {
            hasGameStarted = value;
        }
    }

    [SerializeField] private List<GameObject> playersInClient = new List<GameObject>();             // List of players in the server
    [SerializeField] private List<Transform> matchSpawnPoints = new List<Transform>();              // Player spawn points
    [SerializeField] private List<Vector3> playerRotations = new List<Vector3>();                   // Player's orientation
    [SerializeField] private List<Sprite> playerGraphics = new List<Sprite>();                      // Player sprites
    [SerializeField] private List<Sprite> playerWeapons = new List<Sprite>();                       // Player weapon sprites
    [SerializeField] private List<Sprite> playerDefenseSprites = new List<Sprite>();                // Player guarding sprites

    [SerializeField] public static PlayerManager pmInstance;                                        // Public instance of this class

    public int NumberOfPlayers { get { return playersInClient.Count; } }                            // Number of players in player list


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (pmInstance == null)                                                                     // Initialize public instance
            pmInstance = this;

        hasGameStarted = false;                                                                     // Game has NOT started!
    }

    //  method Name    :  AddPlayerToList
    //  Synopsis   : Add a player to the list of players
    public void AddPlayerToList(GameObject playerToAdd)
    {
        playersInClient.Add(playerToAdd);                                                           // Add player to player list
    }

    //  method Name    :  AssignPlayerSpawnPointRpc
    //  Synopsis   : Assigns each player a spawn point
    [Rpc(SendTo.ClientsAndHost)]
    public void AssignPlayerSpawnPointRpc()
    {
        try
        {
            for (int counter = 0; counter < playersInClient.Count; counter++)                       // For each player in the list:
            {
                if (playersInClient[counter].GetComponent<Player>().SpawnPoint == null)             // If player does not have spawnpoint
                {
                    Transform spawnPointToSet = matchSpawnPoints[counter];                          // Get spawnpoint

                    playersInClient[counter].GetComponent<Player>().SpawnPoint = spawnPointToSet;   // Assign player a spawn point
                }
            }
        }
        catch (Exception ex)                                                                        // Exception handling
        {
            Debug.Log(ex);
        }
    }

    //  method Name    :  ReturnAllPlayersToSpawnPointRpc
    //  Synopsis   : Teleport each player in the list back to their spawn point
    [Rpc(SendTo.ClientsAndHost)]
    public void ReturnAllPlayersToSpawnPointRpc()
    {
        try
        {
            Player playerTemp;                                                                      // Used to interface with player

            for (int counter = 0; counter < playersInClient.Count; counter++)                       // For each player in the list
            {
                playerTemp = playersInClient[counter].GetComponent<Player>();                       // Get player from list

                playerTemp.player_SR.color = new Color(1f, 1f, 1f, 1f);                             // Reset player's color

                playerTemp.gameObject.layer = LayerMask.NameToLayer("Player");                      // Set player's layer to "Player"

                playerTemp.life = 3;                                                                // Set player's life to max

                playerTemp.SetNewTransform(playerTemp.SpawnPoint.transform.position);               // Move player to their spawn point
            }
        }
        catch (Exception ex)                                                                        // Error handling
        {
            Debug.Log(ex);
        }
    }


    //  method Name    : FreezeAllPlayersRpc
    //  Synopsis   : Lock player movement by applying rigidbody constraints
    [Rpc(SendTo.ClientsAndHost)]
    public void FreezeAllPlayersRpc()
    {
        foreach (GameObject player in playersInClient)                                              // For each player in the list:
        {
            player.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;      // Freeze ridigbody movement
        }
    }

    //  method Name    : UnfreezeAllPlayersRpc
    //  Synopsis   : Un-Lock player movement by removing rigidbody constraints
    [Rpc(SendTo.ClientsAndHost)]
    public void UnfreezeAllPlayersRpc()
    {
        foreach (GameObject player in playersInClient)                                              // For each player in the list:
        {
            player.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;           // Remove all constraints...
                                                                                                    // Except for z-axis rotation!
            player.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    //  method Name    : SetGameStatusRpc
    //  Synopsis   : Set the game's status (Started or not started)
    [Rpc(SendTo.ClientsAndHost)]
    public void SetGameStatusRpc(bool value)
    {
        hasGameStarted = value;                                                                     // Set game status
    }

    //  method Name    : RespawnAllPlayers
    //  Synopsis   : Signal all players to respawn by assigning them their spawn points
    public void RespawnAllPlayers()
    {
        AssignPlayerSpawnPointRpc();                                                                // Assign spawn points
        ReturnAllPlayersToSpawnPointRpc();                                                          // Move players to spawn
    }

    //  method Name    :  GetPlayerRotation
    //  Synopsis   : Get a player's rotation based on their index
    public Vector3 GetPlayerRotation(int playerIndex)
    {
        return playerRotations[playerIndex];                                                        // Return rotation based on index                                        
    }

    //  method Name    :  GetPlayerSprite
    //  Synopsis   : Get a player's Main sprite based on their index
    public Sprite GetPlayerSprite(int playerIndex) 
    {
        return playerGraphics[playerIndex];                                                         // Return sprite based on index
    }

    //  method Name    :  GetPlayerWeapon
    //  Synopsis   : Get a player's weapon sprite based on their index
    public Sprite GetPlayerWeapon(int playerIndex)
    {
        return playerWeapons[playerIndex];                                                          // Return Weapon based on index
    }

    //  method Name    :  GetPlayerDefenseSprite
    //  Synopsis   : Get a player's defense sprite based on their index
    public Sprite GetPlayerDefenseSprites(int playerIndex)
    {
        return playerDefenseSprites[playerIndex];                                                   // Return defense sprite based on index
    }
}
