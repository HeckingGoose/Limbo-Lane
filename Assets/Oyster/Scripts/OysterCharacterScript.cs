using UnityEngine; // Import required assemblies

public class OysterCharacterScript : MonoBehaviour
{
    [SerializeField]
    private Oyster oyster; // Links to the main Oyster script in the scene
    [SerializeField]
    private int characterID; // ID of character that is being interacted with
    [SerializeField]
    private string conversationName; // Name in database for conversation linked to character <- this needs a way to be modifiable for when a conversation ends and the name needs to change
    [SerializeField]
    private int currentLine = 0; // Current line of the script, needs to be changeable so that when script ends script can be re-entered not at beginning
    public void StartSpeech() // Calls the 'Speak' function within Oyster
    {
        if (!oyster.inConversation) // If a conversation is not already happening
        {
            oyster.Speak(characterID, conversationName, currentLine); // Call the speak function within oyster
        }
    }
    public string GetConversationName()
    {
        return conversationName;
    }
    public void SetConversationName(string newConversationName)
    {
        conversationName = newConversationName;
    }
}
