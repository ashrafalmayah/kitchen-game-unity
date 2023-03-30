using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseCounter : MonoBehaviour,IKitchenObjectParent
{
    public static event EventHandler OnAnyObjectDroped;

    [SerializeField] private Transform counterTopPoint;
    
    private KitchenObject kitchenObject;


    public virtual void Interact(Player player){
        Debug.LogError("BaseCounter.Interact();");
    }

    public virtual void InteractAlternate(Player player){
        
    }

    public Transform GetKitchenObjectFollowTransform(){
        return counterTopPoint;
    }
    
    public KitchenObject GetKitchenObject(){
        return kitchenObject;
    }

    public void SetKitchenObject(KitchenObject kitchenObject){
        this.kitchenObject = kitchenObject;
        OnAnyObjectDroped?.Invoke(this , EventArgs.Empty);
    }

    public void ClearKitchenObject(){
        this.kitchenObject = null;
    }

    public bool HasKitchenObject(){
        return kitchenObject != null;
    }
}
