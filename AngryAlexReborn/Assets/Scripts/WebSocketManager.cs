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
using UnityEngine.UI;

public class WebSocketManager : MonoBehaviour
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
    private string playerNameStr = Player.UserName;
    public GameObject player;
    public Slider healthSlider;
    public Image healthFill;

    private Dictionary<Tuple<int, int>, string> _vehicleWeaponNames = new Dictionary<Tuple<int, int>, string>()
    {
        { new Tuple<int, int>(1,1), "CannonCar" },
        { new Tuple<int, int>(2,1), "CannonTruck" },
        { new Tuple<int, int>(3,1), "CannonMotorcycle" },
        { new Tuple<int, int>(1,2), "MachinegunCar" },
        { new Tuple<int, int>(2,2), "MachinegunTruck" },
        { new Tuple<int, int>(3,2), "MachinegunMotorcycle" }
    };

    void Start()
    {
        print("Starting web socket..");

        StartCoroutine("RecvEvent");    //Then run the receive message loop
    }


    //Receive message loop function
    IEnumerator RecvEvent()
    {
        print("Starting coroutine.");
        InitWebSocket("ws://ec2-3-84-148-203.compute-1.amazonaws.com:8080"); //First we create the connection.

        while (true)
        {
            if (recvList.Count > 0)
            {
                //When our message queue has string coming.
                // check message type: play, other_player_connected, move, turn, disconnect
                string message = recvList.Dequeue();
                //                print("Received message: " + message);

                if (message == "You are connected to the server!")
                {
                    // send server player name and let server randomly pick spawn point from list of spawn points
                    // spawn point for player
                    List<SpawnPoint> playerSpawnPoints = GetComponent<PlayerSpawner>().playerSpawnPoints;
                    PlayerJson playerJson = new PlayerJson(playerNameStr, playerSpawnPoints);
                    string data = JsonUtility.ToJson(playerJson);
                    Dispatch("play", data, true);
                }
                else
                {

                    string[] dataArr = message.Split(' ');
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
        else if (type == "turn")
        {
            if (sendMsg)
            {
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
        else
        {
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
        Vector3 position = new Vector3(userJson.position[0], userJson.position[1], userJson.position[2]);
        Quaternion rotation = Quaternion.Euler(userJson.rotation[0], userJson.rotation[1], userJson.rotation[2]);

        GameObject obj = GameObject.Find(userJson.name) as GameObject;
        if (obj != null)
        {
            return;
        }

        player = (GameObject)Resources.Load(_vehicleWeaponNames[Player.VehicleLoadout]);
        // todo need to get player vehicle type from json and use that to determine player type
        GameObject p = Instantiate(player, position, rotation) as GameObject;
        p.name = userJson.name;
    }

    void OnPlay(string data)
    {
        print("You have joined Angry Alex.");
        OnPlayJson playInfo = OnPlayJson.CreateFromJson(data);
        UserJson currentUserJson = playInfo.currentPlayer;
        UserJson[] players = playInfo.otherPlayers;

        // instantiate all current players as objects
        foreach (UserJson user in players)
        {
            GameObject obj = GameObject.Find(user.name) as GameObject;
            if (obj != null)
            {
                return;
            }
            Vector3 pos = new Vector3(user.position[0], user.position[1], user.position[2]);
            Quaternion rot = Quaternion.Euler(user.rotation[0], user.rotation[1], user.rotation[2]);

            // todo need to get player vehicle type from json and use that to determine player type
            print("Instantiating other player: " + user.name);
            
            player = (GameObject)Resources.Load(_vehicleWeaponNames[Player.VehicleLoadout]);

            GameObject pTemp = Instantiate(player, pos, rot) as GameObject;
            pTemp.name = user.name;
        }

        // instantiate your own player object
        Vector3 position = new Vector3(currentUserJson.position[0], currentUserJson.position[1], currentUserJson.position[2]);
        Quaternion rotation = Quaternion.Euler(currentUserJson.rotation[0], currentUserJson.rotation[1], currentUserJson.rotation[2]);

        // todo need to get player vehicle type from json and use that to determine player type
        GameObject p = Instantiate(player, position, rotation) as GameObject;

        p.name = playerNameStr;

        Camera[] camArr = Camera.allCameras;
        foreach (Camera cam in camArr)
        {
            CameraController cc = cam.GetComponent<CameraController>();
            cc.isLocalPlayer = true;
            cc.target = p.GetComponent<Rigidbody2D>();
        }

        CarController pc = p.GetComponent<CarController>();
        pc.isLocalPlayer = true;

        HealthBar hb = p.GetComponent<HealthBar>();
        hb.carObject = p;
        hb.isLocalPlayer = true;
        hb.m_Slider = healthSlider;
        hb.m_Fill = healthFill;
        


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




    #endregion







    #region JsonMessageClasses

    [Serializable]
    public class PlayerJson
    {
        public string name;
        public List<PointJson> playerSpawnPoints;

        public PlayerJson(string _name, List<SpawnPoint> _playerSpawnPoints)
        {
            playerSpawnPoints = new List<PointJson>();
            name = _name;
            foreach (SpawnPoint playerSpawnPoint in _playerSpawnPoints)
            {
                PointJson pointJson = new PointJson(playerSpawnPoint);
                playerSpawnPoints.Add(pointJson);
            }
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


    #endregion


}
