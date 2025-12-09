using Unity.Services.Lobbies.Models;
using UnityEngine;
using static Collectable;

public class HazardObjectDamage : MonoBehaviour
{
    public enum StaticEnemyType { Spikes, Saw_Blade, BladeTrap }
    public StaticEnemyType type;
    public int spikeDamageAmount = 5; 
    public int otherDamageAmount = 10; 

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealthBatteryManager player = other.GetComponent<PlayerHealthBatteryManager>();
            if (player != null)
            {

                if (type == StaticEnemyType.Spikes)
                {
                    player.TakeDamageServerRpc(spikeDamageAmount);
                }
                else
                {
                    player.TakeDamageServerRpc(otherDamageAmount);
                }



            }
        }
    }
}
