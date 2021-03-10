using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectorManager : MonoBehaviour
{
    public GameObject Car;
    public GameObject Truck;
    public GameObject Motorcycle;
    public GameObject Cannon;
    public GameObject MachineGun;
    public GameObject MotorcycleCannon;
    public GameObject MotorcycleMachineGun;
    public GameObject CurrentWeapon;

    private Vector2 VehiclePosition;
    private Vector2 CarWeaponPosition;
    private Vector2 TruckWeaponPosition;
    private Vector2 MotorcycleWeaponPosition;
    private Vector2 Offscreen;

    private int _vehicleNumber = 1, _weaponNumber = 1;

    private SpriteRenderer _carRender, _truckRender, _motorcycleRender, _lazerRender, _machineGunRender, _cannonRender, 
        _motorcycleCannonRender, _motorcycleMachinegunRender, _motorcycleLazerRender, _currentWeaponRender;

    private Dictionary<int, Vector2> WeaponPositions = new Dictionary<int, Vector2>();
    private Dictionary<int, Dictionary<int, GameObject>> Weapons = new Dictionary<int, Dictionary<int, GameObject>>();
    private Dictionary<int, GameObject> MotorcycleWeapons = new Dictionary<int, GameObject>();
    private Dictionary<int, GameObject> VehicleWeapons = new Dictionary<int, GameObject>();
    private Dictionary<int, Dictionary<int, SpriteRenderer>> WeaponRenderers = new Dictionary<int, Dictionary<int, SpriteRenderer>>();

    private Dictionary<int, SpriteRenderer> motorcycleWeaponRenderers = new Dictionary<int, SpriteRenderer>();
    private Dictionary<int, SpriteRenderer> weaponRenderers = new Dictionary<int, SpriteRenderer>();

    private void Awake()
    {
        VehiclePosition = Car.transform.position;
        CarWeaponPosition = Cannon.transform.position;
        TruckWeaponPosition = Cannon.transform.position;
        TruckWeaponPosition.y = TruckWeaponPosition.y - .6f;
        MotorcycleWeaponPosition = Cannon.transform.position;
        MotorcycleWeaponPosition.y = TruckWeaponPosition.y + 1f;
        MotorcycleWeaponPosition.x = TruckWeaponPosition.x - .04f;
        Offscreen = Truck.transform.position;
        _carRender = Car.GetComponent<SpriteRenderer>();
        _truckRender = Truck.GetComponent<SpriteRenderer>();
        _motorcycleRender = Motorcycle.GetComponent<SpriteRenderer>();
        _cannonRender = Cannon.GetComponent<SpriteRenderer>();
        _machineGunRender = MachineGun.GetComponent<SpriteRenderer>();
        _motorcycleCannonRender = MotorcycleCannon.GetComponent<SpriteRenderer>();
        _motorcycleMachinegunRender = MotorcycleMachineGun.GetComponent<SpriteRenderer>();

        WeaponPositions.Add(1, CarWeaponPosition);
        WeaponPositions.Add(2, TruckWeaponPosition);
        WeaponPositions.Add(3, MotorcycleWeaponPosition);

        VehicleWeapons.Add(1, Cannon);
        VehicleWeapons.Add(2, MachineGun);

        MotorcycleWeapons.Add(1, MotorcycleCannon);
        MotorcycleWeapons.Add(2, MotorcycleMachineGun);

        Weapons.Add(1, VehicleWeapons);
        Weapons.Add(2, VehicleWeapons);
        Weapons.Add(3, MotorcycleWeapons);
        
        weaponRenderers.Add(1, _cannonRender);
        weaponRenderers.Add(2, _machineGunRender);

        motorcycleWeaponRenderers.Add(1, _motorcycleCannonRender);
        motorcycleWeaponRenderers.Add(2, _motorcycleMachinegunRender);

        WeaponRenderers.Add(1, weaponRenderers);
        WeaponRenderers.Add(2, weaponRenderers);
        WeaponRenderers.Add(3, motorcycleWeaponRenderers);

        CurrentWeapon = Cannon;
        _currentWeaponRender = _cannonRender;

        _truckRender.enabled = false;
        _motorcycleRender.enabled = false;
        _machineGunRender.enabled = false;
        _motorcycleCannonRender.enabled = false;
        _motorcycleMachinegunRender.enabled = false;
    }

    public void NextVehicle()
    {
        switch (_vehicleNumber)
        {
            case 1:
                _carRender.enabled = false;
                _truckRender.enabled = true;
                CurrentWeapon.transform.position = TruckWeaponPosition;
                WeaponRenderers[_vehicleNumber][_weaponNumber].enabled = false;
                _vehicleNumber = 2;
                _currentWeaponRender = WeaponRenderers[_vehicleNumber][_weaponNumber];
                WeaponRenderers[_vehicleNumber][_weaponNumber].enabled = true;
                break;
            case 2:
                _truckRender.enabled = false;
                _motorcycleRender.enabled = true;
                WeaponRenderers[_vehicleNumber][_weaponNumber].enabled = false;
                CurrentWeapon.transform.position = Offscreen;
                _vehicleNumber = 3;
                CurrentWeapon = Weapons[_vehicleNumber][_weaponNumber];
                CurrentWeapon.transform.position = MotorcycleWeaponPosition;
                _currentWeaponRender = WeaponRenderers[_vehicleNumber][_weaponNumber];
                WeaponRenderers[_vehicleNumber][_weaponNumber].enabled = true;
                break;
            case 3:
                _carRender.enabled = true;
                _motorcycleRender.enabled = false;
                WeaponRenderers[_vehicleNumber][_weaponNumber].enabled = false;
                CurrentWeapon.transform.position = Offscreen;
                _vehicleNumber = 1;
                CurrentWeapon = Weapons[_vehicleNumber][_weaponNumber];
                CurrentWeapon.transform.position = CarWeaponPosition;
                _currentWeaponRender = WeaponRenderers[_vehicleNumber][_weaponNumber];
                WeaponRenderers[_vehicleNumber][_weaponNumber].enabled = true;
                break;
            default:
                break;
        }
    }

    public void PreviousVehicle()
    {
        switch (_vehicleNumber)
        {
            case 1:
                _carRender.enabled = false;
                _motorcycleRender.enabled = true;
                WeaponRenderers[_vehicleNumber][_weaponNumber].enabled = false;
                CurrentWeapon.transform.position = Offscreen;
                _vehicleNumber = 3;
                CurrentWeapon = Weapons[_vehicleNumber][_weaponNumber];
                CurrentWeapon.transform.position = MotorcycleWeaponPosition;
                _currentWeaponRender = WeaponRenderers[3][_weaponNumber];
                WeaponRenderers[_vehicleNumber][_weaponNumber].enabled = true;
                break;
            case 2:
                _truckRender.enabled = false;
                _carRender.enabled = true;
                WeaponRenderers[_vehicleNumber][_weaponNumber].enabled = false;
                CurrentWeapon.transform.position = Offscreen;
                _vehicleNumber = 1;
                CurrentWeapon = Weapons[_vehicleNumber][_weaponNumber];
                CurrentWeapon.transform.position = CarWeaponPosition;
                WeaponRenderers[_vehicleNumber][_weaponNumber].enabled = true;
                break;
            case 3:
                _motorcycleRender.enabled = false;
                _truckRender.enabled = true;
                WeaponRenderers[_vehicleNumber][_weaponNumber].enabled = false;
                CurrentWeapon.transform.position = Offscreen;
                _vehicleNumber = 2;
                CurrentWeapon = Weapons[_vehicleNumber][_weaponNumber];
                CurrentWeapon.transform.position = TruckWeaponPosition;
                _currentWeaponRender = WeaponRenderers[_vehicleNumber][_weaponNumber];
                WeaponRenderers[_vehicleNumber][_weaponNumber].enabled = true;
                break;
            default:
                break;
        }
    }

    public void NextWeapon()
    {
        switch (_weaponNumber)
        {
            case 1:
                _currentWeaponRender.enabled = false;
                _weaponNumber = 2;
                WeaponRenderers[_vehicleNumber][_weaponNumber].enabled = true;
                _currentWeaponRender = WeaponRenderers[_vehicleNumber][_weaponNumber];
                CurrentWeapon = Weapons[_vehicleNumber][_weaponNumber];
                CurrentWeapon.transform.position = WeaponPositions[_vehicleNumber];
                break;
            case 2:
                _currentWeaponRender.enabled = false;
                _weaponNumber = 1;
                WeaponRenderers[_vehicleNumber][_weaponNumber].enabled = true;
                _currentWeaponRender = WeaponRenderers[_vehicleNumber][_weaponNumber];
                CurrentWeapon = Weapons[_vehicleNumber][_weaponNumber];
                CurrentWeapon.transform.position = WeaponPositions[_vehicleNumber];
                break;
            default:
                break;
        }
    }

    public void PreviousWeapon()
    {
        switch (_weaponNumber)
        {
            case 1:
                _currentWeaponRender.enabled = false;
                _weaponNumber = 2;
                WeaponRenderers[_vehicleNumber][_weaponNumber].enabled = true;
                _currentWeaponRender = WeaponRenderers[_vehicleNumber][_weaponNumber];
                CurrentWeapon = Weapons[_vehicleNumber][_weaponNumber];
                break;
            case 2:
                _currentWeaponRender.enabled = false;
                _weaponNumber = 1;
                WeaponRenderers[_vehicleNumber][_weaponNumber].enabled = true;
                _currentWeaponRender = WeaponRenderers[_vehicleNumber][_weaponNumber];
                CurrentWeapon = Weapons[_vehicleNumber][_weaponNumber];
                break;
            default:
                break;
        }
    }

    public void LoadNextScene(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
        print("Main scene loaded");
        Player.VehicleLoadout = new Tuple<int, int>(_vehicleNumber, _weaponNumber);
        Debug.Log(Player.VehicleLoadout);



        List<SpawnPoint> playerSpawnPoints = GetComponent<PlayerSpawner>().playerSpawnPoints;
        print("Current vehicle selection: " + Player.VehicleLoadout.Item1 + " " + Player.VehicleLoadout.Item2);
        WebSocketManager.PlayerJson playerJson = new WebSocketManager.PlayerJson(Player.UserName, playerSpawnPoints, Player.VehicleLoadout);
        string data = JsonUtility.ToJson(playerJson);
        WebSocketManager.instance.Dispatch("play", data, true);
    }
}
