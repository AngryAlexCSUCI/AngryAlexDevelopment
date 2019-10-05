using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShellExplosion : MonoBehaviour
{
    public LayerMask VehicleMask;
    public float Damage;
    public 
    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, 0);
    }

    private void OnTriggerEnter(Collider other)
    {
   }

    // Update is called once per frame
    void Update()
    {
        
    }
}
