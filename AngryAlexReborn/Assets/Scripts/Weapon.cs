using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : Player
{
    public GameObject Projectile;
    public Transform ProjectileSpawn;
    public float FireRate = 0.02F;
    private float NextFire = 0.0F;

    protected AudioSource fireSound;

    [HideInInspector]
    public bool isLocalPlayer = false;

    // Start is called before the first frame update
    void Start()
    {
        fireSound = GetComponent<AudioSource>();
    }
    
    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        FollowMouse();

        if (Input.GetMouseButton(0) && Time.time > NextFire)
        {
            WebSocketManager.instance.Dispatch("fire", quatJson(), true);
            fireWeapon();
        }
    }

    private void FollowMouse()
    {
        if (!isLocalPlayer)
        {
            return;
        }
        Vector2 mouseScreen = Input.mousePosition;
        Vector2 mouse = Camera.main.ScreenToWorldPoint(mouseScreen);

        transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(mouse.y - transform.position.y, mouse.x -
            transform.position.x) * Mathf.Rad2Deg - 90);
    }

    public void fireWeapon()
    {
        NextFire = Time.time + FireRate;
        //possibly old code for creating the bullet, imported when merging - Christian
        //Instantiate(Projectile, ProjectileSpawn.position, ProjectileSpawn.rotation);
        fireSound.PlayOneShot(fireSound.clip);
        var bullet = Instantiate(Projectile, ProjectileSpawn.position, transform.rotation);
        string tag = this.transform.parent.gameObject.tag;
        bullet.gameObject.tag = tag;
        //Physics2D.IgnoreCollision(bullet.GetComponent<Collider2D>(), this.GetComponent<Collider2D>());
        //Physics2D.IgnoreCollision(bullet.GetComponent<Collider2D>(), this.transform.parent.gameObject.GetComponent<Collider2D>());
    }

    string quatJson()
    {
        Quaternion quat = transform.rotation; // todo rotation is only in z plane?
        WebSocketManager.RotationJson rot = new WebSocketManager.RotationJson(quat);
        return JsonUtility.ToJson(rot);
    }
}
