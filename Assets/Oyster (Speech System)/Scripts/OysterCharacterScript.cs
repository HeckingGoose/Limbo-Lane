using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OysterCharacterScript : MonoBehaviour
{
    [SerializeField]
    private Oyster oyster; // Links to the main Oyster script in the scene
    [SerializeField]
    private int characterID; // ID of character that is being interacted with
    [SerializeField]
    private int conversationIndex; // Index in database for conversation linked to character <- this needs a way to be modifiable for when a conversation ends and the index needs to change
    [SerializeField]
    private int currentLine = 0; // Current line of the script, needs to be changeable so that when script ends script can be re-entered not at beginning
    void Start()
    {
        
    }
    void Update()
    {
        //oyster.SetConversationIndex(conversationIndex);
    }
    public void SetConversationIndex(int inputNum) // Allows for external scripts to modify the value of conversationIndex
    {
        conversationIndex = inputNum;
    }
    public void StartSpeech() // Calls the 'Speak' function within Oyster
    {
        oyster.Speak(conversationIndex);
    }
}
