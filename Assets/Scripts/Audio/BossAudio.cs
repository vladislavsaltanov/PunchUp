using UnityEngine;

public class BossAudio : MonoBehaviour
{
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
