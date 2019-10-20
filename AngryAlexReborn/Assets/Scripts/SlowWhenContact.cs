using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowWhenContact : MonoBehaviour
{
    CarController gameObj;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Slow the car");
        gameObj = collision.gameObject.GetComponent<CarController>() as CarController;
 
        gameObj.velocity = 10;
        Debug.Log("Slow the car by 20");
         
        Invoke("ChangeBackVelocity", 3);

    }

    private void ChangeBackVelocity()
    {

        gameObj.velocity += 70;
        Debug.Log("Faster the car by 20");

        gameObj = null;
    }
}
