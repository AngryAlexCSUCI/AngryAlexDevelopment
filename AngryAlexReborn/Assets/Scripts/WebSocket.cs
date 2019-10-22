//Copyright (c) 2015 Gumpanat Keardkeawfa
//Licensed under the MIT license

//Websocket C# for UnityWebgl

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEditor;

public class WebSocket : MonoBehaviour
{

    #region WebSockets Implement
    Queue<string> recvList = new Queue<string>(); //keep receive messages

    [DllImport("__Internal")]
    private static extern string Hello(); //test javascript plugin

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

    void Start()
    {
        //        var currentPath = Environment.GetEnvironmentVariable("PATH",
        //            EnvironmentVariableTarget.Process);
        //        print("Current path: " + currentPath);
        //
        //        var dllPath = Application.dataPath
        //                      + Path.AltDirectorySeparatorChar + "Plugins";
        //        print("DLL path: " + dllPath);
        //
        //        if (currentPath != null && currentPath.Contains(dllPath) == false)
        //            Environment.SetEnvironmentVariable("PATH", currentPath + Path.PathSeparator
        //                                                                   + dllPath, EnvironmentVariableTarget.Process);
        //        print(Environment.GetEnvironmentVariable("PATH"));

        print("Starting web socket..");
        print(Hello());        //We start with testing plugin
        
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
            {         //When our message queue has string coming.
                Dispatch("default",recvList.Dequeue());    //We will dequeue message and send to Dispatch function.
            }
            yield return null;
        }
    }

    //You can implement your game method here :)
    public void Dispatch(string type, string msg)
    {
        // msg is a string json "{ "x": position.x, "y": position.y, "z": position.z }"
        switch (type)
        {
            case "play":
                //DO SOMETHING
                Send("Play " + msg); // send server player name and let server randomly pick spawn point from list of spawn points
                break;
            case "turn":
                //DO SOMETHING
                Send("Turn " + msg);
                break;
            case "move":
                //DO SOMETHING
                Send("Move " + msg);
                break;
            case "shoot":
                //DO SOMETHING
                Send("Shoot " + msg);
                break;
            default:
                Send("Dispatch : " + msg);
                break;
        }
    }

    //For UI, we defined it here
    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 110, 150, 30), "Hello,World"))
            Send("Hello there!");
//
//        if (GUI.Button(new Rect(10, 60, 150, 30), "Turn Right"))
//            Send("turn r");
//
//        if (GUI.Button(new Rect(10, 110, 150, 30), "Turn Left"))
//            Send("turn l");
    }
}
