using UnityEngine;

public class BossFist : MonoBehaviour
{
    [SerializeField] LayerMask playerLayer;
    [SerializeField] public ushort slamDamage = 25;
    [SerializeField] public ushort swipeDamage = 15;
    [SerializeField] float damageCooldown = 0.5f;
    [SerializeField] float defaultMoveSpeed = 10f;

    public bool IsHitboxActive { get; set; }
    public bool IsMoving => Vector2.Distance(
        transform.position, _targetPosition) > 0.05f;

    Vector2 _targetPosition;
    float _currentMoveSpeed;
    float _lastDamageTime;
    bool _isSlamMode;
    bool _returningToRest;

    EnemyAIBoss _boss;
    Vector2 _restOffset;

    public void Init(EnemyAIBoss boss, Vector2 restOffset)
    {
        _boss = boss;
        _restOffset = restOffset;
        _currentMoveSpeed = defaultMoveSpeed;
        transform.position = (Vector2)boss.transform.position + restOffset;
        _targetPosition = transform.position;
    }

    void Update()
    {
        if (_returningToRest)
            _targetPosition = (Vector2)_boss.transform.position + _restOffset;

        transform.position = Vector2.MoveTowards(
            transform.position, _targetPosition,
            _currentMoveSpeed * Time.deltaTime);
    }

    public void ReturnToRest()
    {
        IsHitboxActive = false;
        _returningToRest = true;
        _currentMoveSpeed = defaultMoveSpeed;
    }

    public void ActivateSlam(Vector2 targetPos, float speed)
    {
        _returningToRest = false;
        _isSlamMode = true;
        IsHitboxActive = true;
        _targetPosition = targetPos;
        _currentMoveSpeed = speed;
    }

    public void ActivateSwipe(Vector2 startPos, Vector2 endPos, float speed)
    {
        _returningToRest = false;
        _isSlamMode = false;
        IsHitboxActive = true;
        transform.position = startPos;
        _targetPosition = endPos;
        _currentMoveSpeed = speed;
    }
    public void ReturnToRestFast()
    {
        IsHitboxActive = false;
        _returningToRest = true;
        _currentMoveSpeed = defaultMoveSpeed * 3f;
    }
    void OnTriggerStay2D(Collider2D other)
    {
        if (!IsHitboxActive) return;
        if (Time.time < _lastDamageTime + damageCooldown) return;
        if (((1 << other.gameObject.layer) & playerLayer.value) == 0) return;

        var entity = other.GetComponentInParent<BaseEntity>();
        if (entity == null) return;

        ushort dmg = _isSlamMode ? slamDamage : swipeDamage;
        _lastDamageTime = Time.time;
        entity.TakeDamage(dmg, transform, "кулак Тесея");
    }
}
