using TMPro;
using UnityEngine;

public class CardScript : MonoBehaviour
{
    private GameObject mainCamera;
    [HideInInspector]
    public bool mouseIsOver = false; // Used by other scripts to check if mouse is over a card without doing raycasts in that script
    private bool selected = false;
    private Vector3 ogPos;
    private Vector3 newPos;
    private Quaternion ogRot;
    private Quaternion newRot;  // Define variables
    private Quaternion zeroRot;
    private Vector3 direction;
    private float magnitude = 0.1f;
    private BoxCollider boxCollider;
    public bool disabled = false;
    public Card cardData;
    private CardScript cardScript;

    private int attackState = 0;
    private float animTimer = 0;
    private Vector3 animOgPos; // Variables for handling the attack animation
    private Vector3 animLastPos;

    private float nadaTimer = 0;
    private bool showingNada = false;
    [SerializeField]
    private GameObject nadaPrefab; // Variables for handling the NADA particle
    private GameObject nada = null;
    private bool showing = false;

    private GameObject slapSprite; // Variables for handlign the slap sprite

    private void Update()
    {
        if (showingNada) // If the NADA object needs to be handled
        {
            ShowNada(); // Run the method to handle NADA
        }
        switch (showing) // Choose whether slap sprite is to be shown or not showing
        {
            case true: // show slap sprite
                if (slapSprite.activeSelf != true) // If the sprite is not active
                {
                    slapSprite.SetActive(true); // Set it to active
                }
                break;
            case false: // hide slap sprite
                if (slapSprite.activeSelf != false) // If the sprite is not inactive
                {
                    slapSprite.SetActive(false); // Set the sprite to be inactive
                }
                break;
        }
    }
    private void LateUpdate() // Runs after main update
    {
        showing = false; // Reset showing to false for next frame
    }

