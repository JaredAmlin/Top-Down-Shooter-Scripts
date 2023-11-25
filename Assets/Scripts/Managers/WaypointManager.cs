using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointManager : MonoSingleton<WaypointManager>
{
    [SerializeField] private Transform[] _room1Waypoints, _room2Waypoints, _room3Waypoints, _room4Waypoints, _room5Waypoints, _room6Waypoints, _room7Waypoints, _room8Waypoints, _room9Waypoints, _room10Waypoints;
    private Transform[][] _waypoints;

    // Start is called before the first frame update
    private void Awake()
    {
        _waypoints = new Transform[][]
        {
            new Transform[_room1Waypoints.Length],
            new Transform[_room1Waypoints.Length],
            new Transform[_room2Waypoints.Length],
            new Transform[_room3Waypoints.Length],
            new Transform[_room4Waypoints.Length],
            new Transform[_room5Waypoints.Length],
            new Transform[_room6Waypoints.Length],
            new Transform[_room7Waypoints.Length],
            new Transform[_room8Waypoints.Length],
            new Transform[_room9Waypoints.Length],
            new Transform[_room10Waypoints.Length]
        };

        _waypoints[0] = _room1Waypoints;
        _waypoints[1] = _room2Waypoints;
        _waypoints[2] = _room3Waypoints;
        _waypoints[3] = _room4Waypoints;
        _waypoints[4] = _room5Waypoints;
        _waypoints[5] = _room6Waypoints;
        _waypoints[6] = _room7Waypoints;
        _waypoints[7] = _room8Waypoints;
        _waypoints[8] = _room9Waypoints;
        _waypoints[9] = _room10Waypoints;
    }

    public Transform[] GetWaypoints()
    {
        int roomsCompleted = GameManager.Instance.RoomsCompleted();

        return _waypoints[roomsCompleted];
    }
}
