using UnityEngine;

public class PlayerLocomotionSprintState : PlayerBaseState
{
    public PlayerLocomotionSprintState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        player.useFloating = true;
    }

    public override void LogicUpdate()
    {
        if (player.MoveInput.magnitude < 0.1f)
        {
            stateMachine.ChangeState(player.LocoIdle);
            return;
        }

        if (!player.IsSprintingInput)
        {
            stateMachine.ChangeState(player.LocoMove);
        }
    }

    public override void PhysicsUpdate()
    {
        player.HandleMovement(player.SprintSpeed);
    }
}