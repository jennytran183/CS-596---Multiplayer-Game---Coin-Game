using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : NetworkBehaviour
{
    public static Transform[] SpawnPoints;

    [Header("Appearance")]
    public Renderer bodyRenderer;
    public Material player1Mat;
    public Material player2Mat;

    [Header("Movement")]
    public float moveSpeed = 6f;

    public NetworkVariable<int> Score = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    Rigidbody rb;
    Camera playerCam;
    NetworkGameManager gm;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerCam = GetComponentInChildren<Camera>(true);
    }

    public override void OnNetworkSpawn()
    {
        gm = FindObjectOfType<NetworkGameManager>();

        // SERVER places players at spawn points
        if (IsServer && SpawnPoints != null && SpawnPoints.Length >= 2)
        {
            int spawnIndex = (OwnerClientId == NetworkManager.ServerClientId) ? 0 : 1;
            spawnIndex = Mathf.Clamp(spawnIndex, 0, SpawnPoints.Length - 1);

            Transform sp = SpawnPoints[spawnIndex];
            if (sp != null)
            {
                transform.SetPositionAndRotation(sp.position, sp.rotation);
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        // Local camera only
        if (playerCam != null)
        {
            bool isLocal = IsOwner;
            playerCam.gameObject.SetActive(isLocal);

            var listener = playerCam.GetComponent<AudioListener>();
            if (listener != null) listener.enabled = isLocal;
        }

        // Appearance by id
        if (bodyRenderer != null)
            bodyRenderer.material = (OwnerClientId == 0) ? player1Mat : player2Mat;
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;

        // Stop movement when game ends
        if (gm != null && gm.GameOver.Value)
        {
            rb.velocity = Vector3.zero;
            return;
        }

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 move = new Vector3(h, 0f, v).normalized * moveSpeed;
        rb.velocity = new Vector3(move.x, rb.velocity.y, move.z);
    }
}