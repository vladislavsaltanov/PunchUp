using UnityEngine;

public abstract class AttackSO : ActionSO
{
    [Header("Damage")]
    public ushort baseDamage = 10;
    public float windupTime = 0.15f;

    [Header("Knockback")]
    public Vector2 knockbackForce = new Vector2(8f, 4f);
    public float knockbackDuration = 0.2f;

    protected void ApplyDamageAndKnockback(BaseEntity owner, BaseEntity target)
    {
        if (target == null || target == owner || target.gameObject == owner.gameObject) return;

        // 1. Наносим урон
        // (Используем Stats["attackPower"] если есть, иначе 1)
        float multiplier = owner.Stats != null ? owner.Stats[StatType.AttackPower] : 1f;
        ushort finalDamage = (ushort)(baseDamage * multiplier);

        target.TakeDamage(finalDamage, owner.transform, owner._name);

        // 2. Рассчитываем отталкивание
        // Вектор ОТ атакующего К цели
        float dirX = Mathf.Sign(target.transform.position.x - owner.transform.position.x);
        Vector2 knockback = new Vector2(dirX * knockbackForce.x, knockbackForce.y);

        // 3. Применяем
        target.ApplyVelocityOverride(knockback, knockbackDuration);
    }
}