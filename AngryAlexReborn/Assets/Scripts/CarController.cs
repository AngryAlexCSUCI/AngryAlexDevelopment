using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    protected Rigidbody2D rb;
    protected TrailRenderer[] skidMarkTrails;

    public bool isLocalPlayer = false;

    //speed of the car (80 for default buggy)
    public float velocity;
    //reverse speed of the car (20 for default buggy)
    public float reverseVelocity;
    //ratio of the car's speed vs its turn speed, higher value = bigger turn radius (4 for default buggy)
    public float velocityToTurnSpeedRatio;

    //Car "Drift" variables
    public float normalTurnSideFricton; //0.1
    public float driftingSideFriction; //0.99

    private float currentSideFriction = 0;

    public float startDriftVelocity; //2.3f
    public float endDriftVelocity; //2.3f;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentSideFriction = normalTurnSideFricton;
        skidMarkTrails = gameObject.GetComponentsInChildren<TrailRenderer>();
        turnOffSkidMarks();
        //skidMarkTrail.enabled = false;
    }

    // Update is called once per frame

    void FixedUpdate()
    {

        if (!isLocalPlayer)
        {
            return;
        }


        if (Input.GetKey("w") || Input.GetKey("up"))
        {
            rb.AddForce(transform.up * velocity);
        }

        if (Input.GetKey("s") || Input.GetKey("down"))
        {
            rb.AddForce(-transform.up * reverseVelocity);
        }

        //Torque is added only when the car is in motion, as a ratio of
        //the current magnitude to the velocityToTurnSpeedRatio declared above
        rb.AddTorque(Input.GetAxis("Horizontal") * -rb.velocity.magnitude/velocityToTurnSpeedRatio);

        //Once torque and forces have been applied, mitigate some sideways velocity
        //Depending on whether we are "drifting" or not
        
        if (getRightVelocity().magnitude > startDriftVelocity)
        {
            Debug.Log("Drifting! : " + getRightVelocity().magnitude);
            currentSideFriction = driftingSideFriction;
            turnOnSkidMarks();
            //skidMarkTrail.enabled = true;
        }

        if (getRightVelocity().magnitude < endDriftVelocity)
        {
            currentSideFriction = normalTurnSideFricton;
            turnOffSkidMarks();
            //skidMarkTrail.enabled = false;
            //skidMarkTrail.enabled = false;
            //skidMarkTrail.Clear();
        }

        rb.velocity = getForwardVelocity() + (getRightVelocity() * currentSideFriction);
    }

    Vector2 getForwardVelocity()
    {
        return transform.up * Vector2.Dot(rb.velocity, transform.up);
    }

    Vector2 getRightVelocity()
    {
        return transform.right * Vector2.Dot(rb.velocity, transform.right);
    }

    void turnOnSkidMarks()
    {
        foreach (TrailRenderer skidTrail in skidMarkTrails)
        {
            skidTrail.emitting = true;
        }
    }

    void turnOffSkidMarks()
    {
        foreach (TrailRenderer skidTrail in skidMarkTrails)
        {
            skidTrail.emitting = false;
        }
    }
}
