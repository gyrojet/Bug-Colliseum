using System.Collections;
using System.Collections.Generic;
//using UnityEditorInternal;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using TMPro;
using Unity.Services.Lobbies.Models;

public class LobbyManager : MonoBehaviour
{
    private int maxNumberOfPlayers = 4;
    private int minNumberOfPlayers = 2;

    [SerializeField] string lobbyID;

    [SerializeField] private TMP_InputField lobbyNameField;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private async void Start()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async void CreateNewLobby()
    {
        if (lobbyNameField.text == string.Empty)
        {
            Debug.Log("Please enter a name for your lobby.");
            return;
        }

        Lobby newLobby;

        try
        {
            newLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyNameField.text, maxNumberOfPlayers);
            lobbyID = newLobby.Id;

            Debug.Log($"Created lobby with the ID {lobbyID}");
        }
        catch (LobbyServiceException ex)
        { 
            Debug.Log(ex);
        }
    }
}
