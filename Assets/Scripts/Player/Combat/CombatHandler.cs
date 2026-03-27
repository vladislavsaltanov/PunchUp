using System.Threading;
using UnityEngine;

public class CombatHandler : MonoBehaviour
{
    [SerializeField] BaseEntity owner;

    float primaryCooldownTimer;
    float specialCooldownTimer;

    public bool IsBusy { get; private set; }
    CancellationTokenSource cts;

    // --- Soft-lock guard ---
    float busySinceTime = -1f;
    [SerializeField] float busySoftLockTimeout = 3f; // should be > max action duration

    void Awake()
    {
        if (owner == null) owner = GetComponent<BaseEntity>();
        IsBusy = false;
    }

    void Update()
    {
        if (primaryCooldownTimer > 0) primaryCooldownTimer -= Time.deltaTime;
        if (specialCooldownTimer > 0) specialCooldownTimer -= Time.deltaTime;

        // If timeScale == 0, Time.deltaTime is 0, so timeout won't progress here.
        // We'll also trigger the guard from UIManager on pause/unpause.
        if (IsBusy && busySinceTime > 0f && Time.timeScale > 0f)
        {
            if (Time.time - busySinceTime > busySoftLockTimeout)
                ForceResetIfSoftLocked("timeout");
        }
    }

    public bool TryPrimaryAttack()
    {
        return TryExecute(
            owner.primaryAttack,
            primaryCooldownTimer,
            (newTime) => primaryCooldownTimer = newTime
        );
    }

    public bool TrySpecialAbility()
    {
        return TryExecute(
            owner.specialAbility,
            specialCooldownTimer,
            (newTime) => specialCooldownTimer = newTime
        );
    }

    bool TryExecute(ActionSO action, float currentTimer, System.Action<float> setCooldownCallback)
    {
        if (action == null) return false;

        if (IsBusy || currentTimer > 0 || owner.CurrentHealth <= 0)
        {
            return false;
        }

        cts?.Cancel();
        cts?.Dispose();
        cts = new CancellationTokenSource();

        _ = ExecuteRoutine(action, setCooldownCallback, cts.Token);

        return true;
    }

    async Awaitable ExecuteRoutine(ActionSO action, System.Action<float> setCooldownCallback, CancellationToken token)
    {
        IsBusy = true;
        busySinceTime = Time.time;

        await action.Execute(owner);

        if (token.IsCancellationRequested)
        {
            IsBusy = false;
            busySinceTime = -1f;
            return;
        }

        setCooldownCallback?.Invoke(action.cooldown);
        IsBusy = false;
        busySinceTime = -1f;
    }

    /// <summary>
    /// Resets only the stuck Busy state (and pending routine), without touching cooldown timers.
    /// Safe to call on pause/unpause.
    /// </summary>
    public void ForceResetIfSoftLocked(string reason)
    {
        if (!IsBusy) return;

        // We do NOT change cooldown timers here to avoid abuse.
        cts?.Cancel();
        cts?.Dispose();
        cts = null;

        IsBusy = false;
        busySinceTime = -1f;

        // Also clear velocity override to avoid being stuck in knockback/override state.
        owner.ClearVelocityOverride();

        Debug.LogWarning($"CombatHandler soft-reset ({reason}) on {owner.name}");
    }

    public void CancelAll()
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = null;
        IsBusy = false;
        busySinceTime = -1f;
        owner.ClearVelocityOverride();
    }
}