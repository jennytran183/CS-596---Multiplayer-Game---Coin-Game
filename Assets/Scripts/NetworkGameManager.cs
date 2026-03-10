using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkGameManager : NetworkBehaviour
{
    [Header("Coin Spawning")]
    public NetworkObject coinPrefab;
    public Transform coinSpawnPointsParent;
    public float spawnInterval = 1.0f;
    public int maxCoinsAlive = 20;

    [Header("Match Timer")]
    public float matchLengthSeconds = 60f;

    public NetworkVariable<float> TimeLeft = new NetworkVariable<float>(
        0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkVariable<bool> GameOver = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkVariable<ulong> WinnerClientId = new NetworkVariable<ulong>(
        ulong.MaxValue, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkVariable<int> WinnerScore = new NetworkVariable<int>(
        0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkVariable<bool> IsTie = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    float spawnTimer;

    void Awake()
    {
        
        // PlayerController.SpawnPoints = new Transform[] { p1, p2 };
        // var parent = GameObject.Find("SpawnPoints");
        // if (parent == null)
        // {
        //     Debug.LogWarning("SpawnPoints parent not found");
        //     return;
        // }
        var parent = GameObject.Find("SpawnPoints");
        var p1 = parent.transform.Find("P1Spawn");
        var p2 = parent.transform.Find("P2Spawn");

        if (p1 == null || p2 == null)
        {
            Debug.LogWarning("No P1Spawn/P2Spawn under SpawnPoints");
            return;
        }

        PlayerController.SpawnPoints = new Transform[] { p1, p2 };

        Debug.Log($"SpawnPoints[0]={p1.name} pos={p1.position}");
        Debug.Log($"SpawnPoints[1]={p2.name} pos={p2.position}");
    }

    void Start()
    {
        // Making sure the scene GameManager is spawned on server so NetworkVariables replicate
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            var no = GetComponent<NetworkObject>();
            if (no != null && !no.IsSpawned) no.Spawn();
        }
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        // Reset match state
        GameOver.Value = false;
        WinnerClientId.Value = ulong.MaxValue;
        WinnerScore.Value = 0;
        IsTie.Value = false;

        TimeLeft.Value = matchLengthSeconds;
        spawnTimer = 0f;
    }

    void Update()
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer) return;

        // If already ended -> do nothing
        if (GameOver.Value) return;

        // timer
        if (TimeLeft.Value > 0f)
        {
            TimeLeft.Value = Mathf.Max(0f, TimeLeft.Value - Time.deltaTime);
        }

        // When time hits 0, end game ONCE
        if (TimeLeft.Value <= 0f)
        {
            EndGameAndPickWinner();
            return;
        }

        // coin spawning
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            TrySpawnCoin();
        }
    }

    void EndGameAndPickWinner()
    {
        var players = FindObjectsOfType<PlayerController>();

        int bestScore = int.MinValue;
        ulong bestId = ulong.MaxValue;
        bool tie = false;

        foreach (var p in players)
        {
            int s = p.Score.Value;
            if (s > bestScore)
            {
                bestScore = s;
                bestId = p.OwnerClientId;
                tie = false;
            }
            else if (s == bestScore)
            {
                tie = true;
            }
        }

        WinnerClientId.Value = bestId;
        WinnerScore.Value = (bestScore == int.MinValue) ? 0 : bestScore;
        IsTie.Value = tie;

        GameOver.Value = true;
    }

    void TrySpawnCoin()
    {
        if (CountAliveCoins() >= maxCoinsAlive) return;
        if (coinPrefab == null || coinSpawnPointsParent == null) return;

        List<Transform> points = new List<Transform>();
        foreach (Transform child in coinSpawnPointsParent) points.Add(child);
        if (points.Count == 0) return;

        Transform chosen = points[Random.Range(0, points.Count)];
        var coin = Instantiate(coinPrefab, chosen.position, Quaternion.identity);
        coin.Spawn(true);
    }

    int CountAliveCoins()
    {
        if (NetworkManager.Singleton == null) return 0;

        int count = 0;
        foreach (var kvp in NetworkManager.Singleton.SpawnManager.SpawnedObjects)
        {
            if (kvp.Value != null && kvp.Value.GetComponent<Coin>() != null)
                count++;
        }
        return count;
    }
}