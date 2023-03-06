using UnityEngine;

public class DeckPileScript : MonoBehaviour
{
    [SerializeField]
    private GameObject cardPrefab;

    [HideInInspector]
    public bool mouseOver = false;
    private Deck freeDeck; // Setup variables
    private void OnMouseOver() // When mouse is over the card
    {
        mouseOver = true; // Set mouse over to true
    }
    private void OnMouseExit() // When mouse is not over the card
    {
        mouseOver = false; // Set mouse over to false
    }
    public void RePile(Deck fullDeck, GameObject pile)
    {
        int freeCards = 0;
        if (fullDeck != null) // If fullDeck exists
        {
            freeDeck = new Deck(); // Create a new deck
            foreach (Card card in fullDeck.cards) // Loop through each card in fullDeck
            {
                if (card.state == 0) // If the card is free
                {
                    freeCards++; // Increment freeCards
                }
            }
            freeDeck.cards = new Card[freeCards]; // Set freecards to be a deck of size freeCards
            int j = 0;
            for (int i = 0; i < fullDeck.cards.Length; i++) // Loop through fullDeck again
            {
                if (fullDeck.cards[i].state == 0) // If the card is free
                {
                    freeDeck.cards[j] = fullDeck.cards[i]; // Add it to freeDeck
                    j++;
                }
            }
        }
        else // Otherwise
        {
            freeDeck = new Deck();
            freeDeck.cards = new Card[0]; // Create an empty array of free Cards
        }
    }
    public void GeneratePile(GameObject pile, Material[] cardMaterials)
    {
        GameObject topCard = null;
        try // Try to run the below code
        {
            topCard = pile.transform.GetChild(0).gameObject; // Find the child of this object
        }
        catch { }
        if (topCard != null) // If there was a child
        {
            GameObject.Destroy(topCard); // Destroy the child
        }
        if (freeDeck.cards.Length != 0) // If there are free cards
        {
            // find correct card material
            Material correctMaterial = cardMaterials[0]; // Set the correctMaterial to the 0th material in cardMaterials
            foreach (Material material in cardMaterials) // Loop through every material in cardMaterials
            {
                if (material.name == freeDeck.cards[0].materialName) // If the material names match
                {
                    correctMaterial = material; // Set the correct material to the current material
                }
            }
            topCard = Instantiate(cardPrefab, new Vector3(), new Quaternion()); // Instantiate a new card and load the correct values for the card
            CardScript topCardScript = topCard.GetComponent<CardScript>();
            // Implement loading name, attack, etc values of card
            topCard.transform.parent = pile.transform;
            topCard.transform.localPosition = new Vector3();
            topCard.transform.localRotation = new Quaternion();
            topCard.GetComponent<MeshRenderer>().material = correctMaterial;
            Destroy(topCard.GetComponent<CardScript>());
        }
        else // Otherwise
        {
            Debug.Log("FreeDeck is empty!"); // Inform the Unity console that something went wrong
        }
    }
    public Card DrawCard(Deck fullDeck, GameObject pile)
    {
        RePile(fullDeck, pile); // Repile <- I love that word for some reason
        Card returnCard = null;
        if (freeDeck.cards.Length != 0) // If there are free cards
        {
            returnCard = freeDeck.cards[0]; // Pick the first free card
            foreach (Card card in fullDeck.cards) // Loop through fullDeck
            {
                if (card.ID == freeDeck.cards[0].ID) // If the card is found in fullDeck
                {
                    card.state = 1; // Set its state to 1
                }
            }
            RePile(fullDeck, pile); // REPILE again
        }
        return returnCard; // Return the card
    }
}
