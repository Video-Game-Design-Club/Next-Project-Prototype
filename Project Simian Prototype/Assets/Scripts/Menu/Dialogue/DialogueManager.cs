using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{

    public TextMeshProUGUI charName; //textbox for character name
    public TextMeshProUGUI charText; //textbox for character dialogue
   
    public string charFileName;
    public string charFileNum;

   void PrintDialogue()
   {
         foreach (string line in ReadDialogue(charFileName,charFileNum))
         {
            charText.text = line;
         }
   }

   List<string> ReadDialogue(string name, string number)
    {
        string filePath = Application.streamingAssetsPath + "/Dialogue/" + name + number + ".txt";
        List<string> output = File.ReadAllLines(filePath).ToList();
        return output; 
    }




}
