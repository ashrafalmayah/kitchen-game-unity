using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ConnectionResponseMessageUI : MonoBehaviour
{
    [SerializeField]private TextMeshProUGUI messageText;
    [SerializeField]private Button closeButton;

    private void Awake() {
        closeButton.onClick.AddListener(Hide);
    }

    private void Start() {
        KitchenGameMultiplayer.Instance.OnFailedToConnect += KitchenGameMultiplayer_OnFailedToConnect;

        Hide();
    }

    private void KitchenGameMultiplayer_OnFailedToConnect(object sender, EventArgs e){
        Show();

        messageText.text = NetworkManager.Singleton.DisconnectReason;

        if(messageText.text == ""){
            messageText.text = "Failed to connect!";
        }
    }

    private void Show(){
        gameObject.SetActive(true);
    }
    
    private void Hide(){
        gameObject.SetActive(false);
    }

    private void OnDestroy() {
        KitchenGameMultiplayer.Instance.OnFailedToConnect -= KitchenGameMultiplayer_OnFailedToConnect;
    }
    
}
