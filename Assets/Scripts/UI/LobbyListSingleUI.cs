using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Lobbies.Models;
using TMPro;

public class LobbyListSingleUI : MonoBehaviour
{
    
    [SerializeField]private TextMeshProUGUI lobbyNameText;
    private Lobby lobby;

    private void Awake() {
        GetComponent<Button>().onClick.AddListener(() => {
            KitchenGameLobby.Instance.JoinWithId(lobby.Id);
        });
    }

    public void SetLobby(Lobby lobby){
        this.lobby = lobby;
        lobbyNameText.text = lobby.Name;
    }

}
