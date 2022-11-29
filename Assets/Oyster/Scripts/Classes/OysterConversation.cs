using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class OysterConversation
{
    public string title;
    public string scriptVersion;
    public string[] commands;
    public static OysterConversation Create(string _title, string _scriptVersion, string[] _commands)
    {
        OysterConversation conversation = new OysterConversation();
        conversation.title = _title;
        conversation.scriptVersion = _scriptVersion;
        conversation.commands = _commands;
        return conversation;
    }
}
