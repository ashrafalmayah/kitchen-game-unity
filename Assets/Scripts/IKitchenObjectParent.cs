using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IKitchenObjectParent
{
    static event EventHandler OnAnyPickupOrDrop;

    Transform GetKitchenObjectFollowTransform();
    
    KitchenObject GetKitchenObject();

    void SetKitchenObject(KitchenObject kitchenObject);

    void ClearKitchenObject();

    bool HasKitchenObject();
}
