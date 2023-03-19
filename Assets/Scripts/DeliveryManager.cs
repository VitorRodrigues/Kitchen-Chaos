using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DeliveryManager : NetworkBehaviour
{

    public event EventHandler OnRecipeSpawned;
    public event EventHandler OnRecipeCompleted;
    public event EventHandler OnRecipeSuccess;
    public event EventHandler OnRecipeFailed;

    public static DeliveryManager Instance { get; private set; }

    [SerializeField] private RecipeListSO privateListSO;
    [SerializeField] private int waitingRecipesMax = 4;

    private List<RecipeSO> waitingRecipeSOList;

    private int successfulRecipeAmount;
    private float spawnRecipeTimer = 4f;
    private float spawnRecipeTimerMax = 4;

    private void Awake() {
        waitingRecipeSOList = new List<RecipeSO>();
        Instance = this;
    }

    private void Update() {
        if (!IsServer) { return; }

        spawnRecipeTimer -= Time.deltaTime;
        if (spawnRecipeTimer <= 0) {
            spawnRecipeTimer = spawnRecipeTimerMax;

            if (GameManager.Instance.IsGamePlaying() && waitingRecipeSOList.Count < waitingRecipesMax) {
                int waitingRecipeSOIndex = UnityEngine.Random.Range(0, privateListSO.recipeSOList.Count);
                SpawnNewWaitingRecipeClientRpc(waitingRecipeSOIndex);
            }
        }
    }

    [ClientRpc]
    private void SpawnNewWaitingRecipeClientRpc(int waitingRecipeSOIndex) {
        RecipeSO waitingRecipeSO = privateListSO.recipeSOList[waitingRecipeSOIndex];
        waitingRecipeSOList.Add(waitingRecipeSO);
        OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
    }

    public void DeliverRecipe(PlateKitchenObject plateKitchenObject) {
        for (int i = 0; i < waitingRecipeSOList.Count; i++) {
            RecipeSO waitingRecipeSO = waitingRecipeSOList[i];
            if (waitingRecipeSO.kitchenObjectSOList.Count == plateKitchenObject.GetKitchenObjectSOList().Count) {
                // Has the same number of ingredients, let's match them
                bool plateContentsMatchesRecipe = true;
                foreach (var recipeKitchenObjectSO in waitingRecipeSO.kitchenObjectSOList) {
                    // Cycle through all the ingredients in the recipe. Let's check the plate items
                    bool ingredientFound = false;
                    foreach (var plateKitchenObjectSO in plateKitchenObject.GetKitchenObjectSOList()) {
                        // Cycle through all ingredients on the plate
                        if (plateKitchenObjectSO == recipeKitchenObjectSO) {
                            ingredientFound = true;
                            break;
                        }
                    }

                    if (!ingredientFound) {
                        // This ingredient was not found on plate, so let's check the next one
                        plateContentsMatchesRecipe = false;
                    }
                }

                if (plateContentsMatchesRecipe) {
                    // Player delivered correct recipe!
                    DeliverCorrectRecipeServerRpc(i);
                    return;
                }
            }
        }

        // No matches found! 
        // Player did not delivered anything that matches
        DeliverIncorrectRecipeServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void DeliverIncorrectRecipeServerRpc() {
        DeliverIncorrectRecipeClientRpc();
    }
    
    [ClientRpc]
    private void DeliverIncorrectRecipeClientRpc() {

        OnRecipeFailed.Invoke(this, EventArgs.Empty);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DeliverCorrectRecipeServerRpc(int waitingRecipeSOListIndex) {

        DeliverCorrectRecipeClientRpc(waitingRecipeSOListIndex);
    }

    [ClientRpc]
    private void DeliverCorrectRecipeClientRpc(int waitingRecipeSOListIndex) {
        // Player delivered correct recipe!
        successfulRecipeAmount++;

        waitingRecipeSOList.RemoveAt(waitingRecipeSOListIndex);

        OnRecipeCompleted?.Invoke(this, EventArgs.Empty);
        OnRecipeSuccess?.Invoke(this, EventArgs.Empty);
    }

    public List<RecipeSO> GetWaitingRecipeSOList() {
        return waitingRecipeSOList;
    }

    public int GetSuccessfulRecipesAmount() {
        return successfulRecipeAmount;
    }
}
