using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoveSpriteWithMouseTest : MonoBehaviour
{
    [SerializeField]
    private Transform spriteObject;
    [SerializeField]
    private RectTransform HUD;
    void Update()
    {
        Debug.Log("Screen Size: (" + Screen.width + ", " + Screen.height + ")");
        Debug.Log("Game Resolution: (" + Screen.currentResolution.width + ", " + Screen.currentResolution.height + ")");
        Debug.Log("Camera Size: (" + Camera.main.scaledPixelWidth + ", " + Camera.main.scaledPixelHeight + ")");
        Debug.Log("HUD Size: (" + HUD.rect.width + ", " + HUD.rect.height + ")");

        Vector2 mouseToCanvasScale = new Vector2(HUD.rect.width / Screen.width, HUD.rect.height / Screen.height);
        Debug.Log("Scaled mouse position: (" + Input.mousePosition.x * mouseToCanvasScale.x + ", " + Input.mousePosition.y * mouseToCanvasScale.y + ")");
        Vector2 offset = new Vector2 (HUD.rect.width / 2, HUD.rect.height / 2); // take this from final value to place sprite (0,0) at bottom corner of screen
        Vector2 spritePosition = new Vector2(Input.mousePosition.x * mouseToCanvasScale.x, Input.mousePosition.y * mouseToCanvasScale.y);
        spriteObject.transform.localPosition = spritePosition - offset;
    }
}
