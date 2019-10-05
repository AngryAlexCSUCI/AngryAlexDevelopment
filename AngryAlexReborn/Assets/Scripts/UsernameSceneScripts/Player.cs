using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Player
{
    private static string userName;
    
    public static string UserName 
    {
        get 
        {
            return userName;
        }
        set 
        {
            userName = value;
        }
    }
}
