using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class StoryManager : MonoBehaviour
{
    public Color ambientColor = Color.black; // Set to desired ambient color
    public Material skyboxMaterial = null; // Set to desired skybox material or leave null for no skybox
    public Color flickerColor = Color.white; // Color to flicker to
    public float flickerDuration = 0.1f; // Duration of each flicker
    public TextMeshProUGUI dialogueText; // Reference to dialogue text
    public DialogueManager dialogueManager; // Reference to DialogueManager

    private GameObject directionalLight; // Reference to directional light object
    private GameObject flashlight; // Reference to flashlight object
    private Color originalAmbientColor;
    private Material originalSkyboxMaterial;

    void Start()
    {
        directionalLight = GameObject.FindGameObjectWithTag("DirectionalLight");
        flashlight = GameObject.FindGameObjectWithTag("Flashlight");
        if(flashlight != null)
        {
            flashlight.SetActive(false);
            flashlight.GetComponentInChildren<FlashLightBeam>().enabled = false;
        }
        if(directionalLight != null)
        {
            directionalLight.SetActive(false);
        }

        Debug.Log("DarkenScene script started.");

        // Set ambient lighting
        originalAmbientColor = RenderSettings.ambientLight;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = ambientColor;
        Debug.Log("Ambient light set to: " + ambientColor);

        // Set skybox
        originalSkyboxMaterial = RenderSettings.skybox;
        RenderSettings.skybox = skyboxMaterial;
        if (skyboxMaterial == null)
        {
            Debug.Log("Skybox material set to none.");
        }
        else
        {
            Debug.Log("Skybox material set to: " + skyboxMaterial.name);
        }

        // Optionally adjust reflection probes
        ReflectionProbe[] probes = FindObjectsOfType<ReflectionProbe>();
        foreach (var probe in probes)
        {
            probe.intensity = 0.1f; // Adjust intensity as needed
            Debug.Log("Reflection probe intensity set to 0.1 for probe: " + probe.name);
        }

        // Subscribe to the dialogue event
        dialogueManager.OnDialogueDisplayed.AddListener(OnDialogueDisplayed);
    }

    private void OnDialogueDisplayed(string dialogue)
    {
        Debug.Log(dialogue);
        if (dialogue.Contains("flicker"))
        {
            Debug.Log("Detected dialogue with '...here'");
            if (flashlight != null)
            {
                Debug.Log("There is a flashlight in the scene");
                flashlight.SetActive(true);
            }
            StartCoroutine(FlickerEffect());
        }
        if (dialogue.Contains("snaps"))
        {
            AudioManager.instance.Play("Thunder");
            RenderSettings.ambientLight = originalAmbientColor;
            if(directionalLight!= null)
            {
                directionalLight.SetActive(true);
            }   
            flashlight.GetComponentInChildren<FlashLightBeam>().enabled = true;

        }
    }

        // Coroutine to handle flickering effect
    private IEnumerator FlickerEffect()
    {
        // Turn on the light for half a second
        RenderSettings.ambientLight = originalAmbientColor;
        yield return new WaitForSeconds(0.5f);

        // Start flickering
        Color currentColor = flickerColor;
        for (int i = 0; i < 5; i++) // Number of flickers
        {
            RenderSettings.ambientLight = currentColor;
            yield return new WaitForSeconds(flickerDuration);
            // Fade the color towards black
            currentColor = Color.Lerp(currentColor, Color.black, 0.5f); 
            RenderSettings.ambientLight = currentColor;
            yield return new WaitForSeconds(flickerDuration);
        }
    }
}
