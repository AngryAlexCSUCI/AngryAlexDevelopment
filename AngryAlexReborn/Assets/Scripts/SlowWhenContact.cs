using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowWhenContact : MonoBehaviour
{
    CarController gameObj;
    public float m_originalVelocity;
    public float currentVelocity;
    public float timeLeft = 0;
    public int Counter = 0;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Slow the car");
        gameObj = collision.gameObject.GetComponent<CarController>() as CarController;

        if (gameObj != null)
        {
            if (Counter == 0)
                m_originalVelocity = gameObj.velocity;

            Counter++;

            gameObj.velocity -= 40;

            //minimum speed. prevent negative velocity
            if (gameObj.velocity < 10)
            {
                gameObj.velocity = 10;
            }
            currentVelocity = gameObj.velocity;
            Debug.Log("Slow the car by 40");

            // change back to original speed after n seconds
            timeLeft += 5.0f;
            Update();
        }
    }

    void Update()
    {
        timeLeft -= Time.deltaTime;
        if (timeLeft < 0)
        {
            ChangeBackVelocity();
            timeLeft = 0;
            Counter = 0;
        }
    }

    private void ChangeBackVelocity()
    {

        gameObj.velocity = m_originalVelocity;
        Debug.Log("Faster the car by 30");

    }
}
