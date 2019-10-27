using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Rigidbody2D target;
    public float zValue;
    public int orthographicFloor;
    protected new Camera camera; //reference to camera object that this script should be attached to

    public bool isLocalPlayer = false;

    void Awake()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        if (!target)
        {
            target = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody2D>();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        camera = GetComponent<Camera>();
        if (!isLocalPlayer)
        {
            return;
        }

        if (!target)
        {
            target = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody2D>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer)
        {
            //return;
        }

        if (!target)
        {
            //target = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody2D>();
        }

        //follow target and zoom out slightly based off magnitude of the velocity of object we are following
        transform.position = new Vector3(target.position.x, target.position.y, zValue);
        camera.orthographicSize = this.orthographicFloor + target.velocity.magnitude / 8;
    }

    public void setTarget(Rigidbody2D _target)
    {
        target = _target;
        transform.position = new Vector3(target.position.x, target.position.y, zValue);
    }

//    public void setTarget(Transform _target)
//    {
//        target = _target;
//        transform.position = new Vector3(target.position.x, target.position.y, zValue);
//    }
//
//    public void isLocal(bool value)
//    {
//        isLocalPlayer = value; 
//    }
}
