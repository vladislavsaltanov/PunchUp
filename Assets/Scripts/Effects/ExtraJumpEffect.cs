using UnityEngine;

[CreateAssetMenu(menuName = "Effects/Extra Jump")]
public class ExtraJumpEffect : EntityEffectData
{
    public byte extraJumps = 1;

    public override void Execute(BaseEntity entity, ActiveEffect activeEffect)
    {
        var movement = entity.GetComponent<PlayerMovement>();
        movement?.AddJump(extraJumps);
    }

    public override void Remove(BaseEntity entity, ActiveEffect activeEffect)
    {
        var movement = entity.GetComponent<PlayerMovement>();
        movement?.RemoveJump(extraJumps);
    }
}
