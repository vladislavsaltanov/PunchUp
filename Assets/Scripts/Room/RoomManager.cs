using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public int roomID;
    public Transform elevatorPlaceholder;
    public List<GameObject> elevators;
    public Transform playerSpawnPosition;
    public List<Transform> roomObjectsPlaceholders;
    public List<Transform> enemyPositions;
    // Audio bank

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitializeElevator(int i)
    {
        Instantiate(elevators[i], elevatorPlaceholder);
    }

    public void SetRoomID(int i)
    {
        roomID = i;
    }
}
