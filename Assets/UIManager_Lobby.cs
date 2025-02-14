using TMPro;
using Unity.Netcode;
using UnityEngine;

public class UIManager_Lobby : NetworkBehaviour
{
    [SerializeField] PlayerManager pm;

    [SerializeField] TextMeshProUGUI numPlayers;

    private void Update()
    {
        UpdatePlayerCount();
    }

    private void UpdatePlayerCount()
    {
        int count = pm.NumberOfPlayers;
        numPlayers.text = count.ToString();
    }
}
