using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.WSA;

public class DialogueManager : MonoBehaviour
{
    public string charName;
    public TMP_Text textBox;
    public TMP_Text nameTextBox;
    public TextAsset textFile;
    private string[] lines;
    Queue<string> queuedLines;

    

    public void PrintDialogue()
    {
        for (int i=0 ;i<=lines.Count(); i++)
        {
            textBox.text = lines[i];
        }
    }

    

    // Start is called before the first frame update
    void Start()
    {
        textBox = GetComponent<TMP_Text>();
        queuedLines = new Queue<string>();
        ReadDialogue();   
    }


    void ReadDialogue()
    {
        string filePath = /*UnityEngine.Application.streamingAssetsPath*/ "Assets" + "/Dialogue/" + charName + "/" + textFile.name + ".txt";
        string[] lines = System.IO.File.ReadAllLines(filePath); 
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
