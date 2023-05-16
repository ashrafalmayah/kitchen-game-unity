using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public event EventHandler OnGameStateChanged;
    public event EventHandler OnLocalGamePaused;
    public event EventHandler OnLocalGameUnPaused;
    public event EventHandler OnMultiplayerGamePaused;
    public event EventHandler OnMultiplayerGameUnpaused;
    public event EventHandler OnLocalPlayerReadyChanged;

    enum State{
        WaitingToStart,
        CountDownToStart,
        GamePlaying,
        GameOver
    }
    
    private NetworkVariable<State> state = new NetworkVariable<State>(State.WaitingToStart);
    private NetworkVariable<float> countDownToStartTimer = new NetworkVariable<float>(3f);
    private NetworkVariable<float> gamePlayingTimer = new NetworkVariable<float>(0f);
    private float gamePlayingTimerMax = 10f;
    private bool isLocalGamePaused = false;
    private NetworkVariable<bool> isGamePaused = new NetworkVariable<bool>(false);
    private bool isLocalPlayerReady = false;
    private Dictionary<ulong,bool> playerReadyDictionary;
    private Dictionary<ulong,bool> playerPausedDictionary;


    private void Awake() {
        Instance = this;

        playerReadyDictionary = new Dictionary<ulong, bool>();
        playerPausedDictionary = new Dictionary<ulong, bool>();
    }

    private void Start() {
        GameInput.Instance.OnGamePaused += GameInput_OnGamePaused;
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAcion;
    }

    public override void OnNetworkSpawn(){
        state.OnValueChanged += State_OnValueChanged;
        isGamePaused.OnValueChanged += IsGamePaused_OnValueChanged;
    }

    private void IsGamePaused_OnValueChanged(bool previousValue, bool newValue){
        if(isGamePaused.Value){
            Time.timeScale = 0f;

            OnMultiplayerGamePaused?.Invoke(this , EventArgs.Empty);
        }else{
            Time.timeScale = 1f;

            OnMultiplayerGameUnpaused?.Invoke(this , EventArgs.Empty);
        }
    }

    private void State_OnValueChanged(State previousValue, State newValue){
        OnGameStateChanged?.Invoke(this , EventArgs.Empty);
    }

    private void Update() {
        if(!IsServer){
            return;
        }

        switch(state.Value){
            case State.WaitingToStart:
                break;
            case State.CountDownToStart: 
                countDownToStartTimer.Value -= Time.deltaTime;
                if(countDownToStartTimer.Value <= 0){
                    state.Value = State.GamePlaying;
                    gamePlayingTimer.Value = gamePlayingTimerMax;
                }
                break;
            case State.GamePlaying: 
                gamePlayingTimer.Value -= Time.deltaTime;
                if(gamePlayingTimer.Value <= 0){
                    state.Value = State.GameOver;
                }
                break;
            case State.GameOver: 
                break;
        }
    }

    private void GameInput_OnInteractAcion(object sender, EventArgs e){
        if(state.Value == State.WaitingToStart){
            isLocalPlayerReady = true;

            OnLocalPlayerReadyChanged?.Invoke(this , EventArgs.Empty);

            SetPlayerReadyServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default){
        playerReadyDictionary[serverRpcParams.Receive.SenderClientId] = true;


        bool allClientsReady = true;
        foreach(ulong clientId in NetworkManager.Singleton.ConnectedClientsIds){
            if(!playerReadyDictionary.ContainsKey(clientId) || !playerReadyDictionary[clientId]){
                allClientsReady = false;

                break;
            }
        }

        if(allClientsReady){
            state.Value = State.CountDownToStart;
        }
    }

    private void GameInput_OnGamePaused(object sender, EventArgs e){
        TogglePauseGame();
    }

    public bool IsPlaying(){
        return state.Value == State.GamePlaying;
    }

    public bool IsCountDownToStartActive(){
        return state.Value == State.CountDownToStart;
    }

    public bool IsGameOver(){
        return state.Value == State.GameOver;
    }

    public float GetCountDownToStartTimer(){
        return countDownToStartTimer.Value;
    }

    public float GetGamePlayingTimerNormalized(){
        return 1 - (gamePlayingTimer.Value / gamePlayingTimerMax);
    }

    public void TogglePauseGame(){
        isLocalGamePaused = !isLocalGamePaused;
        if(isLocalGamePaused){
            OnLocalGamePaused?.Invoke(this , EventArgs.Empty);

            PauseGameServerRpc();

            Time.timeScale = 0f;
        }else{
            OnLocalGameUnPaused?.Invoke(this , EventArgs.Empty);

            UnpauseGameServerRpc();

            Time.timeScale = 1f;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void PauseGameServerRpc(ServerRpcParams serverRpcParams = default){
        playerPausedDictionary[serverRpcParams.Receive.SenderClientId] = true;

        TestGamePausedState();
    }

    [ServerRpc(RequireOwnership = false)]
    private void UnpauseGameServerRpc(ServerRpcParams serverRpcParams = default){
        playerPausedDictionary[serverRpcParams.Receive.SenderClientId] = false;

        TestGamePausedState();
    }

    private void TestGamePausedState(){
        foreach(ulong clientId in NetworkManager.Singleton.ConnectedClientsIds){
            if(playerPausedDictionary.ContainsKey(clientId) && playerPausedDictionary[clientId]){
                //This player is paused

                isGamePaused.Value = true;

                return;
            }
        }

        //All players are unpaused

        isGamePaused.Value = false;

    }

    public bool IsLocalPlayerReady(){
        return isLocalPlayerReady;
    }
}
