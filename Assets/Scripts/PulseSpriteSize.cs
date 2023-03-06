using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PulseSpriteSize : MonoBehaviour
{
    [SerializeField]
    private float maxScale;
    [SerializeField]
    private float timePeriod;
    private float currentTime;
    private string state;
    private Vector3 ogScale;
    private Vector3 finalScale;
    private void Start()
    {
        if (maxScale < 1)
        {
            maxScale = 1;
        }
        ogScale = this.transform.localScale;
        finalScale = this.transform.localScale * maxScale;
        state = "increasing";
    }
    private void Update()
    {
        switch (state)
        {
            default:
                Debug.Log("State not recognised! (" + state + ")");
                break;
            case "increasing":
                if (currentTime < timePeriod)
                {
                    this.transform.localScale = Vector3.Lerp(ogScale, finalScale, currentTime / timePeriod);
                }
                else
                {
                    this.transform.localScale = finalScale;
                    state = "decreasing";
                    currentTime = 0;
                }
                break;
            case "decreasing":
                if (currentTime < timePeriod)
                {
                    this.transform.localScale = Vector3.Lerp(finalScale, ogScale, currentTime / timePeriod);
                }
                else
                {
                    this.transform.localScale = ogScale;
                    state = "increasing";
                    currentTime = 0;
                }
                break;
        }
        currentTime += Time.deltaTime;
    } 
}