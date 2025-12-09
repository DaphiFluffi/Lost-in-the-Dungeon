using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonSoundAssigner : MonoBehaviour
{
    private void Start()
    {
        AudioManager audioManager = AudioManager.instance;

        if (audioManager == null)
        {
            Debug.LogError("AudioManager instance not found!");
            return;
        }

        Canvas canvas = FindObjectOfType<Canvas>();
        List<Button> buttons = new List<Button>();
        GetAllButtonsInCanvas(canvas.gameObject, buttons);

        AddSoundToAllButtons(buttons, audioManager, "Click");
    }

    private void GetAllButtonsInCanvas(GameObject parent, List<Button> buttons)
    {
        if (parent == null) return;

        foreach (Transform child in parent.transform)
        {
            Button button = child.GetComponent<Button>();
            if (button != null)
            {
                buttons.Add(button);
            }

            if (child.childCount > 0)
            {
                GetAllButtonsInCanvas(child.gameObject, buttons);
            }
        }
    }

    private void AddSoundToAllButtons(List<Button> buttons, AudioManager audioManager, string defaultSoundName)
    {
        foreach (Button button in buttons)
        {
            button.onClick.AddListener(() => audioManager.Play(defaultSoundName));
        }
    }
}
