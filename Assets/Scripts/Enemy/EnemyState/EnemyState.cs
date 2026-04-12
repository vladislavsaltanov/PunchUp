public enum EnemyState
{
    Idle, // waiting for state change
    Walking, // walking around
    Combat, // fighting the player
    Waiting, // waiting for rotation
    UsingAbility,  // using a special ability
    WalkingTowardsPlayer, // walking towards the player to attack
    Attacking,

    // Bat-specific
    FlyingPatrol,
    DivingToPoint,
    WaitingOnGround,
    RisingBack
}