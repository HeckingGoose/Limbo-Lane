using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections.Generic;

public class Oyster : MonoBehaviour
{
    #region Test Variables
    [SerializeField]
    private TextMesh testText;
    #endregion
    public bool inConversation = false;

    #region Conversation Variables
    private int characterIndex;
    private int conversationIndex;
    private int currentLine;
    private TextAsset conversationData;
    #endregion

    void Start()
    {
        #region Starting the process of loading version data
        AsyncOperationHandle<TextAsset> versionDataHandle = Addressables.LoadAssetAsync<TextAsset>("Assets/Oyster/JSON/Version.json"); // Creates a handle that loads the file Version.JSON asynchronously
        versionDataHandle.Completed += VersionDataHandle_Completed; // Tells the asset loader to call the listed method when it finishes loading
        #endregion
    }
    void Update()
    {

    }
    public void Speak(int _characterIndex, int _conversationIndex, int _currentLine)
    {
        if (!inConversation)
        {
            inConversation = true;
            AsyncOperationHandle<TextAsset> conversationDataHandle = Addressables.LoadAssetAsync<TextAsset>("Assets/Oyster/JSON/Conversations/Character" + _characterIndex.ToString() + "-Conversations.json");
            conversationDataHandle.Completed += ConversationHandle_Completed;
            Debug.Log("AGHAHAH");
            _characterIndex = characterIndex;
            _conversationIndex = conversationIndex;
            _currentLine = currentLine;
        }
        else
        {
            Debug.Log("AH?");
            OysterConversation[] conversations = JsonUtility.FromJson<OysterConversation[]>(conversationData.text);
            Debug.Log(conversations.Length); // Add a parent class to OysterConversation that stores OysterConversation, to match JSON hierarchy
        }
        
    }
    private void ConversationHandle_Completed(AsyncOperationHandle<TextAsset> handle) // Method that is called once Character#-Conversations.json has been loaded
    {
        if (handle.Status == AsyncOperationStatus.Succeeded) // If the asset was loaded successfully
        {
            conversationData = handle.Result; // Stores the result of handle in conversationData
            Speak(characterIndex, conversationIndex, currentLine); // Calls the speak method again, so that the method now continues since the required data has been loaded
            Addressables.Release(handle); // Removes the asset Character#-Conversations.json from memory
        }
        else // If the asset failed to load
        {
            Debug.Log("Conversation data failed to load"); // Relays an error to the Unity console stating that the asset failed to load
            Addressables.Release(handle); // Removes the asset Character#-Conversations.json from memory
        }
    }
    #region Loading version data
    private void VersionDataHandle_Completed(AsyncOperationHandle<TextAsset> handle) // Method that is called once Version.json has been loaded
    {
        if (handle.Status == AsyncOperationStatus.Succeeded) // If the asset was loaded successfully
        {
            DisplayVersionData(handle.Result); // Passes the output of this method to DisplayVersionData()
            Addressables.Release(handle); // Removes the asset Version.json from memory
        }
        else // If the asset failed to load
        {
            Debug.Log("Version data failed to load"); // Relays an error to the Unity console stating that the asset failed to load
            Addressables.Release(handle); // Removes the asset Version.json from memory
        }
    }
    private void DisplayVersionData(TextAsset versionDataAsset) // Displays the current version of Oyster to the Unity console
    {
        OysterVersion versionData = JsonUtility.FromJson<OysterVersion>(versionDataAsset.text); // Converts the JSON file into an OysterVersion object
        Debug.Log(versionData.oysterVersion.name + " version " + versionData.oysterVersion.packageVersion + " loaded successfully!"); // Relays the version information to the Unity console
        testText.text = versionData.oysterVersion.packageVersion; // displays the current version on 3D text <- just a sanity check that will be removed later
    }
    #endregion
}