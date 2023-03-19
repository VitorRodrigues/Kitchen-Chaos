using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static IHasProgress;

public class CuttingCounter : BaseCounter, IHasProgress
{
    
    public event EventHandler OnCut;
    
    new public static void ResetStaticData() {
        OnAnyCut = null;
    }

    public event EventHandler<IHasProgress.OnProgressEventArgs> OnProgressChanged;
    public static event EventHandler OnAnyCut;

    [SerializeField] private CuttingRecipeSO[] cutKitchenObjectSOArray;

    private int cuttingProgress;



    public override void Interact(Player player) {
        if (!HasKitchenObject()) {
            if (player.HasKitchenObject()) {
                // Put player's object on counter
                if (HasRecipeWithInput(player.GetKitchenObject().GetKitchenObjectSO())) {
                    KitchenObject kitchenObject = player.GetKitchenObject();
                    kitchenObject.SetKitchenObjectParent(this);
                    InteractLogicPlaceObjectOnCounterServerRpc();
                }
            }
        } else{

            if (player.HasKitchenObject()) {
                // Player is holding something
                if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject)) {
                    // Player is carrying a plate, so add the item to the plate
                    if (plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO())) {
                        GetKitchenObject().DestroySelf();
                    }
                }
            } else { 
                // Give object to the player
                GetKitchenObject().SetKitchenObjectParent(player);
            }
        }
        //Cut kitchen object
    }

    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicPlaceObjectOnCounterServerRpc() {
        InteractLogicPlaceObjectOnCounterClientRpc();
    }
    [ClientRpc]
    private void InteractLogicPlaceObjectOnCounterClientRpc() {
        cuttingProgress = 0;

        OnProgressChanged?.Invoke(this, new OnProgressEventArgs {
            progressNormalized = 0f
        });
    }

    public override void InteractAlternate(Player player) {
        if (HasKitchenObject() && HasRecipeWithInput(GetKitchenObject().GetKitchenObjectSO())) {
            CutObjectServerRpc();
            TestCuttingProgressDoneServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void CutObjectServerRpc() {
        CutObjectClientRpc();
    }

    [ClientRpc]
    private void CutObjectClientRpc() {
        // There's a kitchen object AND it can be cut!
        cuttingProgress++;

        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());
        OnProgressChanged?.Invoke(this, new OnProgressEventArgs {
            progressNormalized = (float)cuttingProgress / cuttingRecipeSO.cuttingProgressMax
        });

        OnCut?.Invoke(this, EventArgs.Empty);
        OnAnyCut?.Invoke(this, EventArgs.Empty);

        if (cuttingProgress >= cuttingRecipeSO.cuttingProgressMax) {
            KitchenObjectSO outputKitchenObjectSO = GetOuputForInput(GetKitchenObject().GetKitchenObjectSO());

            if (outputKitchenObjectSO != null) {
                KitchenObject.DestroyKitchenObject(GetKitchenObject());
                KitchenObject.SpawnKitchenObject(outputKitchenObjectSO, this);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void TestCuttingProgressDoneServerRpc() {
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());

        if (cuttingProgress >= cuttingRecipeSO.cuttingProgressMax) {
            KitchenObjectSO outputKitchenObjectSO = GetOuputForInput(GetKitchenObject().GetKitchenObjectSO());

            if (outputKitchenObjectSO != null) {
                KitchenObject.DestroyKitchenObject(GetKitchenObject());
                KitchenObject.SpawnKitchenObject(outputKitchenObjectSO, this);
            }
        }

    }

    private bool HasRecipeWithInput(KitchenObjectSO inputKitchenObjectSO) {
        return GetCuttingRecipeSOWithInput(inputKitchenObjectSO) != null;
    }

    private KitchenObjectSO GetOuputForInput(KitchenObjectSO inputKitchenObjectSO) {
        return GetCuttingRecipeSOWithInput(inputKitchenObjectSO)?.output;
    }

    private CuttingRecipeSO GetCuttingRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO) {
        foreach (var cuttingRecipeSO in cutKitchenObjectSOArray) {
            if (cuttingRecipeSO.input == inputKitchenObjectSO) {
                return cuttingRecipeSO;
            }
        }
        return null;
    }
}
