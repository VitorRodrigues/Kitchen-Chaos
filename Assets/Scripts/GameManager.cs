using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public event EventHandler OnStateChanges;
    public event EventHandler OnGamePaused;
    public event EventHandler OnGameUnpaused;

    private enum States {
        WaitingToStart,
        CountdownToStart,
        GamePlaying,
        GameOver
    }

    private States state;
    private float countdownToStartTimer = 1f;
    private float gamePlayingTimer;
    [SerializeField] private float gamePlayingTimerMax = 300f;
    [SerializeField] private bool isCountdownEnabled = false;
    private bool isGamePaused = false;

    private void Awake() {
        state = States.WaitingToStart;
        Instance = this;
    }

    private void Start() {
        GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;
        GameInput.Instance.OnInteractAction += GameInput_OnInteractionAction;
        gamePlayingTimer = gamePlayingTimerMax;

        // DEBUG TRIGGER GAME START AUTOMATICALLY
        state = States.CountdownToStart;
        OnStateChanges?.Invoke(this, EventArgs.Empty);
    }

    private void GameInput_OnInteractionAction(object sender, EventArgs e) {
        if (state == States.WaitingToStart) {
            state = States.CountdownToStart;
            OnStateChanges?.Invoke(this, EventArgs.Empty);
        }
    }

    private void GameInput_OnPauseAction(object sender, EventArgs e) {
        TogglePauseGame();
    }

    private void Update() {
        switch (state) {
            case States.WaitingToStart:
                gamePlayingTimer = gamePlayingTimerMax;
                break;
            case States.CountdownToStart:
                countdownToStartTimer -= Time.deltaTime;
                if (countdownToStartTimer < 0f) {
                    state = States.GamePlaying;
                    OnStateChanges?.Invoke(this, EventArgs.Empty);
                }
                break;
            case States.GamePlaying:
                if (isCountdownEnabled) {
                    gamePlayingTimer -= Time.deltaTime;
                }
                if (gamePlayingTimer < 0f) {
                    state = States.GameOver;
                    OnStateChanges?.Invoke(this, EventArgs.Empty);
                }
                break;
            case States.GameOver:
                break;
        }
    }

    public bool IsGamePlaying() {
        return state == States.GamePlaying;
    }

    public bool IsCountDownToStartActive() {
        return state == States.CountdownToStart;
    }

    public float GetCountdownToStartTimer() {
        return countdownToStartTimer;
    }

    public bool IsGameOver() {
        return state == States.GameOver;
    }

    public float GetGamePlayingTimerNormalized() {
        return 1 - (gamePlayingTimer / gamePlayingTimerMax);
    }
    public bool IsGamePaused() {
        return isGamePaused;
    }
    public void TogglePauseGame() {
        isGamePaused = !isGamePaused;
        if (isGamePaused) {
            Time.timeScale = 0f;
            OnGamePaused?.Invoke(this, EventArgs.Empty);
        } else {
            Time.timeScale = 1f;
            OnGameUnpaused?.Invoke(this, EventArgs.Empty);
        }
    }
}
