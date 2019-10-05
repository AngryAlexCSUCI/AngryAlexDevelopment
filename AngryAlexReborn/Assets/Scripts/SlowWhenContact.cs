using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowWhenContact : MonoBehaviour
{
     
    private void OnTriggerEnter2D(Collider2D collision)

    {
        Debug.Log("Touch the Mud");

        var gameObj = collision.gameObject.GetComponent<CarController>() as CarController;

        Debug.Log("Slow the car");

        gameObj.velocity -= 20;
       
    }

}
