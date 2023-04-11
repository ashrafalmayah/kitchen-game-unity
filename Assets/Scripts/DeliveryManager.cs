using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DeliveryManager : NetworkBehaviour
{
    public static DeliveryManager Instance {get; private set;}


    public event EventHandler OnRecipeSpawned;
    public event EventHandler OnRecipeCompleted;
    public event EventHandler OnDeliverySuccess;
    public event EventHandler OnDeliveryFail;


    [SerializeField]private RecipeListSO recipeListSO;
    private List<RecipeSO> waitingRecipeSOList;
    private float spawnRecipeTimer;
    private float spawnRecipeTimerMax = 4f;
    private int waitingRecipesMax = 4;
    private int recipesDeliveredSuccessfully;


    private void Awake() {
        DeliveryManager.Instance = this;
        waitingRecipeSOList = new List<RecipeSO>();
    }

    private void Update() {
        if(!IsServer){
            return;
        }

        if(GameManager.Instance.IsPlaying() &&  waitingRecipeSOList.Count < waitingRecipesMax){
            spawnRecipeTimer += Time.deltaTime;
            if(spawnRecipeTimer >= spawnRecipeTimerMax){
                spawnRecipeTimer = 0;

                int waitingrecipeSOIndex = UnityEngine.Random.Range(0,recipeListSO.recipeSOList.Count);

                SpawnNewWaitingRecipeSOClientRpc(waitingrecipeSOIndex);
            }
        }
    }

    [ClientRpc]
    private void SpawnNewWaitingRecipeSOClientRpc(int waitingrecipeSOIndex){
        RecipeSO recipeSO = recipeListSO.recipeSOList[waitingrecipeSOIndex];

        waitingRecipeSOList.Add(recipeSO);

        OnRecipeSpawned?.Invoke(this , EventArgs.Empty);

    }

    public void DeliverRecipe(PlateKitchenObject plateKitchenObject){
        for(int i = 0;i < waitingRecipeSOList.Count; i++){
            RecipeSO waitingRecipeSO = waitingRecipeSOList[i];
            if(waitingRecipeSO.kitchenObjectSOList.Count == plateKitchenObject.GetKitchenObjectSOList().Count){
                //There are the same ingredients amount in the plate
                bool plateContentMatchRecipe = true;
                foreach(KitchenObjectSO waitingKitchenObjectSO in waitingRecipeSO.kitchenObjectSOList){
                    //Cycling through all the recipe kitchen objects
                    bool ingredientFound = false;
                    foreach(KitchenObjectSO plateKitchenObjectSO in plateKitchenObject.GetKitchenObjectSOList()){
                        if(plateKitchenObjectSO == waitingKitchenObjectSO){
                            //There's a match!
                            ingredientFound = true;
                            break;
                        }
                    }
                    if(!ingredientFound){
                        //This plate did not give the right recipe
                        plateContentMatchRecipe = false;
                    }
                }
                if(plateContentMatchRecipe){
                    //Player delivered the correct recipe

                    DeliverCorrectRecipeServerRpc(i);

                    return;
                }
            }
        }
        //Playre did not deliver the correct recipe
        DeliverInCorrectRecipeServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void DeliverInCorrectRecipeServerRpc(){
        DeliverInCorrectRecipeClientRpc();
    }

    [ClientRpc]
    private void DeliverInCorrectRecipeClientRpc(){
        OnDeliveryFail?.Invoke(this , EventArgs.Empty);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DeliverCorrectRecipeServerRpc(int waitingRecipeSOListIndex){
        DeliverCorrectRecipeClientRpc(waitingRecipeSOListIndex);
    }
    
    [ClientRpc]
    private void DeliverCorrectRecipeClientRpc(int waitingRecipeSOListIndex){
        recipesDeliveredSuccessfully++;

        waitingRecipeSOList.RemoveAt(waitingRecipeSOListIndex);

        OnRecipeCompleted?.Invoke(this , EventArgs.Empty);
        OnDeliverySuccess?.Invoke(this , EventArgs.Empty);
    }


    public List<RecipeSO> GetWaitingRecipeSOList(){
        return waitingRecipeSOList;
    }

    public int GetRecipesDeliveredSuccessfully(){
        return recipesDeliveredSuccessfully;
    }
}
