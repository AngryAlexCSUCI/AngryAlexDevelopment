using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LeaderboardManager : MonoBehaviour
{

    private Dictionary<string, Dictionary<string, int>> playerScores;

    private int changeCounter = 0;

    // todo add from clients list received from server

    void Start()
    {
        SetScore("Player1", "kills", 124);
//        SetScore("Player1", "place", 1);
        SetScore("Player2", "kills", 280);
//        SetScore("Player2", "place", 2);
        SetScore("Player3", "kills", 53);
//        SetScore("Player3", "place", 3);
        SetScore("Player4", "kills", 99);
//        SetScore("Player4", "place", 4);
        SetScore("Player5", "kills", 1);
//        SetScore("Player5", "place", 5);
        
    }

    void Update()
    {

    }

    void Init()
    {
        if (playerScores != null)
        {
            return;
        }
        playerScores = new Dictionary<string, Dictionary<string, int>>();
    }

    public int GetScore(string playerName, string scoreType)
    {
        Init();
        if (playerScores.ContainsKey(playerName) == false)
        {
            return 0;
        }

        if (playerScores[playerName].ContainsKey(scoreType) == false)
        {
            return 0;
        }
        return playerScores[playerName][scoreType];
    }

    public void SetScore(string playerName, string scoreType, int kills)
    {
        Init();
        changeCounter++;
        if (playerScores.ContainsKey(playerName) == false)
        {
            playerScores[playerName] = new Dictionary<string, int>();
        }

        playerScores[playerName][scoreType] = kills;
    }

    public void ChangeScore(string playerName, string scoreType, int kills)
    {
        Init();
        int currentScore = GetScore(playerName, scoreType);
        SetScore(playerName, scoreType, currentScore + kills);
    }

    public string[] GetPlayerNames()
    {
        Init();
        return playerScores.Keys.ToArray();
    }

    public string[] GetPlayerNames(string sortBy)
    {
        Init();

        string[] names = playerScores.Keys.ToArray();
        return names.OrderByDescending(n => GetScore(n, sortBy)).ToArray();
    }

    public int getChangeCounter()
    {
        return changeCounter;
    }
}
