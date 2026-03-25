using System.Threading;
using UnityEngine;

public class CombatHandler : MonoBehaviour
{
    [SerializeField] BaseEntity owner;

    float primaryCooldownTimer;
    float specialCooldownTimer;

    public bool IsBusy { get; private set; }
    CancellationTokenSource cts;
    void Awake()
    {
        if (owner == null) owner = GetComponent<BaseEntity>();
        IsBusy = false;
    }

    void Update()
    {
        if (primaryCooldownTimer > 0) primaryCooldownTimer -= Time.deltaTime;
        if (specialCooldownTimer > 0) specialCooldownTimer -= Time.deltaTime;
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

        await action.Execute(owner);

        if (token.IsCancellationRequested)
        {
            IsBusy = false;
            return;
        }

        setCooldownCallback?.Invoke(action.cooldown);
        IsBusy = false;
    }

    public void CancelAll()
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = null;
        IsBusy = false;
        owner.ClearVelocityOverride();
    }
}