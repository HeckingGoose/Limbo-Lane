using System.IO;
using UnityEngine; // Reference required assemblies

public class SceneStateLoader : MonoBehaviour
{
    [SerializeField]
    private string locationName;
    [SerializeField]
    private OysterInitialiseScript oysterCaller;
    [SerializeField] // Define variables
    private OysterCharacterScript oysterCharacter;
    private int locationState = 71077345;
    public bool done = false;
    private void Start()
    {
        Run(); // Call public run method on scene start
    }
    public void Run() // Method is public so that Oyster can call it on conversation end
    {
        if (PersistentVariables.documentsPath == "") // If documents path has not been defined
        {
            PersistentVariables.documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments); // Define documents path
        }
        try // Try to run the below code
        {
            string output = File.ReadAllText(PersistentVariables.documentsPath + @"\My Games\LimboLane\Profiles\" + PersistentVariables.profileName + ".json"); // Load current profile data
            ProfileData data = JsonUtility.FromJson<ProfileData>(output); // Convert loaded profile data to ProfileData instance

            for (int i = 0; i < data.locationStates.Length; i++) // Loop through all location states in profile
            {
                if (locationName == data.locationStates[i].name) // If the profile location name matches the current location name
                {
                    locationState = data.locationStates[i].state; // Set locationState to the state of the profile location name
                }
            }
            if (locationState == 71077345) // If no locationState was loaded 
            {
                locationState = 0; // Set locationState to 0
                Debug.Log("Location state could not be loaded!"); // Inform the Unity console that something went wrong
            }
        }
        catch // If any of the above code fails to run
        {
            Debug.Log("Profile data could not be found!"); // Inform the Unity console that something went wrong
            locationState = 0; // Set locationState to 0
        }
        switch (locationName) // Compare locationName against below cases
        {
            default: // If locationName matches no cases
                Debug.Log("Location '" + locationName + "' does not exist."); // Inform Unity that the location does not exist
                break;
            case "AlexHouse": // If the location is 'AlexHouse'
                break; // Do nothing
            case "AlyxRoom": // If the location is AlyxRoom
                switch (locationState) // Compare locationState against below cases
                {
                    default: // If locationState matches no cases
                        Debug.Log("Location state '" + locationState.ToString() + "' not recognised for location '" + locationName + "'"); // Inform the Unity console that the state does not exist
                        break;
                    case 0: // If the locationState is 0
                        oysterCharacter.SetConversationName("AlyxTutorial"); // Set and run the conversation 'AlyxTutorial'
                        oysterCaller.CallScript(oysterCharacter);
                        break;
                    case 1: // If the locationState is 1
                        oysterCharacter.SetConversationName("AlyxPostTutorialIntro"); // Set and run the conversation 'AlyxPostTutorialIntro'
                        oysterCaller.CallScript(oysterCharacter);
                        
                        // Begin a card battle here

                        break;
                }
                break;
            case "EmilyRoom": // If the location is EmilyRoom
                break; // Do nothing
        }
    }
}