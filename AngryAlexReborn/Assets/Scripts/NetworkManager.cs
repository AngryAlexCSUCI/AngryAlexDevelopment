using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviour
{

    public static NetworkManager instance;
    public Canvas enterCanvas;
    public Canvas uiCanvas;
    public WebSocketManager socket;
    private string playerNameStr = Player.UserName;
    public GameObject player;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        //        socket.On("other player connected", OnOtherPlayerConnected);
        //        socket.On("play", OnPlay);
//        Application.ExternalCall("play");
        //        socket.On("player move", OnPlayerMove);
        //        socket.On("player rotate", OnPlayerRotate);
        //        socket.On("other player disconnected", OnOtherPlayerDisconnect);
        //        int result = Hello();
        //        print(result);

    }


    public void JoinGame()
    {
//        StartCoroutine(ConnectToServer());
    }

    #region Commands

    IEnumerator ConnectToServer()
    {
        yield return new WaitForSeconds(0.5f);

//        socket.Emit("player connected");

        yield return new WaitForSeconds(1f);

        // generate random player name
        var rand = new System.Random();
        string playerName = "Player_" + rand.Next(1, 100);
        playerNameStr = playerName;

        List<SpawnPoint> playerSpawnPoints = GetComponent<PlayerSpawner>().playerSpawnPoints;
        PlayerJson playerJson = new PlayerJson(playerNameStr, playerSpawnPoints);
        string data = JsonUtility.ToJson(playerJson);

//        socket.Dispatch("play", data);
        
        enterCanvas.gameObject.SetActive(false); 
        uiCanvas.gameObject.SetActive(true);

    }

    public void CommandMove(Vector3 vec)
    {
        string data = JsonUtility.ToJson(new PositionJson(vec));
//        socket.Emit("player move", new JSONObject(data));
    }

    public void CommandRotate(Quaternion quat)
    {
        string data = JsonUtility.ToJson(new RotationJson(quat));
//        socket.Emit("player rotate", new JSONObject(data));
    }


    // add command for shooting weapons and health change

    #endregion


    #region Listening

    void OnOtherPlayerConnected()//SocketIOEvent socketIOEvent // todo some other socket event
    {
        print("Another player joined Angry Alex.");
        string data = "";// socketIOEvent.data.ToString();
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

        //Transform tr1 = p.transform.Find("HealthBar Canvas");
        //Transform tr2 = tr1.transform.Find("Player Name");
        // todo get player name from health bar canvas attached to player for display?

        pc.isLocalPlayer = false;

        CameraController cc = Camera.main.GetComponent<CameraController>();
        cc.isLocalPlayer = false;

        // todo set health and reference on change health event 


    }

    void OnPlay()//SocketIOEvent socketIOEvent // todo some other socket event
    {
        print("You have joined Angry Alex.");
        string data = "";// socketIOEvent.data.ToString();
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

    void OnPlayerMove()//SocketIOEvent socketIOEvent // todo some other socket event
    {
        string data = "";// socketIOEvent.data.ToString();
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

    void OnPlayerRotate()//SocketIOEvent socketIOEvent // todo some other socket event
    {
        string data = "";// socketIOEvent.data.ToString();
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

    void OnOtherPlayerDisconnect()//SocketIOEvent socketIOEvent // todo some other socket event
    {
        print("Player disconnected");
        string data = "";// socketIOEvent.data.ToString();
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

