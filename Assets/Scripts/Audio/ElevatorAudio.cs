using UnityEngine;

public class ElevatorAudio : MonoBehaviour
{
    void Start()
    {
        AudioManager.Instance.StartElevatorAmbient();
    }

    public void ElevatorOpenHandle()
    {
        AudioManager.Instance.PlayOpenElevator();
    }
}
