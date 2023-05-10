using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public event EventHandler OnGameStateChanged;
    public event EventHandler OnGamePaused;
    public event EventHandler OnGameUnPaused;
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
    private bool isGamePaused = false;
    private bool isLocalPlayerReady = false;
    private Dictionary<ulong,bool> playerReadyDictionary;


    private void Awake() {
        Instance = this;

        playerReadyDictionary = new Dictionary<ulong, bool>();
    }

    private void Start() {
        GameInput.Instance.OnGamePaused += GameInput_OnGamePaused;
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAcion;
    }

    public override void OnNetworkSpawn(){
        state.OnValueChanged += State_OnValueChanged;
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
        isGamePaused = !isGamePaused;
        if(isGamePaused){
            OnGamePaused?.Invoke(this , EventArgs.Empty);
            Time.timeScale = 0f;
        }else{
            OnGameUnPaused?.Invoke(this , EventArgs.Empty);
            Time.timeScale = 1f;
        }
    }

    public bool IsLocalPlayerREady(){
        return isLocalPlayerReady;
    }
}
