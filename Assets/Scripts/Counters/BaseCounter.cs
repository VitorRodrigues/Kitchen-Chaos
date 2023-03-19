using System;
using Unity.Netcode;
using UnityEngine;

public abstract class BaseCounter: NetworkBehaviour, IKitchenObjectParent
{
    [SerializeField] private Transform counterTopPoint;
    public static event EventHandler OnAnyObjectPlacedHere;

    public static void ResetStaticData() {
        OnAnyObjectPlacedHere = null;
    }

    private KitchenObject kitchenObject;

    public virtual void Interact(Player player) {
    }

    public virtual void InteractAlternate(Player player) {}

    public void ClearKitchenObject() {
        kitchenObject = null;
    }

    public KitchenObject GetKitchenObject() {
        return kitchenObject;
    }

    public Transform GetKitchenObjectFollowTransform() {
        return counterTopPoint;
    }

    public bool HasKitchenObject() {
        return kitchenObject != null;
    }

    public void SetKitchenObject(KitchenObject kitchenObject) {
        this.kitchenObject = kitchenObject;

        if (kitchenObject != null) {
            OnAnyObjectPlacedHere?.Invoke(this, EventArgs.Empty);
        }
    }

    public NetworkObject GetNetworkObject() {
        return NetworkObject;
    }
}