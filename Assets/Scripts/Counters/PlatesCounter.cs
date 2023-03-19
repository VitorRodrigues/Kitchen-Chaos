using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlatesCounter : BaseCounter
{
    public event EventHandler OnPlateSpawned;
    public event EventHandler OnPlateRemoved;

    [SerializeField] private KitchenObjectSO plateKitchenObjectSO;
    [SerializeField] private const float spawnPlateTimerMax = 4;
    private float spawnPlateTimer;

    private int platesSpawnedAmountMax = 4;
    private int platesSpawnedAmount;

    private void Update() {
        if (!IsServer) { return; }

        if (platesSpawnedAmount >= platesSpawnedAmountMax || !GameManager.Instance.IsGamePlaying()) { return; }
        spawnPlateTimer += Time.deltaTime;

        if (spawnPlateTimer > spawnPlateTimerMax) {
            spawnPlateTimer = 0;
            SpawnPlateServerRpc();
        }
    }

    [ServerRpc]
    private void SpawnPlateServerRpc() {
        // NOTE: This is not needed since Update() only runs on server (check !IsServer condition)
        // But we are using this method to keep the same pattern from other classes by calling
        // first the ServerRPC method then the ClientRPC method to broadcast the event to the clients
        SpawnPlateClientRpc();
    }

    [ClientRpc]
    private void SpawnPlateClientRpc() {
        platesSpawnedAmount++;

        OnPlateSpawned?.Invoke(this, EventArgs.Empty);

    }

    public override void Interact(Player player) {
        if (!player.HasKitchenObject()) {
            // Player is empty handed
            if (platesSpawnedAmount > 0) {
                KitchenObject.SpawnKitchenObject(plateKitchenObjectSO, player);
                InteractLogicServerRpc();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicServerRpc() {
        InteractLogicClientRpc();
    }

    [ClientRpc]
    private void InteractLogicClientRpc() {
        platesSpawnedAmount--;
        OnPlateRemoved?.Invoke(this, EventArgs.Empty);
    }
}
