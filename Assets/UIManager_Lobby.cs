using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UIManager_Lobby : NetworkBehaviour
{
    [SerializeField] PlayerManager pm;

    [SerializeField] TextMeshProUGUI numPlayers;
    [SerializeField] Button startGame;

    private void Start()
    {
        startGame.onClick.AddListener(StartGame_TEST);
    }

    private void Update()
    {
        UpdatePlayerCount();
        SetStartGameButton();
    }

    private void UpdatePlayerCount()
    {
        int count = pm.NumberOfPlayers;
        numPlayers.text = count.ToString();
    }

    private void SetStartGameButton()
    {
        if (pm.NumberOfPlayers > 1) 
        {
            startGame.interactable = true;
        }
        else
        {
            startGame.interactable = false;
        }
    }

    private void StartGame_TEST()
    {
        pm.TEST_RespawnAllPlayers();
    }
}
