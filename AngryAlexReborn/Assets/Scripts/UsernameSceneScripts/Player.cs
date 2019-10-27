using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Player
{
    private static string userName;
    private static Tuple<int, int> vehicleLoadout;

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

    public static Tuple<int, int> VehicleLoadout { get => vehicleLoadout; set => vehicleLoadout = value; }
}
