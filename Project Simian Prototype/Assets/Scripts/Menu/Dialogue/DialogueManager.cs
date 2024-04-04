using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
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

    public DialogueManager self;
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
    private bool dialogueIsLoaded = false;
    private int[] dialogueJumps;
    private int defaultOptionExit;
    private bool usingTextTag = false;
    private int jumpNumber = 0;
    private bool doJumpText = false;
    

    public void PrintDialogue()
    {
        if (yapping)
            {
                yapping = false;
                textBox.text = tempLine;
                return;
            }
        if(optionsAreDisplayed)
            {
                return;
            }

        if (!dialogBox.activeInHierarchy) // && index <= (lines.Count()-1)
            {
                dialogBox.SetActive(true);
                dialogueIsRunning = true;
                myDialogueIsRunning = true;
            }
        if (index>(lines.Count()-1))
            {
                
                ClearDialogue();
                ReadDialogue();
                dialogBox.SetActive(false);
                dialogueIsRunning = false;
                myDialogueIsRunning = false;
                pause.UnFreeze();
                Debug.Log("Text End");
                return;
            }

        if (doJumpText)
        {
            if (!(jumpNumber-1>=lines.Count()))
            {
                index = jumpNumber-1;
                doJumpText=false;
            }
            else
            {
                index = 0;
                doJumpText=false;
                dialogBox.SetActive(false);
                dialogueIsRunning = false;
                myDialogueIsRunning = false;
                pause.UnFreeze();
                return;    
            }
            
        }

        NoOptions:
        Debug.Log("Current Index: " + index);
        var temp = CleanOutComments(lines[index]);
        
        if(temp=="" || temp==" " ||temp==null)
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
                tempName = tempName.Remove(tempName.IndexOf("]"),tempName.Length-tempName.IndexOf("]"));
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
                int tempNumber = int.Parse(tempNumberString.Remove(tempNumberString.IndexOf("]"),tempNumberString.Length-tempNumberString.IndexOf("]")));
                //Debug.Log(tempNumber);
                numberOfOptions = tempNumber;
                dialogueOptions = new string[numberOfOptions];
                defaultOptionExit = numberOfOptions+index+2;
                index +=1; 

                dialogueJumps = new int[numberOfOptions+1];
                if(numberOfOptions==0)
                {
                    index++;
                    goto NoOptions;
                }
                


                string tempStorage;
                for (int i = 0; i < numberOfOptions; i++)
                {
                    tempStorage = CleanOutComments(lines[index]);   
                    int tempJumpNumber = defaultOptionExit;
                    if (tempStorage.Contains("->["))
                    {
                        string evenMoreTempStorage = tempStorage.Remove(tempStorage.IndexOf("->"),2); //evenMoreTempStorage = "message[#]"
                        // Debug.Log(evenMoreTempStorage);
                        tempStorage = tempStorage.Remove(tempStorage.IndexOf("->["),3); //tempStorage = "message#]"
                        // Debug.Log("tempStorage = " + tempStorage);
                        evenMoreTempStorage = evenMoreTempStorage.Remove(evenMoreTempStorage.IndexOf("]"),evenMoreTempStorage.Length-evenMoreTempStorage.IndexOf("]")); //eMTS = "message[#"
                        tempStorage = evenMoreTempStorage.Remove(evenMoreTempStorage.IndexOf("["),evenMoreTempStorage.Length-evenMoreTempStorage.IndexOf("[")); 
                        // Debug.Log(evenMoreTempStorage);
                        evenMoreTempStorage = evenMoreTempStorage.Remove(0, evenMoreTempStorage.IndexOf("[")+1); //eMTS = "#"
                        // Debug.Log(evenMoreTempStorage);
                        tempJumpNumber = int.Parse(evenMoreTempStorage);
    
                    }
                    dialogueJumps[i] = tempJumpNumber;
                    Debug.Log("dialogueJumps at " + i + " set to: " + dialogueJumps[i]);
                        if(tempStorage==null||tempStorage==""||tempStorage==" ")
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
        if (temp.Contains("->["))
                    {
                        
                        string tempStorage = temp.Remove(temp.IndexOf("->"),2); //tempStorage = "message[#]"
                        
                        tempStorage = tempStorage.Remove(tempStorage.IndexOf("]"),tempStorage.Length-tempStorage.IndexOf("]")); //tempStorage = "message[#"
                        temp = tempStorage.Remove(tempStorage.IndexOf("["),tempStorage.Length-tempStorage.IndexOf("[")); //temp = message
                       
                        tempStorage = tempStorage.Remove(0, tempStorage.IndexOf("[")+1); //tempStorage = "#"
                        
                        jumpNumber = int.Parse(tempStorage);
                        Debug.Log("jumpNumber: " + jumpNumber);
                        doJumpText = true;
                        tempLine=temp;
                    }
        

         //type out message
            
                nameTextBox.SetText(typedName); //setting name to last saved name in case of two DialogueManagers running at once
                if (!doJumpText)
                {
                    index+=1; //keeping count of line
                }
                
                // textBox.SetText(tempLine);    //SETS TEXTBOX STRING TO TEMPLINE
                StartCoroutine(TypeText()); //types text based on textspeed
            
    }

    void ReadDialogue()
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

        
        // for (int i= lines.Count() - 1; i >= 0; i--) //for each string, puts a string from the end of the array onto the top of a queue 
        // {
        //     queuedLines.Enqueue(lines[lines.Count()-1-i]);
        //     // Debug.Log(i);
        //     // Debug.Log(lines[i]);
        // }
        //lines = new string[0]; //clears array
    }

    IEnumerator TypeText()
    {
        // textBox.text = "";

        textBox.text=tempLine;
        textBox.text.Insert(0,"<color=#0000>");


        yapping = true;
        for (int i = 0; i<tempLine.Length;i++)
        {
            if (!PauseScript.GameIsPaused)
            {
                if (tempLine[i] == '<')
                {   
                    i++;
                    usingTextTag=true;
                    while (usingTextTag)
                    {
                        if(tempLine[i]!='>')
                        {
                            i++;
                        }
                        else if(tempLine[i]=='>'&&tempLine[i+1]!='<')
                        {
                            textBox.text = tempLine;
                            textBox.text=textBox.text.Insert(i+1,"<color=#0000>");
                            Debug.Log("leaving invisible tag at index " + (i+1));
                            usingTextTag=false;
                            i++;
                        }
                        else
                        {
                            
                            i++;
                            
                        }

                    }
                }
                else if (tempLine[i] != '<')
                {
                    textBox.text = tempLine;
                    textBox.text=textBox.text.Insert(i+1,"<color=#0000>");
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
                    i--;
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
            GameObject optionButton = Instantiate(dialogueChoice, tempTransform, quaternion.identity); //create button
            optionButton.GetComponentInChildren<TMP_Text>().SetText(tempButtonWords);
            int jumpTo = dialogueJumps[i]-1;
            optionButton.GetComponent<Button>().onClick.AddListener(delegate{self.OptionPicked(jumpTo);});
            optionButton.transform.localScale = canvas.transform.localScale; //match the scaling with local scale
            optionButton.transform.SetParent(emptyOptions.transform); //set paret as emptyOptions
            float buttonWidth = optionButton.GetComponent<RectTransform>().rect.x;
            float buttonHeight = optionButton.GetComponent<RectTransform>().rect.y;
            optionButton.transform.position = emptyOptionsLocation; //moves
            //float tempXForTransform = canvasWidth/2 - buttonWidth/2;
            tempTransform = new Vector3(0,-optionsButtonHeight*canvasScaleY*i, 0);
            optionButton.transform.position += tempTransform;
            
            
        }
        optionsAreDisplayed = true;
    }

    public void DestroyOptionButtons()
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

    public void ChangeIndex(int newIndex)
    {
        index = newIndex;
        Debug.Log("Set Index to: " + index);
    }



    public void OptionPicked(int chosenIndex)
    {
        if (optionsAreDisplayed && !PauseScript.GameIsPaused)
        {
            optionsAreDisplayed = false;
            DestroyOptionButtons();
            ChangeIndex(chosenIndex);
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
            if (!dialogueIsLoaded)
            {
                ReadDialogue();
                dialogueIsLoaded = true;
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
        // if (index==0)
        // {
        //     gameShouldBeFrozenForDialog = false;
        // }
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
        lines = new string[0];
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
        dialogueIsLoaded = false;
        //ReadDialogue();
    }


    void Update()
    {
        switch ((int)PlayerPrefs.GetFloat("TextSpeed"))
        {
            // case 1:
            //     yapRate = .03f;
            //     break;
            case 2:
                yapRate = .01f;
                break;
            case 3:
                yapRate = .0001f;
                break;
            default:
                yapRate = .03f;
                break;

        }

        // textBox.ForceMeshUpdate();
        // var textInfo    = textBox.textInfo;
        // for (int i = 0; i < textInfo.characterCount; i++)
        // {
        //     var charInfo = textInfo.characterInfo[i];
        //     var verts = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;

        //     for (int j = 0; j <4; j++)
        //     {
        //         var orig = verts[charInfo.vertexIndex+j];
        //         verts[charInfo.vertexIndex+j] = orig + new Vector3(0, Mathf.Sin(Time.time*2f + orig.x*0.01f)*10f ,0); //Mathf.Sin(Time.time*2f + orig.x*0.01f)*10f
        //     }
        // }

        // for (int i = 0; i < textInfo.meshInfo.Length; i++)
        // {
        //     var meshInfo = textInfo.meshInfo[i];
        //     meshInfo.mesh.vertices = meshInfo.vertices;
        //     textBox.UpdateGeometry(meshInfo.mesh, i);
        // }

        // emptyOptions.transform.position = canvas.transform.position;
        // emptyOptions.transform.localScale = canvas.transform.localScale;
    }
}
