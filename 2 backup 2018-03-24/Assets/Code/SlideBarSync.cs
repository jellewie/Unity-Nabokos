using System;                                                                       //To be able to catch errors
using UnityEngine;
using UnityEngine.UI;                                                               //Required when Using UI elements.

public class SlideBarSync : MonoBehaviour {

//public Text TextNumber;
    public InputField TheTextfield;
    public Slider TheSlider;

    void Start () {
        ChangeDisplayValue();                                                       //Sync number on startup
    }

    public void ChangeSliderValue()
    {
        try                                                                         //Test the code below (see if it gives an error)
        {
            TheSlider.value = System.Convert.ToInt32(TheTextfield.text);            //Convert the player input to an number
        }
        catch (Exception e)                                                         //if the above try gives an error
        {
            Debug.LogError("#ERROR006 - That slider value isn't a value; '" + TheTextfield.text + "' " + e);  //Show an error message
        }
    }

    public void ChangeDisplayValue()
    {
        TheTextfield.text = TheSlider.value.ToString();
    }
}
