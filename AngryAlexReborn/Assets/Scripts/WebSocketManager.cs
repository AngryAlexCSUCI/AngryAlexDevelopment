//Copyright (c) 2015 Gumpanat Keardkeawfa
//Licensed under the MIT license

//Websocket C# for UnityWebgl

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class WebSocketManager : Player
{

    #region WebSockets Implement
    Queue<string> recvList = new Queue<string>(); //keep receive messages

    [DllImport("__Internal")]
    private static extern void InitWebSocket(string url); //create websocket connection

    [DllImport("__Internal")]
    private static extern int State(); //check websocket state

    [DllImport("__Internal")]
    private static extern void Send(string message); //send message

    [DllImport("__Internal")]
    private static extern void Close(); //close websocket connection

    //For Receive Message, this function was call by plugin, we need to keep this name.
    void RecvString(string message)
    {
        recvList.Enqueue(message);   //We will put the new message to the recvList queue.
    }

    //For Receive Message, this function was call by plugin, we need to keep this name.
    void ErrorString(string message)
    {
        //We can do the same as RecvString here.
    }
    #endregion


    public static WebSocketManager instance;
    public Canvas uiCanvas;
    private string playerNameStr;
    public GameObject player;
    public Slider healthSlider;
    public Image healthFill;
    public bool skipPlay;
    public GameObject errorMessage;

    private LeaderboardManager leaderboardManager;


    void Start()
    {
        print("Starting web socket.");
        playerNameStr = UserName;
        print("Importing playerNameStr as: " + playerNameStr);
        StartCoroutine("RecvEvent");    //Then run the receive message loop

        leaderboardManager = GameObject.FindObjectOfType<LeaderboardManager>();
        DontDestroyOnLoad(this.gameObject);
    }


    //Receive message loop function
    IEnumerator RecvEvent()
    {
        print("Starting RecvEvent coroutine.");
        //InitWebSocket("ws://ec2-3-84-148-203.compute-1.amazonaws.com:8080"); //First we create the connection.
        print("Opening web socket to ws://47.144.13.214:8080");
        InitWebSocket("ws://47.144.13.214:8080"); //First we create the connection.
        print("Web socket open");
        //InitWebSocket("ws://47.144.8.185");
        //InitWebSocket("ws://ec2-3-88-230-113.compute-1.amazonaws.com:8080"); //TEMPORARY TEST CONNECTION FOR CHRISTIAN'S EC2.

        while (true)
        {
            if (recvList.Count > 0)
            {
                //When our message queue has string coming.
                // check message type: play, other_player_connected, move, turn, disconnect
                string message = recvList.Dequeue();
                //print("Message received from server: " + message);

                if (message == "You are connected to the server!" && !skipPlay) {
                    print("--- Sending play message!");
                    // send server player name and let server randomly pick spawn point from list of spawn points
                    // spawn point for player
                    List<SpawnPoint> playerSpawnPoints = GetComponent<PlayerSpawner>().playerSpawnPoints;
                    print("Current vehicle selection: " + Player.VehicleLoadout.Item1 + " " + Player.VehicleLoadout.Item2);
                    PlayerJson playerJson = new PlayerJson(playerNameStr, playerSpawnPoints, Player.VehicleLoadout);
                    string data = JsonUtility.ToJson(playerJson);
                    Dispatch("play", data, true);
                } else {
                    string[] dataArr = message.Split(' ');
                    
                    //print("Client message at [0]: " + dataArr[0]);
                    //print("Client message at [1]: " + dataArr[1]);
                    
                    if (dataArr[0] == "play") {
                        // receive play from server means you need to get the spawn point from here and assign to player. 
                        Dispatch("play", dataArr[1], false);
                    } else if (dataArr[0] == "other_player_connected") {
                        // receive other player connected means you need to spawn another player in that spot to represent them
                        Dispatch("other_player_connected", dataArr[1], false);
                    } else if (dataArr[0] == "move") {
                        Dispatch("move", dataArr[1], false);
                    } else if (dataArr[0] == "ackMessage") {
                        Dispatch("ackMessage", dataArr[1], false);
                    } else if (dataArr[0] == "collision") {
                        print("Received collision message! Data: " + dataArr[1]);
                        Dispatch("collision", dataArr[1], false);
                    } else if (dataArr[0] == "collisionAck") {
                        print("Received collisionAck message! Data: " + dataArr[1]);
                        Dispatch("collisionAck", dataArr[1], false);
                    } else if (dataArr[0] == "fire") {
                        Dispatch("fire", dataArr[1], false);
                    } else if (dataArr[0] == "projectile_damage") {
                        Dispatch("projectile_damage", dataArr[1], false);
                    } else if (dataArr[0] == "weapon") {
                        Dispatch("weapon", dataArr[1], false);
                    } else if (dataArr[0] == "health_damage") {
                        print("Received message with data: 0 = " + dataArr[0]);
                        print("Received message with data: 1 = " + dataArr[1]);

                        Dispatch("health_damage", dataArr[1], false);
                    } else if (dataArr[0] == "disconnect" || dataArr[0] == "disconnected") {
                        print("Received message with data: 0 = " + dataArr[0]);
                        print("Received message with data: 1 = " + dataArr[1]);

                        Dispatch("disconnect", dataArr[1], false);
                    } else if (dataArr[0] == "name_registration") {
                        Dispatch("name_registration", dataArr[1], false);
                    } else if (dataArr[0] == "killed") {
                        Dispatch("killed", dataArr[1], false);
                    } else {
                        Dispatch("default", message, false);
                        //We will dequeue message and send to Dispatch function.
                    }
                }

            }
            yield return null; // yield return new WaitForSeconds(5); if too slow
        }
    }

    #region Dispatch

    public void Dispatch(string type, string msg, bool sendMsg)
    {
        // msg is a string json, so for on 'play' send name and spawn points: 
        // "{
        //     name: playerName,
        //     position: [x, y, z],
        //     rotation: [x, y, z],
        //     health: int,
        //     playerSpawnPoints: [
        //         { position: [ x, y, z], rotation: [x, y, z] },
        //         { position: [ x, y, z], rotation: [x, y, z] },
        //         { position: [ x, y, z], rotation: [x, y, z] },
        //         { position: [ x, y, z], rotation: [x, y, z] },
        //         { position: [ x, y, z], rotation: [x, y, z] },
        //         { position: [ x, y, z], rotation: [x, y, z] },
        //         { position: [ x, y, z], rotation: [x, y, z] },
        //         { position: [ x, y, z], rotation: [x, y, z] },
        //         { position: [ x, y, z], rotation: [x, y, z] },
        //         { position: [ x, y, z], rotation: [x, y, z] },
        //         { position: [ x, y, z], rotation: [x, y, z] }
        // }"
        //print("dispatching " + type + " with data: " + msg);
        if (type == "play") {
            if (sendMsg) {
                print("In Dispatch, sending 'play' to server");
                Send(type + " " + msg);
            } else {
                print("In Dispatch, received 'play' from server");
                OnPlay(msg);
            }
        } else if (type == "other_player_connected") {
            if (sendMsg) {
                Send(type + " " + msg);
            } else {
                OnOtherPlayerConnected(msg);
            }
        } else if (type == "move") {
            if (sendMsg) {
                Send(type + " " + msg);
            } else {
                OnPlayerMove(msg);
            }
        } else if (type == "ackMessage") {
            if (sendMsg) {
                Send(type + " " + msg);
            } else {
                OnAckMessage(msg);
            }
        }  else if (type == "collision") {
            if (sendMsg) {
                Send(type + " " + msg);
            } else {
                OnCollision(msg);
            }
        } else if (type == "collisionAck") {
            if (sendMsg) {
                print("Sending collisionAck message: " + type + " " + msg);
                Send(type + " " + msg);
            } else {
                OnCollisionAck(msg);
            }
        } else if (type == "fire") {
            if (sendMsg) {
                Send(type + " " + msg);
            } else {
                OnFire(msg);
            }
        } else if (type == "projectile_damage") {
            if (sendMsg) {
                Send(type + " " + msg);
            } else {
                OnProjectileDamage(msg);
            }
        } else if (type == "weapon") {
            if (sendMsg) {
                Send(type + " " + msg);
            } else {
                OnWeaponRotateAndFire(msg);
            }
        } else if (type == "health_damage") {
            if (sendMsg) {
                Send(type + " " + msg);
            } else {
                OnPlayerDamage(msg);
            }
        } else if (type == "disconnect" || type == "disconnected") {
            if (sendMsg) {
                Send(type + " " + msg);
            } else {
                OnOtherPlayerDisconnect(msg);
            }
        } else if (type == "killed" || type == "killed") {
            if (sendMsg) {
                Send(type + " " + msg);
            } else {
                OnPlayerKilled(msg);
            }
        } else if (type == "name_registration") {
            if (sendMsg) {
                print("Attempting to send name registration message...");
                Send(type + " " + msg);
            } else {
                print("Received name registration response message");
                OnNameRegistration(msg);
            }
        } else {
            print("Unrecognized message: " + msg);
            Send("dispatch " + msg);
        }
    }

    #endregion


    /**
     * This region contains all the action
     */
    #region Listening

    void OnOtherPlayerConnected(string data)
    {
        print("Another player joined Angry Alex.");
        UserJson userJson = UserJson.CreateFromJson(data);

        // Check to make sure we're not adding ourselves
        if (userJson.name != playerNameStr) {
            print("Player name: " + userJson.name);

            // Check to see if we don't already have this player in the game, return if we do
            GameObject obj = GameObject.Find(userJson.name) as GameObject;
            if (obj != null) {
                print("Found reference to obj associated with " + userJson.name + ", ending OnOtherPlayerConnected");
                return;
            }

            // Get position and rotation for new player
            Vector3 position = new Vector3(userJson.position[0], userJson.position[1], userJson.position[2]);
            Quaternion rotation = Quaternion.Euler(userJson.rotation[0], userJson.rotation[1], userJson.rotation[2]);

            // Assign vehicle and weapon
            int vehicle = userJson.vehicleSelection[0] > 0 ? userJson.vehicleSelection[0] : 1;
            int weapon = userJson.vehicleSelection[1] > 0 ? userJson.vehicleSelection[1] : 1;
            print("Other player selection: " + vehicle + " " + weapon);

            // Get a vehicle/weapon combo object to assign to our new player
            player = (GameObject)Resources.Load(_vehicleWeaponNames[new Tuple<int, int>(vehicle, weapon)]);
            player.name = userJson.name;
            //player.tag = "LocalPlayer";
            //base.LocalPlayer = player;

            // Instantiate player object at position and rotation, assign name to vehicle and gun objects
            GameObject p = Instantiate(player, position, rotation) as GameObject;
            p.name = userJson.name;
            Weapon gun = p.GetComponent<Weapon>();
            gun.playerName = userJson.name;
            print("Weapon player name: " + gun.playerName + " for player " + userJson.name);

            // Add health bar to vehicle
            HealthBar hb = p.GetComponent<HealthBar>();
            hb.carObject = p;
            hb.m_Slider_self = healthSlider;
            hb.m_Fill_self = healthFill;
            hb.playerName = userJson.name;
            print("Health bar player name: " + hb.playerName + " for player " + userJson.name);

            // Set leaderboard for new player
            print("About to update leaderboard for player " + userJson.name);
            //leaderboardManager.ChangeScore(userJson.name, "kills", userJson.killCount);
            print("Leaderboard updated for player " + userJson.name);

        }
    }

    void OnPlay(string data)
    {
        print("You have joined Angry Alex.");
        OnPlayJson playInfo = OnPlayJson.CreateFromJson(data);
        UserJson currentUserJson = playInfo.currentPlayer;
        UserJson[] players = playInfo.otherPlayers;
        print("pre-other player loop");
        // instantiate all current players as objects
        foreach (UserJson user in players) {
            print("currentplayername = " + playerNameStr);
            print("username = " + user.name);
            if (user.name != playerNameStr) {
                print("other player");
                GameObject obj = GameObject.Find(user.name) as GameObject;
                if (obj != null) {
                    return;
                }
                print("setting position");
                Vector3 pos = new Vector3(user.position[0], user.position[1], user.position[2]);
                Quaternion rot = Quaternion.Euler(user.rotation[0], user.rotation[1], user.rotation[2]);

                print("Instantiating other player: " + user.name);
                Tuple<int, int> vehicleSelection = new Tuple<int, int>(user.vehicleSelection[0], user.vehicleSelection[1]);
                print("Other players selection: " + user.vehicleSelection[0] + " " + user.vehicleSelection[1]);

                int vehicle = user.vehicleSelection[0] > 0 ? user.vehicleSelection[0] : 1;
                int weapon = user.vehicleSelection[1] > 0 ? user.vehicleSelection[1] : 1;

                player = (GameObject)Resources.Load(_vehicleWeaponNames[new Tuple<int, int>(vehicle, weapon)]);
                player.name = user.name;

                GameObject pOther = Instantiate(player, pos, rot) as GameObject;
                pOther.name = user.name;

                Weapon gun = pOther.GetComponent<Weapon>();
                gun.playerName = user.name;
                print("Weapon player name: " + gun.playerName + " for player " + user.name);

                HealthBar hb = pOther.GetComponent<HealthBar>();
                hb.carObject = pOther;
                hb.m_Slider_self = healthSlider;
                hb.m_Fill_self = healthFill;
                hb.playerName = user.name;
                print("Health bar player name: " + hb.playerName + " for player " + user.name);

                leaderboardManager.ChangeScore(user.name, "kills", user.killCount);
            }
        }
        print("Done with other players");
        // instantiate your own player object
        Vector3 position = new Vector3(currentUserJson.position[0], currentUserJson.position[1], currentUserJson.position[2]);
        Quaternion rotation = Quaternion.Euler(currentUserJson.rotation[0], currentUserJson.rotation[1], currentUserJson.rotation[2]);
        print("Vehicle loadout = " + Player.VehicleLoadout);
        try {
            print("Prefab name: " + _vehicleWeaponNames[Player.VehicleLoadout]);
            player = (GameObject)Resources.Load(_vehicleWeaponNames[Player.VehicleLoadout]);
            print("Player loadout = " + player);

            player.name = playerNameStr;
            print("Player name: " + player.name);
            GameObject p = Instantiate(player, position, rotation) as GameObject;
            //UserName = playerNameStr;
            p.name = playerNameStr;
            print("P.name = " + p.name);
            Camera[] camArr = Camera.allCameras;
            foreach (Camera cam in camArr) {
                CameraController cc = cam.GetComponent<CameraController>();
                cc.isLocalPlayer = true;
                cc.target = p.GetComponent<Rigidbody2D>();
            }
            print("here2");
            CarController pc = p.GetComponent<CarController>();
            pc.setLocalPlayer();
            print("here3");

            HealthBar hb = p.GetComponent<HealthBar>();
            hb.carObject = p;
            hb.isLocalPlayer = true;
            //hb.m_Slider = healthSlider;
            //hb.m_Fill = healthFill;
            hb.playerName = playerNameStr;
            print("Health bar player name: " + hb.playerName + " for player " + playerNameStr);

            Weapon gun = p.GetComponent<Weapon>();
            gun.isLocalPlayer = true;
            gun.playerName = playerNameStr;
            print("Weapon player name: " + gun.playerName + " for player " + playerNameStr);
            leaderboardManager.ChangeScore(p.name, "kills", 0);
        }
        catch (Exception e) {
            print(e.Message);
        }

    }

    void OnPlayerMove(string data)
    {
        //print("WebSocketManager OnPlayerMove: START");
        //print("WebSocketManager OnPlayerMove: Received string: " + data);
        UserJson userJSON = UserJson.CreateFromJson(data);
        // if it is the current player, exit
        if (userJSON.name == playerNameStr) {
            return;
        }
        GameObject p = GameObject.Find(userJSON.name) as GameObject;
        if (p != null) {
            //print("Got userJSON: " + userJSON);
            Vector3 position = new Vector3(userJSON.position[0], userJSON.position[1], userJSON.position[2]);
            Vector2 velocity = new Vector2(userJSON.velocity[0], userJSON.velocity[1]);
            Vector2 acceleration = new Vector2(userJSON.acceleration[0], userJSON.acceleration[1]);
            Quaternion rotation = Quaternion.Euler(userJSON.rotation[0], userJSON.rotation[1], userJSON.rotation[2]);
            CarController carController = p.GetComponent<CarController>();
            carController.updateDestination(position, velocity, acceleration, rotation);
            // Send back an ack to the player that sent this message to get a RTT estimate
            simStepAckJson simStepAck = new simStepAckJson( userJSON.simulationStep, userJSON.name );
            Dispatch("ackMessage", JsonUtility.ToJson(simStepAck), true);
        }

    }

    void OnAckMessage(string data)
    {
        print("WebSocketManager OnAckMessage: START");
        UserJson userJSON = UserJson.CreateFromJson(data);
        // We only care to continue if this is the current player
        if (userJSON.name == playerNameStr)
        {
            GameObject p = GameObject.Find(userJSON.name) as GameObject;
            if (p != null)
            {
                print("Got userJSON: " + userJSON);
                CarController carController = p.GetComponent<CarController>();
                carController.processAcknowledgement(userJSON.simulationStep);
            }
            return;
        }
    }

    void OnCollision(string data)
    {
        print("WebSocketManager OnCollision: START, data:\n" + data);
        UserJson userJSON = UserJson.CreateFromJson(data);
        // We only care to continue if this is the remote player
        if (userJSON.name != playerNameStr)
        {
            GameObject p = GameObject.Find(userJSON.name) as GameObject;
            if (p != null)
            {
                print("Got userJSON: " + userJSON);
                // Set remote car player's position
                Vector3 position = new Vector3(userJSON.position[0], userJSON.position[1], userJSON.position[2]);
                Vector2 velocity = new Vector2(userJSON.velocity[0], userJSON.velocity[1]);
                Vector2 acceleration = new Vector2(userJSON.acceleration[0], userJSON.acceleration[1]);
                Quaternion rotation = Quaternion.Euler(userJSON.rotation[0], userJSON.rotation[1], userJSON.rotation[2]);
                CarController carController = p.GetComponent<CarController>();
                carController.setCollisionPosVelAccRot(position, velocity, acceleration, rotation);

                // Set ourselves back by RTT/2 and wait RTT/2 before resuming simulation
                GameObject selfObj = GameObject.Find(playerNameStr) as GameObject;
                CarController selfCarController = selfObj.GetComponent<CarController>();
                if (selfCarController.inCollision) // If we are already in a collision, we can resolve now
                {
                    print("We were already in collision, set lockStep to false and startBlendCollision");
                    selfCarController.lockStep = false;
                    selfCarController.startBlendCollision();
                } else { // Otherwise, we need to go back half a RTT and see where we were when the other player said we collided
                    print("We were not aware of a collision, sending new collision with prior position");
                    selfCarController.inCollision = true;
                    int updateIndex = selfCarController.currentSimulationStep - selfCarController.rtt;
                    CarController.PositionVelocityAccelerationRotationJson updateAtCollision = selfCarController.updateBuffer[updateIndex];
                    Dispatch("collision", JsonUtility.ToJson(updateAtCollision), true);
                }
            }
            return;
        }
    }

    void OnCollisionAck(string data)
    {
        print("WebSocketManager OnCollisionAck: START, data:\n" + data);
        UserJson userJSON = UserJson.CreateFromJson(data);
        // We only care to continue if this is the remote player
        if (userJSON.name != playerNameStr)
        {
            GameObject p = GameObject.Find(userJSON.name) as GameObject;
            if (p != null)
            {
                print("Got userJSON: " + userJSON);
                // Set remote car player's position
                Vector3 position = new Vector3(userJSON.position[0], userJSON.position[1], userJSON.position[2]);
                Vector2 velocity = new Vector2(userJSON.velocity[0], userJSON.velocity[1]);
                Vector2 acceleration = new Vector2(userJSON.acceleration[0], userJSON.acceleration[1]);
                Quaternion rotation = Quaternion.Euler(userJSON.rotation[0], userJSON.rotation[1], userJSON.rotation[2]);
                CarController carController = p.GetComponent<CarController>();
                carController.setCollisionPosVelAccRot(position, velocity, acceleration, rotation);
                // Set ourselves back by RTT/2 and wait RTT/2 before resuming simulation
            }
            return;
        }
    }

    private void OnFire(string data)
    {
        UserJson userJSON = UserJson.CreateFromJson(data);
        // if it is the current player exit
        if (userJSON.name == playerNameStr)
        {
            return;
        }
        Quaternion rotation = Quaternion.Euler(userJSON.weapon.rotation[0], userJSON.weapon.rotation[1], userJSON.weapon.rotation[2]);
        GameObject p = GameObject.Find(userJSON.name) as GameObject;
        if (p != null)
        {
            Weapon weapon = p.GetComponentInChildren<Weapon>();
            weapon.transform.rotation = rotation;
            weapon.fireWeapon();
        }
    }

    private void OnProjectileDamage(string data)
    {
        HealthChangeJson hcJSON = HealthChangeJson.CreateFromJson(data);

        GameObject playerDealtTo = GameObject.Find(hcJSON.name);
        if (playerDealtTo != null)
        {
            //Works for local player receiving damage from bullets
            //Possibly improve on this area so that damage is applied to enemy instances?
            HealthBar dealtToHealthBar = playerDealtTo.GetComponent<HealthBar>();
            dealtToHealthBar.TakeDamage(hcJSON.damage);
            if (hcJSON.killerName != null)
            {
                leaderboardManager.ChangeScore(hcJSON.killerName, "kills", hcJSON.killerCount);
            }
        }
    }



    void OnPlayerDamage(string data)
    {
        print("Player was damaged");
        UserJson userJson = UserJson.CreateFromJson(data);

        // todo player damage, use UserHealthJson or HealthChangeJson?
        // send message with damage = true and calculate damage and possible kill in server



    }


    void OnWeaponRotateAndFire(string data)
    {
        print("Player weapon rotated and possibly fired");
        UserJson userJson = UserJson.CreateFromJson(data);

        // todo weapon rotates and fires (true/false), use or rework BulletJson?

    }


    void OnOtherPlayerDisconnect(string data)
    {
        UserJson userJson = UserJson.CreateFromJson(data);
        print("Player disconnected: " + userJson.name);
        Destroy(GameObject.Find(userJson.name));

        leaderboardManager.ChangeScore(userJson.name, "kills", -1);
            
    }

    void OnPlayerKilled(string data)
    {
        HealthChangeJson userJson = HealthChangeJson.CreateFromJson(data);
        print("Player killed another player: " + userJson.killerName);
        Destroy(GameObject.Find(userJson.name));

        leaderboardManager.ChangeScore(userJson.killerName, "kills", userJson.killerCount);
        
    }


    void OnNameRegistration(string data)
    {
        print("Name Registration Msg Received");
        NameRegistrationJson nameRegistrationJson = NameRegistrationJson.CreateFromJson(data);

        if (nameRegistrationJson.name_registration_success)
        {
            print("Successfully registered name with server!");
            playerNameStr = UserName;
            SceneManager.LoadScene(1);
            print("Vehicle/weapon scene loaded");
            //List<SpawnPoint> playerSpawnPoints = GetComponent<PlayerSpawner>().playerSpawnPoints;
            //print("Current vehicle selection: " + Player.VehicleLoadout.Item1 + "" + Player.VehicleLoadout.Item2);
            //PlayerJson playerJson = new PlayerJson(playerNameStr, playerSpawnPoints, Player.VehicleLoadout);
            //string playerData = JsonUtility.ToJson(playerJson);
            //print("Dispatching 'play' to server");
            //Dispatch("play", playerData, true);
        } else
        {
            print("ERROR: Failed to register name with server");
            errorMessage.transform.localScale = new Vector3(1, 1, 1);
        }
    }


    #endregion
     


    #region JsonMessageClasses

    [Serializable]
    public class PlayerJson
    {
        public string name;
        public List<PointJson> playerSpawnPoints;
        public int[] vehicleSelection;

        public PlayerJson(string _name, List<SpawnPoint> _playerSpawnPoints, Tuple<int, int> _vehicleSelection)
        {
            playerSpawnPoints = new List<PointJson>();
            name = _name;
            foreach (SpawnPoint playerSpawnPoint in _playerSpawnPoints)
            {
                PointJson pointJson = new PointJson(playerSpawnPoint);
                playerSpawnPoints.Add(pointJson);
            }

            vehicleSelection = new int[2];
            vehicleSelection[0] = _vehicleSelection.Item1;
            vehicleSelection[1] = _vehicleSelection.Item2;
        }
    }

    [Serializable]
    public class PointJson
    {
        public float[] position;
        public float[] rotation;
        public PointJson(SpawnPoint spawnPoint)
        {
            position = new float[] {
                    spawnPoint.transform.position.x,
                    spawnPoint.transform.position.y,
                    spawnPoint.transform.position.z
                };
            rotation = new float[] {
                    spawnPoint.transform.eulerAngles.x,
                    spawnPoint.transform.eulerAngles.y,
                    spawnPoint.transform.eulerAngles.z
                };


        }
    }

    [Serializable]
    public class PositionJson
    {
        public float[] position;
        public PositionJson(Vector3 _position)
        {
            position = new float[] { _position.x, _position.y, _position.z };

        }
    }

    [Serializable]
    public class RotationJson
    {
        public float[] rotation;
        public RotationJson(Quaternion _rotation)
        {
            rotation = new float[] { _rotation.eulerAngles.x, _rotation.eulerAngles.y, _rotation.eulerAngles.z };

        }
    }

    [Serializable]
    public class PositionRotationJson
    {
        public float[] position;
        public float[] rotation;
        public PositionRotationJson(Vector3 _position, Quaternion _rotation)
        {
            position = new float[] { _position.x, _position.y, _position.z };
            rotation = new float[] { _rotation.eulerAngles.x, _rotation.eulerAngles.y, _rotation.eulerAngles.z };
        }
    }

    [Serializable]
    public class simStepAckJson
    {
        public int simulationStep;
        public string name;
        public simStepAckJson(int _simulationStep, string _name)
        {
            simulationStep = _simulationStep;
            name = _name;
        }
    }

    [Serializable]
    public class PositionVelocityAccelerationRotationJson
    {
        public int simulationStep;
        public float[] position;
        public float[] velocity;
        public float[] acceleration;
        public float[] rotation;
        public PositionVelocityAccelerationRotationJson(int _simulationStep, Vector3 _position, Vector2 _velocity, Vector2 _acceleration, Quaternion _rotation)
        {
            simulationStep = _simulationStep;
            position = new float[] { _position.x, _position.y, _position.z };
            velocity = new float[] { _velocity.x, _velocity.y };
            acceleration = new float[] { _acceleration.x, _acceleration.y };
            rotation = new float[] { _rotation.eulerAngles.x, _rotation.eulerAngles.y, _rotation.eulerAngles.z };
        }

    }


    [Serializable]
    public class UserJson
    {
        public string name;
        public float[] position;
        public float[] velocity;
        public float[] acceleration;
        public float[] rotation;
        public int simulationStep;
        public int health;
        public int killCount;
        public WeaponJson weapon;
        public int[] vehicleSelection;

        public UserJson(string _name)
        {
            name = _name;
        }

        public static UserJson CreateFromJson(string data)
        {
            return JsonUtility.FromJson<UserJson>(data);
        }
    }

    [Serializable]
    public class OnPlayJson
    {
        public UserJson currentPlayer;
        public UserJson[] otherPlayers;

        public static OnPlayJson CreateFromJson(string data)
        {
            return JsonUtility.FromJson<OnPlayJson>(data);
        }
    }


    [Serializable]
    public class WeaponJson
    {
        public float[] rotation;
        public bool fireBullet;


        public static WeaponJson CreateFromJson(string data)
        {
            return JsonUtility.FromJson<WeaponJson>(data);
        }
    }



    // todo finish health json
    [Serializable]
    public class HealthChangeJson
    {
        public string name;
        public int damage;
        public string from;
        public string killerName;
        public int killerCount;
        // todo add damage from enemy?

        public HealthChangeJson(string _name, int _damage, string _from)
        {
            name = _name;
            damage = _damage;
            from = _from;
        }

        // when player is killed
        public HealthChangeJson(string _name, string _from)
        {
            name = _name;
            from = _from;
        }

        // updating player kill count
        public HealthChangeJson(string _name, string _from, string _killerName, int _killerCount)
        {
            name = _name;
            from = _from;
            killerName = _killerName;
            killerCount = _killerCount;
        }


        public static HealthChangeJson CreateFromJson(string data)
        {
            return JsonUtility.FromJson<HealthChangeJson>(data);
        }
    }

    // todo add enemy json ? 

    // todo add shoot json for when players shoot stuff 

    [Serializable]
    public class BulletJson
    {
        public string name;

        public static BulletJson CreateFromJson(string data)
        {
            return JsonUtility.FromJson<BulletJson>(data);
        }
    }

    [Serializable]
    public class UserHealthJson
    {
        public string name;
        public int health;

        public static UserHealthJson CreateFromJson(string data)
        {
            return JsonUtility.FromJson<UserHealthJson>(data);
        }

    }
    
    [Serializable]
    public class NameRegistrationJson
    {
        [FormerlySerializedAs("registrationSuccessful")] public Boolean name_registration_success;
        public string name;

        public NameRegistrationJson(string _name)
        {
            name = _name;
        }
        
        public static NameRegistrationJson CreateFromJson(string data)
        {
            return JsonUtility.FromJson<NameRegistrationJson>(data);
        }

    }


    #endregion


}
