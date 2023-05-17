using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectingUI : MonoBehaviour
{


    private void Start() {
        KitchenGameMultiplayer.Instance.OnFailedToConnect += KitchenGameMultiplayer_OnFailedToConnect;
        KitchenGameMultiplayer.Instance.OnTryingToConnect += KitchenGameMultiplayer_OnTryingToConnect;

        Hide();
    }

    private void KitchenGameMultiplayer_OnTryingToConnect(object sender, EventArgs e){
        Show();
    }

    private void KitchenGameMultiplayer_OnFailedToConnect(object sender, EventArgs e){
        Hide();
    }

    private void Show(){
        gameObject.SetActive(true);
    }
    
    private void Hide(){
        gameObject.SetActive(false);
    }

    private void OnDestroy() {
        KitchenGameMultiplayer.Instance.OnFailedToConnect -= KitchenGameMultiplayer_OnFailedToConnect;
        KitchenGameMultiplayer.Instance.OnTryingToConnect -= KitchenGameMultiplayer_OnTryingToConnect;
    }
    
}
