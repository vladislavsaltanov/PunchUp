using UnityEngine;
using System;

public class PlayerController : MonoBehaviour, IPlayerActions
{
    #region Singleton
        public static PlayerController instance {  get; private set; }
    private void Awake()
        {
            instance = this;
        }
    #endregion

    public bool isGrounded;

    [HideInInspector]
        public float currentTime, lastGroundedTime;

    void Start()
    {
        isGroundedHandler.Instance.hasGrounded += hasGroundedEventHandler;
    }

    private void hasGroundedEventHandler(bool hasGrounded, float time)
    {
        lastGroundedTime = time;
    }

    void Update()
    {
        currentTime += Time.deltaTime;
    }

    public event Action<bool, float> hasBounced;
    public event Action<float> hasJumped;
}
public interface IPlayerActions
{
    public event Action<bool, float> hasBounced;
    public event Action<float> hasJumped;
}