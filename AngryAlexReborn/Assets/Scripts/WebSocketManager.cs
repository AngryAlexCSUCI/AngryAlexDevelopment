//Copyright (c) 2015 Gumpanat Keardkeawfa
//Licensed under the MIT license

//Websocket C# for UnityWebgl

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using UnityEditor;

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


    void Start()
    {
        print("Starting web socket..");

        player = GameObject.FindGameObjectWithTag("Player");

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
                print("Received message: " + message);
                string[] dataArr = message.Split(' ');
                if (dataArr[0] == "play")
                {
                    Dispatch("play", dataArr[1]);
                }
                else if (dataArr[0] == "other_player_connected")
                {
                    Dispatch("other_player_connected", dataArr[1]);
                }
                else if (dataArr[0] == "move")
                {
                    Dispatch("move", dataArr[1]);
                }
                else if (dataArr[0] == "turn")
                {
                    Dispatch("turn", dataArr[1]);
                }
                else if (dataArr[0] == "disconnect")
                {
                    Dispatch("disconnect", dataArr[1]);
                }
                else
                {
                    Dispatch("default", message);    //We will dequeue message and send to Dispatch function.
                }
                
            }
            yield return null; // yield return new WaitForSeconds(5); if too slow
        }
    }

    
    public void Dispatch(string type, string msg)
    {
        // msg is a string json, so for on 'play' send name and spawn points: 
        // "{
        //     name: playerName,
        //     position: { x: position.x, y: position.y, z: position.z }, 
        //     rotation: { x: rotation.x, y: rotation.y, z: rotation.z },
        //     health: int,
        //     playerSpawnPoints: [
        //         { x: position.x, y: position.y, z: position.z },
        //         { x: position.x, y: position.y, z: position.z },
        //         { x: position.x, y: position.y, z: position.z },
        //         { x: position.x, y: position.y, z: position.z },
        //         { x: position.x, y: position.y, z: position.z },
        //         { x: position.x, y: position.y, z: position.z },
        //         { x: position.x, y: position.y, z: position.z },
        //         { x: position.x, y: position.y, z: position.z },
        //         { x: position.x, y: position.y, z: position.z },
        //         { x: position.x, y: position.y, z: position.z },
        //         { x: position.x, y: position.y, z: position.z },
        //         { x: position.x, y: position.y, z: position.z }
        //     ]
        // }"
        switch (type)
        {
            case "play":
                Send(type + " " + msg); // send server player name and let server randomly pick spawn point from list of spawn points
                OnPlay(msg);
                break;
            case "turn":
                Send(type + " " + msg);
//                OnPlayerRotate(msg);
                break;
            case "move":
                Send(type + " " + msg);
//                OnPlayerMove(msg);
                break;
            case "shoot":
                Send(type + " " + msg);
                break;
            case "disconnect":
                Send(type + " " + msg);
                break;
            case "health":
                Send(type + " " + msg);
                break;
            default:
                Send("dispatch " + msg);
                break;
        }
    }

    //For UI, we defined it here
    //    void OnGUI()
    //    {
    //        if (GUI.Button(new Rect(10, 110, 150, 30), "Hello,World"))
    //            Send("Hello there!");
    ////
    ////        if (GUI.Button(new Rect(10, 60, 150, 30), "Turn Right"))
    ////            Send("turn r");
    ////
    ////        if (GUI.Button(new Rect(10, 110, 150, 30), "Turn Left"))
    ////            Send("turn l");
    //    }



    #region Listening

    void OnOtherPlayerConnected(string data)//SocketIOEvent socketIOEvent // todo some other socket event
    {
        print("Another player joined Angry Alex.");
//        string data = "";// socketIOEvent.data.ToString();
        UserJson userJson = UserJson.CreateFromJson(data);
        Vector3 position = new Vector3(userJson.position[0], userJson.position[1], userJson.position[2]);
        Quaternion rotation = Quaternion.Euler(userJson.rotation[0], userJson.rotation[1], userJson.rotation[2]);

        GameObject obj = GameObject.Find(userJson.name) as GameObject;
        if (obj != null)
        {
            return;
        }
        GameObject p = Instantiate(player, position, rotation) as GameObject;
        CarController pc = p.GetComponent<CarController>();

        pc.isLocalPlayer = false;

        CameraController cc = Camera.main.GetComponent<CameraController>();
        cc.isLocalPlayer = false;

        // todo set health and reference on change health event 


    }

    void OnPlay(string data)//SocketIOEvent socketIOEvent // todo some other socket event
    {
        print("You have joined Angry Alex.");
//        string data = "";// socketIOEvent.data.ToString();
        UserJson currentUserJson = UserJson.CreateFromJson(data);
        Vector3 position = new Vector3(currentUserJson.position[0], currentUserJson.position[1], currentUserJson.position[2]);
        Quaternion rotation = Quaternion.Euler(currentUserJson.rotation[0], currentUserJson.rotation[1], currentUserJson.rotation[2]);

        GameObject p = Instantiate(player, position, rotation) as GameObject;

        CameraController cc = Camera.main.GetComponent<CameraController>();
        cc.isLocalPlayer = true;
        cc.target = p.transform;

        CarController pc = p.GetComponent<CarController>();
        pc.isLocalPlayer = true;

    }

    void OnPlayerMove(string data)//SocketIOEvent socketIOEvent // todo some other socket event
    {
//        string data = "";// socketIOEvent.data.ToString();
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

    void OnPlayerRotate(string data)//SocketIOEvent socketIOEvent // todo some other socket event
    {
//        string data = "";// socketIOEvent.data.ToString();
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

    // todo add player health and bullet events

    void OnOtherPlayerDisconnect(string data)//SocketIOEvent socketIOEvent // todo some other socket event
    {
        print("Player disconnected");
//        string data = "";// socketIOEvent.data.ToString();
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
        public float[] position;
        public RotationJson(Quaternion _rotation)
        {
            position = new float[] { _rotation.eulerAngles.x, _rotation.eulerAngles.y, _rotation.eulerAngles.z };

        }
    }


    [Serializable]
    public class UserJson
    {
        public string name;
        public float[] position;
        public float[] rotation;
        public int health;

        public static UserJson CreateFromJson(string data)
        {
            return JsonUtility.FromJson<UserJson>(data);
        }
    }

    // todo finish health json
    [Serializable]
    public class HealthChangeJson
    {
        public string name;
        public int healthChange;
        public string from;
        // todo add damage from enemy?

        public HealthChangeJson(string _name, int _healthChange, string _from)
        {
            name = _name;
            healthChange = _healthChange;
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
