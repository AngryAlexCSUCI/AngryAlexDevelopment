using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UsernameScene : Player
{
    public InputField userNameField;
    
    private void Start()
    {
        GameObject.Find ("ErrorMessageField").transform.localScale = new Vector3(0, 0, 0);
    }
    
    public void StartButtonPress()
    {
        print("StartButtonPress invoked.");
        //input sanitization
        string nameString = userNameField.text;
        nameString = nameString.Replace(" ", "");
        UserName = nameString;
        //we need to send a query to server to check the username
        WebSocketManager.NameRegistrationJson nameJson = new WebSocketManager.NameRegistrationJson(nameString);
        string data = JsonUtility.ToJson(nameJson);
        WebSocketManager.instance.Dispatch("name_registration", data, true);

    }

    public void RandomizeUsername()
    {
        System.Random r = new System.Random();
        int nameLength = r.Next(0, 10);

        const string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        StringBuilder result = new StringBuilder(nameLength);
        for (int i = 0; i < nameLength; i++)
        {
            result.Append(characters[r.Next(characters.Length)]);
        }

        userNameField.text = result.ToString();
    }
}
