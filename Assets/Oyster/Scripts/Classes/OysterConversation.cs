using System.Collections.Generic;
[System.Serializable]
public class OysterConversation
{
    public List<string> conversationMembers;
    public List<List<string>> conversation;

    public static OysterConversation Create(List<string> _conversationMembers, List<List<string>> _conversation)
    {
        OysterConversation oysterConversation = new OysterConversation();
        oysterConversation.conversationMembers = _conversationMembers;
        oysterConversation.conversation = _conversation;
        return oysterConversation;
    }
}
