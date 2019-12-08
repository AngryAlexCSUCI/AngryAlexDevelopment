using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageWhenContact : MonoBehaviour
{
    // Update is called once per frame
    void OnTriggerEnter2D(Collider2D collider)
    {
        var healthBar = collider.gameObject.GetComponent<HealthBar>() as HealthBar;

        if (!healthBar)
        {
            Debug.Log("return");
            return;
        }
        Debug.Log(collider.gameObject.name + ": took damage from obstacle.");
        healthBar.TakeDamage(10, null, true);
    }
}
