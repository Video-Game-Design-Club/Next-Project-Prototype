using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
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
     public static bool optionsAreDisplayed = false;
    public GameObject dialogueChoice;
    public GameObject emptyOptions;
    public GameObject canvas;
   
    


    private string[] lines; //array of all lines of the txt doc in order
    private Queue<string> queuedLines; // queue of all lines of the txt doc in reverse order
    private int index = 0; //keeps track of which line we are on.
    private string tempLine = ""; //stores current line
    private string typedName = "..."; //stores current name
    private bool yapping = false; //when the TypeText() is going, yapping = true
    private float yapRate;
    private bool myDialogueIsRunning = false;
    private string[] dialogueOptions;
    private int numberOfOptions;
    

    public void PrintDialogue()
    {
        if (yapping)
            {
                yapping = false;
                return;
            }
        if(optionsAreDisplayed)
            {
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
        var temp = CleanOutComments(queuedLines.Dequeue());
        
        if(temp=="" || temp==" ")
                {
                    index++;
                    Debug.Log("skipped line " + index);
                    PrintDialogue();
                    return;
                }
                else
                {
                    tempLine = temp;
                    Debug.Log(temp);    
                    //tempLine = queuedLines.Dequeue();
                }

            bool tempCheckForBracket = false;
            if(temp[0] == '[')
                {
                    tempCheckForBracket = true;
                }
        
        if(tempCheckForBracket)
        {
             if (temp.Contains("[NAME=")) //if it finds the formatting "[NAME=" then it cuts the line down to only the name, changes the nameTextBox, then saves it to typedName
            {
                string tempName = tempLine.Remove(0,6);
                tempName = tempName.Remove(tempName.IndexOf("]"));
                // Debug.Log(tempName);
                typedName = tempName; //only saving to typedName because if multiple DialogueManagers are happening, any line that is set
                nameTextBox.SetText(tempName);
                index +=1; //keeping count of which line we are on so we know when to reset dialogue or close textbox
                PrintDialogue();
                return;
            }
            else if(temp.Contains("[OPTIONS="))
            {
                string tempNumberString = tempLine.Remove(0,9);
                int tempNumber = int.Parse(tempNumberString.Remove(tempNumberString.IndexOf("]")));
                //Debug.Log(tempNumber);
                numberOfOptions = tempNumber;
                dialogueOptions = new string[numberOfOptions];
                index +=1; //keeping count of which line we are on so we know when to reset dialogue or close textbox
                string tempStorage;
                for (int i = 0; i < numberOfOptions; i++)
                {
                    tempStorage = CleanOutComments(queuedLines.Dequeue());   //dequeues bottom to check if it is null
                        if(tempStorage==null)
                            {
                                tempLine="";
                                dialogueOptions[i] = tempStorage;
                            }
                        else
                            {
                                dialogueOptions[i] = tempStorage;    //if not null, puts into tempstorage
                            }
                        Debug.Log("Option " + (i+1) + " is " +  "\"" + tempStorage + "\"");
                        index +=1;
                }
                CreateOptionButtons();
                //PrintDialogue(); //create option buttons and each button will give a different output
                return;
            }

        }
        else //type out message
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
            // Debug.Log(i);
            // Debug.Log(lines[i]);
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


    // public class DialogueBox : MonoBehaviour
    // {
        
    // }

    string CleanOutComments(string temp)
    {
        if(temp.Contains("//"))
        {
            return temp.Remove(temp.IndexOf("//"),temp.Length-temp.IndexOf("//"));
        }
        return temp;
    }

    void CreateOptionButtons()
    {
        string tempButtonWords = "";
        //float tempTransformx = -1715;
        float tempTransformy = dialogBox.transform.position.y; //(-3165 is bottom, -2659 is top)
        //float tempTransformz = 0;
        float tempButtony = tempTransformy;

        // float canvasWidth = canvas.GetComponent<RectTransform>().rect.width;
        // float canvasHeight = canvas.GetComponent<RectTransform>().rect.height;
        // float canvasCenterX = canvas.GetComponent<RectTransform>().rect.center.x;
        // float canvasCenterY = canvas.GetComponent<RectTransform>().rect.center.y;
        // float textBoxWidth = textBox.GetComponent<RectTransform>().rect.width;
        // float textBoxHeight = textBox.GetComponent<RectTransform>().rect.height;
        // float textBoxCenterX = textBox.GetComponent<RectTransform>().rect.center.x;
        // float textBoxCenterY = textBox.GetComponent<RectTransform>().rect.center.y;

        float optionsButtonHeight = dialogueChoice.GetComponent<RectTransform>().rect.height;
        float canvasScaleY = canvas.transform.localScale.y;
        // float canvasScaleX = canvas.transform.localScale.x;
        // Debug.Log(optionsButtonHeight);

    //create at center of text box, then move over to new Vector3(edge of screen - half width of button, height of text box/2 - height of optionbox*i, 0)
        Vector3 emptyOptionsLocation = new Vector3(emptyOptions.transform.position.x,emptyOptions.transform.position.y,0);
        Vector3 tempTransform = emptyOptionsLocation;
        
        for (int i = 0; i <= dialogueOptions.Count()-1;i++)
        {
            tempButtonWords = dialogueOptions[i]; //dialogueOptions.Count()-1-i

            //DialogueBox Option = Instantiate<DialogueBox>(dialogBox, tempTransform,false, canvas.);  
            GameObject optionButton = Instantiate(dialogueChoice, tempTransform, quaternion.identity);
            optionButton.transform.localScale = canvas.transform.localScale;
            float buttonWidth = optionButton.GetComponent<RectTransform>().rect.x;
            float buttonHeight = optionButton.GetComponent<RectTransform>().rect.y;
            optionButton.transform.SetParent(emptyOptions.transform);
            optionButton.transform.position = emptyOptionsLocation;
            //float tempXForTransform = canvasWidth/2 - buttonWidth/2;
            tempTransform = new Vector3(0,-optionsButtonHeight*canvasScaleY*i, 0);
            optionButton.transform.position += tempTransform;
            optionButton.GetComponentInChildren<TMP_Text>().SetText(tempButtonWords);
            
        }
        optionsAreDisplayed = true;
    }

    void DestroyOptionButtons()
    {
        foreach (Transform child in emptyOptions.transform) 
        {
	        GameObject.Destroy(child.gameObject);
        }
    }

    IEnumerator WaitForOptionResponse()
    {
        yield return new WaitUntil(() => optionsAreDisplayed == false);
        DestroyOptionButtons();
        yield return null;
    }

    public void OptionPicked()
    {
        if (optionsAreDisplayed && !PauseScript.GameIsPaused)
        {
            optionsAreDisplayed = false;
            DestroyOptionButtons();
            TriggerDialogue();
        }
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

    void Awake()
    {
        index = 0;
        queuedLines = new Queue<string>();
        dialogueIsRunning = false;
        gameShouldBeFrozenForDialog = false;
        optionsAreDisplayed = false;
        yapping = false;
        myDialogueIsRunning = false;
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

        // emptyOptions.transform.position = canvas.transform.position;
        // emptyOptions.transform.localScale = canvas.transform.localScale;
    }
}
