using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class StoveCounter : BaseCounter,IHasProgress {
    public event EventHandler<IHasProgress.OnProgressChangedEvenArgs> OnProgressChanged;

    public event EventHandler<OnStateChangedEventsArgs> OnStateChanged;
    public class OnStateChangedEventsArgs : EventArgs{
        public State state;
    }

    public enum State {
        Idle,
        Frying,
        Fried,
        Burned,
    }

    [SerializeField]private FryingRecipeSO[] fryingRecipeSOArray;
    [SerializeField]private BurningRecipeSO[] burningRecipeSOArray;
    private FryingRecipeSO fryingRecipeSO;
    private BurningRecipeSO burningRecipeSO;

    private NetworkVariable<float> fryingTimer = new NetworkVariable<float>(0f);
    private NetworkVariable<float> burningTimer = new NetworkVariable<float>(0f);

    private NetworkVariable<State> state = new NetworkVariable<State>(State.Idle);

    public override void OnNetworkSpawn(){
        fryingTimer.OnValueChanged += FryingTimer_OnValueChanged;
        burningTimer.OnValueChanged += BurningTimer_OnValueChanged;
        state.OnValueChanged += State_OnValueChanged;
    }

    private void State_OnValueChanged(State previousValue, State newValue){
        OnStateChanged?.Invoke(this, new OnStateChangedEventsArgs{
            state = state.Value
        });
        if(state.Value == State.Idle || state.Value == State.Burned){
            OnProgressChanged?.Invoke(this , new IHasProgress.OnProgressChangedEvenArgs {
                ProgressNormalized = 0
            });
        }
    }

    private void FryingTimer_OnValueChanged(float previousValue, float newValue){
        OnProgressChanged?.Invoke(this , new IHasProgress.OnProgressChangedEvenArgs {
            ProgressNormalized = fryingTimer.Value / fryingRecipeSO.fryingTimerMax
        });
    }

    private void BurningTimer_OnValueChanged(float previousValue, float newValue){
        OnProgressChanged?.Invoke(this , new IHasProgress.OnProgressChangedEvenArgs {
            ProgressNormalized = burningTimer.Value / burningRecipeSO.burningTimerMax
        });
    }

    private void Update() {
        if(!IsServer){
            return;
        }
        if(HasKitchenObject()){
            switch(state.Value){
                case State.Idle:
                    break;
                case State.Frying:
                    // float fryingTimerMax = fryingRecipeSO != null ? fryingRecipeSO.fryingTimerMax : 1f;
                    fryingTimer.Value += Time.deltaTime;

                    if(fryingTimer.Value > fryingRecipeSO.fryingTimerMax){
                        KitchenObject.DestroyKitchenObject(GetKitchenObject());
                        KitchenObject.SpawnKitchenObject(fryingRecipeSO.output, this);

                        state.Value = State.Fried;
                        burningTimer.Value = 0f;
                        SetBurningRecipeSOClientRpc(
                            KitchenGameMultiplayer.Instance.GetKithcenObjectSOIndex(GetKitchenObject().GetKitchenObjectSO())
                        );
                    }
                    break;
                case State.Fried:
                    burningTimer.Value += Time.deltaTime;

                    if(burningTimer.Value > burningRecipeSO.burningTimerMax){
                        KitchenObject.DestroyKitchenObject(GetKitchenObject());
                        KitchenObject.SpawnKitchenObject(burningRecipeSO.output, this);

                        state.Value = State.Burned;
                    }
                    break;
                case State.Burned:
                    break;
            }
        }
    }

    public override void Interact(Player player){
        if(!HasKitchenObject()){
            //Counter doesn't have anything
            if(player.HasKitchenObject()){
                //Player is carrying an object
                if(HasRecipeWithInput(player.GetKitchenObject().GetKitchenObjectSO())){
                    //Can be placed to fry
                    //Drop!!
                    KitchenObject kitchenObject = player.GetKitchenObject();
                    kitchenObject.SetKitchenObjectParent(this);

                    InteractLogicPlaceObjectOnCounterServerRpc(
                        KitchenGameMultiplayer.Instance.GetKithcenObjectSOIndex(kitchenObject.GetKitchenObjectSO())
                    );
                }
            }
        }
        else{
            //Counter has an object above it
            if(!player.HasKitchenObject()){
                //Player isn't carrying anything
                //Pick Up!!
                GetKitchenObject().SetKitchenObjectParent(player);
                
                SetStateServerRpc();
            }else{
                //Player is carrying something
                if(player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject)){
                    //Player is carrying a plate
                    if(plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO())){
                        KitchenObject.DestroyKitchenObject(GetKitchenObject());
                        SetStateServerRpc();
                    }
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetStateServerRpc(){
        state.Value = State.Idle;
    }

    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicPlaceObjectOnCounterServerRpc(int kitchenObjectSOIndex){
        fryingTimer.Value = 0f;
        state.Value = State.Frying;
        SetFryingRecipeSOClientRpc(kitchenObjectSOIndex);
    }

    [ClientRpc]
    private void SetFryingRecipeSOClientRpc(int kitchenObjectSOIndex){
        KitchenObjectSO kitchenObjectSO = KitchenGameMultiplayer.Instance.GetKitchenObjectSOFromIndex(kitchenObjectSOIndex);

        fryingRecipeSO = GetFryingRecipeSOWithInput(kitchenObjectSO);

    }

    [ClientRpc]
    private void SetBurningRecipeSOClientRpc(int kitchenObjectSOIndex){
        KitchenObjectSO kitchenObjectSO = KitchenGameMultiplayer.Instance.GetKitchenObjectSOFromIndex(kitchenObjectSOIndex);

        burningRecipeSO = GetBurningRecipeSOWithInput(kitchenObjectSO);

    }

    private bool HasRecipeWithInput(KitchenObjectSO inputKitchenObjectSO){
        FryingRecipeSO fryingRecipeSO = GetFryingRecipeSOWithInput(inputKitchenObjectSO);
        return fryingRecipeSO != null;
    }

    private KitchenObjectSO GetInputForOutput(KitchenObjectSO inputKitchenObjectSO){
        FryingRecipeSO fryingRecipeSO = GetFryingRecipeSOWithInput(inputKitchenObjectSO);
        if(inputKitchenObjectSO == fryingRecipeSO.input){
            return fryingRecipeSO.output;
        }else{
            return null;
        }
    }

    private FryingRecipeSO GetFryingRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO){
        foreach (FryingRecipeSO fryingRecipeSO in fryingRecipeSOArray){
            if(inputKitchenObjectSO == fryingRecipeSO.input){
                return fryingRecipeSO;
            }
        }
        return null;
    }

    private BurningRecipeSO GetBurningRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO){
        foreach (BurningRecipeSO burningRecipeSO in burningRecipeSOArray){
            if(inputKitchenObjectSO == burningRecipeSO.input){
                return burningRecipeSO;
            }
        }
        return null;
    }

    public bool IsFried(){
        return state.Value == State.Fried;
    }
}
