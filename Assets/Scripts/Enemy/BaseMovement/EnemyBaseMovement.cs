using System;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyBaseMovement", menuName = "ScriptableObjects/Enemy/EnemyBaseMovement", order = 1)]
public class EnemyBaseMovement : EnemyMovementBaseSO
{
    public float movementSpeed = 3f;
    public float rotationDelay = 2f;

    public override void Movement(EnemyLogic logic, EnemyBaseMovementState state)
    {
        if (logic.nearLeftWallOrEdge && state.direction == -1 && !state.isWaiting)
        {
            state.direction = 1;
            state.isWaiting = true;
            logic.rb.linearVelocityX = 0;

            if (state.currentCoroutine != null)
                logic.StopCoroutine(state.currentCoroutine);

            state.currentCoroutine = logic.StartCoroutine(logic.WaitFor(rotationDelay, () => state.isWaiting = false));
        }
        else if (logic.nearRightWallOrEdge && state.direction == 1 && !state.isWaiting)
        {
            state.direction = -1;
            state.isWaiting = true;
            logic.rb.linearVelocityX = 0;

            if (state.currentCoroutine != null)
                logic.StopCoroutine(state.currentCoroutine);

            state.currentCoroutine = logic.StartCoroutine(logic.WaitFor(rotationDelay, () => state.isWaiting = false));
        }

        if (!state.isWaiting)
            logic.rb.linearVelocityX = state.direction * movementSpeed;
    }
}
