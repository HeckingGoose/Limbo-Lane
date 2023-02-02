using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyScript : MonoBehaviour
{
    #region Public variables for main script
    public int health;
    public string enemyName;
    public Deck fullDeck;
    [HideInInspector]
    public int currency;
    [HideInInspector] // Setup variables that mainScript can see for handling card battle
    public bool ready = false;
    #endregion
    [SerializeField]
    private MainCardBattleHandler mainScript;
    [SerializeField]
    private GameObject boardObject;
    private int maxHealth;
    private int handSize;
    private Deck deck;
    private GameObject deckObject;
    private List<GameObject> cardObjects;
    private System.Random random;

    private void Start()
    {
        random = new System.Random();
        switch (enemyName.ToLower()) // Pick which enemy data to load
        {
            default: // If enemyName matches none of the cases
                Debug.Log("Enemy name not recognised!"); // Inform the Unity console that something went wrong
                break;
            case "dawn": // If the enemyName is dawn
                currency = 5;
                health = 10;
                handSize = 3;
                fullDeck = new Deck();
                deck = new Deck();
                deck.cards = new Card[handSize];
                fullDeck.cards = new Card[10];
                fullDeck.cards[0] = mainScript.CreateCardData("Reaper");
                fullDeck.cards[1] = mainScript.CreateCardData("Reaper");
                fullDeck.cards[2] = mainScript.CreateCardData("Reaper");
                fullDeck.cards[3] = mainScript.CreateCardData("Dusk"); // Setup values and deck for character
                fullDeck.cards[4] = mainScript.CreateCardData("Dusk");
                fullDeck.cards[5] = mainScript.CreateCardData("Dusk");
                fullDeck.cards[6] = mainScript.CreateCardData("Rock");
                fullDeck.cards[7] = mainScript.CreateCardData("Rock");
                fullDeck.cards[8] = mainScript.CreateCardData("Rock");
                fullDeck.cards[9] = mainScript.CreateCardData("Dusk");
                deck = mainScript.PopulateDeck(fullDeck, handSize, deck);
                GenerateDeck();
                break;
        }
        maxHealth = health; // Set maxHealth equal to health
        ready = true; // Set ready to true
    }
    private void GenerateDeck()
    {
        #region Destroy old deck
        try
        {
            GameObject.Destroy(GameObject.Find("EnemyDeck"));
        }
        catch
        {
            Debug.Log("Unable to find EnemyDeck");
        }
        #endregion
        #region Create new deck
        deckObject = new GameObject();
        deckObject.transform.parent = this.transform;
        deckObject.transform.localPosition = new Vector3(0, 0, 0);
        deckObject.name = "EnemyDeck";
        deckObject.transform.localScale = new Vector3(1, 1, 1);
        deckObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
        #endregion
        #region Populate deck
        if (deck.cards.Length > 0) // If the player's deck has cards
        {
            float cardSpacing = 0.6f / (deck.cards.Length - 1); // Calculate the space between each card
            float currentPos = 0.6f / 2; // Calculate the current position for adding cards
            cardObjects = new List<GameObject>(); // Create a new list of gameObjects
            if (deck.cards.Length > 3) // If the player's deck is more than 3 cards in length
            {
                foreach (Card card in deck.cards) // Loop through every card in the player's deck
                {
                    cardObjects.Add(mainScript.CreateCard(card, new Vector3(currentPos, 0f, 0f), deckObject)); // Add a card object to the deck that is created by the CreateCard function
                    cardObjects[cardObjects.Count - 1].GetComponent<CardScript>().Disable();
                    currentPos = currentPos - cardSpacing; // Move currentPos by cardSpacing
                }
            }
            #region Special cases for small decks
            else if (deck.cards.Length == 3) // These below cases are for when the player's deck is too small for the above code to look correct
            { // They are simply a set of pre-defined positions for where each card should go
                cardObjects.Add(mainScript.CreateCard(deck.cards[0], new Vector3(-0.14f, 0, 0), deckObject));
                cardObjects.Add(mainScript.CreateCard(deck.cards[1], new Vector3(0, 0, 0), deckObject));
                cardObjects.Add(mainScript.CreateCard(deck.cards[2], new Vector3(0.14f, 0, 0), deckObject));
                cardObjects[0].GetComponent<CardScript>().Disable();
                cardObjects[1].GetComponent<CardScript>().Disable();
                cardObjects[2].GetComponent<CardScript>().Disable();
            }
            else if (deck.cards.Length == 2)
            {
                cardObjects.Add(mainScript.CreateCard(deck.cards[0], new Vector3(-0.075f, 0, 0), deckObject));
                cardObjects.Add(mainScript.CreateCard(deck.cards[1], new Vector3(0.075f, 0, 0), deckObject));
                cardObjects[0].GetComponent<CardScript>().Disable();
                cardObjects[1].GetComponent<CardScript>().Disable();
            }
            else if (deck.cards.Length == 1)
            {
                cardObjects.Add(mainScript.CreateCard(deck.cards[0], new Vector3(0, 0, 0), deckObject));
                cardObjects[0].GetComponent<CardScript>().Disable();
            }
            #endregion
        }
        else
        {
            Debug.Log("Enemy deck size is less than 1.");
        }
        #endregion
    }
    public GameObject[ , ] ComputeTurn(GameObject[ , ] board) // Can be called externally when it is enemy's turn
    {
        switch (enemyName.ToLower()) // Pick which enemy AI needs to be used
        {
            default: // If enemyName matches none of the below cases
                deck = mainScript.PopulateDeck(fullDeck, handSize, deck);
                GenerateDeck(); // Re-generate the deck
                // v Create a new Card that is randomly picked from the enemy's deck v
                bool richEnough = false;
                List<int> canAffordIndexes = new List<int>();
                for (int i = 0; i < deck.cards.Length; i++) // Loop through every card in deck
                {
                    if (deck.cards[i].cost <= currency) // If the card's cost is less than the enemy's currency
                    {
                        richEnough = true; // Tell the game that the enemy is rich enough to place a card
                        canAffordIndexes.Add(i);
                    }
                }
                int cardIndex = random.Next(0, cardObjects.Count); // Set CardIndex to a random card within the enemy's deck
                if (richEnough) // If the enemy is rich-enough to place a card
                {
                    cardIndex = canAffordIndexes[random.Next(0, canAffordIndexes.Count)]; // Pick a random index in the enemy's deck
                    int freeSpaces = 0; // Set freeSpaces to 0
                    List<(int, int)> freeSpacesList = new List<(int, int)>();
                    for (int i = 0; i < 2; i++) // Loop through every row on the enemy's side of the board
                    {
                        for (int k = 0; k < 4; k++) // Loop through every column on the enemy's side of the board
                        {
                            if (board[i, k] == null) // If the space is empty
                            {
                                freeSpaces++; // Add 1 to freeSpaces
                                freeSpacesList.Add((i, k));
                            }
                        }
                    }
                    if (freeSpaces > 0) //If there are more than 0 free spaces
                    {
                        (int, int) space = freeSpacesList[random.Next(0, freeSpacesList.Count)];
                        int boardx = space.Item1;
                        int boardy = space.Item2;
                        board[boardx, boardy] = cardObjects[cardIndex]; // Place the card on the board object
                        PlaceCardOnBoard(cardObjects[cardIndex], boardy, boardx, board); // Place card on board
                        currency -= cardObjects[cardIndex].GetComponent<CardScript>().cardData.cost; // Deduct cost of card from currency
                        mainScript.UpdateCurrency(mainScript.playerCurrency, currency); // Update the enemy and player's currency
                        cardObjects[cardIndex] = null; // need some way to track card to say is on board or not on board from perspective of fulldeck
                        foreach (Card card in fullDeck.cards)
                        {
                            if (card.ID == deck.cards[cardIndex].ID)
                            {
                                card.state = 2;
                            }
                        }
                        deck.cards[cardIndex] = null; // Remove reference to card from deck
                        Deck temp = new Deck();
                        temp.cards = deck.cards;
                        deck = new Deck();
                        deck.cards = new Card[temp.cards.Length - 1];
                        int i = 0;
                        foreach (Card card in temp.cards) // Sort out deck to remove any null values
                        {
                            if (card != null)
                            {
                                deck.cards[i] = card;
                                i++;
                            }
                        }
                    }
                    else // If there are no free spaces
                    {
                        deck = DrawPhisch(deck); // Draw a phisch
                    }
                }
                else // If the player is not rich enough
                {
                    deck = DrawPhisch(deck); // Draw a phisch
                }
                //GenerateDeck(); // Re-generate the deck
                return board; // Return the board
        }
    }
    private Deck DrawPhisch(Deck deck)
    {
        Deck temp = deck;
        deck = new Deck();
        deck.cards = new Card[temp.cards.Length + 1]; // Add a phisch to an array of size deck + 1, and then set deck equal to that
        for (int i = 0; i < temp.cards.Length; i++)
        {
            deck.cards[i] = temp.cards[i];
        }
        deck.cards[deck.cards.Length - 1] = mainScript.CreateCardData("Phisch");
        deck.cards[deck.cards.Length - 1].state = 1;
        Debug.Log("Enemy drew phisch");
        return deck;
    }
    public void UpdateHealth(GameObject cardBattleHUD)
    {
        float h = health;
        float maxH = maxHealth; // Find and update the text and bar representing the enemy health
        cardBattleHUD.transform.Find("EnemyInfo").Find("Health").Find("HealthText").GetComponent<TextMeshProUGUI>().text = health.ToString();
        cardBattleHUD.transform.Find("EnemyInfo").Find("Health").Find("Health").GetComponent<Image>().fillAmount = h / maxH;
    }
    private void PlaceCardOnBoard(GameObject card, int boardx, int boardy, GameObject[ , ] boardArray)
    {
        int row = 0; // Set row to 0
        GameObject frontRow = boardObject.transform.Find("Opponent").Find("FrontRow").gameObject;
        GameObject backRow = boardObject.transform.Find("Opponent").Find("BackRow").gameObject; // Cache the front and back row
        for (int i = 0; i < boardArray.Length; i++) // Loop through every index in board array
        {
            if (i - (row * 4) == 4) // Bump row by 1 every time i reaches a multiple of 4
            {
                row++;
            }
            if (boardArray[row, i - (row * 4)] != null && row == boardy && (i - (row * 4)) == boardx) // If the space matches the randomized space
            {
                switch (row) // Pick which row is currently being looked at
                {
                    default: // If none of the below cases match
                        Debug.Log("Row " + row.ToString() + " not relevant."); // Inform the Unity console that something has gone wrong
                        break;
                    case 0: // If row is 0
                        switch (i - (row * 4)) // Back row
                        { // Pick a case based on which column it is in
                            default: // If none of the below cases match
                                Debug.Log("i is larger than expected ! (" + (i - (row * 4)).ToString() + ")"); // Inform the Unity console that something went wrong
                                break;
                            case 0: // If column is 0
                                card.transform.parent = backRow.transform.Find("Card4").Find("13");
                                card.transform.localPosition = new Vector3(0, 0, -0.01f); // Update the card's parent to be the listed object
                                break;
                            case 1: // If the column is 1
                                card.transform.parent = backRow.transform.Find("Card3").Find("12");
                                card.transform.localPosition = new Vector3(0, 0, -0.01f); // Update the card's parent to be the listed object
                                break;
                            case 2: // If the column is 2
                                card.transform.parent = backRow.transform.Find("Card2").Find("11");
                                card.transform.localPosition = new Vector3(0, 0, -0.01f); // Update the card's parent to be the listed object
                                break;
                            case 3: // If the column is 3
                                card.transform.parent = backRow.transform.Find("Card1").Find("10");
                                card.transform.localPosition = new Vector3(0, 0, -0.01f); // Update the card's parent to be the listed object
                                break;
                        }
                        break;
                    case 1: // If row is 1
                        switch (i - (row * 4)) // Front row
                        { // Pick a case based on which column it is in
                            default: // If none of the below cases match
                                Debug.Log("i is larger than expected ! (" + (i - (row * 4)).ToString() + ")"); // Inform the Unity console that something went wrong
                                break;
                            case 0: // If the column is 0
                                card.transform.parent = frontRow.transform.Find("Card4").Find("03");
                                card.transform.localPosition = new Vector3(0, 0, -0.01f); // Update the card's parent to be the listed object
                                break;
                            case 1: // If the column is 1
                                card.transform.parent = frontRow.transform.Find("Card3").Find("02");
                                card.transform.localPosition = new Vector3(0, 0, -0.01f); // Update the card's parent to be the listed object
                                break;
                            case 2: // If the column is 2
                                card.transform.parent = frontRow.transform.Find("Card2").Find("01");
                                card.transform.localPosition = new Vector3(0, 0, -0.01f); // Update the card's parent to be the listed object
                                break;
                            case 3: // If the column is 3
                                card.transform.parent = frontRow.transform.Find("Card1").Find("00");
                                card.transform.localPosition = new Vector3(0, 0, -0.01f); // Update the card's parent to be the listed object
                                break;
                        }
                        break;
                }
            }
        }
        card.transform.localRotation = Quaternion.Euler(new Vector3(-90, 0, 0)); // Set the card's rotation to match the board
    }
}
