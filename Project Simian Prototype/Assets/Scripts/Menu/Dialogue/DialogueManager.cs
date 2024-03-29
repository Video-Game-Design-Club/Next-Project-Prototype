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
    public GameObject dialogBox;
    public string charName;
    public TMP_Text textBox;
    public TMP_Text nameTextBox;
    public TextAsset textFile;
    public string textFileName;
    private string[] lines;
    private Queue<string> queuedLines;
    private int index = 0;
    
    

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

            string tempLine = queuedLines.Dequeue();
            Debug.Log(tempLine);

            if (tempLine.Contains("[NAME="))
            {
                string tempName = tempLine.Remove(0,6);
                tempName = tempName.Remove(tempName.IndexOf("]"));
                Debug.Log(tempName);
                nameTextBox.SetText(tempName);
                index +=1;
                PrintDialogue();
            }
            else
            {
                textBox.SetText(tempLine);
                index+=1;
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
    
    void Start()
    {
        index = 0;
        queuedLines = new Queue<string>();
        ReadDialogue();   
    }
    
}
