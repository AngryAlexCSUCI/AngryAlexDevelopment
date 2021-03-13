using System;
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

    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Camera start called");
        camera = GetComponent<Camera>();
        Debug.Log("Camera active state: " + gameObject.activeInHierarchy);
        if (!isLocalPlayer)
        {
            return;
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        //follow target and zoom out slightly based off magnitude of the velocity of object we are following
        transform.position = new Vector3(target.position.x, target.position.y, zValue);
        camera.orthographicSize = this.orthographicFloor + target.velocity.magnitude / 3;
        Debug.Log("CameraController: Update postion to track target");
    }

    public void setTarget(Rigidbody2D _target)
    {
        Debug.Log("CameraController: setTarget called");
        target = _target;
        transform.position = new Vector3(target.position.x, target.position.y, zValue);
    }

    public void isLocal(bool value)
    {
        Debug.Log("CameraController: isLocal called");
        isLocalPlayer = value;
    }
}
