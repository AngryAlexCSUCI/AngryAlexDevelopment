using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Burn : MonoBehaviour
{
     
    void Start()
    {

    }

    // Update is called once per frame
    void OnTriggerEnter2D(Collider2D collision)
    {

        Debug.Log("Touch the Mud");

        var gameObj = collision.gameObject.GetComponent<CarController>() as CarController;

        Debug.Log("Slow the car");

        gameObj.velocity += 40;
    }

    //this one put it in the player
    public void Damage (int damage)
    {   
        //burn
        //burn effect for 10 sseconds

    }
}
