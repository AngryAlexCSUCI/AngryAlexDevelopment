using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : Weapon
{
    public float Speed;
    public int Damage;
    [SerializeField]
    public Weapon owner;


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
        Debug.Log("Projectile OnTriggerEnter2D: Testing owner.isLocal player and that collider is not person who fired");
        //if (collider.gameObject.name != this.gameObject.name)
        //if (collider.gameObject.tag != this.gameObject.tag)
        if (owner.isLocalPlayer && !collider.gameObject.GetComponent<CarController>().isLocalPlayer)
        {
            Debug.Log("Projectile OnTriggerEnter2D: Bullet Hit!");

            Debug.Log("Projectile OnTriggerEnter2D: Fetching health bar");
            var healthBar = collider.gameObject.GetComponent<HealthBar>();// as HealthBar;
            Debug.Log("Projectile OnTriggerEnter2D: Successfully fetched health bar");

            string from = owner.playerName;
            Debug.Log("Projectile OnTriggerEnter2D: " + collider.gameObject.name + ": took damage from bullet from: " + from);

            if (!healthBar)
            {
                Debug.Log("Projectile OnTriggerEnter2D: No health bar found for player" + collider.gameObject.name + ", return.");
                return;
            }

            Debug.Log("Projectile OnTriggerEnter2D: About to call TakeDamage on healthBar from Projectile script");
            healthBar.TakeDamage(10);
            Debug.Log("Projectile OnTriggerEnter2D: Back in Projectile script after call to TakeDamage");

            Debug.Log("Projectile OnTriggerEnter2D: Creating damage record HealthChangeJson");
            WebSocketManager.HealthChangeJson damageRecord = new WebSocketManager.HealthChangeJson(collider.gameObject.name, 10, from);
            Debug.Log("Projectile OnTriggerEnter2D: Stringifying json");
            string jsonDamageRecord = JsonUtility.ToJson(damageRecord);
            Debug.Log("Projectile OnTriggerEnter2D: Dispatching projectile_damage to WebSocketManager");
            WebSocketManager.instance.Dispatch("projectile_damage", jsonDamageRecord, true);

            // Send message that damage was dealt to another player (probably implementing in healthbar script)
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}
