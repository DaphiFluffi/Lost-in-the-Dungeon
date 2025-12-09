using UnityEngine;
using Unity.Netcode;

public class Projectile : NetworkBehaviour
{
    public float speed = 20f;
    public int damage = 10;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = transform.forward * speed;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        PlayerHealthBatteryManager player = other.GetComponent<PlayerHealthBatteryManager>();
        if (player != null)
        {
            player.TakeDamageServerRpc(damage);
            DestroyProjectileClientRpc();
        }
    }

    [ClientRpc]
    private void DestroyProjectileClientRpc()
    {
        if (NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn();
        }
        Destroy(gameObject);
    }
}
