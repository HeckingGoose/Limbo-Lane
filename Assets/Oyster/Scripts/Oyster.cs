using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine; // Link required assemblies
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
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
    // These variables need to be cleaned up a bit, maybe make classes to store the variables for each function
    #region Generic
    private bool versionDataLoaded = false;
    [SerializeField] // SerializeField allows the variable to be viewable within the Unity editor without exposing it to every other script
    private float speechCooldownTime = 2;
    [SerializeField]
    private SceneStateLoader sceneStateLoader;
    private int maxLinesPerFrame; // variable intended to limit how many lines Oyster can process per frame - unlike in version 2 where only 1 line was processed per frame
    private int characterIndex;
    private string conversationName; // Setup variables required for conversation stuffs
    private int currentLine;
    private TextAsset conversationData;
    private OysterConversationsContainer conversations;
    private OysterConversation currentConversation;
    [HideInInspector]
    public bool inConversation = false; // true when a conversation is in progress
    private string scriptVersion = "?"; // default value so that if a check is made against this variables before Oyster is loaded, the script does not crash
    private string[] tempParams;
    private bool mouseDown;
    private float speechCooldown = 0;
    private float failedReadAttempts = 0;
    private Dictionary<string, GameObject> createdObjects = new Dictionary<string, GameObject>();
    private CharacterDataContainer characters;
    private bool charactersLoaded = false;
    private Dictionary<string, CharacterData> charactersInConversation = new Dictionary<string, CharacterData>();
    private Dictionary<string, GameObject> characterObjectsInConversation = new Dictionary<string, GameObject>();
    private Dictionary<string, string> conversationStrings = new Dictionary<string, string>();
    private bool waiting = false;
    private bool mouseClicked = false;
    private bool mouseHasClicked = false;
    private float skipSpeed;
    private float currentSkipTime;
    #endregion
    #region WaitForInput
    private float waitingTime = 0;
    private float maxWaitTime = 1;
    private bool autoSkip = true;
    #endregion
    #region AddSmoothText
    private float charactersPerSecond = 2f;
    private float smoothTextWaitTime = 0;
    private int currentCharIndex = 0; // Variables for the AddSmoothText function
    #endregion
    #region ModifyObject
    private GameObject modObj_object;
    private bool modObj_foundObject = false;
    private bool modObj_moving;
    private bool modObj_rotating;
    private bool modObj_scaling; // Variables for the ManipulateObject function
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
    private string inpField_textInput = null; // There are a lot of variables here, I should really consider trying to reduce the amount of variables
    private GameObject inpField_output;
    #endregion
    #region ModCamFOV
    private int modFOV_state;
    private float modFOV_time;
    private Camera modFOV_camera;
    private string modFOV_interpolation;
    private float modFOV_targetFOV;
    private float modFOV_ogFOV;
    private float modFOV_currentTime;
    private bool modFOV_permanent;
    private bool modFOV_clickToSkip;
    private Dictionary<int, CameraAndFOV> modFOV_camerasToFix = new Dictionary<int, CameraAndFOV>();
    #endregion
    #region ManipulateImage
    private Image manipImg_image;
    private float manipImg_time;
    private float manipImg_currentTime;
    private string manipImg_interpolation;
    private float manipImg_transparency;
    private bool manipImg_modTransparency;
    private float manipImg_ogTransparency;
    private int manipImg_state = 0;
    #endregion
    #region LineMarkers
    [SerializeField]
    private Dictionary<string, int> lineMarkers = new Dictionary<string, int>();
    #endregion
    #region ModShapeKey
    private int msk_state = 0;
    private int msk_shapeKeyId;
    private Mesh msk_mesh;
    private SkinnedMeshRenderer msk_skinnedMesh;
    private string msk_interpolation;
    private float msk_maxTime;
    private float msk_currentTime;
    private float msk_ogShapeKeyValue;
    private float msk_targetShapeKeyValue;
    private bool msk_clickToSkip = false;
    private bool msk_writing = false;
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
    #region Other
    private float maxTimeUntilNextFrame = 0.2f;
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
        if (inConversation || waiting) // If a conversation is currently taking place
        {
            Speak(characterIndex, conversationName, currentLine); // Then push the script back into the conversation
        }
        #endregion
        #region Handle whether mouse is being held or ¬
        if (Input.GetAxis("PrimaryAction") > 0 && mouseHasClicked) // If the mouse is being held
        {
            mouseDown = true; // Then tell the script that it is currently being held, rather than has just been clicked
        }
        else // If the mouse is not being held
        {
            mouseDown = false; // Then tell the script that the mouse is no longer being held
        }
        if (Input.GetAxis("PrimaryAction") > 0 || Input.GetAxis("SkipText") > 0)
        {
            if (currentSkipTime > skipSpeed)
            {
                mouseClicked = true;
                currentSkipTime = 0;
            }
            else
            {
                mouseClicked = false;
                currentSkipTime += Time.deltaTime;
            }
            mouseHasClicked = true;
        }
        else
        {
            mouseClicked = false;
            mouseHasClicked = false;
            currentSkipTime = skipSpeed + 1;
        }
        #endregion
        #region Handle speech cooldown
        if (speechCooldown > 0) // If the cooldown between conversations has not yet reached 0 seconds
        {
            speechCooldown -= Time.deltaTime; // Subtract the time taken to render this frame from the cooldown timer
        }
        #endregion
    }
    private void LateUpdate() // Called before animators are applied but after they are calculated
    {
        #region ModShapeKey
        if (msk_writing) // If the shape key needs changing
        {
            switch (msk_interpolation) // Pick which interpolation to use
            {
                default: // No interpolation
                    msk_skinnedMesh.SetBlendShapeWeight(msk_shapeKeyId, msk_targetShapeKeyValue); // Set the shape key to its target weight
                    msk_writing = false; // Set writing to false
                    break;
                case "linear": // Linear interpolation
                    msk_currentTime += Time.deltaTime; // Add deltaTime to currentTime
                    if (msk_currentTime > msk_maxTime) // If currentTime is more than maxTime
                    {
                        msk_interpolation = "none"; // Set the interpolation to none
                    }
                    else // Otherwise
                    {
                        // Linearly interpolate the weight of the shape key
                        msk_skinnedMesh.SetBlendShapeWeight(msk_shapeKeyId, Mathf.Lerp(msk_ogShapeKeyValue, msk_targetShapeKeyValue, msk_currentTime / msk_maxTime));
                    }
                    break;
            }
        }
        #endregion
    }
    public void Speak(int _characterIndex, string _conversationName, int _currentLine) // The main method, this exists to interpret Oyster script files and pass commands and parameters to the correct methods
    {
        #region Pre-conversation setup
        if (!inConversation)
        {
            waiting = true;
            characterIndex = _characterIndex;
            conversationName = _conversationName; // Store internal method variables in a bit more of a global scope for later re-entry
            currentLine = _currentLine;
        }
        if (!inConversation && versionDataLoaded) // If this is a new conversation
        {
            if (speechCooldown <= 0)
            {
                waiting = false;
                if (PersistentVariables.skipSpeed != -1)
                {
                    skipSpeed = PersistentVariables.skipSpeed;
                }
                else
                {
                    Debug.Log("Unable to load skip speed, defaulting to 0.1");
                    skipSpeed = 0.1f;
                }
                if (PersistentVariables.charactersPerSecond != 0)
                {
                    charactersPerSecond = PersistentVariables.charactersPerSecond;
                }
                else
                {
                    Debug.Log("Characters per second not set, defaulting to 10!");
                    charactersPerSecond = 10;
                }
                if (PersistentVariables.linesPerFrame != -1)
                {
                    maxLinesPerFrame = PersistentVariables.linesPerFrame;
                }
                else
                {
                    Debug.Log("Lines per frame could not be loaded, defaulting to 20!");
                    maxLinesPerFrame = 20;
                }
                inConversation = true; // Set inConversation to true so that on next entry the method knows a conversation is underway
                AsyncOperationHandle<TextAsset> conversationDataHandle = Addressables.LoadAssetAsync<TextAsset>("Assets/Oyster/JSON/Conversations-" + language + "/Character" + _characterIndex.ToString() + "-Conversations.json"); // Begin loading the required conversation, with multiple language support. Nifty
                conversationDataHandle.Completed += ConversationHandle_Completed; // Tell the asset loader to start the listed method when in finishes
            }
        }
        #endregion
        #region Conversation
        else // If this is a re-entry from a currently running conversation
        {
            bool lineAlreadyProcessed = false; // Reset the flag stating whether a line has already been processed or not. This flag is set for time sensitive commands that occur across multiple loops but are required to be ran once a frame so that 1 deltaTime = 1 frame, such as a wait command
            //for (int i = 0; i < maxLinesPerFrame; i++) // Loop until the max amount of lines per frame has been reached
            int i = 0;
            float currentWaitTime = 0;
            while (i < maxLinesPerFrame && maxTimeUntilNextFrame > currentWaitTime)
            {
                i++;
                currentWaitTime += Time.deltaTime;
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
                            if (!lineAlreadyProcessed)
                            {
                                AddSmoothText(currentLineParameters);
                                lineAlreadyProcessed = true;
                            }
                            break;
                        case "ManipulateObject": // Calls the ManipulateObject method when the command ManipulateObject is read
                            if (!lineAlreadyProcessed)
                            {
                                ManipulateObject(currentLineParameters);
                                lineAlreadyProcessed = true;
                            }
                            break;
                        case "ModifyAnimVariable": // Calls the ModifyAnimVariable method when the command ModifyAnimVariable is read
                            ModifyAnimVariable(currentLineParameters);
                            break;
                        case "AddInputField": // Calls the AddInputField method when the command AddInputField is read
                            AddInputField(currentLineParameters);
                            break;
                        case "LoadFont": // Calls the LoadFont method when the command LoadFont is read
                            LoadFont(currentLineParameters);
                            break;
                        case "LoadSprite": // Calls the LoadSprite method when the command LoadSprite is read
                            LoadSprite(currentLineParameters);
                            break;
                        case "LegacyPlayAnimation": // Calls the LegacyPlayAnimation method when the command LegacyPlayAnimation is read
                            LegacyPlayAnimation(currentLineParameters);
                            break;
                        case "PlayAnimation": // Calls the PlayAnimation method when the command PlayAnimation is read
                            PlayAnimation(currentLineParameters);
                            break;
                        case "SetAnimLayerWeight": // Calls the SetAnimLayerWeight method when the command SetAnimLayerWeight is read
                            SetAnimLayerWeight(currentLineParameters);
                            break;
                        case "NewLineMarker": // Skips to the next line when the command NewLineMarker is read
                            currentLine++;
                            break;
                        case "JumpTo": // Calls the JumpTo method when the command JumpTo is read
                            JumpTo(currentLineParameters);
                            break;
                        case "Decision": // Calls the Decision method when the command Decision is read
                            Decision(currentLineParameters);
                            break;
                        case "EndConversation": // Calls the EndConversation method when the command EndConversation is read
                            EndConversation(currentLineParameters);
                            break;
                        case "DelObject": // Calls the DelObject method when the command DelObject is read
                            DelObject(currentLineParameters);
                            break;
                        case "ModifySprite": // Calls the ModifySprite method when the command ModifySprite is read
                            ModifySprite(currentLineParameters);
                            break;
                        case "ModCamFOV": // Calls the ModCamFOV method when the command ModCamFOV is read
                            if (!lineAlreadyProcessed) // Limits this command to running once per frame as it relies on deltatime
                            {
                                lineAlreadyProcessed = true;
                                ModCamFOV(currentLineParameters);
                            }
                            break;
                        case "ManipulateImage": // Calls the ManipulateImage method when the command ManipulateImage is read
                            if (!lineAlreadyProcessed) // Limits this command to running once per frame as it relies on deltatime
                            {
                                lineAlreadyProcessed = true;
                                ManipulateImage(currentLineParameters);
                            }
                            break;
                        case "LoadScene": // Calls the LoadScene method when the command LoadScene is read
                            LoadScene(currentLineParameters);
                            break;
                        case "ModShapeKey":
                            if (!lineAlreadyProcessed)
                            {
                                lineAlreadyProcessed = true;
                                ModShapeKey(currentLineParameters);
                            }
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
    private void OnInputSubmit(string input) // This method is run when a text entry into an input field is returned
    {
        inpField_textInput = input; // Stores the text entry as inpField_textInput
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
        Addressables.Release(handle); // Unload the loaded addressable from memory
        charactersLoaded = true; // Set charactersLoaded to true
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
        fontLoaded = 2; // Set fontLoaded to 2
        LoadFont(tempParams); // Call the method that originally called it
    }
    #endregion
    #region Loading sprite data
    private void SpriteHandle_Completed(AsyncOperationHandle<Sprite> handle) // This method runs once addressables has finished loading a sprite
    {
        if (handle.Status == AsyncOperationStatus.Succeeded) // If the addressable loaded successfully
        {
            loadedSprite = handle.Result; // Set loaded sprite equal to the loaded addressable
            loadedAddressables.Add(loadedAddressables.Count, handle); // Add the addressable to the dictionary of currently loaded addressables
        }
        else // If the addressable failed to load
        {
            Debug.Log("Sprite failed to load! Returning null..."); // Inform the Unity console that the addressable failed to load
            loadedSprite = null; // Set loaded sprite to null
            Addressables.Release(handle); // Release the addressable
        }
        spriteLoaded = 2; // Set sprite loaded to 2
        LoadSprite(tempParams); // Call the method that originally called it
    }
    #endregion
    #region Loading conversation data
    private void ConversationHandle_Completed(AsyncOperationHandle<TextAsset> handle) // Method that is called once Character#-Conversations.json has been loaded
    {
        if (handle.Status == AsyncOperationStatus.Succeeded) // If the asset was loaded successfully
        {
            conversationData = handle.Result; // Stores the result of handle in conversationData
            conversations = JsonUtility.FromJson<OysterConversationsContainer>(conversationData.text); // Convert the loaded asset into a usable class
            //currentConversation = conversations.container[conversationIndex]; // Set currentconversation equal to the conversation currently scored in conversations that matches the index conversationIndex
            currentConversation = null;
            for (int i = 0; i < conversations.container.Length; i++) // Loops through every conversation in the loaded file
            {
                if (conversationName == conversations.container[i].title) // If the conversation name matches that of the requested conversation
                {
                    currentConversation = conversations.container[i]; // Set the current conversation equal to that conversation
                }
            }
            if (currentConversation == null) // If no conversation was loaded
            {
                Debug.Log("Unable to find conversation."); // Inform the Unity console that no conversation could be loaded
                inConversation = false; // Leave the conversation
            }
            else // If a conversation was loaded
            {
                SetupLineMarkers(currentConversation); // Call a method to setup line markers
                if (currentConversation.scriptVersion != scriptVersion) // If script versions do not match
                {
                    Debug.Log("Script versions do not match! Some commands in this script may not be recognised."); // Tell the Unity console that issues may occur as the script versions do not match
                }
                else // If script versions match
                {
                    Debug.Log("Conversation " + currentConversation.title + " loaded without any errors."); // Tell the Unity console to output that the conversation loaded with no errors
                }
                Addressables.Release(handle); // Removes the asset Character#-Conversations.json from memory
                Speak(characterIndex, conversationName, currentLine); // Calls the speak method again, so that the method now continues since the required data has been loaded
            }
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
        versionDataLoaded = true;
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
        // param: size = vector2
        // param: colour = HTML colour
        // param: sort order = string
        // param: alpha = float
        try
        {
            Sprite sprite = null;
            if (parameters[1] != "null")
            {
                sprite = loadedSprites[parameters[1]]; // Set the variable sprite equal to a loadedSprite
            }
            loadedSprite = null; // Set loadedSprite to null for when this method is called again
            string name = parameters[0]; // Set the variable name equal to the first parameter
            Vector2 position = String2Vector(parameters[2]); // Translate the fourth parameter into the sprite's on-screen position
            Vector2 size = new Vector2(100, 100); // Set the variable size equal to a default value
            Color colour = new Color(255,255,255,255);
            float alpha = 255;
            string sort = "main";
            string anchor = "centre";
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
            if (parameters.Length > 3)
            {
                string[] currentParameter;
                for (int i = 0; i < parameters.Length - 3; i++) // Loop through all optional parameters
                {
                    currentParameter = parameters[3 + i].Split('='); // split the optional parameter into a name and a data value
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
                        case "size": // If the parameter is 'size'
                            size = String2Vector(currentParameter[1]); // Set size equal to the current parameter converted to a vector2
                            break;
                        case "anchor": // If the parameter is 'anchor'
                            anchor = currentParameter[1]; // Set the anchor equal to the current parameter
                            break;
                        case "colour": // If the parameter is 'colour'
                            ColorUtility.TryParseHtmlString(currentParameter[1], out colour); // Set the colour equal to the current parameter as a colour
                            break;
                        case "sort": // If the parameter is 'sort'
                            sort = currentParameter[1]; // Set sort equal to the current parameter
                            break;
                        case "alpha": // If the parameter is 'alpha'
                            try // Try to run the below code
                            {
                                alpha = float.Parse(currentParameter[1]); // Convert the current parameter to float and store it as alpha
                                alpha = Mathf.Clamp(alpha, 0, 1); // Clamp alpha beween 0 and 1
                            }
                            catch // If the above code failed to run
                            {
                                Debug.Log("Unable to convert '" + currentParameter[1] + "'"); // Inform the Unity console that something went wrong
                            }
                            break;

                    }
                }
            }
            if (anchor == "topLeft") // If the anchor is topleft
            {
                position = new Vector2((position.x - 960) + size.x / 2, (540 - position.y) - size.y / 2); // Since sprites and the canvas have (0,0) to be their centre, do a little maths to figure out where the draw position should be for (0,0) to be the top left
            }
            GameObject output = new GameObject(); // Create a new gameobject
            output.name = name; // Set it's name equal to the name given in the method's parameters
            switch (sort.ToLower())
            {
                default:
                    output.transform.parent = HUD.transform.GetChild(0); // Make the object a child of the HUD's child object 'Sprites'
                    break;
                case "overlay":
                    output.transform.parent = HUD.transform.GetChild(2); // Make the object a child of the HUD's child object 'Overlay'
                    break;
            }
            RectTransform outputRectTransform = output.AddComponent<RectTransform>(); // Add a RectTransform to the object
            outputRectTransform.localScale = new Vector3(1, 1, 1); // Set it's scale to 1, since when making the object it's scale sometimes becomes more than 1 for some unknown reason
            outputRectTransform.anchoredPosition = position; // Move the object to it's correct position
            outputRectTransform.sizeDelta = size; // Scale the object's dimensions to fit the sprite
            output.AddComponent<CanvasRenderer>(); // Add a CanvasRenderer component so that the object is rendered to the HUD
            Image outputImage = output.AddComponent<Image>(); // Add an Image component to the object
            colour = new Color(colour.r, colour.g, colour.b, alpha); // Incorporate the specified alpha into the input colour
            outputImage.color = colour; // Set the output colour to the colour calculated above
            outputImage.sprite = sprite; // Set the sprite value of the Image component equal to the loaded sprite
            createdObjects.Add(name, output); // Add this object to the list of loaded objects
        }
        catch // If something goes wrong
        {
            Debug.Log("Sprite '" + parameters[1] + "' not currently loaded."); // Inform the Unity console that that sprite is not currently loaded
        }
        currentLine++; // Continue to the next line
    }
    #endregion
    #region DelObject
    private void DelObject(string[] parameters)
    {
        // Required Parameters:
        // param0: object name
        // Optional Parameters:
        // none
        try // Try to run the below code
        {
            GameObject.Destroy(createdObjects[parameters[0]]); // Delete the specified object
            createdObjects.Remove(parameters[0]); // Remove that object from the list of created objects
        }
        catch // If any of the above code fails to run
        {
            Debug.Log("Failed to delete '" + parameters[0] + "'."); // Inform the Unity console that something went wrong
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
        // param: colourFromCharacter = string

        TMP_FontAsset font = null;
        string text = "";
        int fontSize = 56; // Setup default values
        Color fontColour = new Color(0, 0, 0);
        string anchor = "centre";
        string alignment = "topLeft";
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
                        try // Try to run the below code
                        {
                            font = loadedFonts[currentParameter[1]]; // Set font equal to the specified font in the list of loaded fonts
                        }
                        catch // If the above code fails to run
                        {
                            Debug.Log("Font '" + currentParameter[1] + "' not currently loaded."); // Inform the Unity console that the font is not loaded
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
                    case "anchor": // If the current parameter is 'anchor'
                        anchor = currentParameter[1]; // Set anchor equal to the current  parameter
                        break;
                    case "colourFromCharacter": // If the current parameter is 'colourFromCharacter'
                        ColorUtility.TryParseHtmlString(charactersInConversation[currentParameter[1]].colour, out fontColour); // Set colour equal to the specified colour from the list of loaded characters
                        break;
                    case "alignment":
                        alignment = currentParameter[1];
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
        if (anchor == "topLeft") // If anchor is set to topLeft
        {
            position = new Vector2((position.x - 960) + size.x / 2, (540 - position.y) - size.y / 2); // Set the object's position equal to position + some maths to figure out where the top left of the canvas and the object is
        }
        outputRectTransform.anchoredPosition = position; // Set the object's positions
        createdObjects.Add(parameters[0], output); // Add the object to the list of currently created object
        if (font != null) // If a font has been loaded
        {
            outputTextMesh.font = font; // Set the font equal to font
        }
        outputTextMesh.text = text;
        outputTextMesh.fontSize = fontSize; // Set fontsize, text and colour
        outputTextMesh.color = fontColour;
        switch (alignment)
        {
            default:
                Debug.Log("Alignment not recognised, defaulting to left.");
                outputTextMesh.alignment = TextAlignmentOptions.Left;
                break;
            case "left":
                outputTextMesh.alignment = TextAlignmentOptions.Left;
                break;
            case "centre":
                outputTextMesh.alignment = TextAlignmentOptions.Center;
                break;
            case "center":
                outputTextMesh.alignment = TextAlignmentOptions.Center;
                break;
            case "topLeft":
                outputTextMesh.alignment = TextAlignmentOptions.TopLeft;
                break;
        }
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
        if (mouseClicked && !mouseDown) // If the mouse is clicked and not held
        {
            waitingTime = 0;
            maxWaitTime = 1; // Set variables back to their default values
            autoSkip = true;
            mouseDown = true;
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
        // param: colourFromCharacter = string
        try // Attempt to run the below code
        {
            GameObject output = GameObject.Find(parameters[0]); // Find a gameobject by the first input parameter
            RectTransform outputRectTransform = output.GetComponent<RectTransform>(); // Find the object's RectTransform component
            TextMeshProUGUI outputTextMesh = output.GetComponent<TextMeshProUGUI>(); // Find the object's TextMesh component
            Color fontColour = new Color(255, 255, 255);
            string anchor = "centre";
            bool modifyPosition = false;
            string alignment = "topLeft";
            for (int i = 0; i < parameters.Length - 1; i++) // Loop through all parameters given except for the first parameter
            {
                string[] currentParameter = parameters[i + 1].Split('='); // Split the current parameter into a name and a value
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
                        modifyPosition = true; // Find the position of the object using the current parameter value and String2Vector
                        outputRectTransform.anchoredPosition = String2Vector(currentParameter[1]);
                        break;
                    case "colour": // If the parameter is 'colour'
                        fontColour = new Color(); // Create a new colour object
                        ColorUtility.TryParseHtmlString(parameters[i], out fontColour); // Attempt to parse the current parameter value as a hex colour
                        outputTextMesh.color = fontColour; // Set the object's TextMesh's colour to the found colour
                        break;
                    case "colourFromCharacter":
                        fontColour = new Color();
                        ColorUtility.TryParseHtmlString(charactersInConversation[currentParameter[1]].colour, out fontColour);
                        outputTextMesh.color = fontColour;
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
                    case "font": // If the parameter is 'font'
                        try // Attempt to run the below code
                        {
                            outputTextMesh.font = loadedFonts[currentParameter[1]]; // Try to set the font to a currently loaded font
                        }
                        catch // If the above code fails to run
                        {
                            Debug.Log("Font '" + currentParameter[1] + "' not currently loaded."); // Inform the Unity console that the above code failed to run
                        }
                        break;
                    case "anchor": // If the current paramter is 'anchor'
                        anchor = currentParameter[1]; // Set anchor equal to the current parameter
                        break;
                    case "alignment":
                        alignment = currentParameter[1];
                        break;
                }
            }
            if (modifyPosition) // If modifyPosition is set to true
            {
                switch (anchor) // Compare the value of anchor against the below cases
                {
                    case "topLeft": // If anchor is topLeft
                        Vector2 position = outputRectTransform.anchoredPosition;
                        Vector2 size = outputRectTransform.sizeDelta; // Set the values of size and position
                        outputRectTransform.anchoredPosition = new Vector2((position.x - 960) + size.x / 2, (540 - position.y) - size.y / 2); // Do some funky maths to place the object at its intended position from its top left corner, not the centre
                        break;
                }
            }
            switch (alignment)
            {
                default:
                    Debug.Log("Alignment not recognised, defaulting to topLeft.");
                    outputTextMesh.alignment = TextAlignmentOptions.TopLeft;
                    break;
                case "left":
                    outputTextMesh.alignment = TextAlignmentOptions.Left;
                    break;
                case "centre":
                    outputTextMesh.alignment = TextAlignmentOptions.Center;
                    break;
                case "center":
                    outputTextMesh.alignment = TextAlignmentOptions.Center;
                    break;
                case "topLeft":
                    outputTextMesh.alignment = TextAlignmentOptions.TopLeft;
                    break;
            }
        }
        catch // If any of the above code fails to run
        {
            Debug.Log("GameObject '" + parameters[0] + "' does not exist in the current scene."); // Inform the Unity console that something has gone wrong
        }
        currentLine++; // Continue to the next line
    }
    #endregion
    #region ModifySprite
    private void ModifySprite(string[] parameters)
    {
        // Required Parameters:
        // param0: object name
        // Optional parameters:
        // param: sprite = string
        // param: position = vector2
        // param: anchor = string
        // param: size = vector2
        // param: colour = HTML colour
        // param: sort = string
        try // Try to run the below code
        {
            GameObject output = GameObject.Find(parameters[0]); // Find the sprite in the current scene
            RectTransform outputRectTransform = output.GetComponent<RectTransform>(); // Add components to the sprite and cache them
            UnityEngine.UI.Image outputImage = output.GetComponent<UnityEngine.UI.Image>();
            string anchor = "centre";
            bool modifyPosition = false; // Set default values
            if (parameters.Length - 1 > 0) // If there are optional parameters
            {
                for (int i = 0; i < parameters.Length - 1; i++) // Loop through the optional parameters
                {
                    string[] currentParameter = parameters[i + 1].Split('='); // Split the current paramter into a name and value
                    switch (currentParameter[0]) // Compare the current parameter's name against the below cases
                    {
                        default: // If the current parameter's name does not match any of the below cases
                            Debug.Log("Parameter '" + currentParameter[0] + "' not recognised."); // Inform the Unity console that the parameter was not recognised
                            break;
                        case "sprite": // If the current parameter is 'sprite'
                            try // Try to run the below code
                            {
                                outputImage.sprite = loadedSprites[currentParameter[1]]; // Set sprite equal to the sprite specified from loaded sprites
                            }
                            catch // If the above code fails to run
                            {
                                Debug.Log("Sprite '" + currentParameter[1] + "' is not currently loaded."); // Inform the Unity console that the sprite is not loaded
                            }
                            break;
                        case "position": // If the current parameter is 'position'
                            outputRectTransform.anchoredPosition = String2Vector(currentParameter[1]); // Set position equal to the current parameter converted to a vector2
                            modifyPosition = true; // Set modifyPosition to true
                            break;
                        case "anchor": // If the current parameter is 'anchor'
                            anchor = currentParameter[1]; // Set anchor equal to the current parameter
                            break;
                        case "size": // If the current parameter is 'size'
                            outputRectTransform.sizeDelta = String2Vector(currentParameter[1]); // Set size equal to the current parameter converted to a vector2
                            break;
                        case "colour": // If the current parameter is 'colour'
                            Color colour = new Color(); // Create a new colour named colour
                            ColorUtility.TryParseHtmlString(currentParameter[1], out colour); // Convert the current parameter to a colour and store it in colour
                            if (colour != null) // If colour is not null
                            {
                                outputImage.color = colour; // Set the colour of the output image to the value of colour
                            }
                            break;
                        case "sort": // If the current parameter is 'sort'
                            switch (currentParameter[1].ToLower()) // Compare the value of the current parameter against the below cases
                            {
                                default: // If current parameter matches none of the below cases
                                    output.transform.parent = HUD.transform.GetChild(0); // Make the object a child of the HUD's child object 'Sprites'
                                    break;
                                case "overlay": // If the current parameter is 'overlay'
                                    output.transform.parent = HUD.transform.GetChild(2); // Make the object a child of the HUD's child object 'Overlay'
                                    break;
                            }
                            break;
                    }
                }
                if (modifyPosition) // If modifyPosition is true
                {
                    switch (anchor) // Compare anchor against the below cases
                    {
                        case "topLeft": // If anchor is 'topLeft'
                            Vector2 position = outputRectTransform.anchoredPosition;
                            Vector2 size = outputRectTransform.sizeDelta; // Set size and position
                            outputRectTransform.anchoredPosition = new Vector2((position.x - 960) + size.x / 2, (540 - position.y) - size.y / 2); // Do some funky maths to place the object at its intended position from its top left corner, not the centre
                            break;
                    }
                }
            }
        }
        catch // If the above code fails to run
        {
            Debug.Log("Image '" + parameters[0] + "' does not exist in this scene."); // Inform Unity that the sprite does not exist in this scene
        }
        currentLine++; // Continue to the next line
    }
    #endregion
    #region AddSmoothText
    private void AddSmoothText(string[] parameters)
    {
        // Required Parameters:
        // param0: text
        // param1: object to add text to
        // Optional Parameters:
        // param: clickToSkip = bool
        smoothTextWaitTime += Time.deltaTime * charactersPerSecond; // Add the time between the last frame and this frame to the current wait time
        if (smoothTextWaitTime >= 1) // If the wait time multiplied by the total characters per second exceeds 1
        {
            int charactersToAdd = Convert.ToInt32(smoothTextWaitTime); // Truncate the current wait time to find how many characters need to be added - this will almost always return one, however it is here to handle sudden frame dips where delta time may be more than 1
            smoothTextWaitTime = smoothTextWaitTime % 1; // Remove any value on the left of the decimal point
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
                    charactersToAdd = 0;
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
        if (clickToSkip && !mouseDown && mouseClicked) // If clickToSkip is true, the mouse is not currently held and the mouse has been clicked
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
            if (modObj_clickToSkip && mouseClicked && !mouseDown) // If clickToSkip is true and the mouse is being clicked and the mouse is not being held
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
    private void AddInputField(string[] parameters)
    {
        // Required Parameters:
        // param0: object name
        // param1: output variable name
        // param2: sprite
        // param3: position
        // param4: size
        // Optional Parameters:
        // param: font = string (file path)
        // param: fontSize = float
        // param: characterLimit = integer
        if (!inpField_created) // If the input field has not been created yet
        {
            TMP_FontAsset font = null;
            string outputName = parameters[0]; // Set default values
            Sprite sprite = null;
            try // Try to run the below code
            {
                sprite = loadedSprites[parameters[2]]; // Set the sprite equal to one of the loaded sprites
            }
            catch // If the above code fails to run
            {
                Debug.Log("Sprite '" + parameters[2] + "' not currently loaded."); // Inform the Unity console that the above code failed to run
            }
            Vector3 outputPosition = String2Vector(parameters[3]);
            Vector3 outputSize = String2Vector(parameters[4]);
            float fontSize = 30; // Set more default values
            Color fontColour = new Color(0, 0, 0);
            int characterLimit = 0;
            for (int i = 0; i < parameters.Length - 5; i++) // Loop through optional parameters
            {
                string[] currentParameter = parameters[i + 5].Split('='); // Set currentParameter equal to the name and value of the current parameter
                switch (currentParameter[0]) // choose a case based on the name of the current parameter
                {
                    default: // If the name is not recognised
                        Debug.Log("Parameter '" + currentParameter[0] + "' not recognised."); // Inform the Unity console that the parameter was not recognised
                        break;
                    case "font": // If the parameter is font
                        try // Try to run the below code
                        {
                            font = loadedFonts[currentParameter[1]]; // Set font equal to a currently loaded font
                        }
                        catch // If the above code fails to run
                        {
                            Debug.Log("Font '" + currentParameter[1] + "' not recognised."); // Inform the Unity console that the above code failed to run
                        }
                        break;
                    case "fontSize": // If the parameter is fontSize
                        try // Try to run the below code
                        {
                            fontSize = float.Parse(currentParameter[1]); // Set fontsize equal to the current parameter's value
                        }
                        catch // If the above code fails to run
                        {
                            Debug.Log("Unable to convert '" + currentParameter[0] + "' to float."); // Inform the Unity console that the above code failed to run
                        }
                        break;
                    case "fontColour": // If the parameter is fontColour
                        ColorUtility.TryParseHtmlString(currentParameter[1], out fontColour); // Attempt to convert the input colour from hex to rgb
                        break;
                    case "characterLimit": // If the parameter is characterLimit
                        try // Try to run the below code
                        {
                            characterLimit = Convert.ToInt32(currentParameter[1]); // Set the character limit equal to the current parameter's value
                        }
                        catch // If the above code fails to run
                        {
                            Debug.Log("Unable to convert '" + currentParameter[0] + "' to an integer."); // Inform the Unity console that the above code failed to run
                        }
                        break;
                }
            }
            // create the text box
            GameObject output = new GameObject();
            GameObject outputText = new GameObject(); // Create an input field and load all of the previously declared values
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
            outputRectTransform.sizeDelta = outputSize; // There really are a lot of values to load for an input field
            outputRectTransform.anchoredPosition = new Vector2((outputPosition.x - 960) + outputSize.x / 2, (540 - outputPosition.y) - outputSize.y / 2);
            inpField_created = true;
            outputInputField.onSubmit.AddListener(OnInputSubmit); // Add a listener to the input field so that when text is submitted, the script knows what the submitted text is
            tempParams = parameters;
            inpField_output = output;
        }
        else // Once the input field has been created
        {
            if (inpField_textInput != null) // Wait for some text to be entered
            {
                conversationStrings.Add(parameters[1], inpField_textInput); // Store the input text as a conversation variable
                Destroy(inpField_output); // Destroy the input field
                inpField_output = null;
                inpField_textInput = null; // Reset variables to their default values
                inpField_created = false;
                tempParams = null;
                currentLine++; // Continue to the next line
                // Known issues
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
        switch (fontLoaded) // Picks a case depending on what stage of loading the font is in: 0-notloaded, 1-loading, 2-loaded
        {
            case 0: // If the font has not started loading
                AsyncOperationHandle<TMP_FontAsset> fontHandle = Addressables.LoadAssetAsync<TMP_FontAsset>("Assets/Oyster/Fonts/" + parameters[1]);
                fontHandle.Completed += FontHandle_Completed; // Tell addressables to start loading the font and call the listed method when done
                fontLoaded = 1; // Set font loaded to loading
                tempParams = parameters; // Store this method's parameters externally
                break;
            case 2: // If the font has finished loading
                if (loadedFont != null) // If the font loaded successfully
                {
                    loadedFonts.Add(parameters[0], loadedFont); // Add a reference to the font in loaded fonts
                }
                else // If the font failed to load
                {
                    Debug.Log("Unable to add font to 'loadedFonts' as 'loadedFont' is null!"); // Inform the Unity console that the font failed to load
                }
                tempParams = null;
                loadedFont = null; // Reset variables to their default values
                fontLoaded = 0;
                currentLine++; // Continue to the next line
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
        switch (spriteLoaded) // Pick a case depending on the current state of sprite loading: 0-notloaded, 1-loading, 2-loaded
        {
            case 0: // If the sprite has not been loaded yet
                AsyncOperationHandle<Sprite> spriteHandle = Addressables.LoadAssetAsync<Sprite>("Assets/Oyster/Sprites/" + parameters[1]);
                spriteHandle.Completed += SpriteHandle_Completed; // Tell addressables to begin loading the sprite and call the listed method when done
                spriteLoaded = 1; // Set sprite loaded to loading
                tempParams = parameters; // Store the method's parameters externally
                break;
            case 2: // If the sprite has finished loading
                if (loadedSprite != null) // If the sprite loaded successfully
                {
                    loadedSprites.Add(parameters[0], loadedSprite); // Reference the sprite in loadedSprites
                }
                else // If the sprite failed to load
                {
                    Debug.Log("Unable to add sprite to 'loadedSprites' as 'loadedSprite' is null!"); // Inform the Unity console that the sprite failed to load
                }
                tempParams = null;
                loadedSprite = null; // Reset variables to their default values
                spriteLoaded = 0;
                currentLine++; // Continue to the next line
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
        switch (legacyPlayAnim_animState) // Choose between different animation states
        {
            case 0: // No animation is currently playing
                try
                {
                    legacyPlayAnim_waitOnAnim = false;
                    bool ignoreAnimator = false; // Set default values
                    // loop through optional parameters
                    for (int i = 0; i < parameters.Length - 2; i++)
                    {
                        string[] currentParameter = parameters[i + 2].Split('='); // Set current parameter equal to the current parameter and value
                        switch (currentParameter[0]) // Pick a case based on the parameter name
                        {
                            default: // If the parameter name is not recognised
                                Debug.Log("Parameter '" + currentParameter[0] + "' not recognised."); // Inform the Unity console that the parameter could not be recognised
                                break;
                            case "waitOnAnim": // If the parameter is waitOnAnim
                                try // Try to run the below code
                                {
                                    legacyPlayAnim_waitOnAnim = Convert.ToBoolean(currentParameter[1]); // Convert the current paramter value to a boolean and store it
                                }
                                catch // If the above code fails to run
                                {
                                    Debug.Log("Unable to convert '" + currentParameter[0] + "' to boolean."); // Inform the Unity console that the above code failed to run
                                }
                                break;
                            case "ignoreAnimator": // If the parameter is ignoreAnimator
                                try // Try to run the below code
                                {
                                    ignoreAnimator = Convert.ToBoolean(currentParameter[1]); // Convert the current parameter value to boolean and store it
                                }
                                catch // If the above code fails to run
                                {
                                    Debug.Log("Unable to convert '" + currentParameter[0] + "' to boolean."); // Inform the Unity console that the above code failed to run
                                }
                                break;
                        }
                    }
                    GameObject target = GameObject.Find(parameters[0]); // Find and cache a reference of the target gameobject
                    try // Check for animator, this is expected to fail
                    {
                        if (!ignoreAnimator) // If ignoreAnimator is false
                        {
                            target.GetComponent<Animator>();
                            currentLine++; // If it has animator then pass on running this function
                            Debug.Log("Target object has animator! Set the flag 'ignoreAnimator' to continue animating this object."); // Inform the Unity console that the object has an animator
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
                        legacyPlayAnim_targetAnimation.Play(parameters[1]); // Attempt to play the requested animation
                        legacyPlayAnim_animState = 2;
                    }
                    catch
                    {
                        // animation does not exist
                        Debug.Log("Object is either missing animation component or animation clip does not exist."); // Inform the Unity console that it couldn't play the requested animation
                        currentLine++; // Skip to the next line
                    }
                }
                catch
                {
                    Debug.Log("Game object '" + parameters[0] + "' either does not exist in the current scene or lacks an animation component.");
                    currentLine++; // Skip to the next line if the object does not exist
                }
                break;
            case 2: // If the animation is playing
                // anim playing && not waiting || anim finished playing & waiting
                if (!legacyPlayAnim_waitOnAnim || legacyPlayAnim_waitOnAnim && !legacyPlayAnim_targetAnimation.IsPlaying(parameters[1]))
                {
                    currentLine++; // Continue to the next line
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
        switch (playAnim_animState) // Pick which code to run based on the current animation state
        {
            case 0: // If this is the first loop
                try
                {
                    playAnim_targetAnimator = GameObject.Find(parameters[0]).GetComponent<Animator>();
                    playAnim_animState = 2;
                    playAnim_layerName = "Cutscene"; // Set default values
                    playAnim_waitOnAnim = true;
                    playAnim_animName = parameters[1];
                    for (int i = 0; i < parameters.Length - 2; i++) // Loop through optional parameters
                    {
                        string[] currentParameter = parameters[i + 2].Split('='); // Set current parameters equal to the current parameter's name and value
                        switch (currentParameter[0]) // Pick a case based on the current parameter's name
                        {
                            default: // If the current parameter is not recognised
                                Debug.Log("Parameter '" + currentParameter[0] + "' not recognised."); // Inform the Unity console that the current parameter could not be recognised
                                break;
                            case "waitOnAnim": // If the current parameter's name is waitOnAnim
                                try // Try to run the below code
                                {
                                    playAnim_waitOnAnim = Convert.ToBoolean(currentParameter[1]); // Convert the current paramter value to boolean and then store it
                                }
                                catch // If the above code fails to run
                                {
                                    Debug.Log("Unable to convert '" + currentParameter[0] + "' to boolean."); // Inform the Unity console that the above code failed to run
                                }
                                break;
                            case "layerName": // If the current parameter's name is layerName
                                playAnim_layerName = currentParameter[1]; // Set the layer name equal to the currentParamter's value
                                break;
                        }
                    }
                    try // Attempt to run the below code
                    {
                        playAnim_targetAnimator.Play(playAnim_layerName + "." + parameters[1]); // Try to play the requested animation
                        playAnim_animState = 2; // Set the animation state to playing
                    }
                    catch // If the above code fails to run
                    {
                        Debug.Log("Animator '" + parameters[0] + "' does not contain animation '" + playAnim_layerName + "." + parameters[1] + "'."); // Inform the Unity console that the animation does not exist
                        currentLine++; // Continue to the next line
                    }
                }
                catch // If the above code fails to run
                {
                    Debug.Log("Object '" + parameters[0] + "' does not contain an animator."); // Inform the Unity console that the object does not have an animator
                    currentLine++; // Skip to the next line
                }
                break;
            case 2: // !animation playing && waiting || !waiting
                bool finished = false;
                // 0.99 on line below as normalizedTime never quite reaches 1 on the same frame this checks
                if (playAnim_targetAnimator.GetCurrentAnimatorStateInfo(playAnim_targetAnimator.GetLayerIndex(playAnim_layerName)).normalizedTime >= 0.99) // There's surely a better way to check when an animation has finished?
                {
                    finished = true; // If the animation has finished then set finished to true
                }
                if (!playAnim_waitOnAnim || playAnim_waitOnAnim && finished) // If it is not waiting to finish or is waiting and has finished
                {
                    playAnim_targetAnimator = null;
                    playAnim_animState = 0;
                    playAnim_layerName = "Cutscene";
                    playAnim_waitOnAnim = true; // Set variables back to their default values
                    playAnim_animName = null;
                    currentLine++; // Continue to the next line
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
        try // Try to run the below code
        {
            Animator targetAnimator = GameObject.Find(parameters[0]).GetComponent<Animator>(); // Find the object's animator
            targetAnimator.SetLayerWeight(targetAnimator.GetLayerIndex(parameters[1]), float.Parse(parameters[2])); // Set the requested layer to the requested layer weight
        }
        catch // If the above code fails to run
        {
            Debug.Log("Failed to set weight of layer '" + parameters[1] + "' to value '" + parameters[2] + "'."); // Inform the Unity console that the above code failed to run
        }
        currentLine++; // Continue to the next line
    }
    #endregion
    #region SetupLineMarkers
    private void SetupLineMarkers(OysterConversation conversation) // This is mostly a carbon-copy of reading the current line, only difference is that it reads all lines and only pays attention to 'NewLineMarker'
    { // Command to be used within Oyster script is NewLineMarker
        // Required Parameters:
        // param0: Line marker name
        int currentLine = 0;
        foreach (string command in conversation.commands)
        {
            bool removingBlank = true; // Spaces default to not being read
            string tempString = ""; // Custom way of parsing the line. It simply checks through each character stores everything but spaces, unless the spaces are between ''
            foreach (char chr in command)
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
            if (currentLineCommand == "NewLineMarker")
            {
                // Create new line marker
                Debug.Log("New Line marker on line " + currentLine.ToString() + " named " + currentLineParameters[0]);
                lineMarkers.Add(currentLineParameters[0], currentLine);
            }
            currentLine++;
        }
    }
    #endregion
    #region JumpTo
    private void JumpTo(string[] parameters)
    {
        // Required Parameters:
        // param0: Line marker to jump to
        try // Try to run the below code
        {
            currentLine = lineMarkers[parameters[0]]; // Set current line to the requested line marker
        }
        catch // If the above code fails to run
        {
            Debug.Log("Line marker '" + parameters[0] + "' does not exist in this conversation."); // Inform the Unity console that the above code failed to run
        }
    }
    #endregion
    #region Decision
    private void Decision(string[] parameters) // Not yet implemented
    {
        // Required Parameters:
        // param0: Chunky array of buttons and outcomes
        // Single value in param0:
        // 0: Button Name
        // 1: Button Position
        // 2: Button Size
        // 3: Button Sprite
        // 4: Line Marker when pressed

        // Interpret parameter as array, arrays are of one data type so I can't do that
        // I could make a custom class called DecisionElement?
        // parsing a string to that would be a nightmare

        // Are decisions something that I even need right now?

        // I could just make this function at a later date?

        Debug.Log("Decisions are not yet implemented.");
        currentLine++;
    }
    #endregion
    #region EndConversation
    private void EndConversation(string[] parameters) // Ends a conversation
    {
        // Required Parameters:
        // Optional Parameters:
        // param: bumpSceneState = boolean
        bool bumpSceneState = true;
        if (parameters.Length > 0)
        {
            foreach (string parameter in parameters)
            {
                string[] currentParameter = parameter.Split('=');
                switch (currentParameter[0])
                {
                    default:
                        Debug.Log("Parameter '" + currentParameter[0] + "' not recognised.");
                        break;
                    case "bumpSceneState":
                        try
                        {
                            bumpSceneState = Convert.ToBoolean(currentParameter[1]);
                        }
                        catch
                        {
                            Debug.Log("Unable to convert '" + currentParameter[1] + "' to boolean.");
                        }
                        break;
                }
            }
        }
        if (bumpSceneState)
        {
            try
            {
                if (PersistentVariables.documentsPath == "")
                {
                     PersistentVariables.documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                }
                ProfileData profileData = JsonUtility.FromJson<ProfileData>(File.ReadAllText(PersistentVariables.documentsPath + @"\My Games\LimboLane\Profiles\" + PersistentVariables.profileName + ".json"));
                for (int i = 0; i < profileData.locationStates.Length; i++)
                {
                    if (profileData.locationStates[i].name == PersistentVariables.nextSceneName)
                    {
                        profileData.locationStates[i].state++;
                    }
                }
                File.WriteAllText(PersistentVariables.documentsPath + @"\My Games\LimboLane\Profiles\" + PersistentVariables.profileName + ".json", JsonUtility.ToJson(profileData));
                sceneStateLoader.Run();
            }
            catch
            {
                Debug.Log("Unable to update profile with new location!");
            }
        }
        currentLine++;
        inConversation = false; // Tell the script that it is no longer in a conversation
        CleanupSpeech(); // Calls the cleanup method to unload any currently loaded addressables and remove any objects created by the script
    }
    #endregion
    #region ModCamFOV
    private void ModCamFOV(string[] parameters)
    {
        // Required Parameters:
        // param0: camera name
        // param1: target FOV
        // Optional Parameters:
        // param: time = float
        // param: interpolation = string
        // param: permanent = bool
        // param: clickToSkip = bool
        switch (modFOV_state) // Compare modFOV_state against the below cases
        {
            case 0: // setting up modFOV
                bool failed = false; // Set failed to false
                try // Try to run the below code
                {
                    modFOV_camera = GameObject.Find(parameters[0]).GetComponent<Camera>(); // Find the specified camera in the current scene
                }
                catch // If the above code fails to run
                {
                    Debug.Log("Unable to find camera '" + parameters[0] + "' in the current scene."); // Inform the Unity console that something went wrong
                    currentLine++; // Continue to the next line
                    failed = true; // Set failed to true
                }
                try // Try to run the below code
                {
                    modFOV_targetFOV = float.Parse(parameters[1]); // Set the targetFOV equal to the second parameter converted to a float
                }
                catch // If the above code fails to run
                {
                    Debug.Log("Failed to convert '" + parameters[1] + "' to float."); // Inform the Unity console that something went wrong
                    currentLine++; // Continue to the next line
                    failed = true; // Set failed to true
                }
                if (!failed) // If the code did not fail at any point
                {
                    modFOV_time = 1; // Set some default values
                    modFOV_interpolation = "linear";
                    modFOV_permanent = false;
                    modFOV_ogFOV = modFOV_camera.fieldOfView;
                    modFOV_clickToSkip = true;
                    if (parameters.Length - 2 > 0) // If there are optional parameters
                    {
                        for (int i = 0; i < parameters.Length - 2; i++) // Loop through the optional parameters
                        {
                            string[] currentParameter = parameters[i + 2].Split('='); // Split the current parameter into a name and value
                            switch (currentParameter[0]) // Compare the name of the current parameter against the below cases
                            {
                                default: // If the current parameter matches none of the below cases
                                    Debug.Log("Parameter '" + currentParameter[0] + "' not recognised."); // Inform the Unity console that the parameter is not recognised
                                    break;
                                case "time": // If the current parameter is 'time'
                                    try // Try to run the below code
                                    {
                                        modFOV_time = float.Parse(currentParameter[1]); // Convert the current parameter to float and store it in time
                                    }
                                    catch // If the above code fails to run
                                    {
                                        Debug.Log("Failed to convert '" + currentParameter[1] + "' to float."); // Inform the Unity console that something has gone wrong
                                    }
                                    break;
                                case "interpolation": // If the current parameter is 'interpolation'
                                    modFOV_interpolation = currentParameter[1]; // Set interpolation equal to the current parameter
                                    break;
                                case "permanent": // If the current parameter is 'permanent'
                                    try // Try to run the below code
                                    {
                                        modFOV_permanent = Convert.ToBoolean(currentParameter[1]); // Convert the current parameter to boolean and store it in permanent
                                    }
                                    catch // If the above code fails to run
                                    {
                                        Debug.Log("Failed to convert '" + currentParameter[1] + "' to boolean."); // Inform the Unity console that something went wrong
                                    }
                                    break;
                                case "clickToSkip": // If the current parameter is 'clickToSkip'
                                    try // Try to run the below code
                                    {
                                        modFOV_clickToSkip = Convert.ToBoolean(currentParameter[1]); // Convert the current parameter to boolean and store it in clickToSkip
                                    }
                                    catch // If the above code fails to run
                                    {
                                        Debug.Log("Failed to convert '" + currentParameter[1] + "' to boolean."); // Inform the Unity console that something went wrong
                                    }
                                    break;
                            }
                        }
                    }
                    if (modFOV_permanent == false) // If the FOV change is not permanent
                    {
                        CameraAndFOV cameraAndFOV = new CameraAndFOV();
                        cameraAndFOV.camera = modFOV_camera;
                        cameraAndFOV.fov = modFOV_ogFOV; // Create a new camera and fov object
                        modFOV_camerasToFix.Add(modFOV_camerasToFix.Count, cameraAndFOV); // Add that object to the list of cameras that need fixing
                    }
                    modFOV_state = 2; // Set modFOV_state to 2
                }
                break;
            case 2: // modding the FOVing
                bool done = false; // Set done to false
                switch (modFOV_interpolation.ToLower()) // Compare interpolation against the below cases
                {
                    default: // No interpolation
                        modFOV_camera.fieldOfView = modFOV_targetFOV; // Set the camera's FOV to the target FOV
                        done = true; // Set done to true
                        break;
                    case "linear": // Linear interpolation
                        if (modFOV_time < modFOV_currentTime) // If the total time is less that the current time
                        {
                            done = true; // Set done to true
                        }
                        else // Otherwise
                        {
                            modFOV_currentTime += Time.deltaTime; // Add delta time to the current time
                            modFOV_camera.fieldOfView = Mathf.Lerp(modFOV_ogFOV, modFOV_targetFOV, modFOV_currentTime / modFOV_time); // Set the camera's FOV to a value that is linearly interpolated between it's starting point and the FOV target

                        }
                        break;
                }
                if (modFOV_clickToSkip && mouseClicked && !mouseDown) // If click to skip is true and the mouse is clicked but not held
                {
                    modFOV_interpolation = "none"; // Set the interpolation to none
                    mouseDown = true; // Set mousedown to true
                }
                if (done) // If done is true
                {
                    modFOV_camera = null;
                    modFOV_interpolation = null;
                    modFOV_ogFOV = 5;
                    modFOV_permanent = false;
                    modFOV_state = 0; // Reset values to their default
                    modFOV_targetFOV = 5;
                    modFOV_time = 1;
                    modFOV_currentTime = 0;
                    currentLine++; // Continue to the next line
                }
                break;
        }

    }
    #endregion
    #region ManipulateImage
    private void ManipulateImage(string[] parameters)
    {
        // Required Parameters:
        // param0: image
        // Optional Parameters:
        // param: time = float
        // param: alpha = float
        // param: interpolation = string
        switch (manipImg_state) // Compare state against the below cases
        {
            case 0: // Setup variables
                bool success = false; // Set success to false
                try // Try to run the below code
                {
                    manipImg_image = GameObject.Find(parameters[0]).GetComponent<Image>();  // Try to find the specified Image in the current scene
                    success = true; // Set success to true
                }
                catch // If the above code fails to run
                {
                    Debug.Log("Unable to find image '" + parameters[0] + "'."); // Inform the Unity console that something went wrong
                    currentLine++; // Continue to the next line
                }
                if (success) // If the above code succeeded
                {
                    manipImg_state = 1;
                    manipImg_time = 1; // Set default values
                    manipImg_currentTime = 0;
                    manipImg_transparency = 255;
                    manipImg_interpolation = "linear";
                    manipImg_modTransparency = false;
                    manipImg_ogTransparency = manipImg_image.color.a;
                    if (parameters.Length > 1) // If there are optional parameters
                    {
                        for (int i = 0; i < parameters.Length - 1; i++) // Loop through the optional parameters
                        {
                            string[] currentParameter = parameters[i + 1].Split('='); // Split the current parameter into a name and value
                            switch (currentParameter[0]) // Compare the name of the current parameter against the below cases
                            {
                                default: // If the parameter matches none of the below cases
                                    Debug.Log("Parameter '" + currentParameter[0] + "' not recognised."); // Inform the Unity console that the parameter was not recognised
                                    break;
                                case "time": // If the parameter is 'time'
                                    try // Try to run the below code
                                    {
                                        manipImg_time = float.Parse(currentParameter[1]); // Convert the current parameter to float and store the value as time
                                    }
                                    catch // If the above code fails to run
                                    {
                                        Debug.Log("Failed to convert '" + currentParameter[1] + "' to float."); // Inform the Unity console that something went wrong
                                    }
                                    break;
                                case "alpha": // If the current parameter is 'alpha'
                                    try // Try to run the below code
                                    {
                                        manipImg_transparency = float.Parse(currentParameter[1]); // Set transparency equal to the current parameter as a float
                                        manipImg_transparency = Mathf.Clamp(manipImg_transparency, 0, 1); // Clamp the value of transparency to between 0 and 1
                                        manipImg_modTransparency = true; // Set modTransparency to true
                                    }
                                    catch // If any of the above code fails to run
                                    {
                                        Debug.Log("Failed to convert '" + currentParameter[1] + "' to float."); // Inform the Unity console that something went wrong
                                    }
                                    break;
                                case "interpolation": // If the parameter is 'interpolation'
                                    manipImg_interpolation = currentParameter[1]; // Set interpolation equal to the current parameter
                                    break;
                            }
                        }
                    }
                }
                break;
            case 1: // do the stuff
                bool done = true; // Set done to true
                switch (manipImg_interpolation) // Compare interpolation to the below cases
                {
                    default: // no interpolation
                        if (manipImg_modTransparency) // If transparency is to be changed
                        {
                            manipImg_image.color = new Color(manipImg_image.color.r, manipImg_image.color.g, manipImg_image.color.b, manipImg_transparency); // Set the transparency of the image equal to the target transparency
                        }
                        break;
                    case "linear": // linear interpolation
                        if (manipImg_modTransparency && manipImg_currentTime < manipImg_time) // If the transparecy is to be changed and the current time is less than the total time
                        {
                            manipImg_image.color = new Color(manipImg_image.color.r, manipImg_image.color.g, manipImg_image.color.b, Mathf.Lerp(manipImg_ogTransparency, manipImg_transparency, manipImg_currentTime / manipImg_time)); // Set the transparency of the image equal to a value linearly interpolated between the original transparency and the target transparency
                            done = false; // Set done to false
                        }
                        manipImg_currentTime += Time.deltaTime; // Add delta time to the current time
                        break;
                }
                if (done) // If done is true
                {
                    manipImg_image.color = new Color(manipImg_image.color.r, manipImg_image.color.g, manipImg_image.color.b, manipImg_transparency); // Set the transparency equal to the target transparency to clear up any rounding error on interpolation
                    manipImg_state = 1; // Set values back to their default
                    manipImg_time = 1;
                    manipImg_currentTime = 0;
                    manipImg_transparency = 255;
                    manipImg_interpolation = "linear";
                    manipImg_ogTransparency = 255;
                    manipImg_modTransparency = false;
                    manipImg_image = null;
                    manipImg_state = 0;
                    currentLine++; // Continue to the next line
                }
                break;
        }
    }
    #endregion
    #region LoadScene
    private void LoadScene(string[] parameters)
    {
        // Required Parameters:
        // param0: scene name
        PersistentVariables.nextSceneName = parameters[0]; // Set the name of the next scene to the current parameter
        SceneManager.LoadScene("LoadingScreen"); // Load the loading scene
    }
    #endregion
    #region ModShapeKey
    private void ModShapeKey(string[] parameters)
    {
        // Required Parameters:
        // param0: Shape key name
        // param1: Object name
        // param2: Target value
        // Optional Parameters:
        // param: time = float
        // param: interpolation = string
        // param: subObjectName = string
        // param: clickToSkip = bool
        switch (msk_state) // Pick which state the method is in
        {
            default: // If none of the cases match the currentState
                Debug.Log("State '" + msk_state.ToString() + "' not recognised!"); // Inform the Unity console that something has gone wrong
                break;
            case 0: // Setting up
                bool failed = false; // Set failed to false
                GameObject tempObject = null; // Create a new null gameObject
                try // Try to run the below code
                {
                    tempObject = GameObject.Find(parameters[1]); // Load values from parameters
                    msk_targetShapeKeyValue = float.Parse(parameters[2]);
                    msk_currentTime = 0;
                    msk_maxTime = 1;
                    msk_interpolation = "none"; // Set default values
                    msk_clickToSkip = false;
                    if (parameters.Length > 3) // If there are optional parameters
                    {
                        for(int i = 0; i < parameters.Length - 3; i++) // Loop through the optional parameters
                        {
                            string[] currentParameter = parameters[i + 3].Split('='); // Split the current parameter into a name and a value
                            switch (currentParameter[0]) // Compare the current parameter against a list of cases
                            {
                                default: // If the parameter name matches none of the below cases
                                    Debug.Log("Parameter '" + currentParameter[0] + "' not recognised."); // Inform the Unity console that something went wrong
                                    break;
                                case "subObjectName": // If the parameter is subObjectName
                                    tempObject = tempObject.transform.Find(currentParameter[1]).gameObject; // Try to find the stated object
                                    if (tempObject == null) // If the object could not be found
                                    {
                                        Debug.Log("Unable to find object child!"); // Inform the Unity console that something went wrong
                                        failed = true; // Set failed to true
                                    }
                                    break;
                                case "time": // If the parameter is time
                                    try // Try to run the below code
                                    {
                                        msk_maxTime = float.Parse(currentParameter[1]); // Set maxTime to the parameter value
                                    }
                                    catch // If the above code fails to run
                                    {
                                        Debug.Log("Unable to resolve '" + currentParameter[1] + "' to float."); // Inform the Unity console that something went wrong
                                    }
                                    break;
                                case "interpolation": // If the parameter name is interpolation
                                    msk_interpolation = currentParameter[1]; // Set interpolation to the parameter value
                                    break;
                                case "clickToSkip": // If the parameter name is clickToSkip
                                    try // Try to run the below code
                                    {
                                        msk_clickToSkip = Convert.ToBoolean(currentParameter[1]); // Convert the parameter value to a boolean
                                    }
                                    catch // If the above code fails to run
                                    {
                                        Debug.Log("Unable to parse '" + currentParameter[1] + "' as boolean."); // Inform the Unity console that something went wrong
                                    }
                                    break;
                            }
                        }
                    }
                }
                catch // If the above code fails to run
                {
                    Debug.Log("Unable to resolve inputs for ModShapeKey!"); // Inform the Unity console that something went wrong
                    failed = true; // Set failed to true
                }
                if (!failed) // If the code did not fail
                {
                    // Find shape key
                    msk_skinnedMesh = tempObject.GetComponent<SkinnedMeshRenderer>(); // Cache the SkinnedMeshRender of theobject
                    msk_mesh = msk_skinnedMesh.sharedMesh; // Cache the mesh of the object
                    msk_shapeKeyId = msk_mesh.GetBlendShapeIndex(parameters[0]); // Find and store the shape key ID
                    if (msk_shapeKeyId == -1) // If the ID is -1
                    {
                        failed = true; // Set failed to true
                        Debug.Log("Shape key '" + parameters[0] + "' does not exist"); // Inform the Unity console that the shape key does not exist
                    }
                    else // If the ID is anything else
                    {
                        msk_ogShapeKeyValue = msk_skinnedMesh.GetBlendShapeWeight(msk_shapeKeyId); // Set the shape key value equal to the weight of the shape key
                        msk_state = 1; // Set state to 1
                        msk_writing = true; // Set writing to true
                    }
                }
                if (failed) // If the code failed at any point
                {
                    msk_targetShapeKeyValue = 0;
                    msk_currentTime = 0;
                    msk_maxTime = 1;
                    msk_interpolation = "none";
                    msk_mesh = null;
                    msk_skinnedMesh = null; // Set variables back to their default values
                    msk_targetShapeKeyValue = 0;
                    msk_ogShapeKeyValue = 0;
                    msk_shapeKeyId = 0;
                    msk_state = 0;
                    msk_clickToSkip = false;
                    currentLine++; // Skip to the next line
                }
                break;
            case 1: // Actually shaping the keying
                if (msk_clickToSkip && mouseClicked && !mouseDown) // If clickToSkip is true, mouseDown is false and the mouse is clicked
                {
                    msk_interpolation = "none"; // Set interpolation to none
                    mouseDown = true; // Set mouseDown to true
                }
                if (!msk_writing) // If writing is false
                {
                    msk_targetShapeKeyValue = 0;
                    msk_currentTime = 0;
                    msk_maxTime = 1;
                    msk_interpolation = "none"; // Set variables back to their default values
                    msk_mesh = null;
                    msk_skinnedMesh = null;
                    msk_targetShapeKeyValue = 0;
                    msk_ogShapeKeyValue = 0;
                    msk_shapeKeyId = 0;
                    msk_state = 0;
                    msk_clickToSkip = false;
                    currentLine++; // Continue to the next line
                }
                break;
        }
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
        return new Vector3(vectorx, vectory, vectorz); // Return a new vector made of vectorx, vectory and vectorz
    }
    private Vector4 String4Vector(string input)
    {
        float vectorx = 0;
        float vectory = 0; // Set default values
        float vectorz = 0;
        float vectorw = 0;
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
                        if (readingValue == 2) // If this is the second value being read
                        {
                            vectory = float.Parse(tempString); // Set vectory equal to tempString
                        }
                        else // If this is any other value (a.k.a: if this is the third value)
                        {
                            vectorz = float.Parse(tempString); // Set vectorz equal to tempString
                        }
                        tempString = ""; // Set tempString back to being empty
                        readingValue++; // Increment which value is currently being read
                        break;
                    case ')':
                        tempString = tempString.Replace(",", ""); // Set tempString equal to itself minus the comma
                        vectorw = float.Parse(tempString); // Set vectorw equal to tempString
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
            Debug.Log("Failed to convert input string to vector! Continuing with (100f, 100f, 100f, 100f)"); // Inform the Unity console that something wrong happened
            vectorx = 100f; // Set vectorx, vectory, vectorz and vectorw to default values
            vectory = 100f;
            vectorz = 100f;
            vectorw = 100f;
        }
        return new Vector4(vectorx, vectory, vectorz, vectorw); // Return a new vector made of vectorx, vectory, vectorz and vectorw
    }
    private void CleanupSpeech() // Destroys all objects and addressables created by a script
    {
        foreach (KeyValuePair<string, GameObject> entry in createdObjects) // Loops through each entry in gameOBjectsNames
        {
            try // Attempts to find and destroy a gameObject
            {
                Destroy(entry.Value); // Find and destroy object
                Debug.Log("Destroyed " + entry.Key + " during cleanp"); // Tell the Unity console that an object was found and destroyed
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
        for (int i = 0; i < modFOV_camerasToFix.Count; i++)
        {
            int c = modFOV_camerasToFix.Count - 1 - i;
            modFOV_camerasToFix[c].camera.fieldOfView = modFOV_camerasToFix[c].fov;
        }
        currentLine = 0;
        loadedAddressables = new Dictionary<int, AsyncOperationHandle>();
        createdObjects = new Dictionary<string, GameObject>(); // Reset the object and handle trackers to being null for when another script is ran
        speechCooldown = speechCooldownTime; // Set the speechCooldown to the provided time, so that a speech cannot be instantly started after this one. Useful for if the previous speech finished with a click event
        charactersInConversation = new Dictionary<string, CharacterData>();
        characterObjectsInConversation = new Dictionary<string, GameObject>(); // Reset some Dictionaries to be empty again
        modObj_objectsToFix = new Dictionary<int, ObjectAndPosition>();
        modFOV_camerasToFix = new Dictionary<int, CameraAndFOV>();
        loadedFonts = new Dictionary<string, TMP_FontAsset>();
        loadedSprites = new Dictionary<string, Sprite>(); // Maybe more than some dictionaries
        conversationStrings = new Dictionary<string, string>();
        lineMarkers = new Dictionary<string, int>();
    }
    #endregion
}