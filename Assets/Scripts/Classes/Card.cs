[System.Serializable] // Tell Unity that this is a class
public class Card
{
    public int health;
    public int attack;
    public int abilityType; // Define the required variables
    public int ability;
    public int canBeDamaged;
    public int cost;
    public static Card Create(int health, int attack, int abilityType, int ability, int canBeDamaged, int cost) // Tell Unity which variables the class uses
    {
        Card card = new Card();
        card.health = health;
        card.attack = attack;
        card.abilityType = abilityType; // Create and return an instance of itself
        card.ability = ability;
        card.canBeDamaged= canBeDamaged;
        card.cost = cost;
        return card;
    }
}
