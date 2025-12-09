using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class EnemyScript : NetworkBehaviour
{
    public NavMeshAgent agent;
    public LayerMask whatIsGround, whatIsPlayer;
    public float health = 10;

    //Patroling
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    //Attacking
    public float timeBetweenAttacks;
    bool alreadyAttacked;

    //States
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;

    //Animation
    private Animator anim;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        anim.SetBool("Dead", false);
    }

    private void Update()
    {
        if (!IsServer) return; // Ensure that only the server controls the enemy logic

        Transform nearestPlayer = FindNearestPlayer();
        if (nearestPlayer != null)
        {
            playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
            playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

            if (!playerInSightRange && !playerInAttackRange) Patroling();
            if (playerInSightRange && !playerInAttackRange) ChasePlayer(nearestPlayer);
            if (playerInSightRange && playerInAttackRange) AttackPlayer(nearestPlayer);
        }
    }

    private Transform FindNearestPlayer()
    {
        Transform nearestPlayer = null;
        float minDistance = float.MaxValue;

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {

            try
            {
                var playerTransform = client.PlayerObject.transform;

                float distance = Vector3.Distance(transform.position, playerTransform.position);


                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestPlayer = playerTransform;
                }
            } catch (Exception ex) 
            {
                Debug.LogWarning("Player transform not found");
            }
            
        }
        return nearestPlayer;
    }

    private void Patroling()
    {
        GetComponent<Shoot>().StopShooting();

        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet)
        {
            agent.SetDestination(walkPoint);
            anim.SetFloat("Speed", agent.velocity.magnitude);
            anim.SetBool("Dead", false);
            anim.SetBool("Attack", false);

            Vector3 distanceToWalkPoint = transform.position - walkPoint;

            //Walkpoint reached
            if (distanceToWalkPoint.magnitude < 1f)
                walkPointSet = false;
        }
    }

    private void SearchWalkPoint()
    {
        float randomZ = UnityEngine.Random.Range(-walkPointRange, walkPointRange);
        float randomX = UnityEngine.Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
            walkPointSet = true;
    }

    private void ChasePlayer(Transform player)
    {
        GetComponent<Shoot>().StopShooting();

        agent.SetDestination(player.position);
        anim.SetFloat("Speed", agent.velocity.magnitude);
        anim.SetBool("Dead", false);
        anim.SetBool("Attack", false);
    }

    private void AttackPlayer(Transform player)
    {
        agent.SetDestination(transform.position);
        anim.SetFloat("Speed", agent.velocity.magnitude);
        anim.SetBool("Dead", false);

        //transform.LookAt(player);
        // Calculate the direction to the player but ignore the Y component
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0; // Keep the direction only in the X-Z plane

        // Create the new rotation
        Quaternion lookRotation = Quaternion.LookRotation(direction);

        // Apply the rotation to the enemy
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);



        if (!alreadyAttacked)
        {
            anim.SetBool("Attack", true);
            GetComponent<Shoot>().StartShooting(player);

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    public void TakeDamage(int damage)
    {
        TakeDamageServerRpc(damage);
        AudioManager.instance.Play("BoneCrack");
    }

    [ServerRpc(RequireOwnership = false)]
    private void TakeDamageServerRpc(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            anim.SetBool("Dead", true);
            anim.SetBool("Attack", false);
            Invoke(nameof(DestroyEnemy), 0.5f);
        }
    }

    private void DestroyEnemy()
    {
        AudioManager.instance.Play("Disappear");
        NetworkObject.Despawn();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}
