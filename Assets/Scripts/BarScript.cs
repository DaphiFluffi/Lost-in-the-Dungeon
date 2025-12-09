using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BarScript : MonoBehaviour
{
    public Slider slider;
    public Gradient gradient;
    public Image fill;

    public void SetGradient(Gradient gradient, Color fillColor)
    {
        this.gradient = gradient;
        fill.color = fillColor;
    }

    public void SetMaxBarValue(int maxValue)
    {
        slider.maxValue = maxValue;
        slider.value = maxValue;
        UpdateFillColor();
    }

    public void SetBarValue(int barValue)
    {
        slider.value = barValue;
        UpdateFillColor();
    }

    private void UpdateFillColor()
    {
        fill.color = gradient.Evaluate(slider.normalizedValue);
    }
}
