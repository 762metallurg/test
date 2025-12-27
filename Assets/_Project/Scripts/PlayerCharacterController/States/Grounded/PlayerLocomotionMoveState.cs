using UnityEngine;

public class PlayerLocomotionMoveState : PlayerGroundedState
{
    public PlayerLocomotionMoveState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        player.useFloating = true; // Включаем левитацию при ходьбе
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // 1. Если стик отпущен -> Стоим
        if (player.MoveInput == Vector2.zero)
        {
            stateMachine.ChangeState(player.LocoIdle);
            return;
        }

        // 2. Спринт (Shift) приоритетнее ходьбы
        if (player.IsSprintingInput)
        {
            stateMachine.ChangeState(player.LocoSprint);
            return;
        }

        // 3. ДОДЖ (Рывок) - Добавь, если уже сделал стейт доджа
        if (player.DodgeInput && Time.time > player.lastDodgeTime + player.stats.dodgeCooldown)
        {
             stateMachine.ChangeState(player.LocoDodge);
             return;
        }

        // === РАСЧЕТ АНИМАЦИИ (0.5 = Ходьба, 1.0 = Бег) ===
        float targetAnimValue = 1f; 

        // Если стик нажат слабо (< 0.55) ИЛИ включен режим ходьбы (Ctrl)
        if (player.MoveAmount < 0.55f || player.IsWalking)
        {
            targetAnimValue = 0.5f; // Ходьба
        }
        else
        {
            targetAnimValue = 1f;   // Бег
        }

        //player.AnimationManager.UpdateAnimatorValues(targetAnimValue, false);
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        // === РАСЧЕТ СКОРОСТИ ===
        float targetSpeed = player.MoveSpeed; // По умолчанию бежим

        // Та же логика: Слабый стик ИЛИ включен Ctrl
        if (player.MoveAmount < 0.55f || player.IsWalking)
        {
            targetSpeed = player.WalkingSpeed;
            // ВРЕМЕННЫЙ DEBUG (Если скорость не меняется - раскомментируй строку ниже)
            // Debug.Log($"Идем пешком! Скорость: {targetSpeed}");
        }
        else
        {
            targetSpeed = player.MoveSpeed;
        }

        // Передаем итоговую скорость в контроллер
        player.HandleMovement(targetSpeed);
    }
}