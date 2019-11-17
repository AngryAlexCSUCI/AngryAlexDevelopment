using System;
using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    [HideInInspector]
    public static string UserName { get; set; }

    [HideInInspector]
    public static Tuple<int, int> VehicleLoadout { get; set; }

    [HideInInspector]
    public GameObject LocalPlayer { get; set; }

    protected Dictionary<Tuple<int, int>, string> _vehicleWeaponNames = new Dictionary<Tuple<int, int>, string>()
    {
        { new Tuple<int, int>(1,1), "CannonCar" },
        { new Tuple<int, int>(2,1), "CannonTruck" },
        { new Tuple<int, int>(3,1), "CannonMotorcycle" },
        { new Tuple<int, int>(1,2), "MachinegunCar" },
        { new Tuple<int, int>(2,2), "MachinegunTruck" },
        { new Tuple<int, int>(3,2), "MachinegunMotorcycle" }
    };

    private string _tag;

    protected string Tag
    {
        get
        {
            _tag = LocalPlayer.tag;
            return _tag;
        }
        set { _tag = value; }
    }

  
    public bool isLocalPlayer = false;

}
