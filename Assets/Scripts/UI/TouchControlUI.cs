using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TouchControlUI : MonoBehaviour
{
    private Player player;
    [SerializeField]private Button pickupDropButton;
    [SerializeField]private Button cuttingButton;
    private BaseCounter selectedCounter;
    private bool canPickupDrop = false;
    private bool canCut = false;


    private void Awake() {
    }

    private void Start() {
        // Player.Instance.OnSelectedCounterChanged += Player_OnSelectedCounterChanged;
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
    }

    private void GameInput_OnInteractAction(object sender, EventArgs e){
        CheckCut();
        CheckPickupDrop();
    }

    private void Player_OnSelectedCounterChanged(object sender, Player.OnSelectedCounterChangedEventArgs e){
        selectedCounter = e.selectedCounter;
        CheckCut();
        CheckPickupDrop();
    }

    private bool IsSelectedCounterNull(){
        if(selectedCounter == null){
            return true;
        }
        return false;
    }

    private void CheckCut(){
        if(IsSelectedCounterNull()) {
            canCut = false;
            UpdateVisual();
            return;
        }
        if(selectedCounter is CuttingCounter && selectedCounter.HasKitchenObject()){
            canCut = true;
        }else{
            canCut = false;
        }
        UpdateVisual();
    }

    private void CheckPickupDrop(){
        if(IsSelectedCounterNull()){
            canPickupDrop = false;
            UpdateVisual();
            return; 
        }
        if(player.HasKitchenObject()){
            if(!selectedCounter.HasKitchenObject()){
                if(!(selectedCounter is DeliveryCounter || selectedCounter is ContainerCounter || selectedCounter is PlatesCounter)){
                    canPickupDrop = true;
                }else if(selectedCounter is DeliveryCounter && player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject)){
                    canPickupDrop = true;
                }else{
                    canPickupDrop = false;
                }
            }else{
                if(player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject)){
                    if(selectedCounter.GetKitchenObject().TryGetPlate(out plateKitchenObject)){
                        canPickupDrop = false;
                    }else{
                        if(!( selectedCounter is ContainerCounter || selectedCounter is PlatesCounter )){
                            canPickupDrop = true;
                        }else{
                            canPickupDrop = false;
                        }
                    }
                }else{
                    if(selectedCounter.GetKitchenObject().TryGetPlate(out plateKitchenObject)){
                        canPickupDrop = true;
                    }else{
                        canPickupDrop = false;
                    }
                }
            }
        }else{
            if(selectedCounter.HasKitchenObject() || selectedCounter is ContainerCounter || selectedCounter is PlatesCounter){
                //Can pickup
                canPickupDrop = true;
            }else{
                canPickupDrop = false;
            }
        }
        UpdateVisual();
    }
    private void UpdateVisual(){
        if( canPickupDrop ){
            pickupDropButton.gameObject.SetActive(true);
        }else{
            pickupDropButton.gameObject.SetActive(false);
        }
        
        if(canCut){
            cuttingButton.gameObject.SetActive(true);
        }else{
            cuttingButton.gameObject.SetActive(false);
        }
    }
}