    private void Start()
    {
        cardScript = this.GetComponent<CardScript>();
        mainCamera = Camera.main.gameObject; // Find main camera in scene
        direction = mainCamera.transform.position - this.transform.position; // Calculate direction vector between camera and self
        direction = direction.normalized; // Normalize direction vector
        direction = new Vector3(direction.x * magnitude, direction.y * magnitude, direction.z * magnitude); // Scale direction vector to required magnitude
        ogPos = this.transform.position;
        newPos = ogPos + direction; // Define other vectors for places the card can move to
        ogRot = this.transform.rotation;
        newRot = Quaternion.Euler(new Vector3(0, 0, 0));
        zeroRot = new Quaternion();
        boxCollider = this.GetComponent<BoxCollider>(); // Store reference to box collider
        slapSprite = this.transform.Find("SlapSprite").gameObject;
    }
    private void OnMouseEnter()
    {
        if (!selected && !disabled) // If the card is not in its selected state
        {
            this.transform.position = newPos; // Move the card to its mouse hover position
            this.transform.rotation = zeroRot; // Rotate the card to 0 rotation on all axis
        }
        mouseIsOver = true; // Set mouseIsOver to true
    }
    private void OnMouseExit()
    {
        if (!selected && !disabled) // If the card is not in its selected state
        {
            this.transform.position = ogPos; // Move the card to its original position
            this.transform.rotation = ogRot; // Rotate the card to its original rotation
        }
        mouseIsOver = false; // Set mouseIsOver to false
    }
    public void Select() // Called when card is selected
    {
        if (!disabled)
        {
            this.transform.position = newPos + transform.forward * -magnitude * 1 / 4; // Set the card's position to its mouse hover position + 1/4 of the required magnitude on the card's forward axis
            this.transform.rotation = newRot; // Rotate the card to its new rotation
            selected = true; // Set selected to true
            boxCollider.enabled = false; // Disable the card's collider
        }
    }
    public void Unselect() // Called when another card is selected or screen is clicked elsewhere
    {
        if (!disabled)
        {
            this.transform.position = ogPos; // Set the card's position to its original position
            this.transform.rotation = ogRot; // Rotate the card to its original rotation
            selected = false; // Set selected to false
            boxCollider.enabled = true; // Enable the card's collider
        }
    }
    public void Disable() // Called when all other functions need to be ignored
    {
        disabled = true;
    }
    public void Enable() // Called when all other functions need to be re-enabled
    {
        disabled = false;
    }
    public void UpdateHealh() // Updates card health text
    {
        this.transform.Find("Health").GetComponent<TextMeshPro>().text = cardScript.cardData.health.ToString(); // Find health text object and set it equal to the card's health as a string
    }
    public void UpdateCost() // Updates card cost text
    {
        this.transform.Find("Cost").GetComponent<TextMeshPro>().text = cardScript.cardData.cost.ToString(); // Find the cost text object and set it equal to the card's cost as a string
    }
    public void UpdateAttack() // Updates card attack value
    {
        this.transform.Find("Attack").GetComponent<TextMeshPro>().text = cardScript.cardData.attack.ToString(); // Find the attack text object and set it equal to the card's attack
    }
    public int DoAttack() // Function for handling attack animation
    {
        switch (attackState) // Pick which step the animation is currently in
        {
            default: // If the step is not recognised
                Debug.Log("AttackState " + attackState + " on card " + this.cardData.name + " not recognised. Skipping to -1."); // Inform the Unity console that something went wrong
                attackState = -1; // Set attackState to -1
                return 0; // Implies script is not done
            case -1: // If animation has just ended
                attackState = 0;
                animTimer = 0; // Reset variables to their default values
                this.transform.localPosition = animOgPos; // Move the card back to its original position
                animOgPos = new Vector3();
                animLastPos = new Vector3();
                return 1; // Implies script is done
            case 0: // First step of the animation, lifts card up
                animTimer += Time.deltaTime; // Add the current deltaTime to the timer
                if (animTimer == Time.deltaTime || animTimer == 0) // If this is the first frame
                {
                    animOgPos = this.transform.localPosition; // Store the card's position as animOgPos
                }
                else if (animTimer >= 0.2f) // If 0.2 seconds have passed
                {
                    animTimer = 0; // Reset the timer
                    attackState = 1; // Move to the next state
                    this.transform.localPosition = animOgPos + new Vector3(0, 0.04f, -0.03f); // Move the card to the target position
                    animLastPos = this.transform.localPosition; // Store this position as animLastPos
                }
                this.transform.localPosition = Vector3.Lerp(animOgPos, animOgPos + new Vector3(0, 0.04f, -0.03f), animTimer / 0.2f); // Interpolate the card between ogPos and the new pog over 0.2s
                return 0; // Implies script is not done
            case 1: // Second step of the animation, throws card forward
                animTimer += Time.deltaTime; // Add deltaTime to the timer
                if (animTimer >= 0.1f) // If 0.1 seconds have passed
                {
                    animTimer = 0; // Reset the timer
                    attackState = 2; // Move to the next state
                    this.transform.localPosition = animLastPos + new Vector3(0, -0.1f, 0); // Set the card to its target position
                    animLastPos = this.transform.localPosition; // Store this position as animLastPof
                }
                this.transform.localPosition = Vector3.Lerp(animLastPos, animLastPos + new Vector3(0, -0.1f, 0), animTimer / 0.1f); // Interpolate the card between its last position and a new position over 0.1s
                return 0; // Implies script is not done
            case 2: // Third step of the animation, returns card to og position
                animTimer += Time.deltaTime; // Add deltaTime to timer
                if (animTimer >= 0.3f) // If 0.3 seconds have passed
                {
                    animTimer = 0; // Reset the timer
                    attackState = -1; // Move to the last state
                    this.transform.localPosition = animOgPos; // Move the card to its original position
                }
                this.transform.localPosition = Vector3.Lerp(animLastPos, animOgPos, animTimer / 0.3f); // Interpolate the card between its last position and its original position
                return 0;  // Implies script is not done
        }
    }
    public void ShowNada() // Function for spawing and raising NADA, then destroying it after set amount of time
    {
        switch (nadaTimer) // Pick which state the function is in based on time passed
        {
            case 0: // If this is the first loop
                showingNada = true; // Ensure that the function is looped
                nada = Instantiate(nadaPrefab, new Vector3(), Quaternion.Euler(new Vector3(180 - mainCamera.transform.eulerAngles.x, mainCamera.transform.eulerAngles.y, mainCamera.transform.eulerAngles.z + 180))); // Spawn NADA object
                nada.transform.parent = this.transform; // Parent nada to the card
                nada.transform.localPosition = this.transform.localPosition + new Vector3(0, 0.07f, 0); // Set the position of nada to the card + 0.07y
                nadaTimer += Time.deltaTime; // Increment the timer
                break;
            default: // If this is not the first or last loop
                nadaTimer += Time.deltaTime; // Increment the timer
                nada.transform.localPosition = Vector3.Lerp(this.transform.localPosition + new Vector3(0, 0.07f, 0), this.transform.localPosition + new Vector3(0, 0.14f, 0), nadaTimer / 1); // Interpolate the card between it's original position and a new position 0.07y away
                if (nadaTimer > 0.6f) // If the timer is more than 0.6
                {
                    nadaTimer = 0.6f; // Set the timer to 0.6
                }
                break;
            case 0.6f: // If this is the last loop
                showingNada = false; // Ensure the function is no londer looped
                GameObject.Destroy(nada); // Destroy the nada object
                nada = null; // Set the reference to the nada object to null
                nadaTimer = 0; // Reset the timer
                break;
        }
    }
    public void HandleSlapSprite(bool show) // Function for setting whether slap sprite is shown
    {
        showing = show; // Set showing to the value of show
    }
}
