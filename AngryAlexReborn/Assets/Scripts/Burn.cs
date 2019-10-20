using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Burn : MonoBehaviour
{

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
        healthBar.startBlinking = true;
        healthBar.TakeDamage(10);
        healthBar.carObject = collider.gameObject;

    }
}
