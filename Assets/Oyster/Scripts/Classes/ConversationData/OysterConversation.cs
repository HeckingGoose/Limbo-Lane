[System.Serializable] // Tell Unity that this is a class
public class OysterConversation
{
    public string title;
    public string scriptVersion; // Variables that are stored within this class
    public string[] commands;
    public static OysterConversation Create(string title, string scriptVersion, string[] commands) // Constructor so that Unity knows what variables actually make up the class
    {
        OysterConversation conversation = new OysterConversation();
        conversation.title = title;
        conversation.scriptVersion = scriptVersion; // Creating and returning an instance of itself
        conversation.commands = commands;
        return conversation;
    }
}
