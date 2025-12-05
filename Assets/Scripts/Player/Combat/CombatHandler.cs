using UnityEngine;

public class CombatHandler : MonoBehaviour
{
    [SerializeField] BaseEntity owner;

    float primaryCooldownTimer;
    float specialCooldownTimer;

    public bool IsBusy { get; private set; }

    void Awake()
    {
        if (owner == null) owner = GetComponent<BaseEntity>();
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

        StartCoroutine(ExecuteRoutine(action, setCooldownCallback));
        return true;
    }

    System.Collections.IEnumerator ExecuteRoutine(ActionSO action, System.Action<float> setCooldownCallback)
    {
        IsBusy = true;

        yield return action.Execute(owner);

        IsBusy = false;

        setCooldownCallback?.Invoke(action.cooldown);
    }

    public void CancelAll()
    {
        StopAllCoroutines();
        IsBusy = false;
        owner.ClearVelocityOverride();
    }
}