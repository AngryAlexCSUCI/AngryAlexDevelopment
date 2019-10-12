using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Burn : MonoBehaviour
{
    public float spriteBlinkingTimer = 0.0f;
    public float spriteBlinkingMiniDuration = 0.1f;
    public float spriteBlinkingTotalTimer = 0.0f;
    public float spriteBlinkingTotalDuration = 1.0f;
    public bool startBlinking = false;
    public GameObject carObject;

    void Start()
    {   //commented out to test
        //player = GameObject.FindGameObjectWithTag("PlayerPrefs").GetComponent<PlayerPrefs>();
    }

    // Update is called once per frame
    void OnTriggerEnter2D(Collider2D collider)
    {
        Debug.Log("Burn!");

        var healthBar = collider.gameObject.GetComponent<HealthBar>() as HealthBar;

        if (!healthBar)
        {
            Debug.Log("return");
            return;
        }
        Debug.Log("take damge");
        startBlinking = true;
        healthBar.TakeDamage(10);
        carObject = collider.gameObject;

    }

    void Update()
    {
        if (startBlinking == true)
        {
            SpriteBlinkingEffect();
        }
    }

    public void SpriteBlinkingEffect()
    {
        spriteBlinkingTotalTimer += Time.deltaTime;
        if (spriteBlinkingTotalTimer >= spriteBlinkingTotalDuration)
        {
            startBlinking = false;
            spriteBlinkingTotalTimer = 0.0f;
            carObject.GetComponent<SpriteRenderer>().enabled = true;   // according to 
                                                                       //your sprite
            return;
        }

        spriteBlinkingTimer += Time.deltaTime;
        if (spriteBlinkingTimer >= spriteBlinkingMiniDuration)
        {
            spriteBlinkingTimer = 0.0f;
            if (carObject.GetComponent<SpriteRenderer>().enabled == true)
            {
                carObject.GetComponent<SpriteRenderer>().enabled = false;  //make changes
            }
            else
            {
                carObject.GetComponent<SpriteRenderer>().enabled = true;   //make changes
            }
        }
    }
}
