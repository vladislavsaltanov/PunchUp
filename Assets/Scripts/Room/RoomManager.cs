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

    public Vector2 InitializeElevator(int i)
    {
        return Instantiate(elevators[i], elevatorPlaceholder).transform.position;
    }

    public void SetRoomID(int i)
    {
        roomID = i;
    }

    public void SpawnRoomEnemy(List<GameObject> Enemys)
    {
        
        for( int i = 0; i < enemyPositions.Count; i++)
        {
            Instantiate(Enemys[Random.Range(0,Enemys.Count)], enemyPositions[i]);
        }
    }
}
