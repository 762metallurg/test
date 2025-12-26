using UnityEngine;

// Наследуемся от BASE, а не от Grounded (чтобы не зациклилось)
public class PlayerLocomotionJumpState : PlayerBaseState
{
    public PlayerLocomotionJumpState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        // 1. ПИНОК ФИЗИКИ (Самое важное!)
        player.HandleJump();
        
        // 2. Анимация (если есть)
        //player.AnimationManager.PlayJump();

        // 3. Уходим в полет
        stateMachine.ChangeState(player.LocoAir);
    }

    public override void LogicUpdate() { }
    public override void PhysicsUpdate() { }
    public override void Exit() { }
}