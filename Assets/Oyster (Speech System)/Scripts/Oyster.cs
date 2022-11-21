using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Data.OleDb;
using System.Transactions;
using UnityEngine.UI;

public class Oyster : MonoBehaviour
{
    private string dataPath = "";
    void Start()
    {
        
    }
    void Update()
    {
        
    }
    public void Speak(int conversationIndex)
    {
        dataPath = Application.dataPath;
        string connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source="+dataPath+@"\Resources\Oyster.accdb";
        //OleDbCommand
    }
}
