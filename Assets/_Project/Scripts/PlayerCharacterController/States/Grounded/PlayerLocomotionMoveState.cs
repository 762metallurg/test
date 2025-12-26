using UnityEngine;

public class PlayerLocomotionMoveState : PlayerGroundedState
{
    public PlayerLocomotionMoveState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        player.useFloating = true; // Левитируем
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        if (player.MoveInput.magnitude < 0.1f)
        {
            stateMachine.ChangeState(player.LocoIdle);
            return;
        }

        if (player.IsSprintingInput)
        {
            stateMachine.ChangeState(player.LocoSprint);
        }
    }

    public override void PhysicsUpdate()
    {
        player.HandleMovement(player.MoveSpeed);
    }
}