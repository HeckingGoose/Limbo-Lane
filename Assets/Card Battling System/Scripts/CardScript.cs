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
    public Card cardData;
    private void Start()
    {
        mainCamera = Camera.main.gameObject; // Find main camera in scene
        direction = mainCamera.transform.position - this.transform.position; // Calculate direction vector between camera and self
        direction = direction.normalized; // Normalize direction vector
        direction = new Vector3(direction.x * magnitude, direction.y * magnitude, direction.z * magnitude); // Scale direction vector to required magnitude
        ogPos = this.transform.position;
        newPos = ogPos + direction; // Define other vectors for places the card can move to
        ogRot = this.transform.rotation;
        newRot = Quaternion.Euler(new Vector3(0, -90f, 0));
        zeroRot = new Quaternion();
        boxCollider = this.GetComponent<BoxCollider>(); // Store reference to box collider
    }
    private void OnMouseEnter()
    {
        if (!selected) // If the card is not in its selected state
        {
            this.transform.position = newPos; // Move the card to its mouse hover position
            this.transform.rotation = zeroRot; // Rotate the card to 0 rotation on all axis
        }
        mouseIsOver = true; // Set mouseIsOver to true
    }
    private void OnMouseExit()
    {
        if (!selected) // If the card is not in its selected state
        {
            this.transform.position = ogPos; // Move the card to its original position
            this.transform.rotation = ogRot; // Rotate the card to its original rotation
        }
        mouseIsOver = false; // Set mouseIsOver to false
    }
    public void Select() // Called when card is selected
    {
        this.transform.position = newPos + transform.forward * -magnitude * 1/4; // Set the card's position to its mouse hover position + 1/4 of the required magnitude on the card's forward axis
        this.transform.rotation = newRot; // Rotate the card to its new rotation
        selected = true; // Set selected to true
        boxCollider.enabled = false; // Disable the card's collider
    }
    public void Unselect() // Called when another card is selected or screen is clicked elsewhere
    {
        this.transform.position = ogPos; // Set the card's position to its original position
        this.transform.rotation = ogRot; // Rotate the card to its original rotation
        selected = false; // Set selected to false
        boxCollider.enabled = true; // Enable the card's collider
    }
}
