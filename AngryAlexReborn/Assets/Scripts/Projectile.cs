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
        if (collider.gameObject.layer != owner.gameObject.layer)
            return;

        if (owner.isLocalPlayer && !collider.gameObject.GetComponent<CarController>().isLocalPlayer)
        {
            Debug.Log("Bullet Hit!");

            var healthBar = collider.gameObject.GetComponent<HealthBar>() as HealthBar;
 
            string from = owner.playerName;
            Debug.Log(collider.gameObject.name + ": took damage from bullet from: " + from);

            if (!healthBar)
            {
                Debug.Log("No health bar found for player" + collider.gameObject.name + ", return.");
                return;
            }

            healthBar.TakeDamage(10);

            WebSocketManager.HealthChangeJson damageRecord = new WebSocketManager.HealthChangeJson(collider.gameObject.name, 10, from);
            string jsonDamageRecord = JsonUtility.ToJson(damageRecord);
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
