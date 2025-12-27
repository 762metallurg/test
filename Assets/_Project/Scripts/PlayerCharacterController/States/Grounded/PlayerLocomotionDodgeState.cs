using UnityEngine;

public class PlayerLocomotionDodgeState : PlayerBaseState
{
    private float startTime;
    private Vector3 dodgeDirection;

    public PlayerLocomotionDodgeState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        startTime = Time.time;
        player.useFloating = true; // Продолжаем левитировать, чтобы не тереться пузом

        // 1. Определяем направление
        // Если давим стик - рывок туда. Если стоим - рывок назад (классика).
        if (player.MoveInput != Vector2.zero)
        {
            // Копируем логику получения направления из MoveState
            Vector3 camFwd = player.cameraTransform.forward;
            Vector3 camRight = player.cameraTransform.right;
            camFwd.y = 0; camRight.y = 0;
            camFwd.Normalize(); camRight.Normalize();

            dodgeDirection = (camFwd * player.MoveInput.y + camRight * player.MoveInput.x).normalized;
        }
        else
        {
            // Рывок назад относительно камеры/персонажа, если нет ввода
            dodgeDirection = -player.transform.forward; 
        }

        // 2. Сразу поворачиваем персонажа в сторону рывка (опционально)
        player.transform.rotation = Quaternion.LookRotation(dodgeDirection);
        
        // 3. Анимация
        //player.AnimationManager.PlayTargetAction("Dodge", true); 
        // true = использовать Root Motion или просто флаг
    }

    public override void LogicUpdate()
    {
        // Выход из состояния по таймеру
        if (Time.time > startTime + player.stats.dodgeTime)
        {
            // Если давим кнопки -> бежим, иначе -> стоим
            if (player.MoveInput != Vector2.zero)
                stateMachine.ChangeState(player.LocoMove);
            else
                stateMachine.ChangeState(player.LocoIdle);
        }
    }

    public override void PhysicsUpdate()
    {
        // ПРИНУДИТЕЛЬНОЕ ДВИЖЕНИЕ
        // Мы не используем HandleMovement, мы толкаем вручную, игнорируя инпут
        
        Vector3 velocity = dodgeDirection * player.stats.dodgeSpeed;
        
        // Сохраняем вертикальную скорость (гравитацию), меняем только X/Z
        float currentY = player.RB.linearVelocity.y;
        velocity.y = currentY; 
        
        player.RB.linearVelocity = velocity;
    }
}