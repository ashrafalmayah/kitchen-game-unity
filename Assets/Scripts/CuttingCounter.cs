using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CuttingCounter : BaseCounter
{
    public event EventHandler<OnProgressChangedEvenArgs> OnProgressChanged;
    public class OnProgressChangedEvenArgs : EventArgs{
        public float cuttingProgressNormalized;
    }
    public event EventHandler OnCut;

    [SerializeField]private CuttingRecipeSO[] CuttingRecipeSOArray; 
    private int cuttingProgress;

    public override void Interact(Player player){
        if(!HasKitchenObject()){
            //Counter doesn't have anything
            if(player.HasKitchenObject()){
                //Player is carrying an object
                if(HasRecipeWithInput(player.GetKitchenObject().GetKitchenObjectSO())){
                    //Can be placed to cut
                    player.GetKitchenObject().SetKitchenObjectParent(this);
                    cuttingProgress = 0;

                    CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());

                    OnProgressChanged?.Invoke(this , new OnProgressChangedEvenArgs {
                    cuttingProgressNormalized = (float)cuttingProgress / cuttingRecipeSO.cuttingProgressMax
                    });
                }
            }
        }
        else{
            //Counter has an object above it
            if(!player.HasKitchenObject()){
                //Player isn't carrying anything
                GetKitchenObject().SetKitchenObjectParent(player);
            }
        }
    }

    public override void InteractAlternate(Player player){
        if(HasKitchenObject() && HasRecipeWithInput(GetKitchenObject().GetKitchenObjectSO())){
            //There is a kitchen object AND can be cut

            OnCut?.Invoke(this, EventArgs.Empty);

            cuttingProgress++;
            CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());
            OnProgressChanged?.Invoke(this , new OnProgressChangedEvenArgs {
               cuttingProgressNormalized = (float)cuttingProgress / cuttingRecipeSO.cuttingProgressMax
            });
            if(cuttingProgress >= cuttingRecipeSO.cuttingProgressMax){
                KitchenObjectSO outputKitchenObjectSO = GetInputForOutput(GetKitchenObject().GetKitchenObjectSO());
                GetKitchenObject().DestroySelf();
                KitchenObject.SpawnKitchenObject(outputKitchenObjectSO,this);
            }
        }
    }

    private bool HasRecipeWithInput(KitchenObjectSO inputKitchenObjectSO){
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(inputKitchenObjectSO);
        return cuttingRecipeSO != null;
    }

    private KitchenObjectSO GetInputForOutput(KitchenObjectSO inputKitchenObjectSO){
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(inputKitchenObjectSO);
        if(inputKitchenObjectSO == cuttingRecipeSO.input){
            return cuttingRecipeSO.output;
        }else{
            return null;
        }
    }

    private CuttingRecipeSO GetCuttingRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO){
        foreach (CuttingRecipeSO cuttingRecipeSO in CuttingRecipeSOArray){
            if(inputKitchenObjectSO == cuttingRecipeSO.input){
                return cuttingRecipeSO;
            }
        }
        return null;
    }

}
