using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    int roomID;
    Transform elevatorPlaceholder;
    GameObject elevator;
    Transform playerSpawnPosition;
    List<Transform> roomObjectsPlaceholders;
    List<Transform> enemyPositions;
    // Audio bank

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void InitializeElevator()
    {
        Instantiate(elevator, elevatorPlaceholder);
    }
}
