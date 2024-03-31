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

    public GameObject dialogBox;
    public string folder;
    public TMP_Text textBox;
    public TMP_Text nameTextBox;
    //textFileName is prioritized over textFile if textFileName != "", just in case
    public TextAsset textFile; 
    public string textFileName;
    public PauseScript pause;
    public static bool dialogueIsRunning = false;
    public static bool gameShouldBeFrozenForDialog = false;
    


    private string[] lines; //array of all lines of the txt doc in order
    private Queue<string> queuedLines; // queue of all lines of the txt doc in reverse order
    private int index = 0; //keeps track of which line we are on.
    private string tempLine = ""; //stores current line
    private string typedName = "..."; //stores current name
    private bool yapping = false; //when the TypeText() is going, yapping = true
    private float yapRate;
    private bool myDialogueIsRunning = false;

    

    public void PrintDialogue()
    {
        if (yapping)
            {
                yapping = false;
                return;
            }

            if (!dialogBox.activeInHierarchy && index <= (lines.Count()-1))
            {
                dialogBox.SetActive(true);
                dialogueIsRunning = true;
                myDialogueIsRunning = true;
            }
            if (index>(lines.Count()-1))
            {
                dialogBox.SetActive(false);
                ClearDialogue();
                ReadDialogue();
                dialogueIsRunning = false;
                myDialogueIsRunning = false;
                pause.UnFreeze();
                return;
            }
            var temp = queuedLines.Dequeue();   //dequeues bottom to check if it is null
            if(temp==null)
                {
                    tempLine="";
                }
                else
                {
                    tempLine = temp;    
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
            // if(tempLine.Contains("[NAME="))
            // {
            //     string tempName = tempLine.Remove(0,6);
            //     tempName = tempName.Remove(tempName.IndexOf("]"));
            //     Debug.Log(tempName);
            //     typedName = tempName; //only saving to typedName because if multiple DialogueManagers are happening, any line that is set
            //     nameTextBox.SetText(tempName);
            //     index +=1; //keeping count of which line we are on so we know when to reset dialogue or close textbox
            //     PrintDialogue();
            // }
        else
            {
                nameTextBox.SetText(typedName); //setting name to last saved name in case of two DialogueManagers running at once
                index+=1; //keeping count of line
                // textBox.SetText(tempLine);    //SETS TEXTBOX STRING TO TEMPLINE

                StartCoroutine(TypeText()); //types text based on textspeed
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

    IEnumerator TypeText()
    {
        textBox.text = "";
        yapping = true;
        for (int i = 0; i<tempLine.Length;i++)
        {
            if (!PauseScript.GameIsPaused)
            {
            textBox.text += tempLine[i];
            if (!yapping)
                {
                    textBox.text = tempLine;
                    yield break;
                }     


            yield return new WaitForSecondsRealtime(yapRate);

            if (!yapping)
                {
                    textBox.text = tempLine;
                    yield break;
                }    
            }
            else
            {
            while (PauseScript.GameIsPaused)
            {
                yield return new WaitForSecondsRealtime(yapRate);
            }
            i--;
            }
        }
        yapping = false;
        yield return null;
    }

    public void TriggerDialogue()
    {
        if(dialogueIsRunning&&!myDialogueIsRunning)
        {
            return;
        }
        if(!PauseScript.GameIsPaused)
        {
            if (index==0)
            {
                ReadDialogue();
            }
            PrintDialogue();
        }
    }

    public void PrintDialogueAndFreeze()
    {
        if(dialogueIsRunning&&!myDialogueIsRunning)
        {
            return;
        }
        pause.Freeze();
        gameShouldBeFrozenForDialog = true;
        TriggerDialogue();
        if (index==0)
        {
            gameShouldBeFrozenForDialog = false;
        }
    }

    public void PrintDialogueAndFreezeMovementOnly()    //wip
    {
        pause.FreezeMovementOnly();
        //PrintDialogue();
        TriggerDialogue();
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

    void Start()
    {
        index = 0;
        queuedLines = new Queue<string>();
        //ReadDialogue(); //loads dialogue  
    }

    void Update()
    {
        switch ((int)PlayerPrefs.GetFloat("TextSpeed"))
        {
            case 1:
                yapRate = .02f;
                break;
            case 2:
                yapRate = .01f;
                break;
            case 3:
                yapRate = .001f;
                break;
            default:
                yapRate = .02f;
                break;

        }
    }
}
