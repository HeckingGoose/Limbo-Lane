using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine; // Link required assemblies
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class Oyster : MonoBehaviour
{
    #region External objects
    [SerializeField]
    private GameObject HUD; // Store a reference to the scene's HUD so that generated objects can be set to its children
    [SerializeField]
    private string language = "EnglishUK"; // Language variable that can potentially be changed to allow for multi-language support (too bad I only speak one language :\)
    #endregion
    #region Conversation Variables
    // These variables need to be cleaned up a bit, maybe create a fontLoader that can be called seperately to load fonts, and make classes to store the variables for each function
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
    private CharacterDataContainer characters;
    private bool charactersLoaded = false;
    private Dictionary<string, CharacterData> charactersInConversation = new Dictionary<string, CharacterData>();
    private Dictionary<string, GameObject> characterObjectsInConversation = new Dictionary<string, GameObject>();
    private Dictionary<string, string> conversationStrings = new Dictionary<string, string>();
    #endregion
    #region WaitForInput
    private float waitingTime = 0;
    private float maxWaitTime = 1;
    private bool autoSkip = true;
    #endregion
    #region AddSmoothText
    private int charactersPerSecond = 2;
    private float smoothTextWaitTime = 0;
    private int currentCharIndex = 0;
    #endregion
    #region ModifyObject
    private GameObject modObj_object;
    private bool modObj_foundObject = false;
    private bool modObj_moving;
    private bool modObj_rotating;
    private bool modObj_scaling;
    private bool modObj_clickToSkip;
    private bool modObj_manipulatePermanent;
    private bool modObj_relativeTargetPosition;
    private float modObj_currentTime;
    private float modObj_totalTime;
    private string modObj_interpolation;
    private Vector3 modObj_newPosition;
    private Vector3 modObj_newRotation;
    private Vector3 modObj_newScale;
    private Vector3 modObj_ogPosition;
    private Vector3 modObj_ogRotation;
    private Vector3 modObj_ogScale;
    private Dictionary<int, ObjectAndPosition> modObj_objectsToFix = new Dictionary<int, ObjectAndPosition>();
    #endregion
    #region ModifyAnimVariable
    private bool modAnimVar_objectFound = false;
    private Animator modAnimVar_animator;
    #endregion
    #region LegacyPlayAnimation
    private int legacyPlayAnim_animState = 0;
    private bool legacyPlayAnim_waitOnAnim;
    private Animation legacyPlayAnim_targetAnimation;
    #endregion
    #region PlayAnimation
    private int playAnim_animState = 0;
    private bool playAnim_waitOnAnim;
    private Animator playAnim_targetAnimator;
    private string playAnim_layerName;
    private string playAnim_animName;
    #endregion
    #region AddInputField
    private bool inpField_created = false;
    private string inpField_textInput = null;
    private GameObject inpField_output;
    #endregion
    #region Addressables variables
    #region General
    private Dictionary<int, AsyncOperationHandle> loadedAddressables = new Dictionary<int, AsyncOperationHandle>(); // Acts as a list of references for addressables that cannot be unloaded until they stop being used. Such as sprites or fonts
    #endregion
    #region Font loading
    private int fontLoaded = 0;
    private TMP_FontAsset loadedFont;
    private Dictionary<string, TMP_FontAsset> loadedFonts = new Dictionary<string, TMP_FontAsset>();
    #endregion
    #region Sprite loading
    private int spriteLoaded = 0;
    private Sprite loadedSprite;
    private Dictionary<string, Sprite> loadedSprites = new Dictionary<string, Sprite>();
    #endregion
    #endregion
    #endregion
    void Start()
    {
        #region Starting the process of loading version data
        AsyncOperationHandle<TextAsset> versionDataHandle = Addressables.LoadAssetAsync<TextAsset>("Assets/Oyster/JSON/Version.json"); // Creates a handle that loads the file Version.JSON asynchronously
        versionDataHandle.Completed += VersionDataHandle_Completed; // Tells the asset loader to call the listed method when it finishes loading
        #endregion
        #region Starting the process of loading character data
        AsyncOperationHandle<TextAsset> characterDataHandle = Addressables.LoadAssetAsync<TextAsset>("Assets/Oyster/JSON/Characters.json");
        characterDataHandle.Completed += CharacterDataHandle_Completed; // Begins loading character data and points the loader to a method that it can call on completion
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
                    bool removingBlank = true; // Spaces default to not being read
                    string tempString = ""; // Custom way of parsing the line. It simply checks through each character stores everything but spaces, unless the spaces are between ''
                    foreach (char chr in currentConversation.commands[currentLine])
                    {
                        if (chr == '\'') // When a ' is recognised, invert whether spaces are read or not
                        {
                            if (removingBlank)
                            {
                                removingBlank = false;
                            }
                            else
                            {
                                removingBlank = true;
                            }
                        }
                        if (removingBlank && chr != ' ') // If spaces are being removed, and the current character is not a space
                        {
                            tempString += chr; // Add a character to tempString
                        }
                        else if (!removingBlank) // If spaces are not being removed
                        {
                            tempString += chr; // Add a character to tempString
                        }
                    }
                    tempString = tempString.Replace("'", ""); // Removes leftover ''
                    string[] currentLineData = tempString.Split(';'); // Split input string into a series of strings along the ; character
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
                        case "AddCharacter": // Calls the AddCharacter method when the command AddCharacter is read
                            AddCharacter(currentLineParameters);
                            break;
                        case "AddCharacterObject": // Calls the AddCharacterObject method when the command AddCharacterObject is read
                            AddCharacterObject(currentLineParameters);
                            break;
                        case "ModifyTextBox": // Calls the ModifyTextBox method when the command ModifyTextBox is read
                            ModifyTextBox(currentLineParameters);
                            break;
                        case "AddSmoothText": // Calls the AddSmoothText method when the command AddSmoothText is read
                            AddSmoothText(currentLineParameters);
                            break;
                        case "ManipulateObject": // Calls the ManipulateObject method when the command ManipulateObject is read
                            ManipulateObject(currentLineParameters);
                            break;
                        case "ModifyAnimVariable": // Calls the ModifyAnimVariable method when the command ModifyAnimVariable is read
                            ModifyAnimVariable(currentLineParameters);
                            break;
                        case "AddInputField": // Calls teh AddInputField method when the command AddInputField is read
                            AddInputField(currentLineParameters);
                            break;
                        case "LoadFont":
                            LoadFont(currentLineParameters);
                            break;
                        case "LoadSprite":
                            LoadSprite(currentLineParameters);
                            break;
                        case "LegacyPlayAnimation":
                            LegacyPlayAnimation(currentLineParameters);
                            break;
                        case "PlayAnimation":
                            PlayAnimation(currentLineParameters);
                            break;
                        case "SetAnimLayerWeight":
                            SetAnimLayerWeight(currentLineParameters);
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
    #region Waiting for Input submission
    private void OnInputSubmit(string input)
    {
        inpField_textInput = input;
    }
    #endregion
    #region Loading character data
    private void CharacterDataHandle_Completed(AsyncOperationHandle<TextAsset> handle) // Method that is called when the characterDataLoader finishes
    {
        if (handle.Status == AsyncOperationStatus.Succeeded) // If the asset was loaded successfully
        {
            try // Try to parse the loaded asset
            {
                characters = JsonUtility.FromJson<CharacterDataContainer>(handle.Result.text); // Convert the asset from JSON into a custom class
            }
            catch
            {
                Debug.Log("Characters.json not in the correct format!"); // Inform the Unity console that the asset is not formatted properly
                characters = new CharacterDataContainer(); // Return an empty value
            }
        }
        else // If the asset was loaded unsuccessfully
        {
            Debug.Log("Character data failed to load!"); // Inform the Unity console that the asset did not load
            characters = new CharacterDataContainer(); // Return an empty value
        }
        Addressables.Release(handle);
        charactersLoaded = true;
    }
    #endregion
    #region Loading font data
    private void FontHandle_Completed(AsyncOperationHandle<TMP_FontAsset> handle) // This method runs once addressables has finished loading a font
    {
        if (handle.Status == AsyncOperationStatus.Succeeded) // If the fontloader succeeded
        {
            loadedFont = handle.Result; // Set the loaded font to the loaded font
            loadedAddressables.Add(loadedAddressables.Count, handle); // Add the font to the list of currently loaded addressables
        }
        else // If the fontlooader failed
        {
            Debug.Log("Font failed to load! Returning null..."); // Tell the Unity console that the font failed to load
            loadedFont = null; // Set the loaded font to be null
            Addressables.Release(handle); // Release the fontloader from memory
        }
        fontLoaded = 2;
        LoadFont(tempParams);
    }
    #endregion
    #region Loading sprite data
    private void SpriteHandle_Completed(AsyncOperationHandle<Sprite> handle) // This method runs once addressables has finished loading a sprite
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            loadedSprite = handle.Result;
            loadedAddressables.Add(loadedAddressables.Count, handle);
        }
        else
        {
            Debug.Log("Sprite failed to load! Returning null...");
            loadedSprite = null;
            Addressables.Release(handle);
        }
        spriteLoaded = 2;
        LoadSprite(tempParams);
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
        // Required Parameters:
        // param0: object name
        // param1: sprite name
        // param2: position
        // Optional Parameters:
        // param: spriteIsCool = bool
        try
        {
            Sprite sprite = loadedSprites[parameters[1]]; // Set the variable sprite equal to loadedSprite
            loadedSprite = null; // Set loadedSprite to null for when this method is called again
            string name = parameters[0]; // Set the variable name equal to the first parameter
            Vector2 position = String2Vector(parameters[2]); // Translate the fourth parameter into the sprite's on-screen position
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
                string[] currentParameter;
                for (int i = 0; i < parameters.Length - 4; i++) // Loop through all optional parameters
                {
                    currentParameter = parameters[4 + i].Split('='); // split the optional parameter into a name and a data value
                    switch (currentParameter[0])
                    {
                        default:
                            Debug.Log("Parameter '" + currentParameter[0] + "' not recognised, ignoring parameter..."); // If the name is not recognized then inform the Unity console that the command 'name' is not recognised
                            break;
                        case "spriteIsCool": // If the parameter is 'spriteIsCool'
                            try // Attempt to run the below code
                            {
                                spriteIsCool = Convert.ToBoolean(currentParameter[1]); // Convert the parameter's value into a boolean and store that in spriteIsCool
                            }
                            catch // If the above code fails to run
                            {
                                Debug.Log("Failed to convert spriteIsCool input to bool, skipping value..."); // Inform the Unity console that the value of spriteIsCool could not be loaded
                            }
                            break;
                    }
                }
            }
            Vector2 drawPosition = new Vector2((position.x - 960) + size.x / 2, (540 - position.y) - size.y / 2); // Since sprites and the canvas have (0,0) to be their centre, do a little maths to figure out where the draw position should be for (0,0) to be the top left
            GameObject output = new GameObject(); // Create a new gameobject
            output.name = name; // Set it's name equal to the name given in the method's parameters
            output.transform.parent = HUD.transform.GetChild(0); // Make the object a child of the HUD's child object 'Sprites'
            RectTransform outputRectTransform = output.AddComponent<RectTransform>(); // Add a RectTransform to the object
            outputRectTransform.localScale = new Vector3(1, 1, 1); // Set it's scale to 1, since when making the object it's scale sometimes becomes more than 1 for some unknown reason
            outputRectTransform.anchoredPosition = drawPosition; // Move the object to it's correct position
            outputRectTransform.sizeDelta = size; // Scale the object's dimensions to fit the sprite
            output.AddComponent<CanvasRenderer>(); // Add a CanvasRenderer component so that the object is rendered to the HUD
            UnityEngine.UI.Image outputImage = output.AddComponent<UnityEngine.UI.Image>(); // Add an Image component to the object
            outputImage.sprite = sprite; // Set the sprite value of the Image component equal to the loaded sprite
            gameObjectsNames.Add(name); // Add this object to the list of loaded objects
        }
        catch
        {
            Debug.Log("Sprite '" + parameters[1] + "' not currently loaded.");
        }
        currentLine++;
    }
    #endregion
    #region AddTextBox
    private void AddTextBox(string[] parameters) // Method that is called when a textbox needs to be added to the scene
    {
        // Required Parameters:
        // param0: object name
        // param1: position
        // param2: size
        // Optional Parameters:
        // param: font = string
        // param: text = string
        // param: fontSize int
        // param: colour = hex colour (#FFFFFF)

        TMP_FontAsset font = null;
        string text = "";
        int fontSize = 56;
        Color fontColour = new Color(0, 0, 0);
        if (parameters.Length > 3) // If there are optional parameters
        {
            string[] currentParameter;
            for (int i = 0; i < parameters.Length - 3; i++) // Loop through all optional parameters
            {
                currentParameter = parameters[3 + i].Split('='); // Split the current optional parameter into a name and a value
                switch (currentParameter[0])
                {
                    default:
                        Debug.Log("Parameter '" + currentParameter[0] + "' not recognised, ignoring parameter..."); // If the parameter name is not recognized then inform the Unity console that a parameter name was not recognised
                        break;
                    case "font": // If the parameter is 'font'
                        try
                        {
                            font = loadedFonts[currentParameter[1]];
                        }
                        catch
                        {
                            Debug.Log("Font '" + currentParameter[1] + "' not currently loaded.");
                        }
                        break;
                    case "text": // If the parameter is 'text'
                        text = currentParameter[1]; // set text equal to the currentParameter
                        break;
                    case "fontSize": // If the parameter is 'fontSize'
                        try // Attempt to parse the currentParameter as an integer
                        {
                            fontSize = Convert.ToInt32(currentParameter[1]);
                        }
                        catch // If it fails, then inform the Unity console
                        {
                            Debug.Log("Failed to convert " + currentParameter[0] + " to an integer.");
                        }
                        break;
                    case "colour": // If the parameter is 'colour'
                        ColorUtility.TryParseHtmlString(currentParameter[1], out fontColour); // Attempt to convert the input colour from hex to rgb
                        break;
                }
            }
        }
        GameObject output = new GameObject(); // Create a new gameobject
        output.transform.parent = HUD.transform.GetChild(1); // Set the object to be a child of the HUD's child 'Text'
        RectTransform outputRectTransform = output.AddComponent<RectTransform>(); // Add a RectTransform component to the object
        TextMeshProUGUI outputTextMesh = output.AddComponent<TextMeshProUGUI>(); // Add a TextMeshProUGUI component to the object
        output.name = parameters[0]; // Set the name of the object equal to the first parameter
        Vector2 size = String2Vector(parameters[2]); // Translate the third parameter to a vector and then set that equal to the variable 'size'
        outputRectTransform.sizeDelta = size; // Set the object's size equal to the size variable
        outputRectTransform.localScale = new Vector3(1, 1, 1); // Set the scale of the object to 1
        Vector2 position = String2Vector(parameters[1]); // Translate the second parameter to a vector which can then be set as the variable 'position'
        outputRectTransform.anchoredPosition = new Vector2((position.x - 960) + size.x / 2, (540 - position.y) - size.y / 2); // Set the object's position equal to position + some maths to figure out where the top left of the canvas and the object is
        gameObjectsNames.Add(parameters[0]); // Add the object to the list of currently created object
        if (font != null)
        {
            outputTextMesh.font = font;
        }
        outputTextMesh.text = text;
        outputTextMesh.fontSize = fontSize;
        outputTextMesh.color = fontColour;
        currentLine++; // Increment currentLine
    }
    #endregion
    #region WaitForInput
    private void WaitForInput(string[] parameters)
    {
        // Required Parameters:
        // none
        // Optional Parameters:
        // param: defaultForwardTime = float
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
                        string[] currentParameter = param.Split('='); // Split the current parameter into a name and a value
                        switch (currentParameter[0])
                        {
                            default: // If the parameter is not recognized
                                Debug.Log("Parameter '" + currentParameter[0] + "' not recognised, ignoring parameter..."); // Inform the Unity console of the name of the parameter that was not recognised
                                break;
                            case "defaultForwardTime": // If the parameter was defaultForwardTime
                                maxWaitTime = float.Parse(currentParameter[1]); // Sets the max wait time to the parameter value
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
        if (Input.GetAxis("PrimaryAction") > 0 && !mouseDown) // If the mouse is clicked and not held
        {
            waitingTime = 0;
            maxWaitTime = 1; // Set variables back to their default values
            autoSkip = true;
            currentLine++; // Increment currentLine
        }
    }
    #endregion
    #region AddCharacter
    private void AddCharacter(string[] parameters)
    {
        // Required Parameters:
        // param0: character name
        if (charactersLoaded) // If the list of characters has finished loading (only relevant for slow drives or a script that has this command being called the moment a scene starts)
        {
            string characterName = parameters[0].ToLower(); // Set characterName equal to the first parameter
            bool addedCharacter = false; // Set addedCharacter to false
            foreach (CharacterData character in characters.characters) // loop through each character in the previously loaded list of characters
            {
                if (characterName == character.altName.ToLower()) // If the characterName input is the same as the altName of the character entry currently being looked at
                {
                    charactersInConversation.Add(character.altName, character); // Add the character to the dictionary of characters in the conversation, using their altName as the key
                    addedCharacter = true; // Set addedCharacter to true
                }
            }
            if (!addedCharacter) // If addedCharacter was never set to true
            {
                Debug.Log("Unable to find character '" + characterName + "'."); // Inform Unity that the character could not be found
            }
            currentLine++; // Continue to the next line
        }
    }
    #endregion
    #region AddCharacterObject
    private void AddCharacterObject(string[] parameters)
    {
        // Required Parameters:
        // param0: character altName
        // param1: character object name
        try // Attempt to run the below code
        {
            GameObject output = GameObject.Find(parameters[1]); // Find a gameobject by the value of the second input parameter
            characterObjectsInConversation.Add(parameters[0], output); // Add the previously found object to a dictionary of characters in the scene, with the first input parameter being used as a key
        }
        catch // If the above code fails to run
        {
            Debug.Log("Unable to find and add " + parameters[0] + "'s character object '" + parameters[1] + "'."); // Inform the Unity console that the object could not be found and added to the dictionary
        }
        currentLine++; // Continue to the next line
    }
    #endregion
    #region ModifyTextBox
    private void ModifyTextBox(string[] parameters)
    {
        // Required Parameters
        // param0: object name
        // Optional Parameters:
        // param: newName = string
        // param: size = Vector2
        // param: position = Vector2
        // param: colour = hex colour (#FFFFFF)
        // param: fontSize = int
        // param: text = string
        // param: font = string
        try // Attempt to run the below code
        {
            GameObject output = GameObject.Find(parameters[0]); // Find a gameobject by the first input parameter
            RectTransform outputRectTransform = output.GetComponent<RectTransform>(); // Find the object's RectTransform component
            TextMeshProUGUI outputTextMesh = output.GetComponent<TextMeshProUGUI>(); // Find the object's TextMesh component
            for (int i = 1; i < parameters.Length - 1; i++) // Loop through all parameters given except for the first parameter
            {
                string[] currentParameter = parameters[i].Split('='); // Split the current parameter into a name and a value
                switch (currentParameter[0]) // Compare the parameter name against a list of 'choices'
                {
                    default: // If the parameter name is not recognised
                        Debug.Log("Parameter '" + currentParameter[1] + "' not recognised."); // Inform the Unity console that the parameter was not recognised
                        break;
                    case "newName": // If the parameter is 'newName'
                        output.name = currentParameter[1]; // Set the previously found gameobject's name to the current parameter's value
                        break;
                    case "size": // If the parameter is 'size'
                        outputRectTransform.sizeDelta = String2Vector(parameters[i]); // Convert the currentParameter value to a Vector2 and then set the size of the found object to that value
                        break;
                    case "position": // If the parameter is 'position'
                        Vector2 size = outputRectTransform.sizeDelta; // Find the size of the previously stored object
                        Vector2 position = String2Vector(currentParameter[1]); // Find the position of the object using the current parameter value and String2Vector
                        outputRectTransform.anchoredPosition = new Vector2((position.x - 960) + size.x / 2, (540 - position.y) - size.y / 2); // Do some funky maths to place the object at its intended position from its top left corner, not the centre
                        break;
                    case "colour": // If the parameter is 'colour'
                        Color fontColour = new Color(); // Create a new colour object
                        ColorUtility.TryParseHtmlString(parameters[i], out fontColour); // Attempt to parse the current parameter value as a hex colour
                        outputTextMesh.color = fontColour; // Set the object's TextMesh's colour to the found colour
                        break;
                    case "fontSize": // If the parameter is 'fontSize'
                        try // Attempt to run the below code
                        {
                            outputTextMesh.fontSize = Convert.ToInt32(currentParameter[1]); // Set the object's TextMesh's fontSize equal to the input parameter's value converted to an integer
                        }
                        catch // If the above code fails to run
                        {
                            Debug.Log("Failed to convert " + currentParameter[0] + " to an integer."); // Inform the Unity console that the above code failed to run
                        }
                        break;
                    case "text": // If the parameter it 'text'
                        outputTextMesh.text = currentParameter[1]; // Set the text value of the object's TextMesh to the current parameter value
                        break;
                    case "font":
                        try
                        {
                            outputTextMesh.font = loadedFonts[currentParameter[1]];
                        }
                        catch
                        {
                            Debug.Log("Font '" + currentParameter[1] + "' not currently loaded.");
                        }
                        break;
                }
            }
        }
        catch // If any of the above code fails to run
        {
            Debug.Log("GameObject '" + parameters[0] + "' does not exist in the current scene."); // Inform the Unity console that something has gone wrong
        }
        currentLine++; // Continue to the next line
    }
    #endregion
    #region AddSmoothText
    private void AddSmoothText(string[] parameters)
    {
        // Required Parameters
        // param0: text
        // param1: object to add text to
        // Optional Parameters:
        // param: clickToSkip = bool
        smoothTextWaitTime += Time.deltaTime; // Add the time between the last frame and this frame to the current wait time
        if (smoothTextWaitTime * charactersPerSecond >= 1) // If the wait time multiplied by the total characters per second exceeds 1
        {
            int charactersToAdd = Convert.ToInt32(smoothTextWaitTime); // Truncate the current wait time to find how many characters need to be added - this will almost always return one, however it is here to handle sudden frame dips where delta time may be more than 1
            smoothTextWaitTime = (smoothTextWaitTime * charactersPerSecond) % 1; // Remove any value on the left of the decimal point
            TextMeshProUGUI outputTextMesh = null; // Create a default value for a TextMesh
            try // Attempt to run the below code
            {
                outputTextMesh = GameObject.Find(parameters[1]).GetComponent<TextMeshProUGUI>(); // Attempt to find an object and its TextMesh
            }
            catch // If the above code fails
            {
                Debug.Log("Failed to find text box '" + parameters[1] + "' to add characters to!"); // Inform the Unity console that the above code failed
            }
            for (int i = 0; i < charactersToAdd; i++) // Loop through every character that needs to be added
            {
                try // Attempt to run the below code
                {
                    outputTextMesh.text += parameters[0][i + currentCharIndex]; // Add the character at index i + currentCharIndex to the object's text
                }
                catch // If any of the above code fails to run
                {
                    Debug.Log("Character to add is out of range!"); // Inform the Unity console that something went wrong
                    currentCharIndex = 0; // Reset the currentCharIndex to 0
                    currentLine++; // Continue to the next line
                }
            }
            currentCharIndex += charactersToAdd; // Add charactersToAdd to the currentCharIndex
        }
        bool clickToSkip = true; // Create clickToSkip and set it to its default value of true
        for (int i = 0; i < parameters.Length - 2; i++) // Loop through every parameter except for the first 2
        {
            string[] currentParameter = parameters[i + 2].Split('='); // Split the current parameter into a name and a value
            switch (currentParameter[0]) // compare the name of the current parameter against each of these options
            {
                default: // If the name of the current parameter is not recognised
                    Debug.Log("Parameter '" + currentParameter[0] + "' not recognised."); // Inform the Unity console that the current parameter was not recognised
                    break;
                case "clickToSkip": // If the current parameter's name is 'clickToSkip'
                    try // Attempt to run the below code
                    {
                        clickToSkip = Convert.ToBoolean(currentParameter[1]); // Conver the current parameter value to a boolean and store it in clickToSkip
                    }
                    catch // If any of the below code fails to run
                    {
                        Debug.Log("Unable to convert '" + currentParameter[1] + "' into a boolean value."); // Inform the Unity console that something went wrong
                    }
                    break;
            }
        }
        if (clickToSkip && !mouseDown && Input.GetAxis("PrimaryAction") > 0) // If clickToSkip is true, the mouse is not currently held and the mouse has been clicked
        {
            mouseDown = true; // Set mouseDown to true
            TextMeshProUGUI outputTextMesh = null; // create an empty TextMesh variable
            try // Attempt to run the below code
            {
                outputTextMesh = GameObject.Find(parameters[1]).GetComponent<TextMeshProUGUI>(); // Find an object's TextMesh using the second input parameter
            }
            catch // If any of the above code fails to run
            {
                Debug.Log("Failed to find text box '" + parameters[1] + "' to add characters to!"); // Inform the Unity console that something went wrong
            }
            for (int i = 0; i < parameters[0].Length - currentCharIndex; i++) // Loop for the total length of the first parameter minus the currentCharIndex
            {
                outputTextMesh.text += parameters[0][currentCharIndex + i]; // Add the character at currentCharIndex + i to the object's TextMesh's text
            }
            currentCharIndex = 0;
            smoothTextWaitTime = 0; // Set these two variables back to defualt values
            currentLine++; // Continue to the next line
        }
    }
    #endregion
    #region ManipulateObject
    private void ManipulateObject(string[] parameters)
    {
        // Required Parameters:
        // param0: object name
        // Optional Parameters:
        // param: moveTo = Vector3
        // param: rotateTo = Vector3
        // param: scaleTo = Vector3
        // param: interpolation = string
        // param: time = float
        // param: clickToSkip = bool
        // param: manipulatePermanent = bool
        // param: relativeTargetPosition = bool

        if (!modObj_foundObject) // If the object has not been loaded yet
        {
            try
            {
                modObj_object = GameObject.Find(parameters[0]);
                modObj_ogPosition = modObj_object.transform.position;
                modObj_ogRotation = modObj_object.transform.eulerAngles;
                modObj_ogScale = modObj_object.transform.localScale;
                modObj_foundObject = true;
                string[] currentParameter;
                modObj_moving = false;
                modObj_rotating = false;
                modObj_scaling = false;
                modObj_totalTime = 1f; // Set a comedically large amount of default values
                modObj_currentTime = 0f;
                modObj_interpolation = "none";
                modObj_clickToSkip = true;
                modObj_relativeTargetPosition = false;
                modObj_manipulatePermanent = false;

                for (int i = 0; i < parameters.Length - 1; i++) // Loop through all parameters
                {
                    currentParameter = parameters[i + 1].Split('='); // Split the current parameter into a name and a value
                    switch (currentParameter[0]) // Compare the name of the current parameter against the below choices
                    {
                        default: // If the current parameter's name is not recognised
                            Debug.Log("Parameter '" + currentParameter[0] + "' not recognised."); // Inform the Unity console that the current parameter is not recognised
                            break;
                        case "moveTo": // If the current parameter is 'moveTo'
                            modObj_moving = true; // Set modObj_moving to true
                            modObj_newPosition = String3Vector(currentParameter[1]); // Set modObj_newPosition to the current parameter's value converted to a Vector3
                            break;
                        case "rotateTo": // If the current parameter is 'rotateTo'
                            modObj_rotating = true; // Set modObj_rotating to true
                            modObj_newRotation = String3Vector(currentParameter[1]); // Set modObj_newRotation to the current parameter's value converted to a Vector3
                            break;
                        case "scaleTo": // If the current parameter is 'scaleTo'
                            modObj_scaling = true; // Set modObj_scaling to true
                            modObj_newScale = String3Vector(currentParameter[1]); // Set modObj_newScale to the current parameter's value converted to a Vector3
                            break;
                        case "interpolation": // If the current parameter is 'interpolation'
                            modObj_interpolation = currentParameter[1]; // Set modObj_interpolation equal to the current parameter's value
                            break;
                        case "time": // If the current parameter is 'time'
                            try // Attempt to run the below code
                            {
                                modObj_totalTime = float.Parse(currentParameter[1]); // Convert and store the current parameter's value as a float
                            }
                            catch // If the above code fails to run
                            {
                                Debug.Log("Failed to convert '" + currentParameter[1] + "' to float."); // Inform the Unity console that something went wrong
                            }
                            break;
                        case "clickToSkip": // If the current parameter is 'clickToSkip'
                            try // Attempt to run the below code
                            {
                                modObj_clickToSkip = Convert.ToBoolean(currentParameter[1]); // Convert and store the current parameter's value as a boolean
                            }
                            catch // If the above code fails to run
                            {
                                Debug.Log("Failed to convert '" + currentParameter[1] + "' to boolean."); // Inform the Unity console that something went wrong
                            }
                            break;
                        case "manipulatePermanent": // If the current parameter is 'manipulatePermanent'
                            try // Attempt to run the below code
                            {
                                modObj_manipulatePermanent = Convert.ToBoolean(currentParameter[1]); // Convert and store the current parameter's value as a boolean
                            }
                            catch // If the above code fails to run
                            {
                                Debug.Log("Failed to convert '" + currentParameter[1] + "' to boolean."); // Inform the Unity console that something went wrong
                            }
                            break;
                        case "relativeTargetPosition": // If the current parameter is 'relativeTargetPosition'
                            try // Attempt to run the below code
                            {
                                modObj_relativeTargetPosition = Convert.ToBoolean(currentParameter[1]); // Convert and store the current parameter's value as a boolean
                            }
                            catch // If the above code fails to run
                            {
                                Debug.Log("Failed to convert '" + currentParameter[1] + "' to boolean."); // Inform the Unity console that something went wrong
                            }
                            break;
                    }
                }
            }
            catch // If the above code fails to run
            {
                Debug.Log("Unable to find '" + parameters[0] + "' in the current scene."); // Inform the Unity console that something went wrong
                currentLine++; // Continue to the next line
            }
        }
        else // If the object has been loaded
        {
            bool done = false; // Set done to a default value
            if (modObj_clickToSkip && Input.GetAxis("PrimaryAction") > 0 && !mouseDown) // If clickToSkip is true and the mouse is being clicked and the mouse is not being held
            {
                mouseDown = true; // Set mouseDown to true
                modObj_interpolation = "none"; // Set the interpolation mode to none
            }
            switch (modObj_interpolation) // Compare the interpolation mode to the below choices
            {
                default: // If the interpolation is not recognised (no interpolation)
                    done = true; // Set done to true
                    if (modObj_moving) // If the object is meant to be moving
                    {
                        done = false; // Set done to false
                        if (modObj_relativeTargetPosition) // If the position it will be moved to is relative
                        {
                            modObj_object.transform.position = modObj_ogPosition + modObj_newPosition; // Set the object's position equal to its own position + its new position
                        }
                        else // If the position it will be moved to is non-relative
                        {
                            modObj_object.transform.position = modObj_newPosition; // Set the object's position equal to the new position
                        }
                        modObj_moving = false; // Set moving to false
                    }
                    if (modObj_rotating) // If the object is meant to be rotating
                    {
                        done = false; // Set done to false
                        modObj_object.transform.eulerAngles = modObj_newRotation; // Set the object's rotation to its new rotation
                        modObj_rotating = false; // Set rotating to false
                    }
                    if (modObj_scaling) // If the object is meant to be scaling
                    {
                        done = false; // Set done to false
                        modObj_object.transform.localScale = modObj_newScale; // Set the object's scale to its new scale
                        modObj_scaling = false; // Set scaling to false
                    }
                    break;
                case "linear": // If the interpolation mode is 'linear'
                    done = false; // Set done to false
                    modObj_currentTime += Time.deltaTime; // Add the time between this frame and the previous frame to the currentTime
                    if (modObj_moving) // If the object is meant to be moving
                    {
                        if (modObj_relativeTargetPosition) // If the movement is supposed to be relative
                        { // Set the object to a new position that is linearly interpolated between its original position and the new position
                            modObj_object.transform.position = Vector3.Lerp(modObj_ogPosition, modObj_newPosition + modObj_ogPosition, modObj_currentTime / modObj_totalTime);
                        }
                        else // If the movement is not supposed to be relative
                        { // Set the object to a new position that is linearly interpolated between its original position and the new position
                            modObj_object.transform.position = Vector3.Lerp(modObj_ogPosition, modObj_newPosition, modObj_currentTime / modObj_totalTime);
                        }
                    }
                    if (modObj_rotating) // If the object is meant to be rotating
                    { // Set the object to a new rotation that is linearly interpolated between its original rotation and the new rotation
                        modObj_object.transform.rotation = Quaternion.Lerp(Quaternion.Euler(modObj_ogRotation), Quaternion.Euler(modObj_newRotation), modObj_currentTime / modObj_totalTime);
                    }
                    if (modObj_scaling) // If the object is meant to be scaling
                    { // Set the object to a new scale that is linearly interpolated between its original scale and the new scale
                        modObj_object.transform.localScale = Vector3.Lerp(modObj_ogScale, modObj_newScale, modObj_currentTime / modObj_totalTime);
                    }
                    if (modObj_currentTime >= modObj_totalTime) // If current time exceeds or is equal to the total time
                    {
                        done = true; // Set done to true
                        modObj_currentTime = 0; // Set the currentTime back to 0
                        modObj_moving = false;
                        modObj_rotating = false; // Set all of the object transformation states back to false
                        modObj_scaling = false;
                    }
                    break;
            }
            if (done) // IF done
            {
                if (!modObj_manipulatePermanent) // If the movement is not meant to be permanent
                {
                    ObjectAndPosition tempObj = new ObjectAndPosition(); // Create a new container for the object and its old transform values
                    tempObj.gameObject = modObj_object; // Store the object in the container
                    tempObj.oldState = new Vector3[] { modObj_ogPosition, modObj_ogRotation, modObj_ogScale }; // Store the object's transform values in the container
                    modObj_objectsToFix.Add(modObj_objectsToFix.Count, tempObj); // Store the container in a dictionary
                }
                modObj_foundObject = false; // Reset foundObject
                currentLine++; // Continue to the next line
            }
        }
    }
    #endregion
    #region ModifyAnimVariable
    private void ModifyAnimVariable(string[] parameters)
    {
        // Required Parameters:
        // param0: character altName
        // param1: variable name
        // param2: variable type
        // param3: variable value
        if (!modAnimVar_objectFound) // If the object has not been found
        {
            try // Attempt to run the below code
            {
                modAnimVar_animator = characterObjectsInConversation[parameters[0]].GetComponent<Animator>(); // Find the animator by the first parameter
                modAnimVar_objectFound = true; // Set objectFound to true
            }
            catch // If the above code fails to run
            {
                Debug.Log("Unable to load " + parameters[0] + "'s animator."); // Inform the Unity console that something has gone wrong
                currentLine++; // Continue to the next line
            }
        }
        else // If the object has been found
        {
            switch (parameters[2].ToLower()) // Compare the third parameter (converted to lowercase) against the below choices
            {
                default: // If the data type is not recognised
                    Debug.Log("Data type '" + parameters[2] + "' not recognised."); // Inform the Unity console that the data type is not recognised
                    break;
                case "float": // If the data type is 'float'
                    try // Attempt to run the below code
                    {
                        float outputFloat = float.Parse(parameters[3]); // Convert the fourth parameter to a float
                        modAnimVar_animator.SetFloat(parameters[1], outputFloat); // Set the float with name equal to the first parameter equal to the fourth parameter as a float
                    }
                    catch // If any of the above code fails to run
                    {
                        Debug.Log("Unable to convert '" + parameters[3] + "' to float."); // Inform the Unity console that something has gone wrong
                    }
                    break;
                case "integer": // If the data type is 'integer'
                    try // Attempt to run the below code
                    {
                        int outputInt = Convert.ToInt32(parameters[3]); // Convert the fourth parameter to an integer
                        modAnimVar_animator.SetInteger(parameters[1], outputInt); // Set the integer with name equal to the first parameter equal to the fourth parameter
                    }
                    catch // If any of the above code fails to run
                    {
                        Debug.Log("Unable to convert '" + parameters[3] + "' to an integer."); // Inform the Unity console that something went wrong
                    }
                    break;
                case "boolean": // If the data type is 'boolean'
                    try // Attempt to run the below code
                    {
                        bool outputBool = Convert.ToBoolean(parameters[3]); // Convert the fourth parameter to a boolean
                        modAnimVar_animator.SetBool(parameters[1], outputBool); // Set the boolean with name equal to the first parameter equal to the fourth parameter
                    }
                    catch // If any of the above code fails to run
                    {
                        Debug.Log("Unable to convert '" + parameters[3] + "' to a boolean."); // Inform the Unity console that something has gone wrong
                    }
                    break;
            }
            currentLine++; // Continue to the next line
        }
    }
    #endregion
    #region AddInputField
    private void AddInputField(string[] parameters) // Function not yet fully implemented
    {
        // Required Parameters
        // param0: object name
        // param1: output variable name
        // param2: sprite
        // param3: position
        // param4: size
        // Optional Parameters
        // param: font = string (file path)
        // param: fontSize = float
        // param: characterLimit = integer
        if (!inpField_created)
        {
            TMP_FontAsset font = null;
            string outputName = parameters[0];
            Sprite sprite = null;
            try
            {
                sprite = loadedSprites[parameters[2]];
            }
            catch
            {
                Debug.Log("Sprite '" + parameters[2] + "' not currently loaded.");
            }
            Vector3 outputPosition = String2Vector(parameters[3]);
            Vector3 outputSize = String2Vector(parameters[4]);
            float fontSize = 30;
            Color fontColour = new Color(0, 0, 0);
            int characterLimit = 0;
            for (int i = 0; i < parameters.Length - 5; i++)
            {
                string[] currentParameter = parameters[i + 5].Split('=');
                switch (currentParameter[0])
                {
                    default:
                        Debug.Log("Parameter '" + currentParameter[0] + "' not recognised.");
                        break;
                    case "font":
                        try
                        {
                            font = loadedFonts[currentParameter[1]];
                        }
                        catch
                        {
                            Debug.Log("Font '" + currentParameter[1] + "' not recognised.");
                        }
                        break;
                    case "fontSize":
                        try
                        {
                            fontSize = float.Parse(currentParameter[1]);
                        }
                        catch
                        {
                            Debug.Log("Unable to convert '" + currentParameter[0] + "' to float.");
                        }
                        break;
                    case "fontColour":
                        ColorUtility.TryParseHtmlString(currentParameter[1], out fontColour); // Attempt to convert the input colour from hex to rgb
                        break;
                    case "characterLimit":
                        try
                        {
                            characterLimit = Convert.ToInt32(currentParameter[1]);
                        }
                        catch
                        {
                            Debug.Log("Unable to convert '" + currentParameter[0] + "' to an integer.");
                        }
                        break;
                }
            }
            // create the text box
            GameObject output = new GameObject();
            GameObject outputText = new GameObject();
            outputText.name = "Text";
            output.transform.parent = HUD.transform.GetChild(1);
            outputText.transform.parent = output.transform;
            TextMeshProUGUI outputTextTextMesh = outputText.AddComponent<TextMeshProUGUI>();
            TMP_InputField outputInputField = output.AddComponent<TMP_InputField>();
            RectTransform outputRectTransform = output.AddComponent<RectTransform>();
            Image outputImage = output.AddComponent<Image>();
            RectTransform outputTextRectTransform = outputText.GetComponent<RectTransform>();

            if (font != null)
            {
                outputTextTextMesh.font = font;
            }
            if (sprite != null)
            {
                outputImage.sprite = sprite;
            }
            output.name = outputName;
            outputInputField.textComponent = outputTextTextMesh;
            outputInputField.characterLimit = characterLimit;
            outputTextTextMesh.fontSize = fontSize;
            outputTextTextMesh.color = fontColour;
            outputInputField.targetGraphic = outputImage;
            outputTextRectTransform.localScale = new Vector3(1, 1, 1);
            outputRectTransform.localScale = new Vector3(1, 1, 1);
            outputTextRectTransform.sizeDelta = outputSize;
            outputRectTransform.sizeDelta = outputSize;
            outputRectTransform.anchoredPosition = new Vector2((outputPosition.x - 960) + outputSize.x / 2, (540 - outputPosition.y) - outputSize.y / 2);
            inpField_created = true;
            outputInputField.onSubmit.AddListener(OnInputSubmit);
            tempParams = parameters;
            inpField_output = output;
        }
        else
        {
            if (inpField_textInput != null)
            {
                conversationStrings.Add(parameters[1], inpField_textInput);
                Destroy(inpField_output);
                inpField_output = null;
                inpField_textInput = null;
                inpField_created = false;
                tempParams = null;
                currentLine++;
                // Known issues
                // - on second conversation with same box, box does not get destroyed on text submission < - fixed this one, but made it harder for external variable storage
                // - no methods can actually access the variables that this method outputs < - maybe some form of function to store variable in JSON as well?
            }
        }
    }
    #endregion
    #region LoadFont
    private void LoadFont(string[] parameters)
    {
        // Required Parameters:
        // param0: FontName
        // param1: FontPath
        switch (fontLoaded)
        {
            case 0:
                AsyncOperationHandle<TMP_FontAsset> fontHandle = Addressables.LoadAssetAsync<TMP_FontAsset>("Assets/Oyster/Fonts/" + parameters[1]);
                fontHandle.Completed += FontHandle_Completed;
                fontLoaded = 1;
                tempParams = parameters;
                break;
            case 2:
                if (loadedFont != null)
                {
                    loadedFonts.Add(parameters[0], loadedFont);
                }
                else
                {
                    Debug.Log("Unable to add font to 'loadedFonts' as 'loadedFont' is null!");
                }
                tempParams = null;
                loadedFont = null;
                fontLoaded = 0;
                currentLine++;
                break;
        }
    }
    #endregion
    #region LoadSprite
    private void LoadSprite(string[] parameters)
    {
        // Required Parameters:
        // param0: SpriteName
        // param1: SpritePath
        switch (spriteLoaded)
        {
            case 0:
                AsyncOperationHandle<Sprite> spriteHandle = Addressables.LoadAssetAsync<Sprite>("Assets/Oyster/Sprites/" + parameters[1]);
                spriteHandle.Completed += SpriteHandle_Completed;
                spriteLoaded = 1;
                tempParams = parameters;
                break;
            case 2:
                if (loadedSprite != null)
                {
                    loadedSprites.Add(parameters[0], loadedSprite);
                }
                else
                {
                    Debug.Log("Unable to add sprite to 'loadedSprites' as 'loadedSprite' is null!");
                }
                tempParams = null;
                loadedSprite = null;
                spriteLoaded = 0;
                currentLine++;
                break;
        }
    }
    #endregion
    #region LegacyPlayAnimation
    private void LegacyPlayAnimation(string[] parameters)
    {
        // Required Parameters:
        // param0: object name
        // param1: animation name
        // Optional Parameters:
        // param: waitOnAnim = bool
        // param: ignoreAnimator = bool
        switch (legacyPlayAnim_animState)
        {
            case 0:
                // no anim playing, first loop
                try
                {
                    legacyPlayAnim_waitOnAnim = false;
                    bool ignoreAnimator = false;
                    // loop through optional parameters
                    for (int i = 0; i < parameters.Length - 2; i++)
                    {
                        string[] currentParameter = parameters[i + 2].Split('=');
                        switch (currentParameter[0])
                        {
                            default:
                                Debug.Log("Parameter '" + currentParameter[0] + "' not recognised.");
                                break;
                            case "waitOnAnim":
                                try
                                {
                                    legacyPlayAnim_waitOnAnim = Convert.ToBoolean(currentParameter[1]);
                                }
                                catch
                                {
                                    Debug.Log("Unable to convert '" + currentParameter[0] + "' to boolean.");
                                }
                                break;
                            case "ignoreAnimator":
                                try
                                {
                                    ignoreAnimator = Convert.ToBoolean(currentParameter[1]);
                                }
                                catch
                                {
                                    Debug.Log("Unable to convert '" + currentParameter[0] + "' to boolean.");
                                }
                                break;
                        }
                    }
                    GameObject target = GameObject.Find(parameters[0]);
                    try // Check for animator, this is expected to fail
                    {
                        if (!ignoreAnimator)
                        {
                            target.GetComponent<Animator>();
                            currentLine++; // If it has animator then pass on running this function
                            Debug.Log("Target object has animator! Set the flag 'ignoreAnimator' to continue animating this object.");
                        }
                    }
                    catch // This is where the main function happens
                    {
                        // If it does not have animator then continue
                    }
                    try
                    {
                        // play animation
                        legacyPlayAnim_targetAnimation = target.GetComponent<Animation>();
                        legacyPlayAnim_targetAnimation.Play(parameters[1]);
                        legacyPlayAnim_animState = 2;
                        Debug.Log("poggers");
                    }
                    catch
                    {
                        // animation does not exist
                        Debug.Log("Object is either missing animation component or animation clip does not exist.");
                        currentLine++;
                    }
                }
                catch
                {
                    Debug.Log("Game object '" + parameters[0] + "' either does not exist in the current scene or lacks an animation component.");
                    currentLine++;
                }
                break;
            case 2:
                // anim playing && not waiting || anim finished playing & waiting
                if (!legacyPlayAnim_waitOnAnim || legacyPlayAnim_waitOnAnim && !legacyPlayAnim_targetAnimation.IsPlaying(parameters[1]))
                {
                    Debug.Log("anim finished");
                    currentLine++;
                }
                break;
        }
    }
    #endregion
    #region PlayAnimation
    private void PlayAnimation(string[] parameters)
    {
        // Required Parameters:
        // param0: object name
        // param1: animation name
        // Optional Parameters:
        // param: layerName = string
        // param: waitOnAnim = boolean
        switch (playAnim_animState)
        {
            case 0: // first loop
                try
                {
                    playAnim_targetAnimator = GameObject.Find(parameters[0]).GetComponent<Animator>();
                    playAnim_animState = 2;
                    playAnim_layerName = "Cutscene";
                    playAnim_waitOnAnim = true;
                    playAnim_animName = parameters[1];
                    for (int i = 0; i < parameters.Length - 2; i++)
                    {
                        string[] currentParameter = parameters[i + 2].Split('=');
                        switch (currentParameter[0])
                        {
                            default:
                                Debug.Log("Parameter '" + currentParameter[0] + "' not recognised.");
                                break;
                            case "waitOnAnim":
                                try
                                {
                                    playAnim_waitOnAnim = Convert.ToBoolean(currentParameter[1]);
                                }
                                catch
                                {
                                    Debug.Log("Unable to convert '" + currentParameter[0] + "' to boolean.");
                                }
                                break;
                            case "layerName":
                                playAnim_layerName = currentParameter[1];
                                break;
                        }
                    }
                    try
                    {
                        playAnim_targetAnimator.Play(playAnim_layerName + "." + parameters[1]);
                        playAnim_animState = 2;
                    }
                    catch
                    {
                        Debug.Log("Animator '" + parameters[0] + "' does not contain animation '" + playAnim_layerName + "." + parameters[1] + "'.");
                        currentLine++;
                    }
                }
                catch
                {
                    Debug.Log("Object '" + parameters[0] + "' does not contain an animator.");
                    currentLine++;
                }
                break;
            case 2: // !animation playing && waiting || !waiting
                bool finished = false;
                Debug.Log(playAnim_targetAnimator.GetCurrentAnimatorStateInfo(playAnim_targetAnimator.GetLayerIndex(playAnim_layerName)).normalizedTime); // 0.99 on line below as normalizedTime never quite reaches 1 on the same frame this checks
                if (playAnim_targetAnimator.GetCurrentAnimatorStateInfo(playAnim_targetAnimator.GetLayerIndex(playAnim_layerName)).normalizedTime >= 0.99) // There's surely a better way to check when an animation has finished?
                {
                    finished = true;
                    Debug.Log("good");
                }
                if (!playAnim_waitOnAnim || playAnim_waitOnAnim && finished)
                {
                    playAnim_targetAnimator = GameObject.Find(parameters[0]).GetComponent<Animator>();
                    playAnim_animState = 0;
                    playAnim_layerName = "Cutscene";
                    playAnim_waitOnAnim = true;
                    playAnim_animName = null;
                    currentLine++;
                }
                break;
        }
    }
    #endregion
    #region SetAnimLayerWeight
    private void SetAnimLayerWeight(string[] parameters)
    {
        // Required Parameters:
        // param0: object name
        // param1: Layer Name
        // param2: Layer Weight
        try
        {
            Animator targetAnimator = GameObject.Find(parameters[0]).GetComponent<Animator>();
            targetAnimator.SetLayerWeight(targetAnimator.GetLayerIndex(parameters[1]), float.Parse(parameters[2]));
        }
        catch
        {
            Debug.Log("Failed to set weight of layer '" + parameters[1] + "' to value '" + parameters[2] + "'.");
        }
        currentLine++;
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
            if (readingValue == 0)
            {
                throw new InvalidDataException();
            }
        }
        catch // If any of the above code fails to run
        {
            Debug.Log("Failed to convert input string to vector! Continuing with (100f, 100f)"); // Inform the Unity console that something wrong happened
            vectorx = 100f; // Set vectorx and vectory to default values
            vectory = 100f;
        }
        return new Vector2(vectorx, vectory); // Return a new vector made of vectorx and vectory
    }
    private Vector3 String3Vector(string input)
    {
        float vectorx = 0;
        float vectory = 0; // Set default values
        float vectorz = 0;
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
                        tempString = tempString.Replace(",", ""); // Set tempString equal to itself minus the comma
                        if (readingValue == 1) // If this is the first value being read
                        {
                            vectorx = float.Parse(tempString); // Set vectorx equal to tempString
                        }
                        else // If this is any other value (a.k.a: if this is the second value)
                        {
                            vectory = float.Parse(tempString); // Set vectory equal to tempString
                        }
                        tempString = ""; // Set tempString back to being empty
                        readingValue++; // Increment which value is currently being read
                        break;
                    case ')':
                        tempString = tempString.Replace(",", ""); // Set tempString equal to itself minus the comma
                        vectorz = float.Parse(tempString); // Set vectorz equal to tempString
                        tempString = ""; // Set tempString back to being empty
                        break;
                }
                tempString += chr; // Add the current character to tempString
            }
            if (readingValue == 0)
            {
                throw new InvalidDataException();
            }
        }
        catch // If any of the above code fails to run
        {
            Debug.Log("Failed to convert input string to vector! Continuing with (100f, 100f, 100f)"); // Inform the Unity console that something wrong happened
            vectorx = 100f; // Set vectorx, vectory and vectorz to default values
            vectory = 100f;
            vectorz = 100f;
        }
        return new Vector3(vectorx, vectory, vectorz); // Return a new vector made of vectorx and vectory
    }
    private void CleanupSpeech() // Destroys all objects and addressables created by a script
    {
        foreach (string str in gameObjectsNames) // Loops through each entry in gameOBjectsNames
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
                Addressables.Release(loadedAddressables[i]);
                Debug.Log("Unloaded addressable successfully!"); // Inform the Unity console that it succeeded
            }
            catch // If it fails to unload an addressable
            {
                Debug.Log("Failed to release addressable!"); // Inform the Unity console that it failed
            }
        }
        for (int i = 0; i < modObj_objectsToFix.Count; i++) // Loop through all objects that need moving back to their original positions
        {
            int c = modObj_objectsToFix.Count - 1 - i; // Calculate an index such that the dictionary is read back to front
            modObj_objectsToFix[c].gameObject.transform.position = modObj_objectsToFix[c].oldState[0];
            modObj_objectsToFix[c].gameObject.transform.eulerAngles = modObj_objectsToFix[c].oldState[1]; // Restore the object's transform values back to the values stored in the dictionary
            modObj_objectsToFix[c].gameObject.transform.localScale = modObj_objectsToFix[c].oldState[2];
        }
        loadedAddressables = new Dictionary<int, AsyncOperationHandle>();
        gameObjectsNames = new List<string>(); // Reset the object and handle trackers to being null for when another script is ran
        speechCooldown = speechCooldownTime; // Set the speechCooldown to the provided time, so that a speech cannot be instantly started after this one. Useful for if the previous speech finished with a click event
        charactersInConversation = new Dictionary<string, CharacterData>();
        characterObjectsInConversation = new Dictionary<string, GameObject>(); // Reset some Dictionaries to be empty again
        modObj_objectsToFix = new Dictionary<int, ObjectAndPosition>();
        loadedFonts = new Dictionary<string, TMP_FontAsset>();
        loadedSprites = new Dictionary<string, Sprite>();
        conversationStrings = new Dictionary<string, string>();
    }
    #endregion
}