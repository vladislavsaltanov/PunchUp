using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Music")]
    [SerializeField] private EventReference[] backgroundMusicPlaylist;

    [Header("Ambience")]
    [SerializeField] private EventReference ambientEvent;

    [Header("Elevator")]
    [SerializeField] private EventReference ElevatorAmbientEvent;
    [SerializeField] private EventReference ElevatorOpenEvent;
    [SerializeField] private EventReference ElevatorRingEvent;
    [SerializeField] private EventReference ElevatorStartMovingEvent;

    [Header("Player")]
    [SerializeField] private EventReference footstepEvent;
    [SerializeField] private EventReference jumpLandEvent;
    [SerializeField] private EventReference dashEvent;
    [SerializeField] private EventReference playerTakeDamageEvent;
    [SerializeField] private EventReference playerAttackEvent;
    [SerializeField] private EventReference playerPunchEvent;

    [Header("Doctor")]
    [SerializeField] private EventReference doctorFootstepEvent;
    [SerializeField] private EventReference doctorTakeDamageEvent;
    [SerializeField] private EventReference doctorAttackEvent;
    [SerializeField] private EventReference doctorAttack2Event;
    [SerializeField] private EventReference doctorBreakGlassEvent;
    [SerializeField] private EventReference doctorIdleEvent;

    [Header("Guardian")]
    [SerializeField] private EventReference guardianFootstepEvent;
    [SerializeField] private EventReference guardianTakeDamageEvent;
    [SerializeField] private EventReference guardianAttackEvent;
    [SerializeField] private EventReference guardianReloadEvent;

    [Header("Bat")]
    [SerializeField] private EventReference batFlyEvent;
    [SerializeField] private EventReference batTakeDamageEvent;
    [SerializeField] private EventReference batAttackEvent;
    [SerializeField] private EventReference batIdleEvent;

    [Header("Shop")]
    [SerializeField] private EventReference shopEnteringEvent;

    [Header("Scene Playlists")]
    [SerializeField] private ScenePlaylist[] scenePlaylists;

    [Header("Bus")]
    private Bus masterBus;
    private Bus SFXBus;
    private Bus musicBus;

    [System.Serializable]
    public class ScenePlaylist
    {
        public string sceneName;
        public EventReference[] tracks;
    }

    private EventInstance backgroundMusicInstance;
    private EventReference[] currentPlaylist;
    private int lastTrackIndex = -1;
    private bool isPlaylistRunning;

    private EventInstance ambientEventInstance;

    public enum JumpLandAction
    {
        Jump = 0,
        Land = 1,
        softLand = 2
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            if (SceneManager.GetActiveScene().name != "MainMenu")
                Debug.LogError("Больше одного AudioManager o_0 - Сцена: " + SceneManager.GetActiveScene().name);
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        masterBus = RuntimeManager.GetBus("bus:/");
        musicBus = RuntimeManager.GetBus("bus:/Music");
        SFXBus = RuntimeManager.GetBus("bus:/SFX");

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        //StartBackgroundPlaylist();
        StartAmbient(ambientEvent);
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

    // Фижма
    public void StartAmbient(EventReference sound)
    {
        ambientEventInstance = RuntimeManager.CreateInstance(sound);
        ambientEventInstance.start();
    }

    // Ты в лифте родился
    public void StartElevatorAmbient()
    {
        StartAmbient(ElevatorAmbientEvent);
    }

    // Ты в открытом лифте родился
    public void PlayOpenElevator()
    {
        if (ElevatorOpenEvent.IsNull) return;
        RuntimeManager.PlayOneShot(ElevatorOpenEvent);
    }
    public void PlayRingElevator()
    {
        if (ElevatorRingEvent.IsNull) return;
        RuntimeManager.PlayOneShot(ElevatorRingEvent);
    }
    public void PlayStartMovingElevator()
    {
        if (ElevatorStartMovingEvent.IsNull) return;
        RuntimeManager.PlayOneShot(ElevatorStartMovingEvent);
    }

    // Йоу печенье - програмное обеспечение >_<
    public void StartBackgroundPlaylist()
    {
        currentPlaylist = backgroundMusicPlaylist;

        if (currentPlaylist == null || currentPlaylist.Length == 0)
        {
            Debug.LogWarning("AudioManager: backgroundMusicPlaylist is empty");
            return;
        }

        isPlaylistRunning = true;
        PlayNextPlaylistTrack();
    }

    // Эй диджей
    private void PlayNextPlaylistTrack()
    {
        if (currentPlaylist == null || currentPlaylist.Length == 0)
            return;

        int nextIndex = Random.Range(0, currentPlaylist.Length);

        if (currentPlaylist.Length > 1)
        {
            while (nextIndex == lastTrackIndex)
            {
                nextIndex = Random.Range(0, currentPlaylist.Length);
            }
        }

        lastTrackIndex = nextIndex;

        EventReference nextTrack = currentPlaylist[nextIndex];

        if (nextTrack.IsNull)
        {
            Debug.LogWarning($"AudioManager: playlist track at index {nextIndex} is null");
            return;
        }

        backgroundMusicInstance = RuntimeManager.CreateInstance(nextTrack);
        backgroundMusicInstance.start();

        Debug.Log($"AudioManager: playing playlist track index {nextIndex}");
    }

    // Конец йоу-йоу
    public void StopBackgroundMusic()
    {
        isPlaylistRunning = false;

        if (backgroundMusicInstance.isValid())
        {
            backgroundMusicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            backgroundMusicInstance.release();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        foreach (var playlist in scenePlaylists)
        {
            if (playlist.sceneName == scene.name)
            {
                currentPlaylist = playlist.tracks;
                StopBackgroundMusic();
                isPlaylistRunning = true;
                PlayNextPlaylistTrack();
                return;
            }
        }

        // если сцена не найдена в списке — используем дефолт
        StartBackgroundPlaylist();
    }

    // Беги Фо-ватафо шнейне-пепе
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

    public void PlayGuardianFootstep(Vector2 worldPosition)
    {
        if (guardianFootstepEvent.IsNull) return;
        RuntimeManager.PlayOneShot(guardianFootstepEvent, worldPosition);
    }

    public void BatFly(Vector2 worldPosition)
    {
        if (batFlyEvent.IsNull) return;
        RuntimeManager.PlayOneShot(batFlyEvent, worldPosition);
    }

    // Прыг-скок-скок-скок-скок
    public void PlayJumpLand(Vector2 position, JumpLandAction action)
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

    // Звуки дешдя
    public void PlayDashSound(Vector2 position)
    {
        if (dashEvent.IsNull) return;
        RuntimeManager.PlayOneShot(dashEvent, position);
    }

    // Ай боль в ноге!
    public void PlayerTakeDamage(Vector2 position)
    {
        if (playerTakeDamageEvent.IsNull) return;
        RuntimeManager.PlayOneShot(playerTakeDamageEvent, position);
    }

    public void DoctorTakeDamage(Vector2 position)
    {
        if (doctorTakeDamageEvent.IsNull) return;
        RuntimeManager.PlayOneShot(doctorTakeDamageEvent, position);
    }
    public void GuardianTakeDamage(Vector2 position)
    {
        if (guardianTakeDamageEvent.IsNull) return;
        RuntimeManager.PlayOneShot(guardianTakeDamageEvent, position);
    }

    public void BatTakeDamage(Vector2 position)
    {
        if (batTakeDamageEvent.IsNull) return;
        RuntimeManager.PlayOneShot(batTakeDamageEvent, position);
    }

    // Ломай меня полностью
    public void PlayerAttack(Vector2 position)
    {
        if (playerAttackEvent.IsNull) return;
        RuntimeManager.PlayOneShot(playerAttackEvent, position);
    }

    public void PlayerPunch(Vector2 position)
    {
        if (playerPunchEvent.IsNull) return;
        RuntimeManager.PlayOneShot(playerPunchEvent, position);
    }

    public void DoctorAttack(Vector2 position)
    {
        if (doctorAttackEvent.IsNull) return;
        RuntimeManager.PlayOneShot(doctorAttackEvent, position);
    }

    public void DoctorAttack2(Vector2 position)
    {
        if (doctorAttack2Event.IsNull) return;
        RuntimeManager.PlayOneShot(doctorAttack2Event, position);
    }
    public void DoctorBreak(Vector2 position)
    {
        if (doctorBreakGlassEvent.IsNull) return;
        RuntimeManager.PlayOneShot(doctorBreakGlassEvent, position);
    }

    public void GuardianAttack(Vector2 position)
    {
        if (guardianAttackEvent.IsNull) return;
        RuntimeManager.PlayOneShot(guardianAttackEvent, position);
    }

    public void GuardianReload(Vector2 position)
    {
        if (guardianReloadEvent.IsNull) return;
        RuntimeManager.PlayOneShot(guardianReloadEvent, position);
    }

    public void BatAttack(Vector2 position)
    {
        if (batAttackEvent.IsNull) return;
        RuntimeManager.PlayOneShot(batAttackEvent, position);
    }

    // Неловие звуки работы
    public void PlayDoctorIdle(Vector2 position)
    {
        if (doctorIdleEvent.IsNull) return;
        RuntimeManager.PlayOneShot(doctorIdleEvent, position);
    }

    public void PlayBatIdle(Vector2 position)
    {
        if (batIdleEvent.IsNull) return;
        RuntimeManager.PlayOneShot(batIdleEvent, position);
    }

    //Магазинчик

    public void PlayMarketEnter(Vector2 position)
    {
        if (shopEnteringEvent.IsNull) return;
        RuntimeManager.PlayOneShot(shopEnteringEvent, position);
    }
}
