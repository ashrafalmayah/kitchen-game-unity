using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class KitchenGameLobby : MonoBehaviour
{
    public static KitchenGameLobby Instance { get; private set; }

    public event EventHandler OnCreateLobbyStarted;
    public event EventHandler OnCreateLobbyFailed;
    public event EventHandler OnJoinLobbyStarted;
    public event EventHandler OnJoinLobbyFailed;
    public event EventHandler OnQuickJoinLobbyFailed;

    private Lobby joinedLobby;
    private float heartBeatTimer = 15f;

    private void Awake() {
        Instance = this;

        DontDestroyOnLoad(gameObject);

        InitializeUnityAuthentication();
    }


    private void Update() {
        HandleHeartBeat();
    }

    private void HandleHeartBeat(){
        if(IsLobbyHost()){
            heartBeatTimer -= Time.deltaTime;
            if(heartBeatTimer<0){
                float heartBeatTimerMax = 15f;
                heartBeatTimer = heartBeatTimerMax;

                LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id);
            }
        }
    }

    private bool IsLobbyHost(){
        return joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }

    private async void InitializeUnityAuthentication(){
        if(UnityServices.State != ServicesInitializationState.Initialized){
            InitializationOptions initializationOptions = new InitializationOptions();
            initializationOptions.SetProfile(UnityEngine.Random.Range(0,1000).ToString());

            await UnityServices.InitializeAsync(initializationOptions);

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    public async void CreateLobby(string lobbyName , bool isPrivate){
        OnCreateLobbyStarted?.Invoke(this , EventArgs.Empty);
        try{
            joinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, KitchenGameMultiplayer.MAX_PLAYERS_COUNT , new CreateLobbyOptions{
                IsPrivate = isPrivate,
            });


            KitchenGameMultiplayer.Instance.StartHost();
            Loader.LoadNetwork(Loader.Scene.CharacterSelectScene);
        }catch (LobbyServiceException e){
            Debug.Log(e);
            OnCreateLobbyFailed?.Invoke(this , EventArgs.Empty);
        }
    }

    public async void QuickJoin(){
        OnJoinLobbyStarted?.Invoke(this , EventArgs.Empty);
        try{
            joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync();

            KitchenGameMultiplayer.Instance.StartClient();
        } catch (LobbyServiceException e){
            Debug.Log(e);
            OnQuickJoinLobbyFailed?.Invoke(this , EventArgs.Empty);
        }
    }

    public async void JoinWithCode(string lobbyCode){
        OnJoinLobbyStarted?.Invoke(this , EventArgs.Empty);
        try{
            joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);

            KitchenGameMultiplayer.Instance.StartClient();
        }catch(LobbyServiceException e) {
            Debug.Log(e);
            OnJoinLobbyFailed?.Invoke(this , EventArgs.Empty);
        }
    }

    public async void DeleteLobby(){
        if(joinedLobby != null){
            try{
                await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);

                joinedLobby = null;
            }catch(LobbyServiceException e){
                Debug.Log(e);
            }
        }
    }

    public async void LeaveLobby(){
        if(joinedLobby != null){
            try{
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id,AuthenticationService.Instance.PlayerId);

                joinedLobby = null;
            }catch(LobbyServiceException e){
                Debug.Log(e);
            }
        }
    }

    public async void KickPlayer(string playerId){
        if(IsLobbyHost()){
            try{
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id,playerId);
            }catch(LobbyServiceException e){
                Debug.Log(e);
            }
        }
    }

    public Lobby GetLobby(){
        return joinedLobby;
    }


}
