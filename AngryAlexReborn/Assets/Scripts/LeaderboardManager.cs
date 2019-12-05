using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaderboardManager : MonoBehaviour
{

    private Dictionary<string, Dictionary<string, int>> playerScores;

    // todo add from clients list received from server

    void Start()
    {
        SetScore("player1", "kills", 124);
        Debug.Log( "Player1 kills: " + GetScore("player1", "kills"));
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
}
