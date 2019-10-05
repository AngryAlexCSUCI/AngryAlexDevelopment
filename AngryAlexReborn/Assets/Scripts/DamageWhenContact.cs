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
    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Touch the Fire!"); 

        var gameObj = collision.gameObject.GetComponent<CarController>() as CarController;

        Debug.Log("Slow the car");

        gameObj.velocity += 60;
        Damage(gameObj);
    }

    //this one put it in the player
    private void Damage (CarController carObj)
    {   
        //carObj health minus damage

    }
}
