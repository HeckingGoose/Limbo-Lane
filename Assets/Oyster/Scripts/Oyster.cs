using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine; // Link required assemblies
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
public class Oyster : MonoBehaviour
{
    #region External objects
    [SerializeField]
    private GameObject HUD;
    [SerializeField]
    private string language = "EnglishUK";
    #endregion
    #region Conversation Variables
    #region Generic
    [SerializeField] // SerializeField allows the variable to be viewable within the Unity editor without exposing it to every other script
    private float speechCooldownTime = 2;
    [SerializeField]
    private int maxLinesPerFrame = 5; // variable intended to limit how many lines Oyster can process per frame - unlike in version 2 where only 1 line was processed per frame
    private int characterIndex;
    private int conversationIndex; // Setup variables required for conversation stuffs
    private int currentLine;
    private TextAsset conversationData;
    private OysterConversationsContainer conversations;
    private OysterConversation currentConversation;
    public bool inConversation = false; // true when a conversation is in progress
    private string scriptVersion = "?"; // default value so that if a check is made against this variables before Oyster is loaded, the script does not crash
    private string[] tempParams;
    private bool mouseDown;
    private float speechCooldown = 0;
    private float failedReadAttempts = 0;
    private List<String> gameObjectsNames = new List<String>();
    #endregion
    #region AddSprite
    private Sprite loadedSprite;
    private int loaded = 0; //0 means unloaded, 1 means loading, 2 means loaded
    #endregion
    #region AddTextBox
    private int fontLoaded = 0;
    private TMP_FontAsset loadedFont = null;
    #endregion
    #region WaitForInput
    private float waitingTime = 0;
    private float maxWaitTime = 1;
    private bool autoSkip = true;
    #endregion
    #endregion
    #region Script-wide addressables variables
    IDictionary<string, AsyncOperationHandle> loadedAddressables = new Dictionary<string, AsyncOperationHandle>(); // Acts as a list of references for addressables that cannot be unloaded until they stop being used. Such as sprites or fonts
    #endregion
    void Start()
    {
        #region Starting the process of loading version data
        AsyncOperationHandle<TextAsset> versionDataHandle = Addressables.LoadAssetAsync<TextAsset>("Assets/Oyster/JSON/Version.json"); // Creates a handle that loads the file Version.JSON asynchronously
        versionDataHandle.Completed += VersionDataHandle_Completed; // Tells the asset loader to call the listed method when it finishes loading
        #endregion
    }
    private void Update()
    {
        #region Jump back into conversation
        if (inConversation) // If a conversation is currently taking place
        {
            Speak(characterIndex, conversationIndex, currentLine); // Then push the script back into the conversation
        }
        #endregion
        #region Handle whether mouse is being held or ¬
        if (Input.GetAxis("PrimaryAction") > 0) // If the mouse is being held
        {
            mouseDown = true; // Then tell the script that it is currently being held, rather than has just been clicked
        }
        else // If the mouse is not being held
        {
            mouseDown = false; // Then tell the script that the mouse is no longer being held
        }
        #endregion
        #region Handle speech cooldown
        if (speechCooldown > 0) // If the cooldown between conversations has not yet reached 0 seconds
        {
            speechCooldown -= Time.deltaTime; // Subtract the time taken to render this frame from the cooldown timer
        }
        #endregion
    }
    public void Speak(int _characterIndex, int _conversationIndex, int _currentLine) // The main method, this exists to interpret Oyster script files and pass commands and parameters to the correct methods
    {
        #region Pre-conversation setup
        if (!inConversation) // If this is a new conversation
        {
            if (speechCooldown <= 0)
            {
                inConversation = true; // Set inConversation to true so that on next entry the method knows a conversation is underway
                AsyncOperationHandle<TextAsset> conversationDataHandle = Addressables.LoadAssetAsync<TextAsset>("Assets/Oyster/JSON/Conversations-" + language + "/Character" + _characterIndex.ToString() + "-Conversations.json"); // Begin loading the required conversation, with multiple language support. Nifty
                conversationDataHandle.Completed += ConversationHandle_Completed; // Tell the asset loader to start the listed method when in finishes
                characterIndex = _characterIndex;
                conversationIndex = _conversationIndex; // Store internal method variables in a bit more of a global scope for later re-entry
                currentLine = _currentLine;
            }
        }
        #endregion
        #region Conversation
        else // If this is a re-entry from a currently running conversation
        {
            bool lineAlreadyProcessed = false; // Reset the flag stating whether a line has already been processed or not. This flag is set for time sensitive commands that occur across multiple loops but are required to be ran once a frame so that 1 deltaTime = 1 frame, such as a wait command
            for (int i = 0; i < maxLinesPerFrame; i++) // Loop until the max amount of lines per frame has been reached
            {
                try // Attempt to interpret the command at index currentLine
                {
                    currentConversation.commands[currentLine] = currentConversation.commands[currentLine].Replace(" ", ""); // Remove whitespace from input string
                    string[] currentLineData = currentConversation.commands[currentLine].Split(';'); // Split input string into a series of strings along the ; character
                    string currentLineCommand = currentLineData[0]; // Sets the current command to the first string within currentLineData
                    string[] currentLineParameters = new string[currentLineData.Length - 1]; // Initialises currentLineParameters with a fixed size
                    Array.Copy(currentLineData, 1, currentLineParameters, 0, currentLineData.Length - 1); // Sets currentLineParameters equal to an array of every string but the first string within currentLine Data
                    switch (currentLineCommand)
                    {
                        default: // Default response is to inform the Unity console that the command does not exist and then skip to the next line
                            Debug.Log("Command '" + currentLineCommand + "' not present in this version of Oyster, continuing to the next line...");
                            currentLine++;
                            break;
                        case "AddSprite": // Calls the AddSprite method when the command AddSprite is called
                            AddSprite(currentLineParameters);
                            break;
                        case "AddTextBox": // Calls the AddTextBox method when the command AddTextBox is called
                            AddTextBox(currentLineParameters);
                            break;
                        case "WaitForInput": // WaitForInput is time sensitive, as such it is only called when a line has not been processed, and once it has been processed it sets the flag to say that a line has been processed
                            if (!lineAlreadyProcessed)
                            {
                                WaitForInput(currentLineParameters);
                                lineAlreadyProcessed = true;
                            }
                            break;
                        case "AddCharacter": // Not yet implemented
                            break;
                    }
                    failedReadAttempts = 0; // Resets the total failures of this try statement to 0
                }
                catch // If the above try statement fails at any point
                {
                    Debug.Log("Failed to load and run line. Ending conversation in " + (10 - (int)failedReadAttempts).ToString() + " seconds."); // Inform the user that the script failed to read and run a line, and then calculate how many seconds are left of attempts until the script auto-quits
                    if (!lineAlreadyProcessed) // The below code decrements the timer when the script will exit, this occurs in this if statement as it is time sensitive
                    {
                        failedReadAttempts += Time.deltaTime;
                        lineAlreadyProcessed = true;
                    }
                    if (failedReadAttempts > 10f) // If 10 seconds has passed, the script auto quits - if a drive somehow takes 10 seconds to load a JSON file, then I will be impressed
                    {
                        inConversation = false; // Tell the script that it is no longer in a conversation
                        CleanupSpeech(); // Calls the cleanup method to unload any currently loaded addressables and remove any objects created by the script
                    }
                }
            }

        }
        #endregion

    }
    #region Loading font data
    private void FontHandle_Completed(AsyncOperationHandle<TMP_FontAsset> handle) // This method runs once addressables has finished loading a font
    {
        if (handle.Status == AsyncOperationStatus.Succeeded) // If the fontloader succeeded
        {
            loadedFont = handle.Result; // Set the loaded font to the loaded font
            fontLoaded = 2; // Tell the script that a font has finished loading
            AddTextBox(tempParams); // Call the method that originally called it
            loadedAddressables.Add(loadedAddressables.Count.ToString(), handle); // Add the font to the list of currently loaded addressables
        }
        else // If the fontlooader failed
        {
            Debug.Log("Font failed to load! Returning null..."); // Tell the Unity console that the font failed to load
            loadedFont = null; // Set the loaded font to be null
            loaded = 2; // Tell the script that loading has finished
            AddTextBox(tempParams); // Call the method that originally called it
            Addressables.Release(handle); // Release the fontloader from memory
        }
    }
    #endregion
    #region Loading sprite data
    private void SpriteHandle_Completed(AsyncOperationHandle<Sprite> handle) // This method runs once addressables has finished loading a sprite
    {
        if (handle.Status == AsyncOperationStatus.Succeeded) // If the spriteloader succeeded
        {
            loadedSprite = handle.Result; // Set the loaded sprite to be equal to the result of the spriteloader
            loaded = 2; // Tell the sprite loader that the sprite has finished loading
            AddSprite(tempParams); // Call the method that originally called it
            loadedAddressables.Add(loadedAddressables.Count.ToString(), handle); // Add the loaded sprite to the list of currently loaded addressables
        }
        else
        {
            Debug.Log("Sprite failed to load! Returning null..."); // Inform the Unity console that the sprite has failed to load
            loadedSprite = null; // Return a null sprite
            loaded = 2; // Tell the script that sprite loading has finsihed
            AddSprite(tempParams); // Call the method that originally called it
            Addressables.Release(handle); // Release the spriteloader from memory
        }
    }
    #endregion
    #region Loading conversation data
    private void ConversationHandle_Completed(AsyncOperationHandle<TextAsset> handle) // Method that is called once Character#-Conversations.json has been loaded
    {
        if (handle.Status == AsyncOperationStatus.Succeeded) // If the asset was loaded successfully
        {
            conversationData = handle.Result; // Stores the result of handle in conversationData
            conversations = JsonUtility.FromJson<OysterConversationsContainer>(conversationData.text); // Convert the loaded asset into a usable class
            currentConversation = conversations.container[conversationIndex]; // Set currentconversation equal to the conversation currently scored in conversations that matches the index conversationIndex
            if (currentConversation.scriptVersion != scriptVersion) // If script versions do not match
            {
                Debug.Log("Script versions do not match! Some commands in this script may not be recognised."); // Tell the Unity console that issues may occur as the script versions do not match
            }
            else // If script versions match
            {
                Debug.Log("Conversation " + currentConversation.title + " loaded without any errors."); // Tell the Unity console to output that the conversation loaded with no errors
            }
            Addressables.Release(handle); // Removes the asset Character#-Conversations.json from memory
            Speak(characterIndex, conversationIndex, currentLine); // Calls the speak method again, so that the method now continues since the required data has been loaded
        }
        else // If the asset failed to load
        {
            Debug.Log("Conversation data failed to load"); // Relays an error to the Unity console stating that the asset failed to load
            Addressables.Release(handle); // Removes the asset Character#-Conversations.json from memory
        }
    }
    #endregion
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
        scriptVersion = versionData.oysterVersion.packageVersion; // Stores the current script version on a more global scale for compatibility checking
        Debug.Log(versionData.oysterVersion.name + " version " + versionData.oysterVersion.packageVersion + " loaded successfully!"); // Relays the version information to the Unity console
    }
    #endregion
    #region Speech System commands
    #region AddSprite
    private void AddSprite(string[] parameters) // Method that is called when a sprite needs to be added to the scene
    {
        if (loaded == 0) // If no loading has taken place so far
        {
            AsyncOperationHandle<Sprite> spriteHandle = Addressables.LoadAssetAsync<Sprite>("Assets/" + parameters[2] + parameters[1] + ".png"); // Begins loading the sprite from a location provided as part of the method's parameters
            spriteHandle.Completed += SpriteHandle_Completed; // Tells the loader to call the listed method when it finishes
            tempParams = parameters; // Stores the method's parameters externally for when the script is called again from the spriteloader
            loaded = 1; // Sets loaded to 1, meaning that the sprite is loading
        }
        else if (loaded == 2) // When the sprite has finsihed loading
        {
            loaded = 0; // Set loading to 0 for the next time that this method is called
            tempParams = null; // Set tempParams back to being null for the same reason as above
            Sprite sprite = loadedSprite; // Set the variable sprite equal to loadedSprite
            loadedSprite = null; // Set loadedSprite to null for when this method is called again
            string name = parameters[0]; // Set the variable name equal to the first parameter
            Vector2 position = String2Vector(parameters[3]); // Translate the fourth parameter into the sprite's on-screen position
            Vector2 size = new Vector2(100, 100); // Set the variable size equal to a default value
            try // Try to run the below code
            {
                size = new Vector2(sprite.rect.width, sprite.rect.height); // Attempt to set the size equal to the width and height of the sprite
            }
            catch // If the above code fails to run
            {
                Debug.Log("Sprite width and height could not be loaded! Continuing with (100,100)..."); // Inform the Unity console that the above code couldn't be run, and then continue with the previously set default values
            }
            //Define optional parameter default values
            bool spriteIsCool = false; // Testing variable that I will likely keep in just for testing purposes
            if (parameters.Length > 4)
            {
                string[] currentParam;
                for(int i = 0; i < parameters.Length - 4; i++) // Loop through all optional parameters
                {
                    currentParam = parameters[4 + i].Split('='); // split the optional parameter into a name and a data value
                    switch (currentParam[0])
                    {
                        default:
                            Debug.Log("Parameter '" + currentParam[0] + "' not recognised, ignoring parameter..."); // If the name is not recognized then inform the Unity console that the command 'name' is not recognised
                            break;
                        case "spriteIsCool": // If the parameter is 'spriteIsCool'
                            try // Attempt to run the below code
                            {
                                spriteIsCool = Convert.ToBoolean(currentParam[1]); // Convert the parameter's value into a boolean and store that in spriteIsCool
                            }
                            catch // If the above code fails to run
                            {
                                Debug.Log("Failed to convert spriteIsCool input to bool, skipping value..."); // Inform the Unity console that the value of spriteIsCool could not be loaded
                            }
                            break;
                    }
                }
            }
            Vector2 drawPosition = new Vector2((position.x-960) + size.x/2, (540-position.y) - size.y/2); // Since sprites and the canvas have (0,0) to be their centre, do a little maths to figure out where the draw position should be for (0,0) to be the top left
            GameObject output = new GameObject(); // Create a new gameobject
            output.name = name; // Set it's name equal to the name given in the method's parameters
            output.transform.parent = HUD.transform.GetChild(0); // Make the object a child of the HUD's child object 'Sprites'
            output.AddComponent<RectTransform>(); // Add a RectTransform to the object
            output.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1); // Set it's scale to 1, since when making the object it's scale sometimes becomes more than 1 for some unknown reason
            output.GetComponent<RectTransform>().anchoredPosition = drawPosition; // Move the object to it's correct position
            output.GetComponent<RectTransform>().sizeDelta = size; // Scale the object's dimensions to fit the sprite
            output.AddComponent<CanvasRenderer>(); // Add a CanvasRenderer component so that the object is rendered to the HUD
            output.AddComponent<Image>(); // Add an Image component to the object
            output.GetComponent<Image>().sprite = sprite; // Set the sprite value of the Image component equal to the loaded sprite
            currentLine++; // Increment currentLine
            gameObjectsNames.Add(name); // Add this object to the list of loaded objects
        }
    }
    #endregion
    #region AddTextBox
    private void AddTextBox(string[] parameters) // Method that is called when a textbox needs to be added to the scene
    {
        if (fontLoaded != 1) // If a font is currently not loading
        {
            string fontName = null; // Set the fontName to a default value
            if (parameters.Length > 3) // If there are optional parameters
            {
                string[] currentParam;
                for (int i = 0; i < parameters.Length - 3; i++) // Loop through all optional parameters
                {
                    currentParam = parameters[3 + i].Split('='); // Split the current optional parameter into a name and a value
                    switch (currentParam[0])
                    {
                        default:
                            Debug.Log("Parameter '" + currentParam[0] + "' not recognised, ignoring parameter..."); // If the parameter name is not recognized then inform the Unity console that a parameter name was not recognised
                            break;
                        case "font": // If the parameter is 'font'
                            if (fontLoaded == 0) // If a font has not already been loaded
                            {
                                fontName = currentParam[1]; // Set the fontName equal to the parameter value
                                fontLoaded = 1; // Tell the script that a font is currently loading
                                AsyncOperationHandle<TMP_FontAsset> fontLoader = Addressables.LoadAssetAsync<TMP_FontAsset>("Assets/" + fontName); // Begin loading the font
                                fontLoader.Completed += FontHandle_Completed; // Tell the fontloader to call this method once it finishes
                                tempParams = parameters; // Set tempParams equal to the input parameter for when the method is called again
                            }
                            break;
                    }
                }
            }
            if (fontLoaded == 0) // If no font needed to be loaded
            {
                fontLoaded = 2; // Then set the value of fontLoaded to 2, meaning that any fontloading has finished
                loadedFont = null; // Set the loadedFont to be null as no font was loaded
            }
            if (fontLoaded == 2) // Once a font has been loaded, or supposedly loaded, then do this
            {
                fontLoaded = 0; // Set fontLoaded back to 0 for the next time this method is used
                GameObject output = new GameObject(); // Create a new gameobject
                output.transform.parent = HUD.transform.GetChild(1); // Set the object to be a child of the HUD's child 'Text'
                output.AddComponent<RectTransform>(); // Add a RectTransform component to the object
                output.AddComponent<TextMeshProUGUI>(); // Add a TextMeshProUGUI component to the object
                output.AddComponent<CanvasRenderer>(); // Add a CanvasRenderer component to the object
                output.name = parameters[0]; // Set the name of the object equal to the first parameter
                Vector2 size = String2Vector(parameters[2]); // Translate the third parameter to a vector and then set that equal to the variable 'size'
                output.GetComponent<RectTransform>().sizeDelta = size; // Set the object's size equal to the size variable
                output.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1); // Set the scale of the object to 1
                Vector2 position = String2Vector(parameters[1]); // Translate the second parameter to a vector which can then be set as the variable 'position'
                output.GetComponent<RectTransform>().anchoredPosition = new Vector2((position.x-960) + size.x/2, (540-position.y) - size.y/2); // Set the object's position equal to position + some maths to figure out where the top left of the canvas and the object is
                gameObjectsNames.Add(parameters[0]); // Add the object to the list of currently created objects
                if (loadedFont != null) // If a font was loaded
                {
                    output.GetComponent<TMP_Text>().font = loadedFont; // Set the object's font to the loadedFont
                    loadedFont = null; // Set the loaded font back to being null
                }
                currentLine++; // Increment currentLine
            }
        }
    }
    #endregion
    #region WaitForInput
    private void WaitForInput(string[] parameters)
    {
        if (autoSkip) // If the wait command is set to have autoskip enabled
        {
            if (waitingTime == 0) // script just started
            {
                waitingTime += Time.deltaTime; // Add the time taken for the last frame to render to the current waitingTime
                maxWaitTime = waitingTime + 1; // Set the max wait time to be 1 more than the current WaitingTime
                autoSkip = false; // Set autoSkip to be false
                if (parameters.Length > 0) // If there are any parameters
                {
                    foreach (string param in parameters) // Loop through the parameters
                    {
                        string[] currentParam = param.Split('='); // Split the current parameter into a name and a value
                        switch (currentParam[0])
                        {
                            default: // If the parameter is not recognized
                                Debug.Log("Parameter '" + currentParam[0] + "' not recognised, ignoring parameter..."); // Inform the Unity console of the name of the parameter that was not recognised
                                break;
                            case "defaultForwardTime": // If the parameter was defaultForwardTime
                                maxWaitTime = float.Parse(currentParam[1]); // Sets the max wait time to the parameter value
                                autoSkip = true; // Sets autoSkip back to true
                                break;
                        }
                    }
                }
            }
            else if (waitingTime >= maxWaitTime) // wait finished
            {
                waitingTime = 0;
                maxWaitTime = 1; // Set varialbes back to their default values
                autoSkip = true;
                currentLine++; // Increment currentLine
            }
            else // Wait not yet finished
            {
                waitingTime += Time.deltaTime; // Add the current time between each frame to the waitingTime
            }
        }
        if(Input.GetAxis("PrimaryAction") > 0 && !mouseDown) // If the mouse is clicked and not held
        {
            waitingTime = 0;
            maxWaitTime = 1; // Set variables back to their default values
            autoSkip = true;
            currentLine++; // Increment currentLine
        }
    }
    #endregion
    #region WriteLine
    private void WriteLine() // Not yet implemented
    {

    }
    #endregion
    #endregion
    #region Assistant functions
    private Vector2 String2Vector(string input) // Translates a string such as (232,504) to a vector2
    {
        float vectorx = 0;
        float vectory = 0; // Set default values
        string tempString = "";
        int readingValue = 0;
        try // Attempt to run the below code
        {
            foreach (char chr in input) // Loop through each character in the input string
            {
                switch (chr)
                {
                    default: // Skip if none of the below conditions are met
                        break;
                    case '(': // If the character is an open bracket
                        readingValue++; // Increment which value is currently being read
                        break;
                    case ',': // If the character is a comma
                        tempString = tempString.Replace("(", ""); // Set tempString equal to itself minus the open bracket
                        vectorx = float.Parse(tempString); // Set vectorx equal to tempString
                        tempString = ""; // Set tempString back to being empty
                        readingValue++; // Increment which value is currently being read
                        break;
                    case ')':
                        tempString = tempString.Replace(",", ""); // Set tempString equal to itself minus the comma
                        vectory = float.Parse(tempString); // Set vectory equal to tempString
                        tempString = ""; // Set tempString back to being empty
                        break;
                }
                tempString += chr; // Add the current character to tempString
            }
        }
        catch // If any of the above code fails to run
        {
            Debug.Log("Failed to convert input string to vector! Continuing with (100f,100f)"); // Inform the Unity console that something wrong happened
            vectorx = 100f; // Set vectorx and vectory to default values
            vectory = 100f;
        }
        return new Vector2(vectorx, vectory); // Return a new vector made of vectorx and vectory
    }
    private void CleanupSpeech() // Destroys all objects and addressables created by a script
    {
        foreach(string str in gameObjectsNames) // Loops through each entry in gameOBjectsNames
        {
            try // Attempts to find and destroy a gameObject
            {
                Destroy(GameObject.Find(str)); // Find and destroy object
                Debug.Log("Destroyed " + str + " during cleanp"); // Tell the Unity console that an object was found and destroyed
            }
            catch // If it fails to destroy an object
            {
                Debug.Log("Attempted to delete a gameObject that doesn't exist!"); // Inform the Unity console of this issue
            }
        }
        for (int i = 0; i < loadedAddressables.Count; i++) // Loop through each index of the loaded addressables dictionary
        {
            try // Attempt to unload the loaded addressable from the current index
            {
                Addressables.Release(loadedAddressables[i.ToString()]);
                Debug.Log("Unloaded addressable successfully!"); // Inform the Unity console that it succeeded
            }
            catch // If it fails to unload an addressable
            {
                Debug.Log("Failed to release addressable!"); // Inform the Unity console that it failed
            }
        }
        loadedAddressables = new Dictionary<string, AsyncOperationHandle>();
        gameObjectsNames = new List<string>(); // Reset the object and handle trackers to being null for when another script is ran
        speechCooldown = speechCooldownTime; // Set the speechCooldown to the provided time, so that a speech cannot be instantly started after this one. Useful for if the previous speech finished with a click event
    }
    #endregion
}