using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CuttingCounter : BaseCounter,IHasProgress
{
    public static event EventHandler OnAnyCut;

    new public static void ResetStaticDate(){
        OnAnyCut = null;
    }

    public event EventHandler<IHasProgress.OnProgressChangedEvenArgs> OnProgressChanged;
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
                }
            }
        }
        else{
            //Counter has an object above it
            if(!player.HasKitchenObject()){
                //Player isn't carrying anything
                GetKitchenObject().SetKitchenObjectParent(player);
                InteractLogicPickObjectServerRpc();

            }else{
                //Player is carrying something
                if(player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject)){
                    //Player is carrying a plate
                    if(plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO())){
                        InteractLogicPickObjectServerRpc();
                        KitchenObject.DestroyKitchenObject(GetKitchenObject());
                    }
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicPickObjectServerRpc(){
        InteractLogicPickObjectClientRpc();
    }

    [ClientRpc]
    private void InteractLogicPickObjectClientRpc(){
        cuttingProgress = 0;

        OnProgressChanged?.Invoke(this , new IHasProgress.OnProgressChangedEvenArgs {
            ProgressNormalized = 0
        });
    }

    public override void InteractAlternate(Player player){
        if(HasKitchenObject() && HasRecipeWithInput(GetKitchenObject().GetKitchenObjectSO())){
            //There is a kitchen object AND can be cut
            CutObjectServerRpc();
            TestCuttingProgressDoneServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void CutObjectServerRpc(){
        if(HasKitchenObject() && HasRecipeWithInput(GetKitchenObject().GetKitchenObjectSO())){
            CutObjectClientRpc();
        }
    }

    [ClientRpc]
    private void CutObjectClientRpc(){
        OnCut?.Invoke(this, EventArgs.Empty);
        OnAnyCut?.Invoke(this , EventArgs.Empty);

        cuttingProgress++;
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());
        OnProgressChanged?.Invoke(this , new IHasProgress.OnProgressChangedEvenArgs {
            ProgressNormalized = (float)cuttingProgress / cuttingRecipeSO.cuttingProgressMax
        });
    }

    [ServerRpc(RequireOwnership = false)]
    private void TestCuttingProgressDoneServerRpc(){
        if(HasKitchenObject() && HasRecipeWithInput(GetKitchenObject().GetKitchenObjectSO())){
            CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());

            if(cuttingProgress >= cuttingRecipeSO.cuttingProgressMax){
                KitchenObjectSO outputKitchenObjectSO = GetInputForOutput(GetKitchenObject().GetKitchenObjectSO());
                KitchenObject.DestroyKitchenObject(GetKitchenObject());
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
