// Combat/CombatHandler.cs
using UnityEngine;

public class CombatHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] BaseEntity owner;

    [Header("Attack")]
    [SerializeField] AttackSO attack;

    [Header("Ability (Optional)")]
    [SerializeField] ActionSO ability;

    // Runtime
    float attackCooldown;
    float abilityCooldown;
    bool isAttacking;
    bool isUsingAbility;
    Coroutine attackCoroutine;
    Coroutine abilityCoroutine;

    public bool IsAttacking => isAttacking;
    public bool IsUsingAbility => isUsingAbility;
    public bool IsBusy => isAttacking || isUsingAbility;

    public bool HasAbility => ability != null;
    public bool AttackReady => attack != null && attackCooldown <= 0f && !isAttacking;
    public bool AbilityReady => ability != null && abilityCooldown <= 0f && !isUsingAbility;

    void Awake()
    {
        if (owner == null)
            owner = GetComponent<BaseEntity>();
    }

    void Update()
    {
        if (attackCooldown > 0f)
            attackCooldown -= Time.deltaTime;

        if (abilityCooldown > 0f)
            abilityCooldown -= Time.deltaTime;
    }

    public bool TryAttack()
    {
        if (!AttackReady || IsBusy || owner.CurrentHealth <= 0)
            return false;

        isAttacking = true;

        attackCoroutine = owner.StartCoroutine(attack.Execute(owner, () =>
        {
            isAttacking = false;
            attackCooldown = attack.cooldown;
            attackCoroutine = null;
        }));

        return true;
    }

    public bool TryUseAbility()
    {
        if (!AbilityReady || IsBusy || owner.CurrentHealth <= 0)
            return false;

        isUsingAbility = true;

        abilityCoroutine = owner.StartCoroutine(ability.Execute(owner, () =>
        {
            isUsingAbility = false;
            abilityCooldown = ability.cooldown;
            abilityCoroutine = null;
        }));

        return true;
    }

    public void SetAbility(ActionSO newAbility)
    {
        ability = newAbility;
        abilityCooldown = 0f;
    }

    public void ClearAbility()
    {
        ability = null;
    }

    public void CancelAll()
    {
        if (attackCoroutine != null)
        {
            owner.StopCoroutine(attackCoroutine);
            isAttacking = false;
            attackCoroutine = null;
        }

        if (abilityCoroutine != null)
        {
            owner.StopCoroutine(abilityCoroutine);
            isUsingAbility = false;
            abilityCoroutine = null;
        }

        owner.ClearVelocityOverride();
    }

    void OnDrawGizmosSelected()
    {
        if (owner == null) return;

        Gizmos.color = Color.red;
        attack?.DrawGizmo(owner.transform, owner.direction);
    }
}