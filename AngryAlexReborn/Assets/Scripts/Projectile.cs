using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : Weapon
{
    public float Speed;
    public int Damage;
    [SerializeField]
    public GameObject Owner;


    // Start is called before the first frame update
    void Start()
    {
        var rigidB = GetComponent<Rigidbody2D>();

        //Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //Vector2 direction = mousePosition - transform.position;
        //direction.Normalize();

        rigidB.velocity = transform.up * Speed;
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        //if (collider.gameObject.name != this.gameObject.name)
        if (collider.gameObject.tag != this.gameObject.tag)
        {
            Debug.Log("Bullet Hit!");

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

    // Update is called once per frame3
    void Update()
    {

    }

    private void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}
