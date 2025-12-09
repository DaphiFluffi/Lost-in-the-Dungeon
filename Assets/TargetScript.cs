using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetScript : MonoBehaviour
{
    public void ChangeColor(string color)
    {
        if (color == "purple")
        {
            GetComponent<Renderer>().material.color = Color.magenta;
        }
        if (color == "green")
        {
            GetComponent<Renderer>().material.color = Color.green;
        }
        if (color == "yellow")
        {
            GetComponent<Renderer>().material.color = Color.yellow;
        }
        if(color == "white")
        {
            GetComponent<Renderer>().material.color = Color.white;
        }
    }
}
