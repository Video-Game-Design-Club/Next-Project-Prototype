using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.WSA;

public class DialogueManager : MonoBehaviour
{
    public float textSpeed = 01f;
    public GameObject dialogBox;
    public string charName;
    public TMP_Text textBox;
    public TMP_Text nameTextBox;
    public TextAsset textFile;
    public string textFileName;
    private string[] lines;
    private Queue<string> queuedLines;
    private int index = 0;
    private string tempLine;
    private string typedName;    
    

    public void PrintDialogue()
    {
            if (!dialogBox.activeInHierarchy && index <= (lines.Count()-1))
            {
                dialogBox.SetActive(true);
            }
            if (index>(lines.Count()-1))
            {
                dialogBox.SetActive(false);
                ClearDialogue();
                ReadDialogue();
                return;
            }
            tempLine = queuedLines.Dequeue();
            Debug.Log(tempLine);
            if (tempLine.Contains("[NAME="))
            {
                string tempName = tempLine.Remove(0,6);
                tempName = tempName.Remove(tempName.IndexOf("]"));
                Debug.Log(tempName);
                typedName = tempName;
                nameTextBox.SetText(tempName);
                index +=1;
                PrintDialogue();
            }
        else
            {
                nameTextBox.SetText(typedName);
                index+=1;
                textBox.SetText(tempLine);
            }
            
            
    }

    void ReadDialogue()
    {
        string filePath = /*UnityEngine.Application.streamingAssetsPath*/ "Assets" + "/Dialogue/" + charName + "/";
        if(textFileName!="")
        {
            filePath = filePath + textFileName;
        }
        else
        {
            filePath = filePath + textFile.name;
        }   
        filePath = filePath + ".txt";
        lines = System.IO.File.ReadAllLines(filePath);

        for (int i= lines.Count() - 1; i >= 0; i--)
        {
            queuedLines.Enqueue(lines[lines.Count()-1-i]);
            Debug.Log(i);
            Debug.Log(lines[i]);
        }
    }

    void ClearDialogue()
    {
        index=0;
        queuedLines.Clear();
    }

    // void NextLine()
    // {

    //     if (tempLine.Contains("[NAME="))
    //         {
    //             string tempName = tempLine.Remove(0,6);
    //             tempName = tempName.Remove(tempName.IndexOf("]"));
    //             Debug.Log(tempName);
    //             nameTextBox.SetText(tempName);
    //             index +=1;
    //             PrintDialogue();
    //         }
    //     else
    //         {
    //             index+=1;
    //             textBox.SetText("");
    //             while (!Input.anyKey)
    //             {
    //                 foreach (char c in tempLine.ToCharArray())
    //                 {
    //                     textBox.SetText(textBox.text+c);
    //                     Wait(textSpeed);
    //                 }
    //             }
                
    //         }
        

    // }

    // IEnumerable Wait(float time)
    // {
    //     yield return new WaitForSeconds(time);
    // }
    
    // IEnumerable TypeText(string message)
    // {
    //     char[] tempChars = message.ToCharArray();
    //     string currentText = "";
    //     foreach (char c in message.ToCharArray())
    //     {
    //         textBox.SetText(currentText+tempChars[c].ToString());
    //         currentText = textBox.GetComponent<TMP_InputField>().text;
    //         if (Input.GetKeyDown(KeyCode.KeypadEnter))
    //         {
    //             textBox.SetText(message);
    //             yield return 0;
    //         }
    //         yield return new WaitForSeconds(textSpeed);
    //     }
    //     yield return 0;
    // }


    void Start()
    {
        index = 0;
        queuedLines = new Queue<string>();
        ReadDialogue();   
    }
    
}
