using UnityEngine;
using Unity.Cinemachine;
using System;
using System.Collections.Generic;

public class CameraJumpFix : MonoBehaviour
{
    [SerializeField] private List<CinemachineCamera> cameras;
    [SerializeField] private List<GameObject> cameraBounds;
    [SerializeField] private GameObject confinerPrefab;

    private float gridHeight = 20f;
    private float gridWidth = 40f;
    private CinemachinePositionComposer positionComposer;
    private Transform _playerPosition;

    void Start()
    {
        //positionComposer = GetComponent<Camera>().GetComponent<CinemachinePositionComposer>();
        //Transform _playerPosition = GetComponent<Camera>().Follow.TrackingTarget;

        /*foreach (var room in  roomPlaceholders)
        {
            Instantiate(confinerPrefab, room.transform.position, room.transform.rotation);
        }*/
    }
    // Update is called once per frame
    void Update()
    {
        //Transform _playerPosition = GetComponent<Camera>().Follow.TrackingTarget;
        //float _gridPositionY = (_playerPosition.position.y + 40) % gridHeight;

        
    }
}
