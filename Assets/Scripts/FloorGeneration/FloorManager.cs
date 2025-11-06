using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class FloorManager : MonoBehaviour
{
    public List<Transform> placeholders;
    public List<GameObject> roomprefabs;
    List<int> roomlist = new List<int>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for(int i = 0; i < 9; i++)
        {
            roomlist.Add(GenerateRoom(i));
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    int GenerateRoom(int i)
    {
        int randomIndex = Random.Range(0, roomprefabs.Count);
        while (roomlist.Contains(randomIndex))
        {
            randomIndex = Random.Range(0, roomprefabs.Count);
        }
        GameObject selectedRoom = roomprefabs[randomIndex];

        Instantiate(selectedRoom, placeholders[i]);
        Debug.Log($"Ќа место номер {i+1} установена комната ноиер {randomIndex+1}");
        return randomIndex;
    }
}
