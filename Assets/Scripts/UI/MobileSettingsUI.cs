using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MobileSettingsUI : MonoBehaviour
{
    public static MobileSettingsUI Instance { get; private set; }

    [SerializeField]private Button soundEffectsButton;
    [SerializeField]private Button musicButton;
    [SerializeField]private Button closeButton;
    [SerializeField]private TextMeshProUGUI soundEffectsText;
    [SerializeField]private TextMeshProUGUI musicText;
    private Action onCloseButtonAction;


    private void Awake() {
        Instance = this;

        soundEffectsButton.onClick.AddListener(() => {
            SoundManager.Instance.ChangeVolume();

            UpdateVisual();
        });
        musicButton.onClick.AddListener(() => {
            MusicManager.Instance.ChangeVolume();

            UpdateVisual();
        });
        closeButton.onClick.AddListener(() => {
            onCloseButtonAction();
            Hide();
        });
    }

    private void Start() {
        GameManager.Instance.OnLocalGameUnPaused += GameManager_OnGameUnPaused;

        UpdateVisual();

        Hide();
    }

    private void UpdateVisual(){
        soundEffectsText.text = "Sound Effects: " + Mathf.Round(SoundManager.Instance.GetVolume() * 10f);
        musicText.text = "Music: " + Mathf.Round(MusicManager.Instance.GetVolume() * 10f);
    }

    private void GameManager_OnGameUnPaused(object sender, EventArgs e){
        Hide();
    }

    public void Show(Action onCloseButtonAcion){
        this.onCloseButtonAction = onCloseButtonAcion;

        gameObject.SetActive(true);

        soundEffectsButton.Select();
    }

    private void Hide(){
        gameObject.SetActive(false);
    }
    
}
