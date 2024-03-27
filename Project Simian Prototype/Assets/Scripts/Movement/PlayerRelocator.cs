using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRelocator : MonoBehaviour
{

public Transform Player;

float TempSavedX;
float TempSavedY;
float TempSavedZ;
float SavedX;
float SavedY;
float SavedZ;

[SerializeField]
bool debugMode = true;
bool createSaveForNoSave = false;
public float xStart = 0f;
public float yStart = 0f;
public float zStart = 0f;


    void Awake()
    {
        TempSavedX = PlayerPrefs.GetFloat("TempSavedX");
        TempSavedY = PlayerPrefs.GetFloat("TempSavedY");
        TempSavedZ = PlayerPrefs.GetFloat("TempSavedZ");
        
        if (PlayerPrefs.GetInt("NoSave") == 0 && createSaveForNoSave)
        {
            PlayerPrefs.SetFloat("SavedX", xStart);
            PlayerPrefs.SetFloat("SavedY", yStart);
            PlayerPrefs.SetFloat("SavedZ", zStart);
            PlayerPrefs.SetInt("NoSave", 1);
        }
        SavedX = PlayerPrefs.GetFloat("SavedX");
        SavedY = PlayerPrefs.GetFloat("SavedY");
        SavedZ = PlayerPrefs.GetFloat("SavedZ");

    }

    void Update()
    {

        if(debugMode)
        {
            if(Input.GetKey(KeyCode.O))
            {
                SaveCurrentPlayerPosition();
            }
            if (Input.GetKey(KeyCode.P))
            {
                TeleportToLastTempSave();
            }

        }    
            
    }

    
    void TeleportToLastTempSave()
    {
        TeleportPlayer(TempSavedX,TempSavedY,TempSavedZ);
        Debug.Log("Changed Player Position to:" + TempSavedX + ", " + TempSavedY + ", " + TempSavedZ);
    }

    void ChangeSavedPlayerPosition(float xPos, float yPos, float zPos)
    {
        PlayerPrefs.SetFloat("SavedX", xPos);
        PlayerPrefs.SetFloat("SavedY", yPos);
        PlayerPrefs.SetFloat("SavedZ", zPos);
        SavedX = PlayerPrefs.GetFloat("SavedX");
        SavedY = PlayerPrefs.GetFloat("SavedY");
        SavedZ = PlayerPrefs.GetFloat("SavedZ");
        Debug.Log("Saved Player Position to:" + SavedX + ", " + SavedY + ", " + SavedZ);
    }

    void SaveCurrentPlayerPosition()
    {
        PlayerPrefs.SetFloat("TempSavedX", Player.position.x);
        PlayerPrefs.SetFloat("TempSavedY", Player.position.y);
        PlayerPrefs.SetFloat("TempSavedZ", Player.position.z);
        TempSavedX = PlayerPrefs.GetFloat("TempSavedX");
        TempSavedY = PlayerPrefs.GetFloat("TempSavedY");
        TempSavedZ = PlayerPrefs.GetFloat("TempSavedZ");
        Debug.Log("Saved Temp Player Position to:" + TempSavedX + ", " + TempSavedY + ", " + TempSavedZ);
    }

    void MoveTempSaveToSave()
    {
        PlayerPrefs.SetFloat("SavedX", TempSavedX);
        PlayerPrefs.SetFloat("SavedY", TempSavedY);
        PlayerPrefs.SetFloat("SavedZ", TempSavedZ);
        SavedX = PlayerPrefs.GetFloat("SavedX");
        SavedY = PlayerPrefs.GetFloat("SavedY");
        SavedZ = PlayerPrefs.GetFloat("SavedZ");
        Debug.Log("Moved Temp Saved Player Position into Saved Player Position");
    }

    void TeleportPlayer(float xPos, float yPos, float zPos)
    {
        Player.position = new Vector3(xPos,yPos,zPos);

    }

}
