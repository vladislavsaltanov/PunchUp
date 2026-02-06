public class ActiveEffect
{
    public EntityEffectData Data { get; private set; }
    public float RemainingTime { get; private set; }
    public BaseEntity Owner { get; private set; }

    // Хранилище для данных эффекта (модификаторы, ссылки и т.д.)
    public object RuntimeData { get; set; }

    public float NormalizedTime => Data.duration > 0 ? RemainingTime / Data.duration : 0f;
    public bool IsExpired => RemainingTime <= 0f;

    public ActiveEffect(EntityEffectData data, BaseEntity owner)
    {
        Data = data;
        Owner = owner;
        RemainingTime = data.duration;
    }

    public void Tick(float deltaTime)
    {
        RemainingTime -= deltaTime;
        Data.Tick(Owner, this, deltaTime);
    }

    public void Refresh()
    {
        RemainingTime = Data.duration;
    }

    public void Execute()
    {
        Data.Execute(Owner, this);
    }

    public void Remove()
    {
        Data.Remove(Owner, this);
    }
}