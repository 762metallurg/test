using UnityEngine;

public class PlayerGroundedState : PlayerBaseState
{
    public PlayerGroundedState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // !!! ВОТ ЭТОТ БЛОК ОТВЕЧАЕТ ЗА ПЕРЕХОД В ПРЫЖОК !!!
        if (player.JumpInput && player.isGrounded)
        {
            stateMachine.ChangeState(player.LocoJump);
            return; 
        }

        if (!player.isGrounded)
        {
            stateMachine.ChangeState(player.LocoAir);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }
}