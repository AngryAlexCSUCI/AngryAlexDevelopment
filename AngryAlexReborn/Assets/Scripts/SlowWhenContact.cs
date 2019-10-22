using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowWhenContact : MonoBehaviour
{
    CarController gameObj;
    float m_originalVelocity = 80f;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Slow the car");
        gameObj = collision.gameObject.GetComponent<CarController>() as CarController;
        m_originalVelocity = gameObj.velocity;
        gameObj.velocity -= 40;

        //minimum speed
        if (gameObj.velocity < 10)
            gameObj.velocity = 10;

        Debug.Log("Slow the car by 30");

        // change back to original speed after n seconds
        int slowTime = 15;
        Invoke("ChangeBackVelocity", slowTime);
    }

    private void ChangeBackVelocity()
    {
        gameObj.velocity = m_originalVelocity;
        Debug.Log("Faster the car by 30");

        gameObj = null;
    }
}
