using UnityEngine;

public class ElevatorAudio : MonoBehaviour
{
    void Start()
    {
        AudioManager.Instance.StartElevatorAmbient();
        AudioManager.Instance.PlayRingElevator();
    }

    public void ElevatorOpenHandle()
    {
        AudioManager.Instance.PlayOpenElevator();
    }

    public void ElevatorRingHandle()
    {
        AudioManager.Instance.PlayRingElevator();
    }
    public void ElevatorStartMovingHandle()
    {
        AudioManager.Instance.PlayStartMovingElevator();
    }
}
