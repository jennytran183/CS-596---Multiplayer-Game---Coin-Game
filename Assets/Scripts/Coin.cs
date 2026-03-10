using Unity.Netcode;
using UnityEngine;

public class Coin : NetworkBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Only the local player should request the pickup (prevents double-requests)
        var player = other.GetComponent<PlayerController>();
        if (player == null) return;
        if (!player.IsOwner) return;

        // Ask server to pick up this coin
        RequestPickupServerRpc(player.NetworkObjectId);
    }

    [ServerRpc(RequireOwnership = false)]
    void RequestPickupServerRpc(ulong playerNetworkObjectId, ServerRpcParams rpcParams = default)
    {
        // Validate player exists
        if (!NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(playerNetworkObjectId, out var playerObj))
            return;

        var player = playerObj.GetComponent<PlayerController>();
        if (player == null) return;

        // distance check (anti-cheat / avoids weird triggers)
        float dist = Vector3.Distance(player.transform.position, transform.position);
        if (dist > 2.0f) return;

        // Award score (server-authoritative)
        player.Score.Value += 1;

        // Despawn coin for everyone
        GetComponent<NetworkObject>().Despawn();
    }
}