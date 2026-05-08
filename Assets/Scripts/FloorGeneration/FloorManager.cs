using NUnit.Framework;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class FloorManager : MonoBehaviour
{
    List<GameObject> currentRooms = new List<GameObject>();
    public List<Transform> placeholders;
    public List<GameObject> roomPrefabs;
    public GameObject MarketPrefab;
    List<GameObject> _roomPrefabs;
    public List<Transform> pathwaysPlaceholders;
    public List<Transform> blockPlaceholders;
    public List<GameObject> pathwaysPrefabs;
    public Transform currentRoom = null;
    public Transform cameraBounds;

    public List<GameObject> enemyPrefabs;

    public int EnterInd;
    public int ExitInd;
    public int MarketInd;

    [SerializeField] GameObject playerObject;

    static public FloorManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cameraBounds = GameObject.FindGameObjectWithTag("CameraBounds").transform;
        _ = StartGenerating();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void GenerateRoom(int i)
    {
        int randomIndex = Random.Range(0, _roomPrefabs.Count);
        GameObject selectedRoom = _roomPrefabs[randomIndex];

        currentRooms.Add(Instantiate(selectedRoom, placeholders[i]));
        currentRooms[i].GetComponent<RoomManager>().SetRoomID(i);
        currentRooms[i].GetComponent<RoomManager>().SpawnRoomEnemy(enemyPrefabs);
        _roomPrefabs.RemoveAt(randomIndex);
    }

    void GenerateMarket(int i)
    {
        currentRooms.Add(Instantiate(MarketPrefab, placeholders[i]));
        currentRooms[i].GetComponent<RoomManager>().SetRoomID(i);
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

    async Awaitable StartGenerating()
    {
        _roomPrefabs = new List<GameObject>(roomPrefabs);
        EnterInd = Random.Range(0, 3);
        ExitInd = Random.Range(3, 9);
        MarketInd = Random.Range(0, 9);
        while (MarketInd == EnterInd || MarketInd == ExitInd)
        {
            MarketInd = Random.Range(0, 9);
        }
        Debug.Log(EnterInd + " " + ExitInd + " " + MarketInd);
        cameraBounds.position = placeholders[EnterInd].position;
        currentRoom = placeholders[EnterInd].transform;
        //Ãåíåðàöèÿ êîìíàò
        for (int i = 0; i < 9; i++)
        {
            if (i == MarketInd)
            {
                GenerateMarket(i);
            } else GenerateRoom(i);
        }
        //Ñïàâí ëèôòîâ
        Vector2 playerSpawnPosition = currentRooms[EnterInd].GetComponent<RoomManager>().InitializeElevator(0);
        currentRooms[ExitInd].GetComponent<RoomManager>().InitializeElevator(1);

        await Awaitable.NextFrameAsync();
        await Awaitable.NextFrameAsync();
        await Awaitable.NextFrameAsync();
        await Awaitable.NextFrameAsync();
        await Awaitable.NextFrameAsync();

        if (PlayerController.instance == null)
            await Awaitable.WaitForSecondsAsync(0.1f);

        playerObject = PlayerController.instance.gameObject;
        playerObject.transform.position = playerSpawnPosition;
        
        CameraManager.Instance.SetTrackingTarget(playerObject.transform);

        //Ñïàâí "Áëîêîâ" íà ãðàíèöàõ óðîâíÿ
        for (int i = 0; i < 24; i++)
        {
            Instantiate(pathwaysPrefabs[2], blockPlaceholders[i]);
        }
        //Ãåíåðàöèÿ ïðîõîäîâ
        GeneratePathways();
    }
}
