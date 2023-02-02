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
    private Vector3 animOgPos;
    private Vector3 animLastPos;

    private float nadaTimer = 0;
    private bool showingNada = false;
    [SerializeField]
    private GameObject nadaPrefab;
    private GameObject nada = null;

    private void Update()
    {
        if (showingNada)
        {
            ShowNada();
        }
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
        newRot = Quaternion.Euler(new Vector3(0, -90f, 0));
        zeroRot = new Quaternion();
        boxCollider = this.GetComponent<BoxCollider>(); // Store reference to box collider
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
    public void UpdateHealh()
    {
        this.transform.Find("Health").GetComponent<TextMeshPro>().text = cardScript.cardData.health.ToString();
    }
    public void UpdateCost()
    {
        this.transform.Find("Cost").GetComponent<TextMeshPro>().text = cardScript.cardData.cost.ToString();
    }
    public void UpdateAttack()
    {
        this.transform.Find("Attack").GetComponent<TextMeshPro>().text = cardScript.cardData.attack.ToString();
    }
    public int DoAttack()
    {
        switch (attackState)
        {
            default:
                Debug.Log("AttackState " + attackState + " on card " + this.cardData.name + " not recognised. Skipping to -1.");
                animOgPos = this.transform.localPosition;
                attackState = -1;
                return 0;
            case -1:
                attackState = 0;
                animTimer = 0;
                this.transform.localPosition = animOgPos;
                animOgPos = new Vector3();
                animLastPos = new Vector3();
                return 1;
            case 0: // Lift card up
                animTimer += Time.deltaTime;
                if (animTimer == Time.deltaTime || animTimer == 0)
                {
                    animOgPos = this.transform.localPosition;
                }
                else if (animTimer >= 0.2f)
                {
                    animTimer = 0;
                    attackState = 1;
                    this.transform.localPosition = animOgPos + new Vector3(0, 0.04f, -0.03f);
                    animLastPos = this.transform.localPosition;
                }
                this.transform.localPosition = Vector3.Lerp(animOgPos, animOgPos + new Vector3(0, 0.04f, -0.03f), animTimer / 0.2f);
                return 0;
            case 1: // Throw card forward
                animTimer += Time.deltaTime;
                if (animTimer >= 0.1f)
                {
                    animTimer = 0;
                    attackState = 2;
                    this.transform.localPosition = animLastPos + new Vector3(0, -0.1f, 0);
                    animLastPos = this.transform.localPosition;
                }
                this.transform.localPosition = Vector3.Lerp(animLastPos, animLastPos + new Vector3(0, -0.1f, 0), animTimer / 0.1f);
                return 0;
            case 2: // Return card to og pos
                animTimer += Time.deltaTime;
                if (animTimer >= 0.3f)
                {
                    animTimer = 0;
                    attackState = -1;
                    this.transform.localPosition = animOgPos;
                }
                this.transform.localPosition = Vector3.Lerp(animLastPos, animOgPos, animTimer / 0.3f);
                return 0;
        }
    }
    public void ShowNada()
    {
        switch (nadaTimer)
        {
            case 0: // First loop
                showingNada = true;
                nada = Instantiate(nadaPrefab, new Vector3(), Quaternion.Euler(new Vector3(180 - mainCamera.transform.eulerAngles.x, mainCamera.transform.eulerAngles.y, mainCamera.transform.eulerAngles.z + 180)));
                nada.transform.parent = this.transform;
                nada.transform.localPosition = this.transform.localPosition + new Vector3(0, 0.07f, 0);
                nadaTimer += Time.deltaTime;
                break;
            default: // Currently running
                nadaTimer += Time.deltaTime;
                nada.transform.localPosition = Vector3.Lerp(this.transform.localPosition + new Vector3(0, 0.07f, 0), this.transform.localPosition + new Vector3(0, 0.14f, 0), nadaTimer / 1);
                if (nadaTimer > 0.6f)
                {
                    nadaTimer = 0.6f;
                }
                break;
            case 0.6f: // When loop done
                showingNada = false;
                Debug.Log("yeag");
                GameObject.Destroy(nada);
                nada = null;
                nadaTimer = 0;
                break;
        }
    }
}
