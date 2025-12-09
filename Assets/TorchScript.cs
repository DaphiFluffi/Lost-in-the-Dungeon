using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorchScript : MonoBehaviour
{
    public float minIntensity = 2.0f;
    public float maxIntensity = 3.0f;
    public float Timer = 0.1f;
    private new Light light = null;


    // Start is called before the first frame update
    void Start()
    {
        light = this.GetComponentInChildren<Light>();
        InvokeRepeating("Flicker",Timer, Timer);
    }

    // Update is called once per frame
    private void Flicker()
    {
        float R = Random.Range(minIntensity,maxIntensity);
        light.intensity = R;
    }
}
