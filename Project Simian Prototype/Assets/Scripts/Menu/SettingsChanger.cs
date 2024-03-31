using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SettingsChanger : MonoBehaviour
{
    public Slider sensSlider;
    public Slider textSpeedSlider;
    public bool StartAsDefault = false;

    //private float sens = 0.4f;  //Base value, in the future we will need to load from the settings file   //changed code to reference "Sensitivity" in PlayerPrefs instead of "private float sens" here

    public float tempSens = 0;
    public float tempTextSpeed = 0;

    void Awake()
    {
        
    }
    void Start()
    {
        if (StartAsDefault)
        {
            SetDefaultSettings();
        }
        sensSlider.value = PlayerPrefs.GetFloat("Sensitivity");
        textSpeedSlider.value = PlayerPrefs.GetFloat("TextSpeed");
        Debug.Log("Sensitivity Slider Changed to: " + PlayerPrefs.GetFloat("Sensitivity"));
        Debug.Log("Text Speed Slider Changed to: " + PlayerPrefs.GetFloat("TextSpeed"));
    }

    public void DisplayCurrentSettings()
    {
        sensSlider.value = PlayerPrefs.GetFloat("Sensitivity");
        textSpeedSlider.value = PlayerPrefs.GetFloat("TextSpeed");
    }

    public void SetDefaultSettings()
    {
        PlayerPrefs.SetFloat("TextSpeed", 2);
        PlayerPrefs.SetFloat("Sensitivity", 0.6f);
    }
    public void ChangeSensitivity(float newSens) 
    {     
        tempSens = newSens;  
        // cam.m_XAxis.m_MaxSpeed = (1000 * PlayerPrefs.GetFloat("Sensitivity"));
        // cam.m_YAxis.m_MaxSpeed = (10 * PlayerPrefs.GetFloat("Sensitivity"));
    }

    public void ChangeTextSpeed(float newTextSpeed)
    {
        tempTextSpeed = newTextSpeed;
    }

    public void SaveSettings()
    {
        if (tempSens != 0)
        {
            PlayerPrefs.SetFloat("Sensitivity", tempSens); 
            Debug.Log("Sensitivity Changed to: " + PlayerPrefs.GetFloat("Sensitivity"));
        }


         if (tempTextSpeed!=0)
        {
            PlayerPrefs.SetFloat("TextSpeed", tempTextSpeed);
            Debug.Log("Text Speed Changed to: " + PlayerPrefs.GetFloat("TextSpeed"));
        }
        if (tempTextSpeed>3)
        {
            PlayerPrefs.SetFloat("TextSpeed", 3f);
            Debug.Log("Text Speed Changed to: " + PlayerPrefs.GetFloat("TextSpeed"));
        }
    }
    

}
