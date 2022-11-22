using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Data.OleDb;
using System.Transactions;
using UnityEngine.UI;
using System.Linq.Expressions;
using System;

public class Oyster : MonoBehaviour
{
    [SerializeField]
    private string sus;
    private string dataPath;
    void Start()
    {
        dataPath = Application.dataPath.Replace('/','\\') + @"\Resources\Oyster\JSON";
    }
    void Update()
    {
        
    }
    public void Speak(int characterIndex, int conversationIndex)
    {

    }
}
