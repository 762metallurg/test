using UnityEngine;

public class PlayerLocomotionAirState : PlayerBaseState
{
    public PlayerLocomotionAirState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

    public override void LogicUpdate()
    {
        // === ЗАЩИТА ОТ МГНОВЕННОГО ПРИЗЕМЛЕНИЯ ===
        // Если прошло меньше 0.2 сек с начала прыжка — не проверяем землю!
        if (Time.time < player.lastJumpTime + player.stats.jumpCooldown) 
        {
            return; 
        }

        // Если коснулись земли ПОСЛЕ задержки -> Приземляемся
        if (player.isGrounded)
        {
            stateMachine.ChangeState(player.LocoIdle);
        }
    }

    public override void PhysicsUpdate()
    {
        // Позволяем немного управлять в воздухе
        float targetSpeed = player.MoveSpeed * player.AirControl;
        player.HandleMovement(targetSpeed);
    }
}