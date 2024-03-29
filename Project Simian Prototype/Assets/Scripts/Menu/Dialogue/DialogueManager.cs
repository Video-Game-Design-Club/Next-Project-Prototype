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
    //the desired txt file should should be at Assets/Dialogue/"folder"/"textFile".txt
    //script grabs txt file by assuming text will be in the Dialogue folder and we guide it by filling in blanks for "folder" and "textFile"
    //intention is that for the Dialogue manager in Unity, we input folder name and the txt file (or alternatively directly as string textFileName) so this DialogueManager knows where to look

    public float textSpeed = 01f; //will be used to make text type character by character
    public GameObject dialogBox;
    public string folder;
    public TMP_Text textBox;
    public TMP_Text nameTextBox;
    //textFileName is prioritized over textFile if textFileName != "", just in case
    public TextAsset textFile; 
    public string textFileName;
    public PauseScript pause;


    private string[] lines; //array of all lines of the txt doc in order
    private Queue<string> queuedLines; // queue of all lines of the txt doc in reverse order
    private int index = 0; //keeps track of which line we are on.
    private string tempLine = ""; //stores current line
    private string typedName = "..."; //stores current name

    

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
                pause.Resume();
                return;
            }
            var temp = queuedLines.Dequeue();   //dequeues bottom to check if it is null
            if(temp==null)
                {
                    tempLine="";
                }
                else
                {
                    tempLine = (string)temp;    
                    //tempLine = queuedLines.Dequeue();
                }
            
            Debug.Log(tempLine);
            if (tempLine.Contains("[NAME=")) //if it finds the formatting "[NAME=" then it cuts the line down to only the name, changes the nameTextBox, then saves it to typedName
            {
                string tempName = tempLine.Remove(0,6);
                tempName = tempName.Remove(tempName.IndexOf("]"));
                Debug.Log(tempName);
                typedName = tempName; //only saving to typedName because if multiple DialogueManagers are happening, any line that is set
                nameTextBox.SetText(tempName);
                index +=1; //keeping count of which line we are on so we know when to reset dialogue or close textbox
                PrintDialogue();
            }
        else
            {
                nameTextBox.SetText(typedName); //setting name to last saved name in case of two DialogueManagers running at once
                index+=1; //keeping count of line
                textBox.SetText(tempLine);
            }
            
            
    }

    void ReadDialogue() //locates .txt then loads lines into queuedLines
    {
        string filePath = /*UnityEngine.Application.streamingAssetsPath*/ "Assets" + "/Dialogue/" + folder + "/";
        if(textFileName!="")
        {
            filePath = filePath + textFileName;
        }
        else
        {
            filePath = filePath + textFile.name;
        }   
        filePath = filePath + ".txt";
        lines = System.IO.File.ReadAllLines(filePath); //reads .txt file and, in order, puts each line as an element in an array

        
        for (int i= lines.Count() - 1; i >= 0; i--) //for each string, puts a string from the end of the array onto the top of a queue 
        {
            queuedLines.Enqueue(lines[lines.Count()-1-i]);
            Debug.Log(i);
            Debug.Log(lines[i]);
        }
        //lines = new string[0]; //clears array
    }

    public void ClearDialogue() //clears everything done in this script
    {
        typedName = "...";
        tempLine = "";
        index=0;
        queuedLines.Clear();
    }

    public void ResetDialogue() //clears then rereads from the .txt file
    {
        ClearDialogue();
        ReadDialogue();
    }

    public void ForceCloseDialogBox()
    {
        dialogBox.SetActive(false);
    }

    public void PrintDialogueAndFreeze()
    {
        pause.Freeze();
        PrintDialogue();
    }

    public void PrintDialogueAndFreezeMovementOnly()
    {
        pause.FreezeMovementOnly();
        PrintDialogue();
    }

    //was thinking about adding more methods
    //also trying to find a way to have text type out character by character

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
