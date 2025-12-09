using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerHealthBatteryManager : NetworkBehaviour
{
    public int maxHealth = 100;
    public BarScript healthBar;
    public NetworkVariable<int> currentHealth = new NetworkVariable<int>();

    private int maxBattery = 200;
    private int currentBattery;
    public BarScript batteryBar;

    public Animator anim;

    private bool isWaitingForBattery = false;
    private NetworkVariable<bool> isFlashlightOn = new NetworkVariable<bool>(true); // Flashlight state

    public void InitializeManager(Gradient grad, Color fillColor)
    {
        if (IsOwner)
        {
            healthBar = GameObject.FindGameObjectWithTag("HealthPotion").GetComponent<BarScript>();
            batteryBar = GameObject.FindGameObjectWithTag("Battery").GetComponent<BarScript>();
            batteryBar.SetGradient(grad, fillColor);
            healthBar.SetMaxBarValue(maxHealth);
            batteryBar.SetMaxBarValue(maxBattery);

            anim.SetBool("isDead", false);
        }

        if (IsServer)
        {
            currentHealth.Value = maxHealth;
            isFlashlightOn.Value = true;
        }

        currentBattery = maxBattery;

        currentHealth.OnValueChanged += OnHealthChanged;
        isFlashlightOn.OnValueChanged += OnFlashlightStateChanged;
    }

    private void OnHealthChanged(int oldValue, int newValue)
    {
        if (IsOwner)
        {
            healthBar.SetBarValue(newValue);
        }
    }

    private void OnFlashlightStateChanged(bool oldValue, bool newValue)
    {
        // Update the flashlight state for all clients
        UpdateFlashlightStateClientRpc(newValue);
    }

    private void Update()
    {
        if (IsOwner)
        {
            StartCoroutine(DecreaseBattery());
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(int damage)
    {
        AudioManager.instance.Play("Hit");
        currentHealth.Value -= damage;
        if (currentHealth.Value < 0) currentHealth.Value = 0;
        Debug.Log("Player got injured! Current health: " + currentHealth.Value);

        CheckIfDead();
    }

    private void CheckIfDead()
    {
        if (currentHealth.Value <= 0)
        {
            Debug.Log("Player Dead");
            anim.SetBool("isDead", true);
            GetComponent<PlayerController>().enabled = false;
            NotifyDeathClientRpc();
            StartCoroutine(ShowDeadBeforeLoad());
        }
    }

    private IEnumerator ShowDeadBeforeLoad()
    {
        yield return new WaitForSeconds(3);

        LoadLoseSceneServerRpc();
    }

    [ClientRpc]
    private void NotifyDeathClientRpc()
    {
        if (!IsServer)
        {
            AudioManager.instance.Play("Death");
            anim.SetBool("isDead", true);
            GetComponent<PlayerController>().enabled = false;
        }
    }

    public void Heal(int amount)
    {
        if (!IsServer) return;

        currentHealth.Value += amount;
        if (currentHealth.Value > maxHealth) currentHealth.Value = maxHealth;
        Debug.Log("Player healed! Current health: " + currentHealth.Value);
    }

    public void RechargeBattery(int amount)
    {
        currentBattery += amount;
        if (currentBattery > maxBattery) currentBattery = maxBattery;

        if (currentBattery > 0 && !isFlashlightOn.Value)
        {
            SetFlashlightState(true);
        }

        batteryBar.SetBarValue(currentBattery);
    }

    private void SetFlashlightState(bool isOn)
    {
        if (IsServer)
        {
            isFlashlightOn.Value = isOn;
        }
        else
        {
            UpdateFlashlightStateServerRpc(isOn);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateFlashlightStateServerRpc(bool isOn)
    {
        isFlashlightOn.Value = isOn;
    }

    [ClientRpc]
    private void UpdateFlashlightStateClientRpc(bool isActive)
    {
        GetComponent<PlayerFlashlightInteraction>().flashLightBeam.SetActive(isActive);
    }

    private IEnumerator DecreaseBattery()
    {
        if (isWaitingForBattery) yield break;

        isWaitingForBattery = true;
        yield return new WaitForSeconds(1.0f);

        currentBattery -= 1;
        batteryBar.SetBarValue(currentBattery);
        if (currentBattery <= 0)
        {
            currentBattery = 0;
            if (isFlashlightOn.Value)
            {
                SetFlashlightState(false);
            }


            StartCoroutine(ShowDeadBeforeLoad());
        }
        isWaitingForBattery = false;
    }

    [ServerRpc(RequireOwnership = false)]
    private void LoadLoseSceneServerRpc()
    {
        LoadLoseSceneClientRpc();
    }

    [ClientRpc]
    private void LoadLoseSceneClientRpc()
    {

        LoadLoseScene();
        
    }

    private void LoadLoseScene()
    {
        Loader.LoadNetwork(Loader.Scene.LoseScene);
    }
}
