using UnityEngine;

public class Collectable : MonoBehaviour
{
    public enum CollectableType { Heart, Battery, Hammer }
    public CollectableType type;
    public int amount = 30;

    private void OnTriggerEnter(Collider other)
    {
        PlayerHealthBatteryManager player = other.GetComponent<PlayerHealthBatteryManager>();

        if (player != null)
        {
            if (type == CollectableType.Heart)
            {

                if(player.currentHealth.Value != player.maxHealth)
                {
                    player.Heal(amount);
                    Destroy(gameObject); // Destroy the collectable after it's collected
                }
                
            }
            else if (type == CollectableType.Battery)
            {
                player.RechargeBattery(2*amount);
                Destroy(gameObject); // Destroy the collectable after it's collected

            }
            else if (type == CollectableType.Hammer)
            {
                Loader.Load(Loader.Scene.WinScene);
            }


        }
    }
}
