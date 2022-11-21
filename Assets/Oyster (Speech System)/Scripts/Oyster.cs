using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Data.OleDb;
using System.Transactions;
using UnityEngine.UI;

public class Oyster : MonoBehaviour
{
    [SerializeField]
    private ConsoleHandler consoleHandler;
    [SerializeField]
    private string sus;
    void Start()
    {
        
    }
    void Update()
    {
        
    }
    public void Speak(int characterIndex, int conversationIndex)
    {
        string connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + Application.dataPath + @"\Resources\Oyster\Oyster.accdb";
        // Code currently stops at this point - may need to have an external service interface with the database, maybe an external C# script that's pre compiled?
        OleDbConnection oysterDatabaseConnection = new OleDbConnection(connectionString);
        OleDbCommand oysterDatabaseCommand = new OleDbCommand("SELECT * FROM character" + characterIndex.ToString() + "-conversations");
        oysterDatabaseCommand.Connection = oysterDatabaseConnection;
        try
        {
            oysterDatabaseConnection.Open();
            try
            {
                OleDbDataReader output = oysterDatabaseCommand.ExecuteReader();
                while (output.Read())
                {
                    sus += output.ToString() + '\n';
                }
            }
            catch
            {
                consoleHandler.RaiseError("Database read Error", "Oyster,cs", "Failed to retrieve any data from the currently open database");
            }
        }
        catch
        {
            consoleHandler.RaiseError("Connection Error", "Oyster.cs", "Failed to initialize a connection between the database and Oyster.");
        }
    }
}
