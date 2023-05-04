using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TrashCounter : BaseCounter {
    public static event EventHandler OnAnyTrashed;

    new public static void ResetStaticDate(){
        OnAnyTrashed = null;
    }

    public override void Interact(Player player)
    {
        if(player.HasKitchenObject()){
            KitchenObject.DestroyKitchenObject(player.GetKitchenObject());

            InteractLogicServerRpc();
        }
    }
    

    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicServerRpc(){
        InteractLogicClientRpc();
    }

    [ClientRpc]
    private void InteractLogicClientRpc(){
        OnAnyTrashed?.Invoke(this , EventArgs.Empty);
    }
}
