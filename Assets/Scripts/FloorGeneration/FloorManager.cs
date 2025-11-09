using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class FloorManager : MonoBehaviour
{
    List<GameObject> currentRoom;
    public List<Transform> placeholders;
    public List<GameObject> roomPrefabs;
    List<GameObject> _roomPrefabs;
    public List<Transform> pathwaysPlaceholders;
    public List<Transform> blockPlaceholders;
    public List<GameObject> pathwaysPrefabs;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartGenerating();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void GenerateRoom(int i)
    {
        int randomIndex = Random.Range(0, _roomPrefabs.Count);
        GameObject selectedRoom = _roomPrefabs[randomIndex];
        
        Instantiate(selectedRoom, placeholders[i]);
        _roomPrefabs.RemoveAt(randomIndex);
    }

    void GeneratePathways()
    {
        for (int i = 0; i < 23; i += 2)
        {
            int randomIndex = Random.Range(0,2);
            Instantiate(pathwaysPrefabs[1], pathwaysPlaceholders[i + randomIndex]);
            Instantiate(pathwaysPrefabs[0], pathwaysPlaceholders[i + 1 - randomIndex]);
        }
    }

    void StartGenerating()
    {
        _roomPrefabs = new List<GameObject>(roomPrefabs);
        for (int i = 0; i < 9; i++)
        {
            GenerateRoom(i);
        }
        for (int i = 0; i < 24; i++)
        {
            Instantiate(pathwaysPrefabs[0], blockPlaceholders[i]);
        }
        GeneratePathways();
    }
}
