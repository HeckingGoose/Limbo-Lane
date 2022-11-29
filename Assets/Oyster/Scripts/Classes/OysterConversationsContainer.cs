using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
[System.Serializable]
public class OysterConversationsContainer
{
    public OysterConversation[] container;
    public static OysterConversationsContainer Create(OysterConversation[] _container)
    {
        OysterConversationsContainer conversations = new OysterConversationsContainer();
        conversations.container = _container;
        return conversations;
    }
}
