using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : Player
{
    protected Rigidbody2D rb;
    protected TrailRenderer[] skidMarkTrails;
    protected AudioSource engineSound;
    public int height;

    [HideInInspector]
    public bool isLocalPlayer = false;

    protected float current_velocity;
    //speed of the car (80 for default buggy)
    public float velocity;
    //reverse speed of the car (20 for default buggy)
    public float reverse_velocity;
    //ratio of the car's speed vs its turn speed, higher value = bigger turn radius (4 for default buggy)
    public float velocity_to_turn_speed_ratio;

    //Car "Drift" variables
    public float normal_turn_side_fricton; //0.1
    public float drifting_side_friction; //0.99

    private float current_side_friction = 0;

    public float min_drift_threshold; //2.3f;
    public float max_drift_threshold; //2.3f;
    protected float drift_threshold;

    // Booleans and ints controlling the application of force to the car's rigidbody
    // Set in the Update method and linked to the message sending through the web socket manager
    protected bool wPressed = false;
    protected bool sPressed = false;
    protected int torqueAmt = 0;
    protected int LEFT = -1;
    protected int RIGHT = 1;

    // Controllers for velocity change when coming into contact with an oil spill
    protected float slow_velocity;
    protected int slow_time_remaining = 0;
    protected int MAX_SLOW_DEBUFF_TIMER = 300;


    // Network send properties
    protected int networkStep = 0;
    protected int sendOnStep = 15; // This will determine how often we send our network state to the other player
                                   // If this is too low we will clog up the network so don't drop it to hard.
                                   // 15 is a nice amount for a baseline.
                                   // IMPORTANT should tweak this to experiment with slower send rate (30, 45, 60 might be good to test)

    // REMOTE PLAYER VARIABLES
    protected Vector3 vec3Current; // Origin of movement, used to lerp from current to correct position
    protected Quaternion quatCurrent; // Origin of rotation, used to lerp from current to correct rotation
    protected Vector3 vec3Destination; // Destination of movement, used to lerp from current to correct position
    protected Quaternion quatDestination; // Destination of rotation, used to lerp from current to correct rotation
    protected float interpolationAmount = 0f; // amount to be passed to lerp to get current interpolated position and rotation
    protected float interpolationStep = 0.05f; // amount to increase interpolation per update // Attempting high amout for faster prediction


    // Collision blend variables
    protected int blendFramesRemaining = 0;

    // Acceleration variables
    protected Vector3 acceleration;
    protected Vector3 lastVelocity;


    // Start is called before the first frame update
    void Start()
    {
        height = 0;
        rb = GetComponent<Rigidbody2D>();
        current_side_friction = normal_turn_side_fricton;
        skidMarkTrails = gameObject.GetComponentsInChildren<TrailRenderer>();
        turnOffSkidMarks();
        //skidMarkTrail.enabled = false;

        drift_threshold = min_drift_threshold;
        //currentEndDriftVelocity = endDriftVelocity;

        engineSound = GetComponent<AudioSource>();

        // Assign slow velocity as half the normal velocity assigned to the vehicle
        current_velocity = velocity;
        slow_velocity = velocity / 2;

        // Initialize remote player variables
        vec3Current = transform.position;
        quatCurrent = transform.rotation;
        vec3Destination = transform.position;
        quatDestination = transform.rotation;
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
            //sendWPressed();
            wPressed = true;
        }
        if (Input.GetKeyUp("w") || Input.GetKeyUp("up"))
        {
            //sendWReleased();
            wPressed = false;
        }

        if (Input.GetKeyDown("s") || Input.GetKeyDown("down"))
        {
            //sendSPressed();
            sPressed = true;
        }
        if (Input.GetKeyUp("s") || Input.GetKeyUp("down"))
        {
            //sendSReleased();
            sPressed = false;
        }

        if (Input.GetKeyDown("a") || Input.GetKeyDown("left"))
        {
            //sendAPressed();
            torqueAmt = LEFT;
        }
        if (Input.GetKeyUp("a") || Input.GetKeyUp("left"))
        {
            //sendAReleased();
            torqueAmt = 0;
        }

        if (Input.GetKeyDown("d") || Input.GetKeyDown("right"))
        {
            //sendDPressed();
            torqueAmt = RIGHT;
        }
        if (Input.GetKeyUp("d") || Input.GetKeyUp("right"))
        {
            //sendDReleased();
            torqueAmt = 0;
        }

        if (networkStep == 0)
        {
            sendUpdate();
        }
        networkStep = (networkStep + 1) % sendOnStep;
    }



    // FixedUpdate is called whenever the screen is refreshed
    void FixedUpdate() {
        Debug.Log("CarController FixedUpdate: ENTER");

        if (!isLocalPlayer) {

            // Determine what our acceleration is. Will be useful to interpolate on remote players
            acceleration = (rigidbody.velocity - lastVelocity) / Time.fixedDeltaTime;
            lastVelocity = rigidbody.velocity;
            Debug.Log("CarController FixedUpdate: Acceleration: " + acceleration);

            // Interpolate current position/rotation with destination
            if (blendFramesRemaining > 0)
            {
                blendFramesRemaining--;
            } else
            {
                transform.position = Vector3.LerpUnclamped(vec3Current, vec3Destination, interpolationAmount);
                transform.rotation = Quaternion.LerpUnclamped(quatCurrent, quatDestination, interpolationAmount);
                interpolationAmount += interpolationStep;
            }

            // Recall that goal of original algorithm from watchdogs presentation was to "represent something
            // believable locally, and then blend into snapshots that take collision into account"

            // Here we'll want to develop something close to the watchdogs algorithm for collision for comparison purposes.

            // Our personal approch is to then switch to lockstep at the start of the collision, then resume the simulation as soon
            // as we have received the synchronization position from the other player.
            // A pseudocode for that might look something like:
            // On collide
            //     Stop simulation
            //     Send message notifying other player that we have engaged in a collision (collision should be uniquely tracked by the players involved in the collision if we plan on adding more players to this mix)
            // Wait until receive confirmation package from remote player
            // Once packet is received:
            //     Apply position/rotation/angular velocity that car reported at the moment we collided
            //     Resume simulation
            // Both the packet that we send to start the collision and the packet we receive from the remote player
            // should contain the pos/rot/velocity at the moment of collision so that we can set the simulation to a
            // somewhat deterministic state, which should allow us to properly simulate how the physics simulation actually went down

        } else {

            if (wPressed)
            {
                rb.AddForce(transform.up * current_velocity);
            }

            if (sPressed)
            {
                rb.AddForce(-transform.up * reverse_velocity);
            }

            //Torque is added only when the car is in motion, as a ratio of
            //the current magnitude to the velocity_to_turn_speed_ratio declared above
            //We also leave a little bit (0.1f) so that the car can be slowly tuned while in standstill
            rb.AddTorque(torqueAmt * ((-rb.velocity.magnitude / velocity_to_turn_speed_ratio) - 0.1f));

            //Once torque and forces have been applied, mitigate some sideways velocity
            //Depending on whether we are "drifting" or not
            rb.velocity = getForwardVelocity() + (getRightVelocity() * current_side_friction);
            if (getRightVelocity().magnitude < drift_threshold)
            {
                current_side_friction = normal_turn_side_fricton;
                drift_threshold = Mathf.Max(getRightVelocity().magnitude - 0.01f, min_drift_threshold);
                turnOffSkidMarks();
                //Debug.Log("Not Drifting, drift_threshold vs Vel : " + drift_threshold + ", " + getRightVelocity().magnitude);
            }
            else if (getRightVelocity().magnitude > drift_threshold) //possibly implicit
            {
                current_side_friction = drifting_side_friction;
                drift_threshold = Mathf.Min(getRightVelocity().magnitude + 0.01f, max_drift_threshold);
                turnOnSkidMarks();
                //Debug.Log("Drifting! drift_threshold vs Vel : " + drift_threshold + ", " + getRightVelocity().magnitude);
            }


            //handle sound
            if (rb.velocity.magnitude > 0.2f)
            {
                if (!engineSound.isPlaying)
                {
                    engineSound.Play();
                }
                //Debug.Log("Sound pitch at start: " + engineSound.pitch);
                engineSound.pitch = 1f + (Mathf.Pow(rb.velocity.magnitude, 1.18f)) / current_velocity;
            } else
            {
                if (engineSound.isPlaying)
                {
                    engineSound.Stop();
                }
            }

            //check for slow debuff
            if (slow_time_remaining > 0)
            {
                slow_time_remaining -= 1;
                if (slow_time_remaining <= 0)
                {
                    current_velocity = velocity;
                    Debug.Log("Restoring vehicle speed");
                }
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

    public void updateDestination(Vector3 newVec3Dest, Quaternion newQuatDest)
    {
        vec3Current = transform.position;
        quatCurrent = transform.rotation;
        vec3Destination = newVec3Dest;
        quatDestination = newQuatDest;
        interpolationAmount = 0.0f;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("CarController OnCollisionEnter2D: ENTER");
        // Only check collisions for the local player, the remote players will check on their end
        Debug.Log("CarController OnCollisionEnter2D: Test if this is the collision call from the local player");
        if (isLocalPlayer)
        {
            Debug.Log("CarController OnCollisionEnter2D: Collided with object: " + collision.gameObject.name + ", Object component: " + collision.gameObject.GetComponent("CarController"));
            if (collision.gameObject.GetComponent("CarController") != null && collision.gameObject.name != UserName)
            {
                // We have collided with a remote player
                Debug.Log("CarController OnCollisionEnter2D: This is a remote collision!");
                // We must notify the other player

                // WATCHDOGS ATTEMPT
                Debug.Log("CarController OnCollisionEnter2D: Call startBlendCollision on other player's CarController");
                collision.gameObject.GetComponent<CarController>().startBlendCollision();
            }
        }
    }

    // Called on the remote player, will blend the interpolation between local physics in a similar fashion to the Watchdogs 2 algorithm
    public void startBlendCollision()
    {
        blendFramesRemaining = 30;
    }

    public void slowDebuff()
    {
        Debug.Log("VEHICLE CONTACTED OIL SPILL, SLOWING CAR DOWN");
        current_velocity = slow_velocity;
        slow_time_remaining = MAX_SLOW_DEBUFF_TIMER;
    }

    // All presses and releases of keys also update position/rotation to make sure the car hasn't desync'd

    void sendUpdate()
    {
        WebSocketManager.instance.Dispatch("move", posRotJson(), true);
    }

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




    string positionVelocityAccelerationRotationJson()
    {
        // Get Current Position Vec 3 and rotation quat
        Vector3 position = rb.position;
        Vector3 velocity = rb.velocity;
        // Vector3 acceleration = defined above
        Quaternion quat = Quaternion.Euler(0, 0, rb.rotation); // todo rotation is only in z plane?

        // JSON-ify position and rotation
        WebSocketManager.PositionRotationJson posrot = new WebSocketManager.PositionRotationJson(position, quat);

        // Then return as string
        return JsonUtility.ToJson(posrot);
    }

    string posRotJson()
    {
        // Get Current Position Vec 3 and rotation quat
        Vector3 vec = rb.position;
        Quaternion quat = Quaternion.Euler(0, 0, rb.rotation); // todo rotation is only in z plane?

        // JSON-ify position and rotation
        WebSocketManager.PositionRotationJson posrot = new WebSocketManager.PositionRotationJson(vec, quat);

        // Then return as string
        return JsonUtility.ToJson(posrot);
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




    public void setLocalPlayer()
    {
        isLocalPlayer = true;
        Weapon weapon = GetComponentInChildren<Weapon>();
        weapon.isLocalPlayer = true;
        weapon.playerName = UserName;
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
