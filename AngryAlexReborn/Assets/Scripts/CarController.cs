using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : Player
{
    protected Rigidbody2D rb;
    protected TrailRenderer[] skidMarkTrails;
    protected AudioSource engineSound;
    public int height;

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

    public float minDriftThreshold; //2.3f;
    public float maxDriftThreshold; //2.3f;
    protected float driftThreshold;

    // Start is called before the first frame update
    void Start()
    {
        height = 0;
        rb = GetComponent<Rigidbody2D>();
        currentSideFriction = normalTurnSideFricton;
        skidMarkTrails = gameObject.GetComponentsInChildren<TrailRenderer>();
        turnOffSkidMarks();
        //skidMarkTrail.enabled = false;

        driftThreshold = minDriftThreshold;
        //currentEndDriftVelocity = endDriftVelocity;

        engineSound = GetComponent<AudioSource>();
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
        //We also leave a little bit (0.1f) so that the car can be slowly tuned while in standstill
        rb.AddTorque(Input.GetAxis("Horizontal") * ((-rb.velocity.magnitude / velocityToTurnSpeedRatio) - 0.1f));

        //Once torque and forces have been applied, mitigate some sideways velocity
        //Depending on whether we are "drifting" or not
        rb.velocity = getForwardVelocity() + (getRightVelocity() * currentSideFriction);
        if (getRightVelocity().magnitude < driftThreshold)
        {
            currentSideFriction = normalTurnSideFricton;
            driftThreshold = Mathf.Max(getRightVelocity().magnitude - 0.01f, minDriftThreshold);
            turnOffSkidMarks();
            Debug.Log("Not Drifting, driftThreshold vs Vel : " + driftThreshold + ", " + getRightVelocity().magnitude);
        }
        else if (getRightVelocity().magnitude > driftThreshold) //possibly implicit
        {
            currentSideFriction = driftingSideFriction;
            driftThreshold = Mathf.Min(getRightVelocity().magnitude + 0.01f, maxDriftThreshold);
            turnOnSkidMarks();
            Debug.Log("Drifting! driftThreshold vs Vel : " + driftThreshold + ", " + getRightVelocity().magnitude);
        }


        //handle sound
        if (rb.velocity.magnitude > 0.2f)
        {
            if (!engineSound.isPlaying)
            {
                engineSound.Play();
            }
            Debug.Log("Sound pitch at start: " + engineSound.pitch);
            engineSound.pitch = 1f + (Mathf.Pow(rb.velocity.magnitude, 1.18f)) / velocity;
        } else
        {
            if (engineSound.isPlaying)
            {
                engineSound.Stop();
            }
        }

        // send position and turn updates
        if (getForwardVelocity().magnitude > 0)
        {
            Vector3 vec = new Vector3(rb.position.x, rb.position.y, 0); // todo what if the player is up in the air? 
            WebSocketManager.PositionJson pos = new WebSocketManager.PositionJson(vec);
            string data = JsonUtility.ToJson(pos);
            WebSocketManager.instance.Dispatch("move", data, true);
        }

        if (getRightVelocity().magnitude > 0)
        {
            Quaternion quat = Quaternion.Euler(0, 0, rb.rotation); // todo rotation is only in z plane?
            WebSocketManager.RotationJson rot = new WebSocketManager.RotationJson(quat);
            string data2 = JsonUtility.ToJson(rot);
            WebSocketManager.instance.Dispatch("turn", data2, true);
        }
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
