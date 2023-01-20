using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental.AI;
using UnityEngine.UI;

public class MainCardBattleHandler : MonoBehaviour
{
    [SerializeField]
    private GameObject cardBattleHUD;
    [SerializeField] // Set up a lot of variables
    private GameObject cardPrefab;
    [SerializeField]
    private EnemyScript enemy;
    [SerializeField]
    private GameObject board;
    [SerializeField]
    private BellScript bellScript;
    [SerializeField]
    private BellScript phischScript;
    private GameObject[ , ] boardCards = new GameObject[4, 4];
    [SerializeField]
    private Material[] cardMaterials;
    [SerializeField]
    private Sprite defaultPortrait;
    [SerializeField]
    private Sprite[] characterPortraits;
    private GameObject mainCamera;
    private GameObject playerDeckObject;
    private List<GameObject> playerCardObjects;
    private GameObject selectedCard;
    private CardScript selectedCardScript;
    private int selectedCardIndex;
    private bool selecting = false;
    private float totalRange = 0.6f;
    private string currentTurn = "Setup";
    private bool inCardBattle = false;
    private bool mouseDown = false;
    private bool drawnPhisch = false;
    #region Player Variables
    private Deck playerFullDeck;
    private Deck playerDeck;
    private int playerHandSize;
    private int playerCurrency;
    #endregion
    #region Enemy Variables
    #endregion
    private Camera mainCameraCam;
    private void Start()
    {
        // Add loading playerHandSize from persistant variables
        mainCamera = Camera.main.gameObject; // Find the main camera gameObject in the scene
        mainCameraCam = Camera.main; // Find the main camera in the scene
        CardBattle();
    }
    private void Update()
    {
        if (inCardBattle) // If a card battle is supposed to be happening
        {
            CardBattle(); // Call the CardBattle method
        }
        if (Input.GetAxis("PrimaryAction") > 0) // If there is mouse input
        {
            mouseDown = true; // Set mouseDown to true
        }
        else // If there is no mouse input
        {
            mouseDown = false; // Set mouseDown to false
        }
    }
    private void CardBattle()
    {
        inCardBattle = true; // Set inCardBattle to true
        if (enemy.ready)
        {
            switch (currentTurn.ToLower()) // Pick which turn it is
            {
                default: // If none of the below cases match the current turn
                    Debug.Log("Turn type '" + currentTurn + "' not recognised."); // Inform the Unity console that the current turn does not exist
                    break;
                case "setup": // If the turn is 'setup', A.K.A: setting up the card battle
                    #region Create or load player deck
                    if (PersistentVariables.matchStartingCurrency != -1)
                    {
                        playerCurrency = PersistentVariables.matchStartingCurrency;
                    }
                    else
                    {
                        Debug.Log("Unable to load player starting currency! Defaulting to 1");
                        playerCurrency = 1;
                    }
                    if (PersistentVariables.handSize != -1)
                    {
                        playerHandSize = PersistentVariables.handSize;
                    }
                    else
                    {
                        Debug.Log("Unable to load player hand size! Defaulting to 7.");
                        playerHandSize = 7;
                    }
                    if (PersistentVariables.profileName != "")// If a profile is loaded
                    {
                        // Load the player's cards and store them in their full deck
                        string[] deck = JsonUtility.FromJson<ProfileData>(File.ReadAllText(PersistentVariables.documentsPath + @"\My Games\LimboLane\Profiles\" + PersistentVariables.profileName + ".json")).deck;
                        int deckSize = deck.Length;
                        playerFullDeck = new Deck();
                        playerFullDeck.cards = new Card[deckSize];
                        for (int i = 0; i < deckSize; i++)
                        {
                            playerFullDeck.cards[i] = CreateCardData(deck[i]);
                        }
                    }
                    else // If no profile is loaded - intended for testing
                    {
                        Debug.Log("No profile is currently loaded! Continuing with default deck."); // Inform the Unity console that no profile is loaded
                        playerFullDeck = new Deck(); // Create a new deck to store all of the player's cards
                        playerFullDeck.cards = new Card[10]; // Set the size of the full deck to the number of cards the player has
                        playerFullDeck.cards[0] = CreateCardData("Reaper");
                        playerFullDeck.cards[1] = CreateCardData("Phisch");
                        playerFullDeck.cards[2] = CreateCardData("Reaper");
                        playerFullDeck.cards[3] = CreateCardData("Reaper"); // Populate the full deck
                        playerFullDeck.cards[4] = CreateCardData("Reaper");
                        playerFullDeck.cards[5] = CreateCardData("Reaper");
                        playerFullDeck.cards[6] = CreateCardData("Reaper");
                        playerFullDeck.cards[7] = CreateCardData("Reaper");
                        playerFullDeck.cards[8] = CreateCardData("Reaper");
                        playerFullDeck.cards[9] = CreateCardData("Reaper");
                        // Make code for populating enemy deck
                    }
                    playerDeck = new Deck(); // Create a new deck to store the player's hand
                    playerDeck.cards = new Card[playerHandSize]; // Initialise the deck in playerDeck to the size of the player's hand
                    playerDeck = PopulateDeck(playerFullDeck, playerHandSize, playerDeck); // Call populate deck to fill the player's deck
                    #endregion
                    #region Setup the UI
                    UpdateCurrency(playerCurrency, enemy.currency); // update to use enemy currency
                    SetPlayerPortrait("Alex");
                    SetEnemyPortrait(enemy.enemyName);
                    #endregion
                    cardBattleHUD.SetActive(true);
                    currentTurn = "Player"; // Set the turn to Player
                    GenerateDeck(); // Generate the player's deck
                    break;
                case "player": // If the current turn is player
                               // handle mouse input from player to pick card
                    bool hitThing = false; // Create a new bool called hitThing and set it to false
                    if (selecting) // If the player is currently selecting a card
                    {
                        RaycastHit hit; // Create a new raycasthit object
                        Ray ray = mainCameraCam.ScreenPointToRay(Input.mousePosition); // Create a new raycast
                        if (Physics.Raycast(ray, out hit)) // If the raycast hits an object
                        {
                            if (hit.transform.tag == "PlayerBoardSpace") // If the object has no children
                            {
                                if (hit.transform.childCount == 0) // If the object is a board space on the player's side
                                {
                                    hitThing = true; // Set hitThing to true
                                    selectedCard.transform.position = new Vector3(hit.transform.position.x, hit.transform.position.y + 0.01f, hit.transform.position.z); // Move the player's selected card to the board space they clicked on
                                    if (!mouseDown && Input.GetAxis("PrimaryAction") > 0 && playerCurrency >= selectedCardScript.cardData.cost) // If mouseDown is false and the player clicks
                                    {
                                        mouseDown = true; // Set mouseDown to true
                                                          // Place card on board
                                        switch (hit.transform.name) // Pick which space the player is clicking on
                                        {
                                            default: // If the space is not recognised
                                                Debug.Log("Object '" + hit.transform.name + "' not recognised."); // Inform the Unity console that the space was not recognised
                                                boardCards[0, 0] = selectedCard;
                                                break;
                                            case "00":
                                                boardCards[2, 0] = selectedCard;
                                                break;
                                            case "01":
                                                boardCards[2, 1] = selectedCard; // This part pretty much explains itself
                                                break;
                                            case "02":
                                                boardCards[2, 2] = selectedCard;
                                                break;
                                            case "03":
                                                boardCards[2, 3] = selectedCard;
                                                break;
                                            case "10":
                                                boardCards[3, 0] = selectedCard;
                                                break;
                                            case "11":
                                                boardCards[3, 1] = selectedCard;
                                                break;
                                            case "12":
                                                boardCards[3, 2] = selectedCard;
                                                break;
                                            case "13":
                                                boardCards[3, 3] = selectedCard;
                                                break;
                                        }
                                        selectedCard.transform.parent = hit.transform; // Set the card's parent to the board space it is on
                                        playerCurrency -= selectedCardScript.cardData.cost;
                                        UpdateCurrency(playerCurrency, enemy.currency); // update to use enemy currency
                                        selectedCard = null; // Remove the reference to the card
                                        selectedCardScript = null;
                                        selecting = false; // Set selecting to false
                                        playerDeck.cards[selectedCardIndex] = null; // Remove reference to card in playerDeck
                                        selectedCardIndex = -1;
                                        int i = 0; // Define i and set it to 0
                                        foreach (Card card in playerDeck.cards) // Loop through every card in the player's deck
                                        {
                                            if (card != null) // If the card does exist
                                            {
                                                i++; // Add 1 to i
                                            }
                                        }
                                        Card[] temp = new Card[i]; // Create temp and set it equal to the length of cards that did exist in the player's deck
                                        int k = 0; // Define k and set it to 0
                                        for (i = 0; i < playerDeck.cards.Length; i++) // Loop through every index of playerDeck.cards
                                        {
                                            if (playerDeck.cards[i] != null) // If the card exists
                                            {
                                                temp[k] = playerDeck.cards[i]; // Set the kth index of temp equal to playerDeck.cards[i]
                                                k++; // Add 1 to k
                                            }
                                        }
                                        playerDeck.cards = temp; // Set the player's deck equal to temp
                                                                 //currentTurn = "Enemy"; <- this should switch once the player presses a button to finish their turn\
                                        GenerateDeck(); // Generate the player's deck again
                                    }
                                }
                                else // If the raycast does not hit an object
                                {
                                    selectedCardScript.Select(); // Move the selected card back to its selecting position
                                }
                            }
                            else // If the raycast does not hit an object
                            {
                                selectedCardScript.Select(); // Move the selected card back to its selecting position
                            }
                        }
                        else // If the raycast does not hit an object
                        {
                            selectedCardScript.Select(); // Move the selected card back to its selecting position
                        }
                    }
                    if (!mouseDown && Input.GetAxis("PrimaryAction") > 0 && !hitThing) // If the player clicks the mouse, mouseDown is false and the raycast did not hit a valid board space
                    {
                        selecting = false; // Set selecting to false
                        selectedCard = null; // Set selectedCard to null
                        mouseDown = true; // Set mouseDown to true
                        for (int i = 0; i < playerCardObjects.Count; i++) // Loop through every card in the player's deck
                        {
                            CardScript script = playerCardObjects[i].GetComponent<CardScript>(); // Find the cardScript component of the card
                            script.Unselect(); // Unselect the card
                            if (script.mouseIsOver) // If the mouse is over the card
                            {
                                // move card into placing state
                                script.Select(); // Set the card to its selected state
                                selecting = true; // Set selecting to true
                                selectedCard = playerCardObjects[i]; // Set selectedCard equal to the current card
                                selectedCardIndex = i; // Set selectedCardIndex to the current index
                                selectedCardScript = script; // Set the selectedCardScript to script
                            }
                        }
                    }
                    if (phischScript.bellClicked) // If the phisch pile is clicked
                    {
                        if (!drawnPhisch && Input.GetAxis("PrimaryAction") > 0) // If a phisch hasn't been drawn and the mouse is held
                        {
                            Deck temp = playerDeck; // Create a new temporary deck from the player's deck
                            playerDeck = new Deck(); // Reset the player's deck to a new deck
                            playerDeck.cards = new Card[temp.cards.Length + 1]; // Create a new array the size of temp + 1
                            for (int i = 0; i < temp.cards.Length; i++) // Loop through every card in temp
                            {
                                playerDeck.cards[i] = temp.cards[i]; // Add the card to the player's deck
                            }
                            playerDeck.cards[playerDeck.cards.Length - 1] = CreateCardData("Phisch"); // Add a phisch to the player's last deck index
                            drawnPhisch = true; // Set drawnPhisch to true
                            GenerateDeck(); // Re-generate the player's deck
                        }
                    }
                    if (bellScript.bellClicked) // If the bell is clicked
                    {
                        // play a ding sound here
                        currentTurn = "Enemy"; // Switch to the enemy's turn
                        drawnPhisch = false; // Set drawnPhisch to false
                    }
                    break;
                case "enemy": // If the current turn is enemy
                    enemy.ComputeTurn(boardCards);// pass script onto enemy script for turn
                    // make way to apply changes to board
                    currentTurn = "Player";
                    break; // From here some way for the enemy's changes to the array needs to be applied to the main board
            }
        }
    }
    private void GenerateDeck()
    {
        #region Delete old deck object
        try // Try to run the below code
        {
            GameObject.Destroy(playerDeckObject); // Destroy the player's deck object
        }
        catch // If the above code fails to run
        {
            Debug.Log("Unable to destroy player deck object."); // Inform the Unity console that something went wrong
        }
        #endregion
        #region Create Deck object
        playerDeckObject = new GameObject(); // Create a new gameObject
        playerDeckObject.name = "PlayerDeck"; // Name it PlayerDeck
        playerDeckObject.transform.parent = mainCamera.transform; // Set the deck's parent to the mainCamera
        playerDeckObject.transform.localPosition = new Vector3(0f, -0.17f, 0.45f); // Set the deck's position to the stated value
        playerDeckObject.transform.localRotation = Quaternion.Euler(new Vector3(80f, -180f, 0f)); // Set the deck's rotation to the stated value
        playerDeckObject.transform.localScale = new Vector3(1, 1, 1); // Set the deck's scale to 1
        #endregion
        #region Create player card objects
        if (playerDeck.cards.Length > 0) // If the player's deck has cards
        {
            float cardSpacing = totalRange / (playerDeck.cards.Length - 1); // Calculate the space between each card
            float currentPos = totalRange / 2; // Calculate the current position for adding cards
            playerCardObjects = new List<GameObject>(); // Create a new list of gameObjects
            if (playerDeck.cards.Length > 3) // If the player's deck is more than 3 cards in length
            {
                foreach (Card card in playerDeck.cards) // Loop through every card in the player's deck
                {
                    playerCardObjects.Add(CreateCard(card, new Vector3(currentPos, 0f, 0f), playerDeckObject)); // Add a card object to the deck that is created by the CreateCard function
                    currentPos = currentPos - cardSpacing; // Move currentPos by cardSpacing
                }
            }
            #region Special cases for small decks
            else if (playerDeck.cards.Length == 3) // These below cases are for when the player's deck is too small for the above code to look correct
            { // They are simply a set of pre-defined positions for where each card should go
                playerCardObjects.Add(CreateCard(playerDeck.cards[0], new Vector3(-0.14f, 0, 0), playerDeckObject));
                playerCardObjects.Add(CreateCard(playerDeck.cards[1], new Vector3(0, 0, 0), playerDeckObject));
                playerCardObjects.Add(CreateCard(playerDeck.cards[2], new Vector3(0.14f, 0, 0), playerDeckObject));
            }
            else if (playerDeck.cards.Length == 2)
            {
                playerCardObjects.Add(CreateCard(playerDeck.cards[0], new Vector3(-0.075f, 0, 0), playerDeckObject));
                playerCardObjects.Add(CreateCard(playerDeck.cards[1], new Vector3(0.075f, 0, 0), playerDeckObject));
            }
            else if (playerDeck.cards.Length == 1)
            {
                playerCardObjects.Add(CreateCard(playerDeck.cards[0], new Vector3(0, 0, 0), playerDeckObject));
            }
            #endregion
        }
        else
        {
            Debug.Log("Player deck size is less than 1.");
        }
        #endregion
    }
    public GameObject CreateCard(Card card, Vector3 position, GameObject deck)
    {
        #region Create Instance of card
        GameObject output = Instantiate(cardPrefab, new Vector3(0, 0, 0), Quaternion.Euler(new Vector3(0, 0, 0))); // Create a new instance of a card object
        output.transform.parent = deck.transform; // Set the card's parent to the given deck
        output.transform.localPosition = position; // Set the card's position to position
        output.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, -10)); // Set the card's rotation to localRotation
        output.transform.localScale = new Vector3(1, 1, 1); // Set the card's scale to 1
        output.GetComponent<CardScript>().cardData = card; // Find and set the cardData component of the card to card
        MeshRenderer outputMesh = output.GetComponent<MeshRenderer>(); // Cache the meshrenderer of the card
        #endregion
        #region Swap card material for required material
        switch (card.materialName.ToLower()) // Choose which material needs to be loaded
        {
            default: // If no cases match the input
                Debug.Log("Card material '" + card.materialName + "' not recognised!"); // Inform the Unity console that the material does not exist
                break;
            case "reaper": // If the material is reaper
                outputMesh.material = cardMaterials[0]; // Set the material to material0
                break;
            case "phisch": // If the material is phisch
                outputMesh.material = cardMaterials[1]; // Set the material to material1
                break;
            case "dusk":
                outputMesh.material = cardMaterials[2];
                break;
            case "rock":
                outputMesh.material = cardMaterials[3];
                break;
        }
        #endregion
        #region Load card details
        try // Try to run the below code
        {
            output.transform.GetChild(0).GetComponent<TextMeshPro>().text = card.health.ToString(); // load health
            output.transform.GetChild(1).GetComponent<TextMeshPro>().text = card.attack.ToString(); // load attack
            output.transform.GetChild(2).GetComponent<TextMeshPro>().text = card.cost.ToString(); // load cost
            output.transform.GetChild(3).GetComponent<TextMeshPro>().text = card.name; // load name
            output.transform.GetChild(4).GetComponent<TextMeshPro>().text = card.description; // load description
        }
        catch // If the above code fails to run
        {
            Debug.Log("Card details for " + card.name + " are missing!"); // Inform the Unity console that something went wrong
        }
        #endregion
        return output; // Return the created card
    }
    public Card CreateCardData(string cardName)
    {
        Card outputCard = new Card(); // Create a new card
        #region Load card data based on name
        switch (cardName.ToLower()) // Create card data based on the provided name
        {
            default: // If no names match
                Debug.Log("Card name not recognised."); // Inform the Unity console that the card does not exist
                break;
            case "reaper": // If the name is reaper
                outputCard.name = "Reaper";
                outputCard.description = "25% chance to deal a critical hit when attacking.";
                outputCard.materialName = "Reaper";
                outputCard.health = 2;
                outputCard.cost = 2;
                outputCard.attack = 2;
                outputCard.state = 0;
                break;
            case "phisch": // If the name is phisch
                outputCard.name = "Phisch";
                outputCard.description = "Appreciates in price by one chip every turn.";
                outputCard.materialName = "Phisch";
                outputCard.health = 1;
                outputCard.cost = 0;
                outputCard.attack = 0;
                outputCard.state = 0;
                break;
            case "dusk": // If the name is dusk
                outputCard.name = "Dusk";
                outputCard.description = "10% chance to reflect damage when being hit.";
                outputCard.materialName = "Dusk";
                outputCard.health = 3;
                outputCard.cost = 3;
                outputCard.attack = 1;
                outputCard.state = 0;
                break;
            case "rock": // If the name is rock
                outputCard.name = "Rock";
                outputCard.description = "Immune to critical hits.";
                outputCard.materialName = "Rock";
                outputCard.health = 6;
                outputCard.cost = 2;
                outputCard.attack = 0;
                outputCard.state = 0;
                break;
        }
        #endregion
        return outputCard; // Return the created card
    }
    public Deck PopulateDeck(Deck fullDeck, int handSize, Deck currentDeck) // !!!Need to add size and deck adding checks for if the card is already on the board
    {
        int l = 0; // Create l and set it to 0
        foreach (Card card in fullDeck.cards) // Loop through every card in fullDeck
        {
            if (card.state != 1 || card.state != 2) // If the card is not in the player's hand or on the board
            {
                l++; // Add 1 to l
            }
        }
        if (l >= currentDeck.cards.Length) // If l is larger than or equal to the currentDeck max size
        {
            System.Random random = new System.Random(); // Create a new random
            List<int> usedIndexes = new List<int>(); // Create a list of used indexes
            for (int i = 0; i < currentDeck.cards.Length; i++) // Loop through every index of currentDeck
            {
                if (currentDeck.cards[i] == null) // If there is a card missing
                {
                    // add a card
                    int index = random.Next(fullDeck.cards.Length); // Randomize a new index
                    while (usedIndexes.Contains(index) || (fullDeck.cards[index].state == 1 || fullDeck.cards[index].state == 2)) // Loop until a unique index is found
                    {
                        index = random.Next(fullDeck.cards.Length); // Randomize a new index
                    }
                    currentDeck.cards[i] = fullDeck.cards[index]; // Add the selected card to currentDeck
                    fullDeck.cards[index].state = 1;
                    usedIndexes.Add(index); // Add the index to usedIndexes
                }
            }
        }
        else // If the current deck is larger than l
        {
            Debug.Log("Cannot fully populate deck when fullDeck is less than the currentDeck!"); // Inform the Unity console that something has gone very wrong
            int i = 0;
            Card[] temp = new Card[fullDeck.cards.Length]; // Code below here simply fills in currentDeck with every card that is left in fullDeck
            for (int k = 0; k < currentDeck.cards.Length; k++)
            {
                if (currentDeck.cards[k] != null)
                {
                    temp[k] = currentDeck.cards[k];
                    i = k;
                }
            }
            for (int j = 0; j < fullDeck.cards.Length; j++)
            {
                if (fullDeck.cards[j].state != 1 || fullDeck.cards[j].state != 2)
                {
                    i++;
                    temp[i] = fullDeck.cards[j];
                    fullDeck.cards[j].state = 1;
                }
            }
        }
        return currentDeck; // Return currentDeck
    }
    private void UpdateCurrency(int playerCurrency, int enemyCurrency) // Finds the enemy and player currency text and updates them with provided values
    {
        cardBattleHUD.transform.Find("PlayerInfo").Find("Currency").Find("CurrencyText").GetComponent<TextMeshProUGUI>().text = playerCurrency.ToString();
        cardBattleHUD.transform.Find("EnemyInfo").Find("Currency").Find("CurrencyText").GetComponent<TextMeshProUGUI>().text = enemyCurrency.ToString();
    }
    private void SetPlayerPortrait(string name)
    {
        SetPortrait(name, "PlayerInfo"); // Set the player's portrait to the portrait of name
    }
    private void SetEnemyPortrait(string name)
    {
        SetPortrait(name, "EnemyInfo"); // Set the enemy's portrait to the portrait of name
    }
    private void SetPortrait(string name, string parentName)
    {
        bool success = false; // Create succcess and make it false
        foreach (Sprite portrait in characterPortraits) // Loop through every portrait in the array of portraits
        {
            if (portrait.name == name) // If the name of the portrait matches name
            {
                Transform sprite = cardBattleHUD.transform.Find(parentName).Find("Icon"); // Find the icon that needs changing
                if (sprite != null) // If the icon was found
                {
                    sprite.GetComponent<Image>().sprite = portrait; // Set the icon to the correct portrait
                    success = true; // Set success to true
                }
                else // If the icon was not found
                {
                    Debug.Log("Unable to find icon for " + parentName); // Inform the Unity console that something went wrong
                }
            }
        }
        if (!success) // If the script failed in any way
        {
            Debug.Log("Unable to find requested portrait! (" + name + ")"); // Inform the Unity console that the portrait could not be found
            cardBattleHUD.transform.Find(parentName).Find("Icon").GetComponent<Image>().sprite = defaultPortrait; // Set the icon to the default icon
        }
    }
}
