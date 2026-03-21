using UnityEngine;
using FMODUnity;
using FMOD.Studio;
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private EventReference backgroundMusicEvent;
    [SerializeField] private EventReference footstepEvent;
    [SerializeField] private EventReference jumpLandEvent;

    private EventInstance backgroundMusicInstance;

    public enum JumpLandAction
    {
        Jump = 0,
        Land = 1
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Больше одного AudioManager o_0");
        }
        Instance = this;
    }

    private void Start()
    {
        PlayBackgroundMusic();
    }

    //Йоу печенье - програмное обеспечение >_<
    public void PlayBackgroundMusic()
    {
        if (backgroundMusicEvent.IsNull) return;

        backgroundMusicInstance = RuntimeManager.CreateInstance(backgroundMusicEvent);
        backgroundMusicInstance.start();
    }

    //Конец йоу-йоу
    public void StopBackgroundMusic()
    {
        backgroundMusicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        backgroundMusicInstance.release();
    }

    //Беги Фо-ватафо шнейне-пепе
    public void PlayFootstep(Vector3 worldPosition)
    {
        if (footstepEvent.IsNull) return;

        RuntimeManager.PlayOneShot(footstepEvent, worldPosition);
    }

    //Прыг-скок-скок-скок-скок
    public void PlayJumpLand(Vector3 position, JumpLandAction action)
    {
        if (jumpLandEvent.IsNull)
        {
            Debug.LogError("AudioManager: jumpLandEvent is null");
            return;
        }

        EventInstance instance = RuntimeManager.CreateInstance(jumpLandEvent);
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(position));
        instance.setParameterByName("Action", (float)action);
        instance.start();
        instance.release();
    }
}
