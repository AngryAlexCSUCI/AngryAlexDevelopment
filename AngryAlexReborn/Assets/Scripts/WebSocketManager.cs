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
     
    void Start()
    {
        print("Starting web socket..");
        playerNameStr = UserName;
        StartCoroutine("RecvEvent");    //Then run the receive message loop
    }


    //Receive message loop function
    IEnumerator RecvEvent()
    {
        print("Starting coroutine.");
        InitWebSocket("ws://ec2-3-84-148-203.compute-1.amazonaws.com:8080"); //First we create the connection.
        //InitWebSocket("ws://localhost:8080"); //First we create the connection.
        //InitWebSocket("ws://ec2-3-85-119-215.compute-1.amazonaws.com:8080"); //TEMPORARY TEST CONNECTION FOR CHRISTIAN'S EC2.

        while (true)
        {
            if (recvList.Count > 0)
            {
                //When our message queue has string coming.
                // check message type: play, other_player_connected, move, turn, disconnect
                string message = recvList.Dequeue();
                //                print("Received message: " + message);

                if (message == "You are connected to the server!" && !skipPlay)
                {
                    print("--- Sending play message!");
                    // send server player name and let server randomly pick spawn point from list of spawn points
                    // spawn point for player
                    List<SpawnPoint> playerSpawnPoints = GetComponent<PlayerSpawner>().playerSpawnPoints;
                    print("Current vehicle selection: " + Player.VehicleLoadout.Item1 + "" + Player.VehicleLoadout.Item2);
                    PlayerJson playerJson = new PlayerJson(playerNameStr, playerSpawnPoints, Player.VehicleLoadout);
                    string data = JsonUtility.ToJson(playerJson);
                    Dispatch("play", data, true);
                }
                else
                {
                    string[] dataArr = message.Split(' ');
                    
                    print("Received message with data: 0 = " + dataArr[0]);
                    print("1 = " + dataArr[1]);
                    
                    if (dataArr[0] == "play")
                    {
                        // receive play from server means you need to get the spawn point from here and assign to player. 
                        Dispatch("play", dataArr[1], false);
                    }
                    else if (dataArr[0] == "other_player_connected")
                    {
                        // receive other player connected means you need to spawn another player in that spot to represent them
                        Dispatch("other_player_connected", dataArr[1], false);
                    }
                    else if (dataArr[0] == "move")
                    {
                        Dispatch("move", dataArr[1], false);
                    }
                    else if (dataArr[0] == "wpressed")
                    {
                        Dispatch("wpressed", dataArr[1], false);
                    }
                    else if (dataArr[0] == "wrelease")
                    {
                        Dispatch("wrelease", dataArr[1], false);
                    }
                    else if (dataArr[0] == "spressed")
                    {
                        Dispatch("spressed", dataArr[1], false);
                    }
                    else if (dataArr[0] == "srelease")
                    {
                        Dispatch("srelease", dataArr[1], false);
                    }
                    else if (dataArr[0] == "apressed")
                    {
                        Dispatch("apressed", dataArr[1], false);
                    }
                    else if (dataArr[0] == "arelease")
                    {
                        Dispatch("arelease", dataArr[1], false);
                    }
                    else if (dataArr[0] == "dpressed")
                    {
                        Dispatch("dpressed", dataArr[1], false);
                    }
                    else if (dataArr[0] == "drelease")
                    {
                        Dispatch("drelease", dataArr[1], false);
                    }
                    else if (dataArr[0] == "fire")
                    {
                        Dispatch("fire", dataArr[1], false);
                    }
                    else if (dataArr[0] == "projectile_damage")
                    {
                        Dispatch("projectile_damage", dataArr[1], false);
                    }
                    else if (dataArr[0] == "turn")
                    {
                        Dispatch("turn", dataArr[1], false);
                    }
                    else if (dataArr[0] == "weapon")
                    {
                        Dispatch("turn", dataArr[1], false);
                    }
                    else if (dataArr[0] == "health_damage")
                    {
                        Dispatch("turn", dataArr[1], false);
                    }
                    else if (dataArr[0] == "disconnect")
                    {
                        Dispatch("disconnect", dataArr[1], false);
                    }
                    else if (dataArr[0] == "name_registration")
                    {
                        Dispatch("name_registration", dataArr[1], false);
                    }
                    else
                    {
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
        print("dispatching " + type + " with data: " + msg);
        if (type == "play")
        {
            if (sendMsg)
            {
                Send(type + " " + msg);
            }
            else
            {
                OnPlay(msg);
            }
        }
        else if (type == "other_player_connected")
        {
            if (sendMsg)
            {
                Send(type + " " + msg);
            }
            else
            {
                OnOtherPlayerConnected(msg);
            }
        }
        else if (type == "move")
        {
            if (sendMsg)
            {
                Send(type + " " + msg);
            }
            else
            {
                OnPlayerMove(msg);
            }
        }
        else if (type == "wpressed")
        {
            if (sendMsg)
            {
                Send(type + " " + msg);
            }
            else
            {
                OnWPressed(msg);
            }
        }
        else if (type == "wrelease")
        {
            if (sendMsg)
            {
                Send(type + " " + msg);
            }
            else
            {
                OnWRelease(msg);
            }
        }
        else if (type == "spressed")
        {
            if (sendMsg)
            {
                Send(type + " " + msg);
            }
            else
            {
                OnSPressed(msg);
            }
        }
        else if (type == "srelease")
        {
            if (sendMsg)
            {
                Send(type + " " + msg);
            }
            else
            {
                OnSRelease(msg);
            }
        }
        else if (type == "apressed")
        {
            if (sendMsg)
            {
                Send(type + " " + msg);
            }
            else
            {
                OnAPressed(msg);
            }
        }
        else if (type == "arelease")
        {
            if (sendMsg)
            {
                Send(type + " " + msg);
            }
            else
            {
                OnARelease(msg);
            }
        }
        else if (type == "dpressed")
        {
            if (sendMsg)
            {
                Send(type + " " + msg);
            }
            else
            {
                OnDPressed(msg);
            }
        }
        else if (type == "drelease")
        {
            if (sendMsg)
            {
                Send(type + " " + msg);
            }
            else
            {
                OnDRelease(msg);
            }
        }
        else if (type == "fire")
        {
            if (sendMsg)
            {
                Send(type + " " + msg);
            }
            else
            {
                OnFire(msg);
            }
        }
        else if (type == "projectile_damage")
        {
            if (sendMsg)
            {
                Send(type + " " + msg);
            }
            else
            {
                OnProjectileDamage(msg);
            }
        }
        else if (type == "turn") {
            if (sendMsg) {
                Send(type + " " + msg);
            }
            else
            {
                OnPlayerRotate(msg);
            }
        }
        else if (type == "weapon")
        {
            if (sendMsg)
            {
                Send(type + " " + msg);
            }
            else
            {
                OnWeaponRotateAndFire(msg);
            }
        }
        else if (type == "health_damage")
        {
            if (sendMsg)
            {
                Send(type + " " + msg);
            }
            else
            {
                OnPlayerDamage(msg);
            }
        }
        else if (type == "disconnect")
        {
            if (sendMsg)
            {
                Send(type + " " + msg);
            }
            else
            {
                OnOtherPlayerDisconnect(msg);
            }
        }
        else if (type == "name_registration")
        {
            if (sendMsg)
            {
                print("Attempting to send name registration message...");
                Send(type + " " + msg);
            }
            else
            {
                print("Received name registration response message");
                OnNameRegistration(msg);
            }
        }
        else
        {
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
        // todo send message with current active players when other player connects to all other users
        print("Another player joined Angry Alex.");
        UserJson userJson = UserJson.CreateFromJson(data);
        if (userJson.name != playerNameStr)
        {
            print("Player name: " + userJson.name);
            Vector3 position = new Vector3(userJson.position[0], userJson.position[1], userJson.position[2]);
            Quaternion rotation = Quaternion.Euler(userJson.rotation[0], userJson.rotation[1], userJson.rotation[2]);

            GameObject obj = GameObject.Find(userJson.name) as GameObject;
            if (obj != null)
            {
                print("Found reference to obj associated with " + userJson.name + ", ending OnOtherPlayerConnected");
                return;
            }
            int vehicle = userJson.vehicleSelection[0] > 0 ? userJson.vehicleSelection[0] : 1;
            int weapon = userJson.vehicleSelection[1] > 0 ? userJson.vehicleSelection[1] : 1;
            
            print("Other player selection: " + vehicle + " " + weapon);

            player = (GameObject)Resources.Load(_vehicleWeaponNames[new Tuple<int, int>(vehicle, weapon)]);
            player.name = userJson.name;
            //player.tag = "LocalPlayer";
            //base.LocalPlayer = player;

            // todo need to get player vehicle type from json and use that to determine player type
            GameObject p = Instantiate(player, position, rotation) as GameObject;
            p.name = userJson.name;
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
        foreach (UserJson user in players)
        {
            print("currentplayername = " + playerNameStr);
            print("username = " + user.name);
            if (user.name != playerNameStr)
            {
                print("other player");
                GameObject obj = GameObject.Find(user.name) as GameObject;
                if (obj != null)
                {
                    return;
                }
                print("setting position");
                Vector3 pos = new Vector3(user.position[0], user.position[1], user.position[2]);
                Quaternion rot = Quaternion.Euler(user.rotation[0], user.rotation[1], user.rotation[2]);

                // todo need to get player vehicle type from json and use that to determine player type
                print("Instantiating other player: " + user.name);
                Tuple<int, int> vehicleSelection = new Tuple<int, int>(user.vehicleSelection[0], user.vehicleSelection[1]);
                print("Other players selection: " + user.vehicleSelection[0] + " " + user.vehicleSelection[1]);

                int vehicle = user.vehicleSelection[0] > 0 ? user.vehicleSelection[0] : 1;
                int weapon = user.vehicleSelection[1] > 0 ? user.vehicleSelection[1] : 1;

                player = (GameObject)Resources.Load(_vehicleWeaponNames[new Tuple<int, int>(vehicle, weapon)]);
                player.name = user.name;

                GameObject pTemp = Instantiate(player, pos, rot) as GameObject;
                pTemp.name = user.name;
            }
        }
        print("Done with other players");
        // instantiate your own player object
        Vector3 position = new Vector3(currentUserJson.position[0], currentUserJson.position[1], currentUserJson.position[2]);
        Quaternion rotation = Quaternion.Euler(currentUserJson.rotation[0], currentUserJson.rotation[1], currentUserJson.rotation[2]);
        print("Vehicle loadout = " + Player.VehicleLoadout);
        try
        {
            print("Prefab name: " + _vehicleWeaponNames[Player.VehicleLoadout]);
            // todo need to get player vehicle type from json and use that to determine player type
            player = (GameObject)Resources.Load(_vehicleWeaponNames[Player.VehicleLoadout]);
            print("Player loadout = " + player);

            player.name = playerNameStr;
            print("Player name: " + player.name);
            GameObject p = Instantiate(player, position, rotation) as GameObject;
            //UserName = playerNameStr;
            p.name = playerNameStr;
            print("P.name = " + p.name);
            Camera[] camArr = Camera.allCameras;
            foreach (Camera cam in camArr)
            {
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
            hb.m_Slider = healthSlider;
            hb.m_Fill = healthFill;
            print("here4");

            Weapon gun = p.GetComponent<Weapon>();
            gun.isLocalPlayer = true;
            print("here5");
        }
        catch (Exception e)
        {
            print(e.Message);
        }

    }

    void OnPlayerMove(string data)
    {
        UserJson userJSON = UserJson.CreateFromJson(data);
        Vector3 position = new Vector3(userJSON.position[0], userJSON.position[1], userJSON.position[2]);
        // if it is the current player exit
        if (userJSON.name == playerNameStr)
        {
            return;
        }
        GameObject p = GameObject.Find(userJSON.name) as GameObject;
        if (p != null)
        {
            p.transform.position = position;
        }

    }

    private void OnWPressed(string data)
    {
        UserJson userJSON = UserJson.CreateFromJson(data);
        // if it is the current player exit
        if (userJSON.name == playerNameStr)
        {
            return;
        }
        Vector3 position = new Vector3(userJSON.position[0], userJSON.position[1], userJSON.position[2]);
        GameObject p = GameObject.Find(userJSON.name) as GameObject;
        if (p != null)
        {
            p.transform.position = position;
            CarController carController = p.GetComponent<CarController>();
            carController.setWPressed(true);
        }
    }

    private void OnWRelease(string data)
    {
        UserJson userJSON = UserJson.CreateFromJson(data);
        // if it is the current player exit
        if (userJSON.name == playerNameStr)
        {
            return;
        }
        Vector3 position = new Vector3(userJSON.position[0], userJSON.position[1], userJSON.position[2]);
        GameObject p = GameObject.Find(userJSON.name) as GameObject;
        if (p != null)
        {
            p.transform.position = position;
            CarController carController = p.GetComponent<CarController>();
            carController.setWPressed(false);
        }
    }

    private void OnSPressed(string data)
    {
        UserJson userJSON = UserJson.CreateFromJson(data);
        // if it is the current player exit
        if (userJSON.name == playerNameStr)
        {
            return;
        }
        Vector3 position = new Vector3(userJSON.position[0], userJSON.position[1], userJSON.position[2]);
        GameObject p = GameObject.Find(userJSON.name) as GameObject;
        if (p != null)
        {
            p.transform.position = position;
            CarController carController = p.GetComponent<CarController>();
            carController.setSPressed(true);
        }
    }

    private void OnSRelease(string data)
    {
        UserJson userJSON = UserJson.CreateFromJson(data);
        // if it is the current player exit
        if (userJSON.name == playerNameStr)
        {
            return;
        }
        Vector3 position = new Vector3(userJSON.position[0], userJSON.position[1], userJSON.position[2]);
        GameObject p = GameObject.Find(userJSON.name) as GameObject;
        if (p != null)
        {
            p.transform.position = position;
            CarController carController = p.GetComponent<CarController>();
            carController.setSPressed(false);
        }
    }



    void OnPlayerRotate(string data)
    {
        UserJson userJSON = UserJson.CreateFromJson(data);
        Quaternion rotation = Quaternion.Euler(userJSON.rotation[0], userJSON.rotation[1], userJSON.rotation[2]);
        // if it is the current player exit
        if (userJSON.name == playerNameStr)
        {
            return;
        }
        GameObject p = GameObject.Find(userJSON.name) as GameObject;
        if (p != null)
        {
            p.transform.rotation = rotation;
        }
    }

    private void OnAPressed(string data)
    {
        UserJson userJSON = UserJson.CreateFromJson(data);
        // if it is the current player exit
        if (userJSON.name == playerNameStr)
        {
            return;
        }
        Quaternion rotation = Quaternion.Euler(userJSON.rotation[0], userJSON.rotation[1], userJSON.rotation[2]);
        GameObject p = GameObject.Find(userJSON.name) as GameObject;
        if (p != null)
        {
            p.transform.rotation = rotation;
            CarController carController = p.GetComponent<CarController>();
            carController.setAPressed(true);
        }
    }

    private void OnARelease(string data)
    {
        UserJson userJSON = UserJson.CreateFromJson(data);
        // if it is the current player exit
        if (userJSON.name == playerNameStr)
        {
            return;
        }
        Quaternion rotation = Quaternion.Euler(userJSON.rotation[0], userJSON.rotation[1], userJSON.rotation[2]);
        GameObject p = GameObject.Find(userJSON.name) as GameObject;
        if (p != null)
        {
            p.transform.rotation = rotation;
            CarController carController = p.GetComponent<CarController>();
            carController.setAPressed(false);
        }
    }

    private void OnDPressed(string data)
    {
        UserJson userJSON = UserJson.CreateFromJson(data);
        // if it is the current player exit
        if (userJSON.name == playerNameStr)
        {
            return;
        }
        Quaternion rotation = Quaternion.Euler(userJSON.rotation[0], userJSON.rotation[1], userJSON.rotation[2]);
        GameObject p = GameObject.Find(userJSON.name) as GameObject;
        if (p != null)
        {
            p.transform.rotation = rotation;
            CarController carController = p.GetComponent<CarController>();
            carController.setDPressed(true);
        }
    }

    private void OnDRelease(string data)
    {
        UserJson userJSON = UserJson.CreateFromJson(data);
        // if it is the current player exit
        if (userJSON.name == playerNameStr)
        {
            return;
        }
        Quaternion rotation = Quaternion.Euler(userJSON.rotation[0], userJSON.rotation[1], userJSON.rotation[2]);
        GameObject p = GameObject.Find(userJSON.name) as GameObject;
        if (p != null)
        {
            p.transform.rotation = rotation;
            CarController carController = p.GetComponent<CarController>();
            carController.setDPressed(false);
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
            HealthBar[] dealtToHealthBars = playerDealtTo.GetComponents<HealthBar>();
            foreach (HealthBar healthBar in dealtToHealthBars)
            {
                healthBar.TakeDamage(hcJSON.damage);
            }
        }
    }

    void OnPlayerDamage(string data)
    {
        print("Player was damaged");
        UserJson userJson = UserJson.CreateFromJson(data);

        // todo player damage, use UserHealthJson or HealthChangeJson?
        // include damage calculation here for player then send message



    }


    void OnWeaponRotateAndFire(string data)
    {
        print("Player weapon rotated and possibly fired");
        UserJson userJson = UserJson.CreateFromJson(data);

        // todo weapon rotates and fires (true/false), use or rework BulletJson?

    }


    void OnOtherPlayerDisconnect(string data)
    {
        print("Player disconnected");
        UserJson userJson = UserJson.CreateFromJson(data);
        Destroy(GameObject.Find(userJson.name));
    }


    void OnNameRegistration(string data)
    {
        print("Name Registration Msg Received");
        NameRegistrationJson nameRegistrationJson = NameRegistrationJson.CreateFromJson(data);

        if (nameRegistrationJson.name_registration_success)
        {
            SceneManager.LoadScene(1);
        }
        else
        {
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
    public class UserJson
    {
        public string name;
        public float[] position;
        public float[] rotation;
        public int health;
        public WeaponJson weapon;
        public int[] vehicleSelection;

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
        // todo add damage from enemy?

        public HealthChangeJson(string _name, int _damage, string _from)
        {
            name = _name;
            damage = _damage;
            from = _from;
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
