using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScoreList : MonoBehaviour
{

    public GameObject playerScoreEntryPrefab;

    private LeaderboardManager leaderboardManager;

    // Start is called before the first frame update
    void Start()
    {
        leaderboardManager = GameObject.FindObjectOfType<LeaderboardManager>();

    }

    // Update is called once per frame
    void Update()
    {
        
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

        string[] names = leaderboardManager.GetPlayerNames();

        foreach (string name in names)
        {
            GameObject go = (GameObject)Instantiate(playerScoreEntryPrefab);
            go.transform.SetParent(this.transform);
            go.transform.Find("Player Name").GetComponent<Text>().text = name;
            go.transform.Find("Place").GetComponent<Text>().text = leaderboardManager.GetScore(name, "place").ToString();
            go.transform.Find("Score").GetComponent<Text>().text = leaderboardManager.GetScore(name, "kills").ToString(); ;
        }

    }
}
