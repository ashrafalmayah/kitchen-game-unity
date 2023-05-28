using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class LobbyMessageUI : MonoBehaviour
{
    [SerializeField]private TextMeshProUGUI messageText;
    [SerializeField]private Button closeButton;

    private void Awake() {
        closeButton.onClick.AddListener(Hide);
    }

    private void Start() {
        KitchenGameMultiplayer.Instance.OnFailedToConnect += KitchenGameMultiplayer_OnFailedToConnect;
        KitchenGameLobby.Instance.OnCreateLobbyStarted += KitchenGameLobby_OnCreateLobbyStarted;
        KitchenGameLobby.Instance.OnCreateLobbyFailed += KitchenGameLobby_OnCreateLobbyFailed;
        KitchenGameLobby.Instance.OnJoinLobbyStarted += KitchenGameLobby_OnJoinLobbyStarted;
        KitchenGameLobby.Instance.OnJoinLobbyFailed += KitchenGameLobby_OnJoinLobbyFailed;
        KitchenGameLobby.Instance.OnQuickJoinLobbyFailed += KitchenGameLobby_OnQuickJoinLobbyFailed;

        Hide();
    }

    private void KitchenGameLobby_OnQuickJoinLobbyFailed(object sender, EventArgs e){
        ShowMessage("Could not find Lobby to Join!");
    }

    private void KitchenGameLobby_OnJoinLobbyFailed(object sender, EventArgs e){
        ShowMessage("Failed to join Lobby!");
    }

    private void KitchenGameLobby_OnJoinLobbyStarted(object sender, EventArgs e){
        ShowMessage("Joining Lobby...");
    }

    private void KitchenGameLobby_OnCreateLobbyFailed(object sender, EventArgs e){
        ShowMessage("Failed to create Lobby!");
    }

    private void KitchenGameLobby_OnCreateLobbyStarted(object sender, EventArgs e){
        ShowMessage("Creating Lobby...");
    }

    private void KitchenGameMultiplayer_OnFailedToConnect(object sender, EventArgs e){
        if(NetworkManager.Singleton.DisconnectReason == ""){
            ShowMessage("Failed to connect!");
            return;
        }
        ShowMessage(NetworkManager.Singleton.DisconnectReason);
    }

    private void ShowMessage(string message){
        Show();

        messageText.text = message;
    }

    private void Show(){
        gameObject.SetActive(true);
    }
    
    private void Hide(){
        gameObject.SetActive(false);
    }

    private void OnDestroy() {
        KitchenGameMultiplayer.Instance.OnFailedToConnect -= KitchenGameMultiplayer_OnFailedToConnect;
        KitchenGameLobby.Instance.OnCreateLobbyStarted -= KitchenGameLobby_OnCreateLobbyStarted;
        KitchenGameLobby.Instance.OnCreateLobbyFailed -= KitchenGameLobby_OnCreateLobbyFailed;
        KitchenGameLobby.Instance.OnJoinLobbyStarted -= KitchenGameLobby_OnJoinLobbyStarted;
        KitchenGameLobby.Instance.OnJoinLobbyFailed -= KitchenGameLobby_OnJoinLobbyFailed;
        KitchenGameLobby.Instance.OnQuickJoinLobbyFailed -= KitchenGameLobby_OnQuickJoinLobbyFailed;
    }
    
}
