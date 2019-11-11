using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UsernameScene : MonoBehaviour
{
    
    public InputField userNameField;
     
    public void StartButtonPress()
    {
        //input sanitization
        string nameString = userNameField.text;
        nameString = nameString.Replace(" ", "");
        //we need to send a query to server to check the username
        WebSocketManager.NameRegistrationJson nameJson = new WebSocketManager.NameRegistrationJson(nameString);
        string data = JsonUtility.ToJson(nameJson);
        WebSocketManager.instance.Dispatch("name_registration", data, true);
        print("StartButtonPress invoked.");
    }

    public static void NameRegistrationSuccessful(string name)
    {
        print("NameRegistrationSuccessful invoked");
        Player.UserName = name;
        SceneManager.LoadScene(1);
    }

    public static void NameRegistrationFailed(string name)
    {
        print("NameRegistrationFailed invoked");
        //todo: implement
    }
    
    public void RandomizeUsername()
    {
        System.Random r = new System.Random();
        int nameLength = r.Next(0, 10);

        const string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        StringBuilder result = new StringBuilder(nameLength);
        for (int i = 0; i < nameLength; i++) {
            result.Append(characters[r.Next(characters.Length)]);
        }

        userNameField.text = result.ToString();
    }
}
