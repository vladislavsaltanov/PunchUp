using UnityEngine;
using FMODUnity;
using FMOD.Studio;
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Music")]
    [SerializeField] private EventReference[] backgroundMusicPlaylist;

    [Header("Player")]
    [SerializeField] private EventReference footstepEvent;
    [SerializeField] private EventReference jumpLandEvent;
    [SerializeField] private EventReference dashEvent;

    [Header("Doctor")]
    [SerializeField] private EventReference doctorFootstepEvent;

    private EventInstance backgroundMusicInstance;
    private int lastTrackIndex = -1;
    private bool isPlaylistRunning;

    public enum JumpLandAction
    {
        Jump = 0,
        Land = 1,
        softLand = 2
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
        StartBackgroundPlaylist();
    }

    private void Update()
    {
        if (!isPlaylistRunning || !backgroundMusicInstance.isValid())
            return;

        backgroundMusicInstance.getPlaybackState(out PLAYBACK_STATE state);

        if (state == PLAYBACK_STATE.STOPPED)
        {
            backgroundMusicInstance.release();
            PlayNextPlaylistTrack();
        }
    }

    //Йоу печенье - програмное обеспечение >_<
    public void StartBackgroundPlaylist()
    {
        if (backgroundMusicPlaylist == null || backgroundMusicPlaylist.Length == 0)
        {
            Debug.LogWarning("AudioManager: backgroundMusicPlaylist is empty");
            return;
        }

        isPlaylistRunning = true;
        PlayNextPlaylistTrack();
    }

    //Эй диджей
    private void PlayNextPlaylistTrack()
    {
        if (backgroundMusicPlaylist == null || backgroundMusicPlaylist.Length == 0)
            return;

        int nextIndex = Random.Range(0, backgroundMusicPlaylist.Length);

        if (backgroundMusicPlaylist.Length > 1)
        {
            while (nextIndex == lastTrackIndex)
            {
                nextIndex = Random.Range(0, backgroundMusicPlaylist.Length);
            }
        }

        lastTrackIndex = nextIndex;

        EventReference nextTrack = backgroundMusicPlaylist[nextIndex];

        if (nextTrack.IsNull)
        {
            Debug.LogWarning($"AudioManager: playlist track at index {nextIndex} is null");
            return;
        }

        backgroundMusicInstance = RuntimeManager.CreateInstance(nextTrack);
        backgroundMusicInstance.start();

        Debug.Log($"AudioManager: playing playlist track index {nextIndex}");
    }



    //Конец йоу-йоу
    public void StopBackgroundMusic()
    {
        isPlaylistRunning = false;

        if (backgroundMusicInstance.isValid())
        {
            backgroundMusicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            backgroundMusicInstance.release();
        }
    }

    //Беги Фо-ватафо шнейне-пепе
    public void PlayFootstep(Vector2 worldPosition)
    {
        if (footstepEvent.IsNull) return;

        RuntimeManager.PlayOneShot(footstepEvent, worldPosition);
    }

    public void PlayDoctorFootstep(Vector2 worldPosition)
    {
        if (doctorFootstepEvent.IsNull) return;

        RuntimeManager.PlayOneShot(doctorFootstepEvent, worldPosition);
    }

    //Прыг-скок-скок-скок-скок
    public void PlayJumpLand(Vector2 position, JumpLandAction action)
    {

        if (jumpLandEvent.IsNull)
        {
            Debug.LogError("AudioManager: jumpLandEvent is null");
            return;
        }

        EventInstance instance = RuntimeManager.CreateInstance(jumpLandEvent);
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(position));
        FMOD.RESULT paramResult = instance.setParameterByName("Action", (float)action);
        FMOD.RESULT startResult = instance.start();
        instance.release();
    }

    //Звуки дешдя
    public void PlayDashSound(Vector2 position)
    {
        if (dashEvent.IsNull) return;

        RuntimeManager.PlayOneShot(dashEvent, position);
    }
}
