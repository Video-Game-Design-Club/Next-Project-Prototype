using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using Unity.VisualScripting;
public class VolumeChanger : MonoBehaviour
{

    public Slider overallVol;
    public Slider musicVol;
    public Slider effectVol;
    // Start is called before the first frame update
    void Start()
    {
           if(overallVol.value==0)
           {
                overallVol.value = PlayerPrefs.GetFloat("OverallVol");
           }
           if(musicVol.value==0)
           {
                musicVol.value = PlayerPrefs.GetFloat("MusicVol");
           }
           if(effectVol.value==0)
           {
                effectVol.value = PlayerPrefs.GetFloat("EffectVol");
           }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeOverallVol()
    {
        if(overallVol.value!=0)
        {
        PlayerPrefs.SetFloat("OverallVol",overallVol.value);
        Debug.Log("Overall Volume set to: " + PlayerPrefs.GetFloat("OverallVol"));
        }
    }

    public void ChangeMusicVol()
    {
        if(musicVol.value!=0)
        {
        PlayerPrefs.SetFloat("MusicVol",musicVol.value);
        Debug.Log("Overall Music set to: " + PlayerPrefs.GetFloat("MusicVol"));
        }
    }

    public void ChangeEffectVol()
    {
        if(effectVol.value!=0)
        {
        PlayerPrefs.SetFloat("EffectVol",effectVol.value);
        Debug.Log("Effect Volume set to: " + PlayerPrefs.GetFloat("EffectVol"));
        }
    }

}
