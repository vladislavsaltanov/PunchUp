using UnityEngine;

[CreateAssetMenu(fileName = "EnemyBaseMovement", menuName = "ScriptableObjects/Enemy/EnemyBaseMovement")]
public class EnemyBaseMovement : EnemyMovementBaseSO
{
    [Header("Movement")]
    public float rotationDelay = 0.5f;
    public float movementTowardsPlayerSpeedMultiplier = 1.5f;

    [Header("Stuck Detection")]
    public float moveThreshold = 0.001f;   // почти стоим
    public float stuckTimeThreshold = 0.2f;   // стоим дольше 0.2 сек → застряли

    public override void Movement(EnemyLogic logic, EnemyContextState state)
    {
        if (IsBlocked(logic, logic.direction) || IsStuck(logic, state))
        {
            logic.rb.linearVelocityX = 0;
            logic.direction *= -1;
            logic.EnterWait(rotationDelay);
            state.stuckTimer = 0f;
            return;
        }

        if (logic.currentState != EnemyState.Waiting)
        {
            float speed = logic.Stats["speed"];
            logic.rb.linearVelocityX = logic.direction * speed;
        }
    }

    public override void MovementTowardsPlayer(EnemyLogic logic, EnemyContextState state, EnemyPlayerDetectionSO detection, sbyte direction)
    {
        logic.direction = direction;

        // При погоне — если упёрлись или застряли, просто стоим (не отворачиваемся от игрока)
        if (IsBlocked(logic, direction))
        {
            logic.rb.linearVelocityX = 0;
            return;
        }

        if (!detection.isPlayerTooClose(state.playerDistance))
        {
            float speed = logic.Stats["speed"];
            logic.rb.linearVelocityX = direction * speed * movementTowardsPlayerSpeedMultiplier;
        }
    }

    public override void Stop(EnemyLogic logic)
    {
        logic.rb.linearVelocityX = 0;
    }

    public override bool IsBlocked(EnemyLogic logic, sbyte direction)
    {
        if (logic.entityCollider == null)
        {
            Debug.LogError("Не назначен Entity Collider в инспекторе врага!");
            return false;
        }

        Bounds bounds = logic.entityCollider.bounds;

        // Определяем "передний край" врага по направлению движения
        // Если direction == 1 (право), берем max.x. Если -1 (лево), берем min.x
        float frontX = (direction > 0) ? bounds.max.x : bounds.min.x;

        // Центр по высоте и Низ коллайдера
        float centerY = bounds.center.y;
        float bottomY = bounds.min.y;

        // --- 1. ПРОВЕРКА СТЕНЫ (Wall Check) ---
        // Стреляем от переднего края по центру высоты
        Vector2 wallOrigin = new Vector2(frontX, centerY);
        Vector2 wallDir = new Vector2(direction, 0);

        RaycastHit2D[] wallHits = Physics2D.RaycastAll(wallOrigin, wallDir, wallCheckDistance, groundLayer | obstacleLayer);
        Debug.DrawRay(wallOrigin, wallDir * wallCheckDistance, Color.red);

        foreach (var hit in wallHits)
        {
            // Фильтруем себя (по иерархии)
            if (hit.transform.IsChildOf(logic.transform)) continue;

            if (!hit.collider.isTrigger)
                return true; // Стена найдена
        }

        // --- 2. ПРОВЕРКА ОБРЫВА (Ledge Check) ---
        // Точка начала: от переднего края сдвигаемся еще немного вперед (LookAhead) и берем уровень ног (bottomY)
        Vector2 ledgeOrigin = new Vector2(frontX + (direction * ledgeLookAhead), bottomY);

        RaycastHit2D floorHit = Physics2D.Raycast(ledgeOrigin, Vector2.down, ledgeRayLength, groundLayer);
        Debug.DrawRay(ledgeOrigin, Vector2.down * ledgeRayLength, Color.green);

        // Если под ногами (чуть впереди) нет земли
        if (floorHit.collider == null)
        {
            return true; // Обрыв найден
        }

        return false;
    }

    bool IsStuck(EnemyLogic logic, EnemyContextState state)
    {
        if (logic.currentState == EnemyState.Waiting)
        {
            state.stuckTimer = 0f;
            state.lastXPosition = logic.transform.position.x;
            return false;
        }

        // Вычисляем, насколько реально сдвинулись с прошлого кадра
        float distanceMoved = Mathf.Abs(logic.transform.position.x - state.lastXPosition);

        // Обновляем позицию для следующего кадра
        state.lastXPosition = logic.transform.position.x;

        // Если сдвинулись меньше чем на порог (почти стоим)
        if (distanceMoved < moveThreshold)
        {
            state.stuckTimer += Time.deltaTime;

            if (state.stuckTimer > stuckTimeThreshold)
            {
                return true; // Застряли окончательно
            }
        }
        else
        {
            // Если нормально двигаемся — сбрасываем таймер
            state.stuckTimer = 0f;
        }

        return false;
    }
}