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

        // 2. РЫВОК (DODGE) --- НОВОЕ ---
    // Проверяем ввод + прошел ли кулдаун
        if (player.DodgeInput && Time.time > player.lastDodgeTime + player.stats.dodgeCooldown)
        {
            stateMachine.ChangeState(player.LocoDodge);
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