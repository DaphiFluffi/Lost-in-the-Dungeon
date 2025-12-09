using System;
using Unity.Netcode;
using UnityEngine;

public class Shoot : NetworkBehaviour
{
    private Transform player;
    public GameObject projectilePrefab;
    public Transform shootPoint;
    public float shootingRange = 10f;
    public float shootingInterval = 4f;
    public float lifespan = 1.5f; // how long the projectile will last before being destroyed
    private float shootingTimer;
    private bool canShoot = false;

    public void StartShooting(Transform player)
    {
        canShoot = true;
        this.player = player;
    }

    public void StopShooting()
    {
        canShoot = false;
        this.player = null;
    }

    void Update()
    {
        if (canShoot)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            if (distanceToPlayer <= shootingRange)
            {
                if (shootingTimer <= 0f)
                {
                    AudioManager.instance.Play("FireSkeleton");
                    if (IsServer)
                    {
                        ShootProjectile(shootPoint.position, (player.position - transform.position).normalized);
                    }
                    else
                    {
                        RequestShootServerRpc(shootPoint.position, (player.position - transform.position).normalized);
                    }
                    shootingTimer = shootingInterval;
                }
            }

            if (shootingTimer > 0f)
            {
                shootingTimer -= Time.deltaTime;
            }
        }
    }

    [ServerRpc]
    private void RequestShootServerRpc(Vector3 shootPosition, Vector3 shootDirection)
    {
        ShootProjectile(shootPosition, shootDirection);
        ShootProjectileClientRpc(shootPosition, shootDirection);
    }

    private void ShootProjectile(Vector3 shootPosition, Vector3 shootDirection)
    {
        GameObject projectile = Instantiate(projectilePrefab, shootPosition, Quaternion.LookRotation(shootDirection));
        NetworkObject networkObject = projectile.GetComponent<NetworkObject>();
        networkObject.Spawn(true);

        Destroy(projectile, lifespan);
    }

    [ClientRpc]
    private void ShootProjectileClientRpc(Vector3 shootPosition, Vector3 shootDirection)
    {
        if (!IsOwner)
        {
            GameObject projectile = Instantiate(projectilePrefab, shootPosition, Quaternion.LookRotation(shootDirection));
            Destroy(projectile, lifespan);
        }
    }
}
