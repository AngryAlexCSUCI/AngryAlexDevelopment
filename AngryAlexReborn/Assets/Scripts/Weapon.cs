using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public GameObject Projectile;
    public Transform ProjectileSpawn;
    public float FireRate = 0.02F;
    private float NextFire = 0.0F;
    public bool isLocalPlayer = false;

    // Start is called before the first frame update
    void Start()
    {

    }
    
    // Update is called once per frame
    void Update()
    {
        FollowMouse();

        if (Input.GetMouseButton(0) && Time.time > NextFire)
        {
            NextFire = Time.time + FireRate;
            var bullet = Instantiate(Projectile, ProjectileSpawn.position, ProjectileSpawn.rotation);
            string tag = this.transform.parent.gameObject.tag;
            bullet.gameObject.tag = tag;
            Physics2D.IgnoreCollision(bullet.GetComponent<Collider2D>(), this.GetComponent<Collider2D>());
            Physics2D.IgnoreCollision(bullet.GetComponent<Collider2D>(), this.transform.parent.gameObject.GetComponent<Collider2D>());
        }
    }

    private void FollowMouse()
    {
        Vector2 mouseScreen = Input.mousePosition;
        Vector2 mouse = Camera.main.ScreenToWorldPoint(mouseScreen);

        transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(mouse.y - transform.position.y, mouse.x -
            transform.position.x) * Mathf.Rad2Deg - 90);
    }
}
