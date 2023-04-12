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
    private bool canPickupOrDrop = false;
    private bool canCut = false;



    private void Start() {
        if(Player.LocalInstance != null){
            Player.LocalInstance.OnSelectedCounterChanged += Player_OnSelectedCounterChanged;
            player = Player.LocalInstance;
        }else{
            Player.OnAnyPlayerSpawned += Player_OnAnyPlayerSpawned;
        }
        Player.OnAnyPickupOrDrop += KitchenObjectParent_OnPickupOrDrop;
        BaseCounter.OnAnyPickupOrDrop += KitchenObjectParent_OnPickupOrDrop;
    }

    private void Player_OnAnyPlayerSpawned(object sender, EventArgs e){
        if(Player.LocalInstance != null){
            Player.LocalInstance.OnSelectedCounterChanged -= Player_OnSelectedCounterChanged;
            Player.LocalInstance.OnSelectedCounterChanged += Player_OnSelectedCounterChanged;
            player = Player.LocalInstance;
        }
    }

    private void KitchenObjectParent_OnPickupOrDrop(object sender, EventArgs e){
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
            canPickupOrDrop = false;
            UpdateVisual();
            return;
        }
        if(player.HasKitchenObject()){
            if(!selectedCounter.HasKitchenObject()){
                if(!(selectedCounter is DeliveryCounter || selectedCounter is ContainerCounter || selectedCounter is PlatesCounter)){
                    canPickupOrDrop = true;
                }else if(selectedCounter is DeliveryCounter && player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject)){
                    canPickupOrDrop = true;
                }else{
                    canPickupOrDrop = false;
                }
            }else{
                if(player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject)){
                    if(selectedCounter.GetKitchenObject().TryGetPlate(out plateKitchenObject)){
                        canPickupOrDrop = false;
                    }else{
                        if(!( selectedCounter is ContainerCounter || selectedCounter is PlatesCounter )){
                            canPickupOrDrop = true;
                        }else{
                            canPickupOrDrop = false;
                        }
                    }
                }else{
                    if(selectedCounter.GetKitchenObject().TryGetPlate(out plateKitchenObject)){
                        canPickupOrDrop = true;
                    }else{
                        canPickupOrDrop = false;
                    }
                }
            }
        }else{
            if(selectedCounter.HasKitchenObject() || selectedCounter is ContainerCounter || selectedCounter is PlatesCounter){
                //Can pickup
                canPickupOrDrop = true;
            }else{
                canPickupOrDrop = false;
            }
        }
        UpdateVisual();
    }
    private void UpdateVisual(){
        if( canPickupOrDrop ){
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
