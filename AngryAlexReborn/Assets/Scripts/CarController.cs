using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    protected Rigidbody2D rb;
    protected TrailRenderer[] skidMarkTrails;
    protected AudioSource engineSound;
    public int height;

    [HideInInspector]
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

    public float minDriftThreshold; //2.3f;
    public float maxDriftThreshold; //2.3f;
    protected float driftThreshold;

    // Booleans and ints controlling the application of force to the car's rigidbody
    // Set in the Update method and linked to the message sending through the web socket manager
    protected bool wPressed = false;
    protected bool sPressed = false;
    protected int torqueAmt = 0;
    protected int LEFT = -1;
    protected int RIGHT = 1;

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

    // Update is called on every tick
    private void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        if (Input.GetKeyDown("w") || Input.GetKeyDown("up"))
        {
            sendWPressed();
            wPressed = true;
        }
        if (Input.GetKeyUp("w") || Input.GetKeyUp("up"))
        {
            sendWReleased();
            wPressed = false;
        }

        if (Input.GetKeyDown("s") || Input.GetKeyDown("down"))
        {
            sendSPressed();
            sPressed = true;
        }
        if (Input.GetKeyUp("s") || Input.GetKeyUp("down"))
        {
            sendSReleased();
            sPressed = false;
        }

        if (Input.GetKeyDown("a") || Input.GetKeyDown("left"))
        {
            sendAPressed();
            torqueAmt = LEFT;
        }
        if (Input.GetKeyUp("a") || Input.GetKeyUp("left"))
        {
            sendAReleased();
            torqueAmt = 0;
        }

        if (Input.GetKeyDown("d") || Input.GetKeyDown("right"))
        {
            sendDPressed();
            torqueAmt = RIGHT;
        }
        if (Input.GetKeyUp("d") || Input.GetKeyUp("right"))
        {
            sendDReleased();
            torqueAmt = 0;
        }
    }



    // FixedUpdate is called whenever the screen is refreshed
    void FixedUpdate()
    {
        if (wPressed)
        {
            rb.AddForce(transform.up * velocity);
        }

        if (sPressed)
        {
            rb.AddForce(-transform.up * reverseVelocity);
        }

        //Torque is added only when the car is in motion, as a ratio of
        //the current magnitude to the velocityToTurnSpeedRatio declared above
        //We also leave a little bit (0.1f) so that the car can be slowly tuned while in standstill
        rb.AddTorque(torqueAmt * ((-rb.velocity.magnitude / velocityToTurnSpeedRatio) - 0.1f));

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
        //if (getForwardVelocity().magnitude > 0)
        //{
        //    Vector3 vec = new Vector3(rb.position.x, rb.position.y, 0); // todo what if the player is up in the air? 
        //    WebSocketManager.PositionJson pos = new WebSocketManager.PositionJson(vec);
        //    string data = JsonUtility.ToJson(pos);
        //    WebSocketManager.instance.Dispatch("move", data, true);
        //}

        //if (rb.angularVelocity > 0)
        //{
        //    Quaternion quat = Quaternion.Euler(0, 0, rb.rotation); // todo rotation is only in z plane?
        //    WebSocketManager.RotationJson rot = new WebSocketManager.RotationJson(quat);
        //    string data2 = JsonUtility.ToJson(rot);
        //    WebSocketManager.instance.Dispatch("turn", data2, true);
        //}
    }

    // All presses and releases of keys also update position/rotation to make sure the car hasn't desync'd
    void sendWPressed()
    {
        WebSocketManager.instance.Dispatch("wpressed", vectorJson(), true);
    }

    void sendWReleased()
    {
        WebSocketManager.instance.Dispatch("wrelease", vectorJson(), true);
    }

    void sendSPressed()
    {
        WebSocketManager.instance.Dispatch("spressed", vectorJson(), true);
    }

    void sendSReleased()
    {
        WebSocketManager.instance.Dispatch("srelease", vectorJson(), true);
    }

    void sendAPressed()
    {
        WebSocketManager.instance.Dispatch("apressed", quatJson(), true);
    }

    void sendAReleased()
    {
        WebSocketManager.instance.Dispatch("arelease", quatJson(), true);
    }

    void sendDPressed()
    {
        WebSocketManager.instance.Dispatch("dpressed", quatJson(), true);
    }

    void sendDReleased()
    {
        WebSocketManager.instance.Dispatch("drelease", quatJson(), true);
    }

    public void setWPressed(bool isPressed)
    {
        wPressed = isPressed;
    }

    public void setSPressed(bool isPressed)
    {
        sPressed = isPressed;
    }

    public void setAPressed(bool isPressed)
    {
        if (isPressed)
        {
            torqueAmt = LEFT;
        } else
        {
            torqueAmt = 0;
        }
    }

    public void setDPressed(bool isPressed)
    {
        if (isPressed)
        {
            torqueAmt = RIGHT;
        } else
        {
            torqueAmt = 0;
        }
    }

    string vectorJson()
    {
        // Get Current Position Vec 3
        Vector3 vec = rb.position;
        // JSON it
        WebSocketManager.PositionJson pos = new WebSocketManager.PositionJson(vec);
        // Then return as string
        return JsonUtility.ToJson(pos);
    }

    string quatJson()
    {
        Quaternion quat = Quaternion.Euler(0, 0, rb.rotation); // todo rotation is only in z plane?
        WebSocketManager.RotationJson rot = new WebSocketManager.RotationJson(quat);
        return JsonUtility.ToJson(rot);
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
