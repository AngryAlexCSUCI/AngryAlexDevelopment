using SocketIO;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviour
{

    public static NetworkManager instance;
    //public Canvas canvas;
    public SocketIOComponent socket;
    //public InputField playerNameInput;
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
        socket.On("other player connected", OnOtherPlayerConnected);
        socket.On("play", OnPlay);
        socket.On("player move", OnPlayerMove);
        socket.On("player rotate", OnPlayerRotate);
        socket.On("other player disconnected", OnOtherPlayerDisconnect);

    }


    public void JoinGame()
    {
        StartCoroutine(ConnectToServer());
    }

    #region Commands

    IEnumerator ConnectToServer()
    {
        yield return new WaitForSeconds(0.5f);

        socket.Emit("player connected");

        yield return new WaitForSeconds(1f);

        //string playerName = playerNameInput.text; // todo after user login canvas created
        var rand = new System.Random(); // until then generate random player name
        string playerName = "Player_" + rand.Next(1, 100);
        List<SpawnPoint> playerSpawnPoints = GetComponent<PlayerSpawner>().playerSpawnPoints;
        PlayerJson playerJson = new PlayerJson(playerName, playerSpawnPoints);
        string data = JsonUtility.ToJson(playerJson);
        socket.Emit("play", new JSONObject(data));
        //canvas.gameObject.SetActive(false); // todo uncomment when login canvas is created

    }

    #endregion


    #region Listening

    void OnOtherPlayerConnected(SocketIOEvent socketIOEvent)
    {

    }

    void OnPlay(SocketIOEvent socketIOEvent)
    {

    }

    void OnPlayerMove(SocketIOEvent socketIOEvent)
    {

    }

    void OnPlayerRotate(SocketIOEvent socketIOEvent)
    {

    }

    // todo add player health and bullet events

    void OnOtherPlayerDisconnect(SocketIOEvent socketIOEvent)
    {

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
