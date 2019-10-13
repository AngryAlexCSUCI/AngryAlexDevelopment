using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageWhenContact : MonoBehaviour
{
    private PlayerPrefs player;
    // Start is called before the first frame update
    void Start()
    {   //commented out to test
        //player = GameObject.FindGameObjectWithTag("PlayerPrefs").GetComponent<PlayerPrefs>();
    }

    // Update is called once per frame
    void OnTriggerEnter2D(Collider2D collider)
    {
        Debug.Log("Touch the cactus!");

        var healthBar = collider.gameObject.GetComponent<HealthBar>() as HealthBar;

        if (!healthBar)
        {
            Debug.Log("return");
            return;
        }
        Debug.Log("take damge");

        healthBar.TakeDamage(10);

    }

}
