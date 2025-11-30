using System;
using System.Collections;
using UnityEngine;

public class EnemyLogic : MonoBehaviour
{
    [SerializeField] EnemyMovementBaseSO movementLogic;
    public Rigidbody2D rb;
    public EnemyState currentState;

    [SerializeField] Collider2D leftSideTrigger, rightSideTrigger;
    public bool nearLeftWallOrEdge, nearRightWallOrEdge;
    public EnemyBaseMovementState movementState = new EnemyBaseMovementState();

    void Awake()
    {
        StartCoroutine(WaitFor(UnityEngine.Random.Range(1f, 3f), () => currentState = EnemyState.Walking));
        movementState.direction = (sbyte)(UnityEngine.Random.value > 0.5f ? 1 : -1);
    }

    private void Update()
    {
        // checking for walls or edges
        nearLeftWallOrEdge = leftSideTrigger.IsTouchingLayers(LayerMask.GetMask("Wall")) || !leftSideTrigger.IsTouchingLayers(LayerMask.GetMask("Ground"));
        nearRightWallOrEdge = rightSideTrigger.IsTouchingLayers(LayerMask.GetMask("Wall")) || !rightSideTrigger.IsTouchingLayers(LayerMask.GetMask("Ground"));

        // execute movement logic
        if (currentState == EnemyState.Walking)
            movementLogic.Movement(this, movementState);
    }

    public IEnumerator WaitFor(float time, Action action)
    {
        yield return new WaitForSeconds(time);
        action?.Invoke();
    }

}
public class EnemyBaseMovementState
{
    public bool isWaiting = false;
    public sbyte direction = 0;
    public Coroutine currentCoroutine = null;
}