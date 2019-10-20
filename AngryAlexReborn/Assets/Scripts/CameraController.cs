using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public float zValue = -10f;

    public bool isLocalPlayer = false;

    void Awake()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        if (!target)
        {
            target = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        if (!target)
        {
            target = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        if (!target)
        {
            target = GameObject.FindGameObjectWithTag("Player").transform;
        }

        transform.position = new Vector3(target.position.x, target.position.y, zValue);
    }

    public void setTarget(Transform _target)
    {
        target = _target;
        transform.position = new Vector3(target.position.x, target.position.y, zValue);
    }

    public void isLocal(bool value)
    {
        isLocalPlayer = value;
    }
}
