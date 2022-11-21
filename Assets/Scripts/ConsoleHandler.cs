using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConsoleHandler : MonoBehaviour
{
    [SerializeField]
    private Canvas canvas;
    private bool consoleOpen = false;
    public void RaiseError(string type, string scriptName, string information)
    {
        // Create a console interface for errors to be raised in
    }
}
