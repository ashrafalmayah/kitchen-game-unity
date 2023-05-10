using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class SettingsUI : MonoBehaviour
{
    public static SettingsUI Instance { get; private set; }

    [SerializeField]private Button soundEffectsButton;
    [SerializeField]private Button musicButton;
    [SerializeField]private Button closeButton;
    [SerializeField]private TextMeshProUGUI soundEffectsText;
    [SerializeField]private TextMeshProUGUI musicText;
    [SerializeField]private Button moveUpButton;
    [SerializeField]private Button moveDownButton;
    [SerializeField]private Button moveLeftButton;
    [SerializeField]private Button moveRightButton;
    [SerializeField]private Button interactButton;
    [SerializeField]private Button interactAlternateButton;
    [SerializeField]private Button pauseButton;
    [SerializeField]private Button gamepadInteractButton;
    [SerializeField]private Button gamepadInteractAlternateButton;
    [SerializeField]private Button gamepadPauseButton;
    [SerializeField]private TextMeshProUGUI moveUpText;
    [SerializeField]private TextMeshProUGUI moveDownText;
    [SerializeField]private TextMeshProUGUI moveLeftText;
    [SerializeField]private TextMeshProUGUI moveRightText;
    [SerializeField]private TextMeshProUGUI interactText;
    [SerializeField]private TextMeshProUGUI interactAlternateText;
    [SerializeField]private TextMeshProUGUI pauseText;
    [SerializeField]private TextMeshProUGUI gamepadInteractText;
    [SerializeField]private TextMeshProUGUI gamepadInteractAlternateText;
    [SerializeField]private TextMeshProUGUI gamepadPauseText;
    [SerializeField]private Transform pressToRebindKeyTransform;

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
        moveUpButton.onClick.AddListener(() => { RebindBinding(GameInput.Binding.Move_Up); });
        moveDownButton.onClick.AddListener(() => { RebindBinding(GameInput.Binding.Move_Down); });
        moveLeftButton.onClick.AddListener(() => { RebindBinding(GameInput.Binding.Move_Left); });
        moveRightButton.onClick.AddListener(() => { RebindBinding(GameInput.Binding.Move_Right); });
        interactButton.onClick.AddListener(() => { RebindBinding(GameInput.Binding.Interact); });
        interactAlternateButton.onClick.AddListener(() => { RebindBinding(GameInput.Binding.InteractAlternate); });
        pauseButton.onClick.AddListener(() => { RebindBinding(GameInput.Binding.Pause); });
        gamepadInteractButton.onClick.AddListener(() => { RebindBinding(GameInput.Binding.GamepadInteract); });
        gamepadInteractAlternateButton.onClick.AddListener(() => { RebindBinding(GameInput.Binding.GamepadInteractAlternate); });
        gamepadPauseButton.onClick.AddListener(() => { RebindBinding(GameInput.Binding.GamepadPause); });
    }

    private void Start() {
        GameManager.Instance.OnLocalGameUnPaused += GameManager_OnGameUnPaused;

        UpdateVisual();

        Hide();
        HidePressToRebindKeyTransform();
    }

    private void UpdateVisual(){
        soundEffectsText.text = "Sound Effects: " + Mathf.Round(SoundManager.Instance.GetVolume() * 10f);
        musicText.text = "Music: " + Mathf.Round(MusicManager.Instance.GetVolume() * 10f);

        moveUpText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Move_Up);
        moveDownText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Move_Down);
        moveLeftText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Move_Left);
        moveRightText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Move_Right);
        interactText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Interact);
        interactAlternateText.text = GameInput.Instance.GetBindingText(GameInput.Binding.InteractAlternate);
        pauseText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Pause);
        gamepadInteractText.text = GameInput.Instance.GetBindingText(GameInput.Binding.GamepadInteract);
        gamepadInteractAlternateText.text = GameInput.Instance.GetBindingText(GameInput.Binding.GamepadInteractAlternate);
        gamepadPauseText.text = GameInput.Instance.GetBindingText(GameInput.Binding.GamepadPause);
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

    private void ShowPressToRebindKeyTransform(){
        pressToRebindKeyTransform.gameObject.SetActive(true);
    }

    private void HidePressToRebindKeyTransform(){
        pressToRebindKeyTransform.gameObject.SetActive(false);
    }

    private void RebindBinding(GameInput.Binding binding){
        ShowPressToRebindKeyTransform();

        GameInput.Instance.RebindBinding(binding, () => {
            HidePressToRebindKeyTransform();
            UpdateVisual();
        });
    }
}
