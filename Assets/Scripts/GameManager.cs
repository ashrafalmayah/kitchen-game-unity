using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public event EventHandler OnGameStateChanged;
    public event EventHandler OnGamePaused;
    public event EventHandler OnGameUnPaused;

    enum State{
        WaitingToStart,
        CountDownToStart,
        GamePlaying,
        GameOver
    }
    
    private State state;
    private float countDownToStartTimer = 1f;
    private float gamePlayingTimer;
    private float gamePlayingTimerMax = 300f;
    private bool isGamePaused = false;


    private void Awake() {
        Instance = this;

        state = State.WaitingToStart;
    }

    private void Start() {
        GameInput.Instance.OnGamePaused += GameInput_OnGamePaused;
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAcion;

        //DEBUG TRIGGER GAME START AUTOMATICALLY
        state = State.CountDownToStart;
        OnGameStateChanged?.Invoke(this , EventArgs.Empty);
    }

    private void Update() {
        switch(state){
            case State.WaitingToStart:
                break;
            case State.CountDownToStart: 
                countDownToStartTimer -= Time.deltaTime;
                if(countDownToStartTimer <= 0){
                    state = State.GamePlaying;
                    gamePlayingTimer = gamePlayingTimerMax;
                    OnGameStateChanged?.Invoke(this , EventArgs.Empty);
                }
                break;
            case State.GamePlaying: 
                gamePlayingTimer -= Time.deltaTime;
                if(gamePlayingTimer <= 0){
                    state = State.GameOver;
                    OnGameStateChanged?.Invoke(this , EventArgs.Empty);
                }
                break;
            case State.GameOver: 
                break;
        }
    }

    private void GameInput_OnInteractAcion(object sender, EventArgs e){
        if(state == State.WaitingToStart){
            state = State.CountDownToStart;
            OnGameStateChanged?.Invoke(this , EventArgs.Empty);
        }
    }

    private void GameInput_OnGamePaused(object sender, EventArgs e){
        TogglePauseGame();
    }

    public bool IsPlaying(){
        return state == State.GamePlaying;
    }

    public bool IsCountDownToStartActive(){
        return state == State.CountDownToStart;
    }

    public bool IsGameOver(){
        return state == State.GameOver;
    }

    public float GetCountDownToStartTimer(){
        return countDownToStartTimer;
    }

    public float GetGamePlayingTimerNormalized(){
        return 1 - (gamePlayingTimer / gamePlayingTimerMax);
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
}
