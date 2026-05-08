using UnityEngine;
using UnityEngine.SceneManagement;

public class BossAudio : MonoBehaviour
{
    public static BossAudio Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        Instance = this;
    }
    // Смерть
    public void HandleDeath()
    {
        AudioManager.Instance?.PlayBossDeath(transform.position);
    }

    // Рывок
    public void HandleDash()
    {
        AudioManager.Instance?.PlayBossDash(transform.position);
    }

    // Приземление
    public void HandleLand()
    {
        AudioManager.Instance?.PlayBossLand(transform.position);
        Debug.Log("Land");
    }

    // Прыжок
    public void HandleJump()
    {
        AudioManager.Instance?.PlayBossJump(transform.position);
    }

    // Idle
    public void HandleIdle()
    {
        AudioManager.Instance?.PlayBossIdle(transform.position);
    }

    // Получение урона
    public void HandleDamage()
    {
        AudioManager.Instance?.PlayBossDamage(transform.position);
    }
}
