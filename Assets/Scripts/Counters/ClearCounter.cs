using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearCounter : BaseCounter
{


    public override void Interact(Player player){
        if(!HasKitchenObject()){
            //Counter doesn't have anything
            if(player.HasKitchenObject()){
                //Player is carrying an object
                player.GetKitchenObject().SetKitchenObjectParent(this);
            }
        }
        else{
            //Counter has an object above it
            if(!player.HasKitchenObject()){
                //Player isn't carrying anything
                GetKitchenObject().SetKitchenObjectParent(player);
            }else{
                //Player is carrying something
                if(player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject)){
                    //Player is carrying a plate
                    if(plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO())){
                        KitchenObject.DestroyKitchenObject(GetKitchenObject());
                    }
                }else{
                    //Player isn't carrying a plate but something else
                    if(GetKitchenObject().TryGetPlate(out plateKitchenObject)){
                        //Counter is holding a plate
                        if(plateKitchenObject.TryAddIngredient(player.GetKitchenObject().GetKitchenObjectSO())){
                            KitchenObject.DestroyKitchenObject(player.GetKitchenObject());
                        }
                    }
                }
            }
        }
        
    }   
}