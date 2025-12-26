using UnityEngine;

public class PlayerLocomotionIdleState : PlayerGroundedState
{
    public PlayerLocomotionIdleState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        player.useFloating = true; // Включаем левитацию
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        // В будущем тут будет: if (!player.isGrounded) stateMachine.ChangeState(player.LocoFall);

        if (player.MoveInput.magnitude > 0.1f)
        {
            stateMachine.ChangeState(player.LocoMove);
        }
    }

    public override void PhysicsUpdate()
    {
        // Тормозим персонажа
        player.ApplyFriction(0.1f);
    }
}