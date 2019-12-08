using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScoreList : MonoBehaviour
{

    public GameObject playerScoreEntryPrefab;

    private LeaderboardManager leaderboardManager;

    private int lastChangeCounter;

    // Start is called before the first frame update
    void Start()
    {
        leaderboardManager = GameObject.FindObjectOfType<LeaderboardManager>();

        lastChangeCounter = leaderboardManager.getChangeCounter();
    }

    // Update is called once per frame
    void Update()
    {
        if (leaderboardManager.getChangeCounter() == lastChangeCounter)
        {
            return;
        }
        lastChangeCounter = leaderboardManager.getChangeCounter();

        if (leaderboardManager == null)
        {
            Debug.Log("No leader board manager found in any game object.");
            return;
        }

        while (this.transform.childCount > 0)
        {
            Transform child = this.transform.GetChild(0);
            child.SetParent(null);
            Destroy(child.gameObject);
        }

        string[] names = leaderboardManager.GetPlayerNames("kills");

        int place = 1;
        for (int i = 0; i < names.Length && place <= 10; ++i)
        {
            if (String.IsNullOrEmpty(names[i]))
            {
                continue;
            }
            GameObject go = (GameObject)Instantiate(playerScoreEntryPrefab);
            go.transform.SetParent(this.transform);
            string score = leaderboardManager.GetScore(names[i], "kills").ToString();
            Debug.Log("Leaderboard update for: " + names[i] + " to place " + place + " with kills " + score);
            go.transform.Find("Player Name").GetComponent<Text>().text = names[i];
            go.transform.Find("Place").GetComponent<Text>().text = place.ToString();
            go.transform.Find("Score").GetComponent<Text>().text = score;
            ++place;
        }

    }
}
