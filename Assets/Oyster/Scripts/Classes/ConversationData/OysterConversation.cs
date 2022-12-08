[System.Serializable] // Tell Unity that this is a class
public class OysterConversation
{
    public string title;
    public string scriptVersion; // Variables that are stored within this class
    public string[] commands;
    public static OysterConversation Create(string _title, string _scriptVersion, string[] _commands) // Constructor so that Unity knows what variables actually make up the class
    {
        OysterConversation conversation = new OysterConversation();
        conversation.title = _title;
        conversation.scriptVersion = _scriptVersion; // Creating and returning an instance of itself
        conversation.commands = _commands;
        return conversation;
    }
}
